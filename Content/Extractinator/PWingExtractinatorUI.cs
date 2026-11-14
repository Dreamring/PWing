using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace PWing.Content.Extractinator
{
    /// <summary>
    /// 批量提取界面：在物品栏下方显示提取槽、提取按钮与库存限制模式按钮。
    /// </summary>
    public class PWingExtractinatorUI : ModSystem
    {
        private const string InterfaceLayerName = "PWing: Extractinator";

        private static UserInterface userInterface;
        private static PWingExtractinatorUIState extractUIState;
        private static GameTime lastUIUpdate;

        public override void Load()
        {
            PWingExtractinatorRegistry.Initialize();
            if (Main.dedServ)
            {
                return;
            }

            userInterface = new UserInterface();
            extractUIState = new PWingExtractinatorUIState();
            extractUIState.Activate();
            userInterface.SetState(extractUIState);
            PWingExtractinatorDetours.Initialize();
        }

        public override void Unload()
        {
            PWingExtractinatorDetours.Unload();
            PWingExtractinatorRegistry.Unload();
            PWingExtractinatorHelper.Unload();
            if (!Main.dedServ)
            {
                extractUIState?.Deactivate();
                userInterface?.SetState(null);
                userInterface = null;
                extractUIState = null;
                lastUIUpdate = null;
            }
        }

        public override void UpdateUI(GameTime gameTime)
        {
            lastUIUpdate = gameTime;
            if (PWingExtractinatorPlayer.ExtractinatorOpenLocally)
            {
                userInterface?.Update(gameTime);
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            if (Main.dedServ || lastUIUpdate == null)
            {
                return;
            }

            int inventoryLayerIndex = layers.FindIndex(layer => layer.Name == "Vanilla: Inventory");
            if (inventoryLayerIndex == -1)
            {
                return;
            }
            layers.Insert(inventoryLayerIndex + 1, new LegacyGameInterfaceLayer(InterfaceLayerName, DrawExtractinatorUI, InterfaceScaleType.UI));
        }

        private static bool DrawExtractinatorUI()
        {
            if (PWingExtractinatorPlayer.ExtractinatorOpenLocally)
            {
                userInterface?.Draw(Main.spriteBatch, lastUIUpdate);
            }
            return true;
        }
    }

    internal sealed class PWingExtractinatorUIState : UIState
    {
        private const int SlotX = 100;
        private const int SlotY = 270;

        private PWingExtractinatorModeButton modeButton;

        public override void OnInitialize()
        {
            PWingExtractinatorExtractButton extractButton = new PWingExtractinatorExtractButton();
            extractButton.Left.Set(150f, 0f);
            extractButton.Top.Set(270f, 0f);
            Append(extractButton);

            modeButton = new PWingExtractinatorModeButton();
            modeButton.Left.Set(90f, 0f);
            modeButton.Top.Set(260f, 0f);
            Append(modeButton);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (Main.recBigList || !PWingExtractinatorPlayer.TryGetLocal(out PWingExtractinatorPlayer extractinatorPlayer))
            {
                return;
            }

            float oldInventoryScale = Main.inventoryScale;
            Main.inventoryScale = 0.85f;

            ref Item slotItem = ref extractinatorPlayer.ExtractinatorSlotItem;
            Texture2D slotBack = TextureAssets.InventoryBack6.Value;
            float slotWidth = slotBack.Width * Main.inventoryScale;
            float slotHeight = slotBack.Height * Main.inventoryScale;

            if (Utils.FloatIntersect(Main.mouseX, Main.mouseY, 0f, 0f, SlotX, SlotY, slotWidth, slotHeight) && !modeButton.IsMouseHovering)
            {
                ItemSlot.OverrideHover(ref slotItem, 3);
                if (Main.mouseItem.IsAir || ItemSlot.ControlInUse || ItemSlot.ShiftInUse
                    || ItemID.Sets.ExtractinatorMode[Main.mouseItem.type] >= 0
                    || PWingExtractinatorHelper.CanConvertWithChlorophyte(Main.mouseItem))
                {
                    ItemSlot.LeftClick(ref slotItem, 0);
                    ItemSlot.RightClick(ref slotItem, 0);
                }
                ItemSlot.MouseHover(ref slotItem, 3);
                Main.LocalPlayer.mouseInterface = true;
            }

            if (slotItem.IsAir)
            {
                string textKey = extractinatorPlayer.OpenExtractinatorType == PWingExtractinatorType.Normal
                    ? "Mods.PWing.UI.Extractinator.InsertToExtractinator"
                    : "Mods.PWing.UI.Extractinator.InsertToChloroExtractinator";
                Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.MouseText.Value, Language.GetTextValue(textKey), SlotX + 60f, SlotY, Color.White, Color.Black, Vector2.Zero);
            }

            ItemSlot.Draw(spriteBatch, ref slotItem, 5, new Vector2(SlotX, SlotY));
            Main.inventoryScale = oldInventoryScale;
        }
    }

    internal sealed class PWingExtractinatorExtractButton : UIElement
    {
        private static bool ShowButton
        {
            get
            {
                return PWingExtractinatorPlayer.TryGetLocal(out PWingExtractinatorPlayer extractinatorPlayer)
                    && extractinatorPlayer.HasExtractinatorOpen
                    && !extractinatorPlayer.ExtractinatorSlotItem.IsAir;
            }
        }

        public override void OnInitialize()
        {
            OnLeftMouseDown += HandlePress;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            // 延迟到界面实际打开后再读取贴图尺寸，避免在模组加载阶段访问资源
            Width.Set(TextureAssets.Reforge[0].Value.Width + 16f, 0f);
            Height.Set(TextureAssets.Reforge[0].Value.Height + 16f, 0f);
        }

        private void HandlePress(UIMouseEvent evt, UIElement listeningElement)
        {
            if (evt.Target != this)
            {
                return;
            }

            bool limitToAvailableSpace = PWingExtractinatorPlayer.TryGetLocal(out PWingExtractinatorPlayer extractinatorPlayer)
                && extractinatorPlayer.LimitExtractToInventory;
            if (PWingExtractinatorHelper.MassConvert(Main.LocalPlayer, limitToAvailableSpace)
                || PWingExtractinatorHelper.MassExtract(Main.LocalPlayer, limitToAvailableSpace))
            {
                SoundEngine.PlaySound(SoundID.Grab);
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (!ShowButton)
            {
                return;
            }

            Texture2D icon = TextureAssets.Reforge[IsMouseHovering ? 1 : 0].Value;
            spriteBatch.Draw(icon, Utils.Center(GetViewCullingArea()), null, Color.White, 0f, Utils.Size(icon) / 2f, 1f, SpriteEffects.None, 0f);
            if (IsMouseHovering)
            {
                Main.LocalPlayer.mouseInterface = true;
            }
        }
    }

    internal sealed class PWingExtractinatorModeButton : UIElement
    {
        public override void OnInitialize()
        {
            OnLeftMouseDown += HandlePress;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            Width.Set(TextureAssets.HbLock[0].Value.Width / 2f, 0f);
            Height.Set(TextureAssets.HbLock[0].Value.Height, 0f);
        }

        private void HandlePress(UIMouseEvent evt, UIElement listeningElement)
        {
            if (evt.Target != this || !PWingExtractinatorPlayer.TryGetLocal(out PWingExtractinatorPlayer extractinatorPlayer))
            {
                return;
            }

            extractinatorPlayer.LimitExtractToInventory = !extractinatorPlayer.LimitExtractToInventory;
            SoundEngine.PlaySound(SoundID.MenuTick);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (!PWingExtractinatorPlayer.TryGetLocal(out PWingExtractinatorPlayer extractinatorPlayer) || !extractinatorPlayer.HasExtractinatorOpen)
            {
                return;
            }

            Texture2D icon = TextureAssets.HbLock[extractinatorPlayer.LimitExtractToInventory ? 0 : 1].Value;
            Rectangle iconFrame = Utils.Frame(icon, 2, 1, 0, 0);
            Vector2 center = Utils.Center(GetViewCullingArea());
            spriteBatch.Draw(icon, center, iconFrame, Color.White, 0f, Utils.Size(iconFrame) / 2f, 0.8f, SpriteEffects.None, 0f);

            if (IsMouseHovering)
            {
                Main.LocalPlayer.mouseInterface = true;
                string textKey = extractinatorPlayer.LimitExtractToInventory
                    ? "Mods.PWing.UI.Extractinator.LimitsToInventory"
                    : "Mods.PWing.UI.Extractinator.IgnoresInventory";
                Main.instance.MouseText(Language.GetTextValue(textKey));
                Main.mouseText = true;
                spriteBatch.Draw(icon, center, Utils.Frame(icon, 2, 1, 1, 0), Main.OurFavoriteColor, 0f, Utils.Size(Utils.Frame(icon, 2, 1, 1, 0)) / 2f, 0.8f, SpriteEffects.None, 0f);
            }
        }
    }
}
