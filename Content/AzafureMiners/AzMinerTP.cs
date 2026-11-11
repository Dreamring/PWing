using System;
using System.Collections.Generic;
using System.IO;
using InnoVault;
using InnoVault.TileProcessors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace PWing.Content.AzafureMiners
{
	public class PWingAzMinerTP : TileProcessor
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

		public static void UnloadStaticData()
		{
			ItemIsOre.Clear();
		}

		// 相邻四方向偏移，避免在洪水填充内层循环重复分配数组
		private static readonly int[] NeighborDx = { 0, 0, -1, 1 };
		private static readonly int[] NeighborDy = { -1, 1, 0, 0 };
		private readonly int[] activeFilterTypes = new int[FiltersCount];
		private int filterTypeCount;
		private readonly HashSet<Point16> mineVisited = new HashSet<Point16>();
		private readonly Queue<Point16> mineQueue = new Queue<Point16>();

		public override int TargetTileID => ModContent.TileType<PWingAzafureMinerTile>();

		public static void SetUpList()
		{
			try
			{
				ItemIsOre.Clear();
				List<int> oreTileIDs = new List<int>();
				// 使用 Tile 特性检测矿石：固体 + spelunker 高亮 + 矿石探测器优先级 > 0
				for (int i = 0; i < TileLoader.TileCount; i++)
				{
					if (Main.tileSolid[i] && Main.tileSpelunker[i] && Main.tileOreFinderPriority[i] > 0)
					{
						oreTileIDs.Add(i);
					}
				}
				// 白名单物品由 PWingAzMinerUISlot.IsOre 在运行时读取配置判断，这里只登记基础矿石表
				for (int j = 0; j < ItemLoader.ItemCount; j++)
				{
					Item item = new(j, 1, 0);
					if (item.type != ItemID.None)
					{
						bool isGem = IsGemItem(j);
						bool createsOre = oreTileIDs.Contains(item.createTile);
						ItemIsOre[j] = createsOre || isGem;
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
					filters.Add(LoadStoredItem(isave, "f" + i));
				}
				for (int j = 0; j < ItemsCount; j++)
				{
					items.Add(LoadStoredItem(isave, "i" + j));
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

		//读取单个物品存档；条目缺失时补空物品，避免旧存档或损坏存档导致世界加载异常
		private static Item LoadStoredItem(TagCompound tag, string key)
		{
			TagCompound itemTag = tag?.Get<TagCompound>(key);
			return itemTag == null ? new Item() : ItemIO.Load(itemTag);
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
			IsWork = false;
			filterTypeCount = 0;
			foreach (Item item in filters)
			{
				if (item.type != ItemID.None)
				{
					if (filterTypeCount < activeFilterTypes.Length)
					{
						activeFilterTypes[filterTypeCount] = item.type;
					}
					filterTypeCount++;
					IsWork = true;
				}
			}
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				return;
			}
			bool didMine = false;
			if (filterTypeCount > 0)
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
					
					if (!IsFilterType(dropItemType))
					{
						continue;
					}
					// 使用洪水填充算法找到所有相连的相同矿石
					mineVisited.Clear();
					mineQueue.Clear();
					Point16 startPos = new Point16(x, y);
					mineQueue.Enqueue(startPos);
					mineVisited.Add(startPos);
					int maxCount = 1000;
					int minedCount = 0;
					
					while (mineQueue.Count > 0 && minedCount < maxCount)
					{
						Point16 current = mineQueue.Dequeue();
						int cx = current.X;
						int cy = current.Y;
						
						if (MineOreWithType(cx, cy, dropItemType))
						{
							didMine = true;
							minedCount++;
						}
						
						// 检查相邻的4个方向
						for (int d = 0; d < 4; d++)
						{
							int nx = cx + NeighborDx[d];
							int ny = cy + NeighborDy[d];
							Point16 nextPos = new Point16(nx, ny);
							
							if (!WorldGen.InWorld(nx, ny) || mineVisited.Contains(nextPos))
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
								mineVisited.Add(nextPos);
								mineQueue.Enqueue(nextPos);
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

		private bool IsFilterType(int itemType)
		{
			for (int i = 0; i < filterTypeCount && i < activeFilterTypes.Length; i++)
			{
				if (activeFilterTypes[i] == itemType)
				{
					return true;
				}
			}
			return false;
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
			return MineOreWithType(x, y, itemType);
		}

		//已在上层算好掉落物类型时直接复用，避免同一格重复调用 GetTileDrop
		private bool MineOreWithType(int x, int y, int itemType)
		{
			if (!Main.tile[x, y].HasTile || !IsFilterType(itemType))
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
			// 主机开服（host & play）时 Main.dedServ 为 false 但仍需把方块变化同步给远程客户端
			if (Main.netMode == NetmodeID.Server)
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
