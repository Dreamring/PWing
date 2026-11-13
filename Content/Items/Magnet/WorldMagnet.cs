using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static PWing.PWing;

namespace PWing.Content.Items.Magnet
{
	public class WorldMagnet : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearchCost(1);
		}

		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.useTurn = true;
			Item.autoReuse = false;
			Item.noMelee = true;
			Item.rare = ItemRarityID.Purple;
			Item.value = Item.sellPrice(0, 10, 0, 0);
			Item.UseSound = SoundID.Item44;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1)
				.AddRecipeGroup("IronBar", 20)
				.AddTile(TileID.HeavyWorkBench)
				.Register();
		}

		public override bool? UseItem(Player player)
		{
			if (player.whoAmI != Main.myPlayer)
			{
				return null;
			}
			SpawnAttractEffects(player);
			//掉落物位置由服务器统一修改并同步，避免联机时各客户端表现不一致
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				ModPacket packet = Mod.GetPacket();
				packet.Write((byte)PWingMessageType.WorldMagnet);
				packet.Send();
			}
			else
			{
				AttractAllItems(player);
			}
			return true;
		}

		//本地施法特效：一圈向内收缩的星尘，模拟磁场牵引
		private static void SpawnAttractEffects(Player player)
		{
			for (int i = 0; i < 40; i++)
			{
				Vector2 edge = Main.rand.NextVector2CircularEdge(1f, 1f);
				Vector2 spawnPos = player.Center + edge * Main.rand.NextFloat(120f, 280f);
				Dust dust = Dust.NewDustPerfect(spawnPos, DustID.YellowStarDust, (player.Center - spawnPos) * 0.07f, 0, default, 1.2f);
				dust.noGravity = true;
			}
		}

		public static void AttractAllItems(Player player)
		{
			if (player == null || !player.active)
			{
				return;
			}
			Vector2 center = player.Center;
			for (int i = 0; i < Main.maxItems; i++)
			{
				Item item = Main.item[i];
				if (!item.active || item.stack <= 0)
				{
					continue;
				}
				//先放到角色四周的圆环上，再给一个指向角色的高速速度，让掉落物飞向角色并被拾取
				Vector2 edge = Main.rand.NextVector2CircularEdge(1f, 1f);
				Vector2 spawnPos = center + edge * Main.rand.NextFloat(80f, 160f);
				spawnPos.X = MathHelper.Clamp(spawnPos.X, 16f, (Main.maxTilesX - 1) * 16f);
				spawnPos.Y = MathHelper.Clamp(spawnPos.Y, 16f, (Main.maxTilesY - 1) * 16f);
				item.position = spawnPos - item.Size * 0.5f;
				item.velocity = (center - spawnPos).SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(12f, 16f);
				if (Main.netMode != NetmodeID.SinglePlayer)
				{
					NetMessage.SendData(MessageID.SyncItem, -1, -1, null, i, 1f);
				}
			}
		}
	}
}
