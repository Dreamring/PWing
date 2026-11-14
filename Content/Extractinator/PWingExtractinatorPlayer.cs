using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace PWing.Content.Extractinator
{
    public enum PWingExtractinatorType : byte
    {
        Normal,
        Chlorophyte
    }

    /// <summary>
    /// 批量提取机的玩家状态：提取槽物品、当前打开的提取机、库存限制模式与未领取的提取产物。
    /// </summary>
    public class PWingExtractinatorPlayer : ModPlayer
    {
        public Rectangle CurrentOpenExtractinator;
        public PWingExtractinatorType OpenExtractinatorType;
        public Item ExtractinatorSlotItem = new Item();
        public readonly Dictionary<int, List<Item>> ExtractinatorBacklog = new Dictionary<int, List<Item>>();
        public bool LimitExtractToInventory;

        public bool HasExtractinatorOpen { get; private set; }

        public static bool ExtractinatorOpenLocally
        {
            get
            {
                return TryGetLocal(out PWingExtractinatorPlayer modPlayer) && modPlayer.HasExtractinatorOpen;
            }
        }

        public static bool TryGetLocal(out PWingExtractinatorPlayer result)
        {
            result = null;
            if (Main.LocalPlayer == null)
            {
                return false;
            }
            return Main.LocalPlayer.TryGetModPlayer(out result);
        }

        public override void PreUpdate()
        {
            if (HasExtractinatorOpen)
            {
                Point16 center = Utils.ToTileCoordinates16(Player.Center);
                int x = CurrentOpenExtractinator.X;
                int y = CurrentOpenExtractinator.Y;
                bool invalidTile = !WorldGen.InWorld(x, y);
                bool outOfRange = center.X < x - Terraria.Player.tileRangeX || center.X > x + CurrentOpenExtractinator.Width + Terraria.Player.tileRangeX
                    || center.Y < y - Terraria.Player.tileRangeY || center.Y > y + CurrentOpenExtractinator.Height + Terraria.Player.tileRangeY;
                if (!invalidTile)
                {
                    invalidTile = !PWingExtractinatorRegistry.IsAnyExtractinator(Main.tile[x, y].TileType);
                }
                if (outOfRange || invalidTile)
                {
                    CloseExtractinator();
                }
            }

            Tile targetTile = Main.tile[Terraria.Player.tileTargetX, Terraria.Player.tileTargetY];
            if (PWingExtractinatorRegistry.IsExtractinator(targetTile.TileType))
            {
                Player.noThrow = 2;
                Player.cursorItemIconEnabled = true;
                Player.cursorItemIconID = ItemID.Extractinator;
            }
            else if (PWingExtractinatorRegistry.IsChlorophyteExtractinator(targetTile.TileType))
            {
                Player.noThrow = 2;
                Player.cursorItemIconEnabled = true;
                Player.cursorItemIconID = ItemID.ChlorophyteExtractinator;
            }
        }

        public override void PostUpdate()
        {
            if (HasExtractinatorOpen && (!Main.playerInventory || Main.npcShop != 0 || Main.LocalPlayer.talkNPC != -1 || Main.LocalPlayer.chest != -1))
            {
                CloseExtractinator(true);
            }
        }

        public override void SaveData(TagCompound tag)
        {
            if (!ExtractinatorSlotItem.IsAir)
            {
                tag["LastExtractinatorItem"] = ExtractinatorSlotItem;
            }
            if (LimitExtractToInventory)
            {
                tag["LimitExtractToInventory"] = true;
            }
        }

        public override void LoadData(TagCompound tag)
        {
            if (tag.TryGet("LastExtractinatorItem", out Item savedItem) && !savedItem.IsAir)
            {
                ExtractinatorSlotItem = savedItem;
            }
            if (tag.TryGet("LimitExtractToInventory", out bool limitToInventory))
            {
                LimitExtractToInventory = limitToInventory;
            }
        }

        public override void OnEnterWorld()
        {
            // 进入世界时界面不会恢复，先把上次保存的槽位物品还给玩家
            HasExtractinatorOpen = false;
            DropExtractinatorItem();
        }

        public override bool HoverSlot(Item[] inventory, int context, int slot)
        {
            if (!ExtractinatorOpenLocally || (context != 0 && context != 13) || !ItemSlot.ShiftInUse)
            {
                return false;
            }

            Item hoveredItem = inventory[slot];
            if (hoveredItem.IsAir || hoveredItem.favorited)
            {
                return false;
            }

            bool extractable = ItemID.Sets.ExtractinatorMode[hoveredItem.type] >= 0
                || (TryGetLocal(out PWingExtractinatorPlayer local) && local.OpenExtractinatorType == PWingExtractinatorType.Chlorophyte && PWingExtractinatorHelper.CanConvertWithChlorophyte(hoveredItem));
            if (extractable)
            {
                Main.cursorOverride = 9;
                return true;
            }
            return false;
        }

        public override bool ShiftClickSlot(Item[] inventory, int context, int slot)
        {
            if (ExtractinatorOpenLocally && (context == 0 || context == 13) && Main.cursorOverride == 9)
            {
                Utils.Swap(ref inventory[slot], ref ExtractinatorSlotItem);
                SoundEngine.PlaySound(SoundID.Grab);
                return true;
            }
            return false;
        }

        public void OpenExtractinator()
        {
            SoundEngine.PlaySound(HasExtractinatorOpen ? SoundID.MenuTick : SoundID.MenuOpen);
            HasExtractinatorOpen = true;
            Main.CreativeMenu.CloseMenu();
            Main.SetNPCShopIndex(0);
            Player.SetTalkNPC(-1);
            Player.chest = -1;
            Main.playerInventory = true;
        }

        public void CloseExtractinator(bool silent = false)
        {
            HasExtractinatorOpen = false;
            DropExtractinatorItem();
            if (!silent)
            {
                SoundEngine.PlaySound(SoundID.MenuClose);
            }
        }

        private void DropExtractinatorItem()
        {
            if (ExtractinatorSlotItem.IsAir)
            {
                return;
            }

            ExtractinatorSlotItem.position = Player.Center;
            int itemIndex = Item.NewItem(Player.GetSource_DropAsItem(), Player.Center, ExtractinatorSlotItem.type, ExtractinatorSlotItem.stack, false, ExtractinatorSlotItem.prefix, true);
            Main.item[itemIndex] = ExtractinatorSlotItem.Clone();
            Main.item[itemIndex].newAndShiny = false;
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemIndex);
            }
            ExtractinatorSlotItem.TurnToAir();
        }
    }
}
