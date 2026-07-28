using Microsoft.Xna.Framework;
using PWing.Content.Projectiles;
using PWing.Content.Buffs;
using PWing.Common;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace PWing.Content.Items.Summon
{
    public class PWingTesseract : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
            this.SetResearchCost(1);
        }

        public override void SetDefaults()
        {
            Item.damage = 1000000;
            Item.knockBack = 5f;
            Item.width = 52;
            Item.height = 52;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.value = Item.sellPrice(1, 0, 0, 0);
            Item.rare = ItemRarityID.Purple;
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.DamageType = DamageClass.Summon;
            Item.buffType = ModContent.BuffType<PWingTesseractBuff>();
            Item.shoot = ModContent.ProjectileType<PWingTesseractServant>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            player.SpawnMinionOnCursor(source, player.whoAmI, type, Item.damage, knockback);
            return false;
        }

        public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
        {
            bool name = line.Name == "ItemName";
            if (name || line.Name == "Damage" || line.Name == "Favorite" || line.Name == "FavoriteDesc")
            {
                Color outer = ColorHelper.TesseractColor(0, 0.4f);
                outer *= 0.2f;
                Color inner = Color.Black;
                TextSnippet[] snippets = ChatManager.ParseMessage(line.Text, inner).ToArray();
                ChatManager.ConvertNormalSnippets(snippets);
                for (int i = 0; i < 16; ++i)
                {
                    float radians = MathHelper.ToRadians(i / 16f * 360 + Main.GameUpdateCount);
                    var outer2 = outer * 0.4f;
                    Vector2 circular = new Vector2(6.5f, 0).RotatedBy(radians);
                    ChatManager.DrawColorCodedStringShadow(Main.spriteBatch, line.Font, line.Text, new Vector2(line.X, line.Y) + circular, outer2, line.Rotation, line.Origin, line.BaseScale, line.MaxWidth, line.Spread);
                }
                for (int i = 0; i < 16; ++i)
                {
                    float radians = MathHelper.ToRadians(i / 16f * 360 + Main.GameUpdateCount);
                    var outer2 = ColorHelper.TesseractColor(radians, 0.8f) * 0.09f;
                    outer2.A = 0;
                    Vector2 circular = new Vector2(2.5f, 0).RotatedBy(radians);
                    ChatManager.DrawColorCodedStringShadow(Main.spriteBatch, line.Font, line.Text, new Vector2(line.X, line.Y) + circular, outer2, line.Rotation, line.Origin, line.BaseScale, line.MaxWidth, 0);
                }
                ChatManager.DrawColorCodedStringShadow(Main.spriteBatch, line.Font, line.Text, new Vector2(line.X, line.Y), outer, line.Rotation, line.Origin, line.BaseScale, line.MaxWidth, line.Spread);
                ChatManager.DrawColorCodedString(Main.spriteBatch, line.Font, snippets, new Vector2(line.X, line.Y), inner, line.Rotation, line.Origin, line.BaseScale, out _, line.MaxWidth);
                return false;
            }
            return true;
        }
    }
}
