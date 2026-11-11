using System;
using System.Collections.Generic;
using InnoVault.UIHandles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.ModLoader;

namespace PWing.Content.Vanity
{
    public class PWingVanityUI : UIHandle
    {
        private const int PanelWidth = 640;
        private const int PanelHeight = 380;
        private const int CardWidth = 112;
        private const int CardHeight = 248;
        private const int CardSpacing = 10;
        private const int CardLeftMargin = 20;
        private const int CardTopOffset = 92;
        private const int FrameWidth = 40;
        private const int FrameHeight = 56;
        private const float PreviewScale = 2.4f;

        private static readonly Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();

        private Rectangle panelRect;
        private readonly Rectangle[] cardRects = new Rectangle[5];

        public static PWingVanityUI Instance => UIHandleLoader.GetUIHandleOfType<PWingVanityUI>();

        public override bool CloseOnEscape => true;

        public static void UnloadStaticData()
        {
            textureCache.Clear();
        }

        public override void Update()
        {
            if (Main.dedServ)
            {
                return;
            }

            if (!IsOpen && OpenProgress.Current <= 0.001f)
            {
                return;
            }

            Size = new Vector2(PanelWidth, PanelHeight);
            DrawPosition = new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f);
            Vector2 topLeft = DrawPosition - Size / 2f;
            panelRect = new Rectangle((int)topLeft.X, (int)topLeft.Y, PanelWidth, PanelHeight);
            UIHitBox = panelRect;

            IReadOnlyList<VanitySet> sets = VanitySets.All;
            for (int i = 0; i < cardRects.Length; i++)
            {
                if (i >= sets.Count)
                {
                    cardRects[i] = Rectangle.Empty;
                    continue;
                }

                int x = panelRect.X + CardLeftMargin + i * (CardWidth + CardSpacing);
                cardRects[i] = new Rectangle(x, panelRect.Y + CardTopOffset, CardWidth, CardHeight);
            }

            hoverInMainPage = UIHitBox.Contains(MouseHitBox);
            if (hoverInMainPage)
            {
                Main.LocalPlayer.mouseInterface = true;
            }

            if (!IsOpen)
            {
                return;
            }

            if (keyLeftPressState == KeyPressState.Pressed)
            {
                for (int i = 0; i < cardRects.Length; i++)
                {
                    if (cardRects[i] != Rectangle.Empty && cardRects[i].Contains(MouseHitBox))
                    {
                        ClaimSet(sets[i]);
                        return;
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            float alpha = OpenProgress.Current;
            if (alpha <= 0.01f)
            {
                return;
            }

            DrawPanel(spriteBatch, alpha);

            IReadOnlyList<VanitySet> sets = VanitySets.All;
            for (int i = 0; i < sets.Count && i < cardRects.Length; i++)
            {
                bool hovered = IsOpen && cardRects[i].Contains(MouseHitBox);
                DrawCard(spriteBatch, sets[i], cardRects[i], hovered, alpha);
            }

            string title = Language.GetTextValue("Mods.PWing.UI.VanityWardrobe.Title");
            string subtitle = Language.GetTextValue("Mods.PWing.UI.VanityWardrobe.Subtitle");
            DrawCenteredText(spriteBatch, title, new Vector2(panelRect.Center.X, panelRect.Y + 26f), new Color(255, 228, 180), alpha, 1.05f);
            DrawCenteredText(spriteBatch, subtitle, new Vector2(panelRect.Center.X, panelRect.Y + 52f), new Color(214, 172, 212), alpha, 0.9f);

            string claimHint = Language.GetTextValue("Mods.PWing.UI.VanityWardrobe.ClaimHint");
            string closeHint = Language.GetTextValue("Mods.PWing.UI.VanityWardrobe.CloseHint");
            DrawCenteredText(spriteBatch, claimHint, new Vector2(panelRect.Center.X, panelRect.Bottom - 30f), new Color(255, 214, 140), alpha, 0.8f);
            DrawCenteredText(spriteBatch, closeHint, new Vector2(panelRect.Center.X, panelRect.Bottom - 12f), new Color(198, 160, 200), alpha, 0.75f);
        }

        private void DrawPanel(SpriteBatch spriteBatch, float alpha)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Color pink = new Color(238, 120, 172);
            Color gold = new Color(242, 202, 112);

            Rectangle shadow = panelRect;
            shadow.Offset(6, 8);
            spriteBatch.Draw(pixel, shadow, Color.Black * (alpha * 0.45f));

            spriteBatch.Draw(pixel, panelRect, new Color(20, 14, 24) * (alpha * 0.96f));

            Rectangle inner = panelRect;
            inner.Inflate(-4, -4);
            spriteBatch.Draw(pixel, inner, new Color(40, 27, 44) * (alpha * 0.9f));

            DrawBorder(spriteBatch, pixel, panelRect, gold * (alpha * 0.85f), 3);

            Rectangle innerGlow = panelRect;
            innerGlow.Inflate(-8, -8);
            DrawBorder(spriteBatch, pixel, innerGlow, pink * (alpha * 0.3f), 1);

            Rectangle divider = new Rectangle(panelRect.X + 24, panelRect.Y + 70, panelRect.Width - 48, 2);
            spriteBatch.Draw(pixel, divider, new Color(238, 120, 172) * (alpha * 0.55f));
        }

        private void DrawCard(SpriteBatch spriteBatch, VanitySet set, Rectangle card, bool hovered, float alpha)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            spriteBatch.Draw(pixel, card, new Color(28, 20, 32) * (alpha * 0.94f));

            Rectangle inner = card;
            inner.Inflate(-3, -3);
            spriteBatch.Draw(pixel, inner, new Color(42, 29, 48) * (alpha * 0.85f));

            Color borderColor = hovered
                ? Color.Lerp(new Color(240, 120, 175), new Color(255, 214, 120), 0.5f)
                : new Color(130, 95, 140);
            DrawBorder(spriteBatch, pixel, card, borderColor * (alpha * (hovered ? 1f : 0.75f)), hovered ? 3 : 2);

            if (hovered)
            {
                Rectangle glow = card;
                glow.Inflate(3, 3);
                DrawBorder(spriteBatch, pixel, glow, new Color(255, 200, 120) * (alpha * 0.35f), 2);
            }

            DrawPreview(spriteBatch, set, card, alpha);

            string name = Language.GetTextValue("Mods.PWing.UI.VanityWardrobe.SetNames." + set.NameKey);
            DrawCenteredText(spriteBatch, name, new Vector2(card.Center.X, card.Y + 168f), new Color(255, 236, 212), alpha, 0.8f);

            float iconY = card.Bottom - 26f;
            DrawItemIcon(spriteBatch, set.HeadType, new Vector2(card.Center.X + 32f, iconY), alpha);
            DrawItemIcon(spriteBatch, set.BodyType, new Vector2(card.Center.X, iconY), alpha);
            DrawItemIcon(spriteBatch, set.LegsType, new Vector2(card.Center.X - 32f, iconY), alpha);
        }

