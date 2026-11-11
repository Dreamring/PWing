using PWing.Content.Vanity;
using Terraria.ID;
using Terraria.ModLoader;

namespace PWing.Content.Vanity.Items
{
    [AutoloadEquip(EquipType.Head)]
    public class RamHead : VanityArmorItem
    {
        public override void SetStaticDefaults()
        {
            // 佩戴头饰时仍绘制完整发型，避免头发被头饰隐藏
            int slot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head);
            if (slot >= 0)
            {
                ArmorIDs.Head.Sets.DrawFullHair[slot] = true;
            }
        }
    }
}
