using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using PWing.Content.Players;
using PWing.Content.Items.Summon;
using PWing;

namespace PWing.Content.FakePlayer
{
    public static class PlayerInventorySlotsManager
    {
        public static Color SavedInventoryColor = Color.White;
        public static bool FakeBorderDrawCycle = false;
        public static int RealBorderDrawCycleSlot = -1;
        public static Texture2D planetariumTextures;

        public static Texture2D tesseractBox => PWing.Instance.Assets.Request<Texture2D>("Assets/FakePlayer/TesseractUI/Box").Value;
        public static Texture2D tesseractBoxA => PWing.Instance.Assets.Request<Texture2D>("Assets/FakePlayer/TesseractUI/BoxAlt").Value;
        public static Texture2D tesseractBoxF => PWing.Instance.Assets.Request<Texture2D>("Assets/FakePlayer/TesseractUI/BoxFavorite").Value;
        public static Texture2D tesseractBoxFA => PWing.Instance.Assets.Request<Texture2D>("Assets/FakePlayer/TesseractUI/BoxFavoriteAlt").Value;
        public static Texture2D tesseractBoxG => PWing.Instance.Assets.Request<Texture2D>("Assets/FakePlayer/TesseractUI/BoxGray").Value;
        public static Texture2D tesseractBoxGA => PWing.Instance.Assets.Request<Texture2D>("Assets/FakePlayer/TesseractUI/BoxGrayAlt").Value;
        public static Texture2D tesseractBoxFG => PWing.Instance.Assets.Request<Texture2D>("Assets/FakePlayer/TesseractUI/BoxFavoriteGray").Value;
        public static Texture2D tesseractBoxFGA => PWing.Instance.Assets.Request<Texture2D>("Assets/FakePlayer/TesseractUI/BoxFavoriteGrayAlt").Value;
        public static Texture2D tesseractBoxGlow => PWing.Instance.Assets.Request<Texture2D>("Assets/FakePlayer/TesseractUI/BoxGlow").Value;
        public static Texture2D tesseractBoxGlowA => PWing.Instance.Assets.Request<Texture2D>("Assets/FakePlayer/TesseractUI/BoxGlowAlt").Value;
        public static Texture2D tesseractBoxFGlow => PWing.Instance.Assets.Request<Texture2D>("Assets/FakePlayer/TesseractUI/BoxGlowFavorite").Value;
        public static Texture2D tesseractBoxFGlowA => PWing.Instance.Assets.Request<Texture2D>("Assets/FakePlayer/TesseractUI/BoxGlowFavoriteAlt").Value;
        public static Texture2D tesseractBoxB => PWing.Instance.Assets.Request<Texture2D>("Assets/FakePlayer/TesseractUI/BoxB").Value;
        public static Texture2D tesseractBoxGB => PWing.Instance.Assets.Request<Texture2D>("Assets/FakePlayer/TesseractUI/BoxGrayB").Value;
        public static Texture2D tesseractBoxFGB => PWing.Instance.Assets.Request<Texture2D>("Assets/FakePlayer/TesseractUI/BoxFavoriteGrayB").Value;
        public static Texture2D tesseractBoxFB => PWing.Instance.Assets.Request<Texture2D>("Assets/FakePlayer/TesseractUI/BoxFavoriteB").Value;
        public static Texture2D tesseractBoxGlowB => PWing.Instance.Assets.Request<Texture2D>("Assets/FakePlayer/TesseractUI/BoxGlowB").Value;
        public static Texture2D tesseractBoxFGlowB => PWing.Instance.Assets.Request<Texture2D>("Assets/FakePlayer/TesseractUI/BoxGlowFavoriteB").Value;

        public static void Load()
        {
            planetariumTextures = PWing.Instance.Assets.Request<Texture2D>("Assets/FakePlayer/TesseractUI/PlanetarySymbols").Value;
        }

        public static bool DrawSubspaceSlot(Item item)
        {
            return false;
        }

        public static bool DrawTesseractSlot(Item item, int slot)
        {
            Player player = Main.LocalPlayer;
            if (player == null || !player.active)
            {
                return false;
            }
            PWingPlayer pwingPlayer = player.GetModPlayer<PWingPlayer>();
            if (player.inventory.Length <= slot || pwingPlayer.tesseractData.Length < 10)
            {
                return false;
            }
            bool correctSlot = slot >= 0 && SlotIsWithinTesseractSlotRange(player, slot);
            if (correctSlot)
            {
                return true;
            }
            bool correctItem = item.type == ModContent.ItemType<PWingTesseract>() && (player.inventory.Contains(item) || player.armor.Contains(item));
            return correctItem;
        }

