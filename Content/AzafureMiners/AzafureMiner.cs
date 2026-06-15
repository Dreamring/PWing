using CalamityMod.Items;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace PWing.Content.AzafureMiners
{
	public class PWingAzafureMiner : ModItem
	{
		public override void SetDefaults()
		{
			((Entity)((ModItem)this).Item).width = 92;
			((Entity)((ModItem)this).Item).height = 50;
			((ModItem)this).Item.maxStack = 9999;
			((ModItem)this).Item.useTurn = true;
			((ModItem)this).Item.autoReuse = true;
			((ModItem)this).Item.useAnimation = 6;
			((ModItem)this).Item.useTime = 6;
			((ModItem)this).Item.useStyle = 1;
			((ModItem)this).Item.consumable = true;
			((ModItem)this).Item.createTile = ModContent.TileType<PWingAzafureMinerTile>();
			((ModItem)this).Item.rare = 3;
			((ModItem)this).Item.value = CalamityGlobalItem.RarityOrangeBuyPrice;
		}

		public override void AddRecipes()
		{
			((ModItem)this).CreateRecipe(1).AddRecipeGroup("IronBar", 10)
				.AddTile(TileID.HeavyWorkBench)
				.Register();
		}
	}
}
