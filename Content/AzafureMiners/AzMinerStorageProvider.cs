using System;
using System.Collections.Generic;
using InnoVault.Storages;
using InnoVault.TileProcessors;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;

namespace PWing.Content.AzafureMiners
{
	internal class AzMinerStorageProvider : IStorageProvider
	{
		private readonly AzMinerTP _azMinerTP;
		private readonly Point16 _position;

		public string Identifier => "CE.AzMiner";
		public Point16 Position => _position;

		public Vector2 WorldCenter
		{
			get
			{
				if (_azMinerTP != null)
				{
					return _azMinerTP.CenterInWorld;
				}
				return Utils.ToWorldCoordinates(_position, 8f, 8f);
			}
		}

		public Rectangle HitBox
		{
			get
			{
				if (_azMinerTP != null)
				{
					return _azMinerTP.HitBox;
				}
				return new Rectangle(_position.X * 16, _position.Y * 16, 64, 48);
			}
		}

		public bool IsValid
		{
			get
			{
				if (_azMinerTP == null)
				{
					return false;
				}
				AzMinerTP tempTP = null;
				if (!TileProcessorLoader.AutoPositionGetTP<AzMinerTP>(_position, out tempTP))
				{
					return false;
				}
				return tempTP == _azMinerTP;
			}
		}

		public bool HasSpace
		{
			get
			{
				if (!IsValid)
				{
					return false;
				}
				foreach (Item item in _azMinerTP.items)
				{
					if (item == null || item.IsAir)
					{
						return true;
					}
				}
				foreach (Item item2 in _azMinerTP.items)
				{
					if (item2 != null && !item2.IsAir && item2.stack < item2.maxStack)
					{
						return true;
					}
				}
				return false;
			}
		}

		public AzMinerStorageProvider(AzMinerTP azMinerTP)
		{
			_azMinerTP = azMinerTP;
			_position = azMinerTP?.Position ?? Point16.NegativeOne;
		}

		public static AzMinerStorageProvider FromPosition(Point16 position)
		{
			AzMinerTP tempTP = null;
			if (!TileProcessorLoader.AutoPositionGetTP<AzMinerTP>(position, out tempTP))
			{
				return null;
			}
			return new AzMinerStorageProvider(tempTP);
		}

		public static AzMinerStorageProvider FindNearPosition(Point16 position, int range, Item item)
		{
			AzMinerTP tp = TileProcessorLoader.FindModuleRangeSearch<AzMinerTP>(position, range);
			if (tp == null)
			{
				return null;
			}
			AzMinerStorageProvider provider = new AzMinerStorageProvider(tp);
			if (item != null && !item.IsAir && !provider.CanAcceptItem(item))
			{
				return null;
			}
			return provider;
		}

		public static AzMinerStorageProvider GetAtPosition(Point16 position, Item item)
		{
			AzMinerTP tempTP = null;
			if (!TileProcessorLoader.AutoPositionGetTP<AzMinerTP>(position, out tempTP))
			{
				return null;
			}
			AzMinerStorageProvider provider = new AzMinerStorageProvider(tempTP);
			if (item != null && !item.IsAir && !provider.CanAcceptItem(item))
			{
				return null;
			}
			return provider;
		}

		public bool CanAcceptItem(Item item)
		{
			if (!IsValid || item == null || item.IsAir)
			{
				return false;
			}
			return HasSpace;
		}

		public bool DepositItem(Item item)
		{
			if (!CanAcceptItem(item))
			{
				return false;
			}
			foreach (Item stored in _azMinerTP.items)
			{
				if (stored.type == item.type && stored.stack < stored.maxStack)
				{
					int addAmount = Math.Min(item.stack, stored.maxStack - stored.stack);
					stored.stack += addAmount;
					item.stack -= addAmount;
					if (item.stack <= 0)
					{
						item.TurnToAir(false);
						_azMinerTP.SendData();
						return true;
					}
				}
			}
			for (int i = 0; i < _azMinerTP.items.Count; i++)
			{
				if (_azMinerTP.items[i] == null || _azMinerTP.items[i].IsAir)
				{
					_azMinerTP.items[i] = item.Clone();
					item.TurnToAir(false);
					_azMinerTP.SendData();
					return true;
				}
			}
			return false;
		}

		public Item WithdrawItem(int itemType, int count)
		{
			if (!IsValid || count <= 0)
			{
				return new Item();
			}
			int remaining = count;
			Item result = new Item(itemType, 0, 0);
			int i = _azMinerTP.items.Count - 1;
			while (i >= 0 && remaining > 0)
			{
				Item slotItem = _azMinerTP.items[i];
				if (slotItem != null && !slotItem.IsAir && slotItem.type == itemType)
				{
					int take = Math.Min(remaining, slotItem.stack);
					slotItem.stack -= take;
					result.stack += take;
					remaining -= take;
					if (slotItem.stack <= 0)
					{
						_azMinerTP.items[i] = new Item();
					}
				}
				i--;
			}
			if (result.stack > 0)
			{
				result.type = itemType;
				_azMinerTP.SendData();
			}
			return result;
		}

		public IEnumerable<Item> GetStoredItems()
		{
			if (!IsValid)
			{
				yield break;
			}
			foreach (Item item in _azMinerTP.items)
			{
				if (item != null && !item.IsAir)
				{
					yield return item;
				}
			}
		}

		public long GetItemCount(int itemType)
		{
			if (!IsValid)
			{
				return 0L;
			}
			long count = 0L;
			foreach (Item item in _azMinerTP.items)
			{
				if (item != null && !item.IsAir && item.type == itemType)
				{
					count += item.stack;
				}
			}
			return count;
		}
	}
}
