using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InnoVault;
using InnoVault.TileProcessors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PWing.Common.Configs;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.IO;

namespace PWing.Content.AzafureMiners
{
	public class AzMinerTP : TileProcessor
	{
		public List<Item> filters;

		public List<Item> items;

		public static int FiltersCount = 6;

		public static int ItemsCount = 35;

		public bool IsWork;

		public Vector2 OffsetPos;

		private Vector2 currentOffset = Vector2.Zero;

		private Vector2 targetOffset = Vector2.Zero;

		private Vector2 targetOffset2 = Vector2.Zero;

		public static readonly Dictionary<int, bool> ItemIsOre = new Dictionary<int, bool>();

		public static bool init = true;

		public bool SmartCursorHover = false;

		public override int TargetTileID => ModContent.TileType<AzafureMinerTile>();

		public static void SetUpList()
		{
			try
			{
				List<int> oreTileIDs = new List<int>();
				// 使用 Tile 特性检测矿石：固体 + spelunker 高亮 + 矿石探测器优先级 > 0
				for (int i = 0; i < TileLoader.TileCount; i++)
				{
					if (Main.tileSolid[i] && Main.tileSpelunker[i] && Main.tileOreFinderPriority[i] > 0)
					{
						oreTileIDs.Add(i);
					}
				}
				// 添加白名单中的物品
				if (PWingConfig.Instance != null && PWingConfig.Instance.azafureMinerWhitelistEnabled)
				{
					foreach (ItemDefinition itemDef in PWingConfig.Instance.azafureMinerWhitelistItems)
					{
						if (itemDef.IsUnloaded)
						{
							continue;
						}
						int itemID = itemDef.Type;
						if (itemID > 0 && itemID < ItemLoader.ItemCount && !ItemIsOre.ContainsKey(itemID))
						{
							ItemIsOre.Add(itemID, true);
						}
					}
				}
				for (int j = 0; j < ItemLoader.ItemCount; j++)
				{
					Item item = new(j, 1, 0);
					if (item.type != ItemID.None)
					{
						bool isGem = IsGemItem(j);
						bool createsOre = oreTileIDs.Contains(item.createTile);
						ItemIsOre.Add(j, createsOre || isGem);
					}
				}
			}
			catch (Exception ex)
			{
				PWing.Instance.Logger.Error("AzMinerTP.SetStaticDefaults: An Error Has Occurred " + ex.Message);
			}
		}

		public override void SetProperty()
		{
			IsWork = true;
			filters = new List<Item>();
			items = new List<Item>();
			for (int i = 0; i < FiltersCount; i++)
			{
				filters.Add(new Item());
			}
			for (int j = 0; j < ItemsCount; j++)
			{
				items.Add(new Item());
			}
		}

		public override void LoadData(TagCompound tag)
		{
			filters = new List<Item>();
			items = new List<Item>();
			if (tag.ContainsKey("items"))
			{
				TagCompound isave = tag.Get<TagCompound>("items");
				for (int i = 0; i < FiltersCount; i++)
				{
					filters.Add(ItemIO.Load(isave.Get<TagCompound>("f" + i)));
				}
				for (int j = 0; j < ItemsCount; j++)
				{
					items.Add(ItemIO.Load(isave.Get<TagCompound>("i" + j)));
				}
			}
			else
			{
				for (int k = 0; k < FiltersCount; k++)
				{
					filters.Add(new Item());
				}
				for (int l = 0; l < ItemsCount; l++)
				{
					items.Add(new Item());
				}
			}
		}

		public override void SaveData(TagCompound tag)
		{
			TagCompound itemSaves = new TagCompound();
			int c = 0;
			foreach (Item item in filters)
			{
				itemSaves.Add("f" + c, ItemIO.Save(item));
				c++;
			}
			c = 0;
			foreach (Item item2 in items)
			{
				itemSaves.Add("i" + c, ItemIO.Save(item2));
				c++;
			}
			tag.Add("items", itemSaves);
		}