        public static bool SlotIsWithinTesseractSlotRange(Player player, int slot)
        {
            PWingPlayer pwingPlayer = player.GetModPlayer<PWingPlayer>();
            int tesCount = pwingPlayer.tesseractPlayerCount;
            
            if (tesCount <= 0)
                return false;
            
            return slot >= 40 && slot < 40 + tesCount;
        }

        public static Color InventoryBoxStandard => new Color(210, 210, 210, 210);

        public static void PreDrawSlots(Item item, SpriteBatch spriteBatch, Vector2 position, Color drawColor)
        {
            try
            {
                Player player = Main.LocalPlayer;
                if (player == null || !player.active)
                {
                    return;
                }
                PWingPlayer pwingPlayer = player.GetModPlayer<PWingPlayer>();
                
                if (DrawTesseractSlot(item, RealBorderDrawCycleSlot))
                {
                    TesseractMinionData data = pwingPlayer.tesseractData[RealBorderDrawCycleSlot % 10];
                    bool foundItem = data.FoundValidItem;
                    bool correctItem = item.type == ModContent.ItemType<PWingTesseract>() && (player.inventory.Contains(item) || player.armor.Contains(item));
                    Color color = Color.White;
                    Rectangle frame2 = new Rectangle(0, 0, 40, 50);
                    Item dummyItem = new Item(ModContent.ItemType<PWingTesseract>());
                    dummyItem.width = 52;
                    dummyItem.height = 52;
                    ItemSlot.DrawItem_GetColorAndScale(dummyItem, Main.inventoryScale, ref color, 52f, ref frame2, out var itemLight, out var finalDrawScale);
                    Texture2D textureOfBox = tesseractBoxG;
                    if ((correctItem || foundItem) && !FakeBorderDrawCycle)
                    {
                        textureOfBox = (data.AltFunctionUse && !correctItem) ? tesseractBoxA : tesseractBox;
                        if (item.favorited)
                            textureOfBox = (data.AltFunctionUse && !correctItem) ? tesseractBoxFA : tesseractBoxF;
                    }
                    else
                    {
                        if (item.favorited)
                            textureOfBox = tesseractBoxFG;
                    }
                    spriteBatch.Draw(textureOfBox, position, null, InventoryBoxStandard, 0f, tesseractBox.Size() / 2, finalDrawScale, SpriteEffects.None, 0f);
                    if (foundItem && data.ChargeFrames < 0 && !correctItem)
                    {
                        Texture2D glowBox;
                        if (item.favorited)
                        {
                            glowBox = (data.AltFunctionUse && !correctItem) ? tesseractBoxFGlowA : tesseractBoxFGlow;
                        }
                        else
                        {
                            glowBox = (data.AltFunctionUse && !correctItem) ? tesseractBoxGlowA : tesseractBoxGlow;
                        }
                        Color alternateColor = Color.Lerp(InventoryBoxStandard, Color.Red, 0.75f);
                        spriteBatch.Draw(glowBox, position, null, alternateColor, 0f, tesseractBox.Size() / 2, finalDrawScale, SpriteEffects.None, 0f);
                    }
                    dummyItem.width = 26;
                    dummyItem.height = 32;
                    if (!correctItem || FakeBorderDrawCycle)
                    {
                        if (planetariumTextures != null)
                        {
                            Color alternateColor = Color.Lerp(InventoryBoxStandard, new Color(231, 95, 203), 0.75f);
                            ItemSlot.DrawItem_GetColorAndScale(dummyItem, Main.inventoryScale, ref color, 48f, ref frame2, out var itemLight2, out var finalDrawScale2);
                            int frameY = RealBorderDrawCycleSlot % 10;
                            int height = planetariumTextures.Height / 10;
                            Rectangle frame = new Rectangle(0, height * frameY, planetariumTextures.Width, height);
                            spriteBatch.Draw(planetariumTextures, position, frame, (foundItem ? alternateColor : InventoryBoxStandard) * 0.5f, 0f, new Vector2(planetariumTextures.Width, planetariumTextures.Height / 10) / 2f, finalDrawScale2 * 1f, SpriteEffects.None, 0f);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Main.NewText($"PWing PreDrawSlots error: {ex.Message}");
            }
        }
    }
}