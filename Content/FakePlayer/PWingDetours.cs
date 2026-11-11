using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using PWing.Content.FakePlayer;
using PWing.Content.Players;
using Terraria.UI;

namespace PWing.Content.FakePlayer
{
    public static class PWingDetours
    {
        public static void Initialize()
        {
            On_Main.DrawPlayers_AfterProjectiles += Main_DrawPlayers_AfterProjectiles;
            On_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += ItemSlot_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color;
            On_ItemSlot.DrawItemIcon += ItemSlot_DrawItemIcon;
            On_Projectile.Update += Projectile_Update;
            On_Main.DrawProj += Main_DrawProj;
            On_Main.DrawCachedProjs += Main_DrawCachedProjs;
        }

        private static void Main_DrawPlayers_AfterProjectiles(On_Main.orig_DrawPlayers_AfterProjectiles orig, Main self)
        {
            PreDrawPlayers();
            orig(self);
        }

        private static void PreDrawPlayers()
        {
            if (!Main.dedServ)
            {
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                PWingFakePlayerDrawing.DrawFakePlayers(PWingFakePlayerTypeID.Subspace, PWingDrawStateID.All);
                PWingFakePlayerDrawing.DrawFakePlayers(PWingFakePlayerTypeID.Tesseract, PWingDrawStateID.All);

                Main.spriteBatch.End();
            }
        }

        private static void ItemSlot_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color(On_ItemSlot.orig_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color orig, SpriteBatch spriteBatch, Item[] inv, int context, int slot, Vector2 position, Color lightColor = default(Color))
        {
            Player player = Main.LocalPlayer;
            if (Main.gameMenu || player == null || !player.active)
            {
                orig(spriteBatch, inv, context, slot, position, lightColor);
                return;
            }

            PWingPlayer pwingPlayer = player.GetModPlayer<PWingPlayer>();
            PlayerInventorySlotsManager.SavedInventoryColor = Main.inventoryBack;
            // 仅本次库存槽绘制期间允许 PreDrawSlots 生效，避免其他 UI（如采矿机）复用过期槽号
            PlayerInventorySlotsManager.PendingPreDrawSlot = slot;
            try
            {
                Item item = inv[slot];
                if (PlayerInventorySlotsManager.DrawSubspaceSlot(item) || PlayerInventorySlotsManager.DrawTesseractSlot(item, slot))
                {
                    Main.inventoryBack = Color.Transparent;
                }
                int tesCount = pwingPlayer.tesseractPlayerCount;

                if (tesCount > 0)
                {
                    if (PlayerInventorySlotsManager.SlotIsWithinTesseractSlotRange(player, slot))
                    {
                        if (item.type <= ItemID.None || item.stack <= 0)
                        {
                            int saveItemType = item.type;
                            int saveItemStack = item.stack;
                            item.type = ModContent.ItemType<Content.Items.Summon.PWingTesseract>();
                            item.stack = 1;
                            PlayerInventorySlotsManager.FakeBorderDrawCycle = true;
                            orig(spriteBatch, inv, context, slot, position, lightColor);
                            PlayerInventorySlotsManager.FakeBorderDrawCycle = false;
                            item.type = saveItemType;
                            item.stack = saveItemStack;
                            return;
                        }
                    }
                }

                int subSlot = 49;
                if (tesCount >= 10)
                    subSlot = 39;

                if (slot == subSlot)
                {
                    if (pwingPlayer.servantActive && !pwingPlayer.servantIsVanity)
                    {
                        if (item.type <= ItemID.None || item.stack <= 0)
                        {
                            int saveItemType = item.type;
                            int saveItemStack = item.stack;
                            item.type = ModContent.ItemType<Content.Items.Summon.PWingTesseract>();
                            item.stack = 1;
                            PlayerInventorySlotsManager.FakeBorderDrawCycle = true;
                            orig(spriteBatch, inv, context, slot, position, lightColor);
                            PlayerInventorySlotsManager.FakeBorderDrawCycle = false;
                            item.type = saveItemType;
                            item.stack = saveItemStack;
                            return;
                        }
                    }
                }

                orig(spriteBatch, inv, context, slot, position, lightColor);
            }
            finally
            {
                PlayerInventorySlotsManager.PendingPreDrawSlot = -1;
            }
        }

        private static float ItemSlot_DrawItemIcon(On_ItemSlot.orig_DrawItemIcon orig, Item item, int context, SpriteBatch spriteBatch, Vector2 screenPositionForItemCenter, float scale, float sizeLimit, Color environmentColor)
        {
            if (!Main.gameMenu)
            {
                int slot = PlayerInventorySlotsManager.PendingPreDrawSlot;
                if (item != null && slot >= 0)
                    PlayerInventorySlotsManager.PreDrawSlots(item, spriteBatch, screenPositionForItemCenter, Color.White, slot);
                Main.inventoryBack = PlayerInventorySlotsManager.SavedInventoryColor;
            }
            if (!PlayerInventorySlotsManager.FakeBorderDrawCycle)
            {
                if (item == null || spriteBatch == null)
                    return 0f;
                return orig(item, context, spriteBatch, screenPositionForItemCenter, scale, sizeLimit, environmentColor);
            }
            return 0f;
        }

        private static void Projectile_Update(On_Projectile.orig_Update orig, Projectile self, int i)
        {
            if (self.active)
            {
                PWingFakePlayerProjectile fPPInstance;
                bool canGetGlobal = self.TryGetGlobalProjectile(out fPPInstance);
                if (canGetGlobal)
                {
                    fPPInstance.UpdateFakeOwner(self);
                    if (fPPInstance.FakeOwnerIdentity != -1 && fPPInstance.FakeOwnerIdentity != PWingFakePlayerProjectile.OwnerOfThisUpdateCycle)
                    {
                        return;
                    }
                }
            }
            if (i < Main.maxProjectiles)
                orig(self, i);
        }

        public static bool DrawingProjectileFromCache = false;

        private static void Main_DrawProj(On_Main.orig_DrawProj orig, Main self, int i)
        {
            if (i >= 0)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active)
                {
                    PWingFakePlayerProjectile fPPInstance;
                    bool canGetGlobal = proj.TryGetGlobalProjectile(out fPPInstance);
                    if (canGetGlobal)
                    {
                        fPPInstance.UpdateFakeOwner(proj);
                        if (fPPInstance.FakeOwnerIdentity != -1 && fPPInstance.FakeOwnerIdentity != PWingFakePlayerProjectile.OwnerOfThisDrawCycle)
                        {
                            if (DrawingProjectileFromCache)
                            {
                                PWingFakePlayerPossessingProjectile fPPP = fPPInstance.WhoOwnsMe(proj);
                                if (fPPP != null && fPPP.FakePlayer != null)
                                {
                                    orig(self, i);
                                }
                            }
                            return;
                        }
                    }
                }
            }
            orig(self, i);
        }

        private static void Main_DrawCachedProjs(On_Main.orig_DrawCachedProjs orig, Main self, System.Collections.Generic.List<int> projCache, bool startSpriteBatch)
        {
            DrawingProjectileFromCache = true;
            orig(self, projCache, startSpriteBatch);
            DrawingProjectileFromCache = false;
        }
    }
}
