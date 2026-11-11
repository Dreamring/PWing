using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace PWing.Content.Vanity
{
    public class VanityWardrobe : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.Dresser}";

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 1;
            Item.consumable = false;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 25;
            Item.useTime = 25;
            Item.UseSound = SoundID.Item4;
            Item.rare = ItemRarityID.Purple;
            Item.value = Item.sellPrice(gold: 1);
        }

        public override bool? UseItem(Player player)
        {
            if (Main.dedServ || player.whoAmI != Main.myPlayer)
            {
                return null;
            }

            PWingVanityUI ui = PWingVanityUI.Instance;
            if (ui != null && !ui.IsOpen)
            {
                ui.Open();
            }

            return true;
        }
    }
}
