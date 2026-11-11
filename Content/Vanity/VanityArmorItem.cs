using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace PWing.Content.Vanity
{
    /// <summary>
    /// 时装物品基类。装备贴图由 [AutoloadEquip] 按
    /// "&lt;物品贴图路径&gt;_&lt;EquipType&gt;" 的命名规则自动注册，
    /// 物品图标使用同目录下与类名同名的 PNG。
    /// </summary>
    public abstract class VanityArmorItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.vanity = true;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(silver: 20);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Silk, 5)
                .AddTile(TileID.Loom)
                .Register();
        }
    }
}
