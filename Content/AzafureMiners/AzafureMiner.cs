using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace PWing.Content.AzafureMiners
{
	public class PWingAzafureMiner : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 92;
			Item.height = 50;
			Item.maxStack = 9999;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 6;
			Item.useTime = 6;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.consumable = true;
			Item.createTile = ModContent.TileType<PWingAzafureMinerTile>();
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(0, 8, 0, 0);
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddRecipeGroup("IronBar", 10)
				.AddTile(TileID.HeavyWorkBench)
				.Register();
		}
	}
}