        private static void DrawPreview(SpriteBatch spriteBatch, VanitySet set, Rectangle card, float alpha)
        {
            Vector2 center = new Vector2(card.Center.X, card.Y + 10f + FrameHeight * PreviewScale / 2f);
            Vector2 origin = new Vector2(FrameWidth / 2f, FrameHeight / 2f);

            // 腿 → 身 → 头 依次叠加，只取条带首帧
            DrawFrame(spriteBatch, GetTexture(set.LegsTexture), center, origin, alpha);
            DrawFrame(spriteBatch, GetTexture(set.BodyTexture), center, origin, alpha);
            DrawFrame(spriteBatch, GetTexture(set.HeadTexture), center, origin, alpha);
        }

        private static void DrawFrame(SpriteBatch spriteBatch, Texture2D texture, Vector2 center, Vector2 origin, float alpha)
        {
            if (texture == null)
            {
                return;
            }

            Rectangle frame = new Rectangle(0, 0, FrameWidth, FrameHeight);
            spriteBatch.Draw(texture, center, frame, Color.White * alpha, 0f, origin, PreviewScale, SpriteEffects.None, 0f);
        }

        private static void DrawItemIcon(SpriteBatch spriteBatch, int itemType, Vector2 center, float alpha)
        {
            if (itemType <= 0 || itemType >= TextureAssets.Item.Length)
            {
                return;
            }

            Texture2D texture = TextureAssets.Item[itemType].Value;
            if (texture == null)
            {
                return;
            }

            spriteBatch.Draw(texture, center, null, Color.White * alpha, 0f, texture.Size() * 0.5f, 0.8f, SpriteEffects.None, 0f);
        }

        private static void DrawBorder(SpriteBatch spriteBatch, Texture2D pixel, Rectangle rect, Color color, int thickness)
        {
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            spriteBatch.Draw(pixel, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
        }

        private static void DrawCenteredText(SpriteBatch spriteBatch, string text, Vector2 center, Color color, float alpha, float scale = 1f)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            DynamicSpriteFont font = FontAssets.MouseText.Value;
            Vector2 size = font.MeasureString(text) * scale;
            Vector2 position = center - size / 2f;
            spriteBatch.DrawString(font, text, position + new Vector2(1f, 1f), Color.Black * (alpha * 0.8f), 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            spriteBatch.DrawString(font, text, position, color * alpha, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        private static Texture2D GetTexture(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            if (textureCache.TryGetValue(path, out Texture2D cached))
            {
                return cached;
            }

            Texture2D texture;
            try
            {
                texture = ModContent.Request<Texture2D>(path).Value;
            }
            catch (Exception)
            {
                texture = null;
            }

            textureCache[path] = texture;
            return texture;
        }

        private void ClaimSet(VanitySet set)
        {
            Player player = Main.LocalPlayer;
            if (player.whoAmI != Main.myPlayer)
            {
                return;
            }

            // 确认衣橱仍在物品栏中，避免打开 UI 后丢弃衣橱仍能免费领取
            int wardrobeType = ModContent.ItemType<VanityWardrobe>();
            int wardrobeIndex = -1;
            for (int i = 0; i < player.inventory.Length; i++)
            {
                Item item = player.inventory[i];
                if (item != null && item.active && item.type == wardrobeType)
                {
                    wardrobeIndex = i;
                    break;
                }
            }

            if (wardrobeIndex < 0)
            {
                Close();
                return;
            }

            player.QuickSpawnItem(player.GetSource_FromThis(), set.HeadType);
            player.QuickSpawnItem(player.GetSource_FromThis(), set.BodyType);
            player.QuickSpawnItem(player.GetSource_FromThis(), set.LegsType);

            Item wardrobe = player.inventory[wardrobeIndex];
            wardrobe.stack--;
            if (wardrobe.stack <= 0)
            {
                wardrobe.TurnToAir();
            }

            Close();
        }
    }
}