		public override void OnKill()
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				return;
			}
			foreach (Item item in filters)
			{
				if (!item.IsAir)
				{
					VaultUtils.SpwanItem(VaultUtils.FromObjectGetParent(this), HitBox, item.Clone(), true);
				}
			}
			foreach (Item item2 in items)
			{
				if (!item2.IsAir)
				{
					VaultUtils.SpwanItem(VaultUtils.FromObjectGetParent(this), HitBox, item2.Clone(), true);
				}
			}
		}

		public override void Update()
		{
			if (init)
			{
				init = false;
				SetUpList();
			}
			if (IsWork)
			{
				targetOffset = targetOffset2 + new Vector2(Utils.NextFloat(Main.rand, -8f, 8f), Utils.NextFloat(Main.rand, -4f, 10f));
				currentOffset = Vector2.Lerp(currentOffset, targetOffset, 0.1f);
			}
			else
			{
				targetOffset2 = Vector2.Zero;
				currentOffset = Vector2.Lerp(currentOffset, Vector2.Zero, 0.1f);
			}
			targetOffset2 *= 0.6f;
			OffsetPos = new Vector2((int)currentOffset.X, (int)currentOffset.Y);
			List<int> types = new List<int>();
			IsWork = false;
			foreach (Item item in filters)
			{
				if (item.type != ItemID.None)
				{
					types.Add(item.type);
					IsWork = true;
				}
			}
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				return;
			}
			bool didMine = false;
			if (types.Count > 0)
			{
				for (int searchCount = 0; searchCount < 1000; searchCount++)
				{
					int x = Main.rand.Next(Main.maxTilesX);
					int y = Main.rand.Next(Main.maxTilesY);
					
					Tile tile = Main.tile[x, y];
					if (!tile.HasTile)
					{
						continue;
					}
					
					int dropItemType = VaultUtils.GetTileDrop(tile, x, y);
					if (dropItemType <= 0)
					{
						continue;
					}
					
					if (IsGemTile(tile.TileType, out int gemDropID))
					{
						dropItemType = gemDropID;
					}
					
					if (types.Contains(dropItemType))
					{
						// 使用洪水填充算法找到所有相连的相同矿石
						HashSet<Point16> visited = new HashSet<Point16>();
						Queue<Point16> queue = new Queue<Point16>();
						queue.Enqueue(new Point16(x, y));
						visited.Add(new Point16(x, y));
						int maxCount = 1000;
						int minedCount = 0;
						
						while (queue.Count > 0 && minedCount < maxCount)
						{
							Point16 current = queue.Dequeue();
							int cx = current.X;
							int cy = current.Y;
							
							if (MineOre(cx, cy))
							{
								didMine = true;
								minedCount++;
							}
							
							// 检查相邻的4个方向
							int[] dx = { 0, 0, -1, 1 };
							int[] dy = { -1, 1, 0, 0 };
							for (int d = 0; d < 4; d++)
							{
								int nx = cx + dx[d];
								int ny = cy + dy[d];
								Point16 nextPos = new Point16(nx, ny);
								
								if (!WorldGen.InWorld(nx, ny) || visited.Contains(nextPos))
								{
									continue;
								}
								
								Tile nearbyTile = Main.tile[nx, ny];
								if (!nearbyTile.HasTile)
								{
									continue;
								}
								
								int nearbyDrop = VaultUtils.GetTileDrop(nearbyTile, nx, ny);
								if (nearbyDrop <= 0)
								{
									continue;
								}
								
								if (IsGemTile(nearbyTile.TileType, out int nearbyGemDrop))
								{
									nearbyDrop = nearbyGemDrop;
								}
								
								if (nearbyDrop == dropItemType)
								{
									visited.Add(nextPos);
									queue.Enqueue(nextPos);
								}
							}
						}
					}
				}
			}
			if (didMine)
			{
				SendData();
			}
		}

		public bool MineOre(int x, int y)
		{
			Tile tile = Main.tile[x, y];
			int itemType = VaultUtils.GetTileDrop(tile, x, y);
			if (itemType <= 0)
			{
				return false;
			}
			if (IsGemTile(tile.TileType, out int dropID))
			{
				itemType = dropID;
			}
			if (!filters.Any(f => f.type == itemType))
			{
				return false;
			}
			foreach (Item slot in items)
			{
				if (slot.type != itemType || slot.stack >= slot.maxStack)
				{
					continue;
				}
				slot.stack++;
				PerformMineEffects(x, y);
				return true;
			}
			foreach (Item slot2 in items)
			{
				if (!slot2.IsAir)
				{
					continue;
				}
				slot2.SetDefaults(itemType);
				PerformMineEffects(x, y);
				return true;
			}
			return false;
		}

		private void PerformMineEffects(int x, int y)
		{
			Tile val = Main.tile[x, y];
			val.ClearTile();
			if (Main.dedServ)
			{
				NetMessage.SendTileSquare(-1, x, y);
			}
			if (Utils.NextBool(Main.rand, 12))
			{
				Dust.NewDust(CenterInWorld + new Vector2(Utils.NextFloat(Main.rand, -24f, 24f), 0f), 8, 8, DustID.YellowStarDust, Utils.NextFloat(Main.rand, -6f, 6f), Utils.NextFloat(Main.rand, -2f, -6f), 0, Color.Lerp(new Color(255, 255, 0), Color.White, (float)Main.rand.NextDouble()), Utils.NextFloat(Main.rand, 0.8f, 1.4f));
				SoundStyle tink = SoundID.Tink;
				tink.PitchRange = (-0.1f, 2f);
				SoundEngine.PlaySound(tink, CenterInWorld);
				if (targetOffset2.Length() <= 1f)
				{
					targetOffset2 += new Vector2(0f, -116f);
				}
			}
		}

		public override void SendData(ModPacket data)
		{
			data.Write(IsWork);
			for (int i = 0; i < FiltersCount; i++)
			{
				Item item = filters[i];
				data.Write(item.type);
				data.Write7BitEncodedInt(item.netID);
				data.Write7BitEncodedInt(item.stack);
			}
			for (int j = 0; j < ItemsCount; j++)
			{
				Item item2 = items[j];
				data.Write(item2.type);
				data.Write7BitEncodedInt(item2.netID);
				data.Write7BitEncodedInt(item2.stack);
			}
		}

		public override void ReceiveData(BinaryReader reader, int whoAmI)
		{
			IsWork = reader.ReadBoolean();
			for (int i = 0; i < FiltersCount; i++)
			{
				Item item = new Item(reader.ReadInt32(), 1, 0);
				item.netDefaults(reader.Read7BitEncodedInt());
				item.stack = reader.Read7BitEncodedInt();
				filters[i] = item;
			}
			for (int j = 0; j < ItemsCount; j++)
			{
				Item item2 = new Item(reader.ReadInt32(), 1, 0);
				item2.netDefaults(reader.Read7BitEncodedInt());
				item2.stack = reader.Read7BitEncodedInt();
				items[j] = item2;
			}
		}

		private static bool IsGemItem(int itemID)
		{
			return itemID is >= ItemID.Amethyst and <= ItemID.Diamond ||
				   itemID == ItemID.Emerald || itemID == ItemID.Ruby ||
				   itemID == ItemID.Topaz || itemID == ItemID.Sapphire;
		}

		private bool IsGemTile(int tileID, out int itemID)
		{
			itemID = tileID switch
			{
				TileID.Amethyst => ItemID.Amethyst,
				TileID.Topaz => ItemID.Topaz,
				TileID.Sapphire => ItemID.Sapphire,
				TileID.Emerald => ItemID.Emerald,
				TileID.Ruby => ItemID.Ruby,
				TileID.Diamond => ItemID.Diamond,
				_ => 0
			};
			return itemID != 0;
		}
	}
}
