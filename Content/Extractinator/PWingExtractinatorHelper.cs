using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace PWing.Content.Extractinator
{
    /// <summary>
    /// 批量提取核心逻辑：反复调用原版提取流程并拦截其掉落，把产物直接并入背包/虚空袋，
    /// 背包放不下的部分按模式选择丢弃到地面或留待下次提取。
    /// </summary>
    internal static class PWingExtractinatorHelper
    {
        private static readonly Dictionary<int, int> ToDropItems = new Dictionary<int, int>();
        private static readonly Dictionary<int, int> AcquiredToInventory = new Dictionary<int, int>();
        private static readonly Dictionary<int, int> AcquiredToVoidVault = new Dictionary<int, int>();
        private static readonly List<Item> JustExtractedItems = new List<Item>();

        private static MethodInfo extractinatorUseMethod;
        private static bool loggedMissingExtractinatorUse;

        internal static bool ShouldInterceptNewItem { get; private set; }

        internal static void Unload()
        {
            ToDropItems.Clear();
            AcquiredToInventory.Clear();
            AcquiredToVoidVault.Clear();
            JustExtractedItems.Clear();
            extractinatorUseMethod = null;
            loggedMissingExtractinatorUse = false;
            ShouldInterceptNewItem = false;
        }

        internal static bool CanConvertWithChlorophyte(Item item)
        {
            return ItemTrader.ChlorophyteExtractinator.TryGetTradeOption(item, out _);
        }

        /// <summary>叶绿提取机的批量转换。</summary>
        internal static bool MassConvert(Player player, bool limitToAvailableSpace)
        {
            if (!player.TryGetModPlayer(out PWingExtractinatorPlayer extractinatorPlayer) || extractinatorPlayer.ExtractinatorSlotItem.IsAir)
            {
                return false;
            }
            if (extractinatorPlayer.OpenExtractinatorType != PWingExtractinatorType.Chlorophyte)
            {
                return false;
            }

            ref Item slotItem = ref extractinatorPlayer.ExtractinatorSlotItem;
            if (!ItemTrader.ChlorophyteExtractinator.TryGetTradeOption(slotItem, out ItemTrader.TradeOption option))
            {
                return false;
            }

            bool converted = false;
            bool progressed = true;
            while (progressed && !slotItem.IsAir)
            {
                progressed = false;
                if (slotItem.stack >= option.TakingItemStack)
                {
                    Item itemToInsert = new Item(option.GivingITemType, option.GivingItemStack);
                    progressed |= InsertToPlayerContainer(player, ref itemToInsert);
                    if (!limitToAvailableSpace)
                    {
                        EnqueueItem(ref itemToInsert);
                        progressed = true;
                    }
                }
                if (progressed)
                {
                    slotItem.stack -= option.TakingItemStack;
                    converted = true;
                }
            }

            AnnounceAcquiredItems(player);
            DropQueuedItems(player);
            return converted;
        }

        /// <summary>普通提取机的批量提取。</summary>
        internal static bool MassExtract(Player player, bool limitToAvailableSpace)
        {
            if (!player.TryGetModPlayer(out PWingExtractinatorPlayer extractinatorPlayer) || extractinatorPlayer.ExtractinatorSlotItem.IsAir)
            {
                return false;
            }

            ref Item slotItem = ref extractinatorPlayer.ExtractinatorSlotItem;
            int extractMode = ItemID.Sets.ExtractinatorMode[slotItem.type];
            if (extractMode < 0)
            {
                return false;
            }

            MethodInfo extractinatorUse = GetExtractinatorUseMethod();
            if (extractinatorUse == null)
            {
                if (!loggedMissingExtractinatorUse)
                {
                    loggedMissingExtractinatorUse = true;
                    PWing.Instance?.Logger.Error("未找到 Player.ExtractinatorUse，批量提取功能不可用。");
                }
                return false;
            }

            List<Item> pending = new List<Item>();
            if (extractinatorPlayer.ExtractinatorBacklog.Remove(extractMode, out List<Item> backlog))
            {
                pending.AddRange(backlog);
            }

            bool extracted = DistributeItems(player, pending, limitToAvailableSpace);
            int extractinatorTileType = extractinatorPlayer.OpenExtractinatorType == PWingExtractinatorType.Normal
                ? TileID.Extractinator
                : TileID.ChlorophyteExtractinator;

            int oldSelectedItem = player.selectedItem;
            player.selectedItem = 58;
            JustExtractedItems.Clear();
            try
            {
                while (!slotItem.IsAir && pending.Count == 0)
                {
                    ShouldInterceptNewItem = true;
                    try
                    {
                        extractinatorUse.Invoke(player, new object[] { extractMode, extractinatorTileType });
                    }
                    finally
                    {
                        ShouldInterceptNewItem = false;
                    }

                    if (JustExtractedItems.Count == 0)
                    {
                        // 本次提取没有产物，仍然消耗一个原料
                        slotItem.stack--;
                        continue;
                    }

                    pending.AddRange(JustExtractedItems);
                    JustExtractedItems.Clear();
                    if (DistributeItems(player, pending, limitToAvailableSpace))
                    {
                        slotItem.stack--;
                        extracted = true;
                    }
                }

                if (pending.Count > 0)
                {
                    extractinatorPlayer.ExtractinatorBacklog[extractMode] = pending;
                }
            }
            finally
            {
                player.selectedItem = oldSelectedItem;
                JustExtractedItems.Clear();
            }

            AnnounceAcquiredItems(player);
            DropQueuedItems(player);
            return extracted;
        }

        internal static void OnNewItemIntercept(int itemType, int stack)
        {
            JustExtractedItems.Add(new Item(itemType, stack));
        }

        private static MethodInfo GetExtractinatorUseMethod()
        {
            extractinatorUseMethod ??= typeof(Player).GetMethod("ExtractinatorUse", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return extractinatorUseMethod;
        }

        /// <summary>把一批产物尽量塞进背包与虚空袋；返回是否至少处理了一件。</summary>
        private static bool DistributeItems(Player player, List<Item> items, bool limitToAvailableSpace)
        {
            bool handledAny = false;
            for (int i = 0; i < items.Count; i++)
            {
                Item itemToInsert = items[i];
                handledAny |= InsertToPlayerContainer(player, ref itemToInsert);
                if (!limitToAvailableSpace)
                {
                    EnqueueItem(ref itemToInsert);
                    handledAny = true;
                }
                items[i] = itemToInsert;
            }
            items.RemoveAll(item => item.IsAir);
            return handledAny;
        }

        private static void AnnounceAcquiredItems(Player player)
        {
            foreach (KeyValuePair<int, int> item in AcquiredToInventory)
            {
                Item popupItem = new Item(item.Key, item.Value);
                popupItem.position = player.Center;
                PopupText.NewText(PopupTextContext.RegularItemPickup, popupItem, popupItem.stack, !popupItem.IsACoin, true);
            }
            foreach (KeyValuePair<int, int> item in AcquiredToVoidVault)
            {
                Item popupItem = new Item(item.Key, item.Value);
                popupItem.position = player.Center;
                PopupText.NewText(PopupTextContext.ItemPickupToVoidContainer, popupItem, popupItem.stack, !popupItem.IsACoin, true);
            }
            AcquiredToInventory.Clear();
            AcquiredToVoidVault.Clear();
        }

        private static bool InsertToPlayerContainer(Player player, ref Item itemToInsert)
        {
            if (itemToInsert.IsAir || !player.CanAcceptItemIntoInventory(itemToInsert))
            {
                return false;
            }

            bool inserted = false;
            Player.ItemSpaceStatus spaceStatus = player.ItemSpace(itemToInsert);
            if (spaceStatus.CanTakeItemToPersonalInventory)
            {
                Item beforeInsert = itemToInsert.Clone();
                inserted = InsertToInventory(player.inventory, ref itemToInsert, 0, itemToInsert.IsACoin ? 54 : 50);
                if (itemToInsert.ammo > 0 && !itemToInsert.notAmmo)
                {
                    inserted = InsertToInventory(player.inventory, ref itemToInsert, 54, 4) | inserted;
                }
                AddAcquiredItem(AcquiredToInventory, beforeInsert.type, beforeInsert.stack - itemToInsert.stack);
            }

            if (!itemToInsert.IsAir)
            {
                spaceStatus = player.ItemSpace(itemToInsert);
                if (spaceStatus.ItemIsGoingToVoidVault)
                {
                    Item beforeInsert = itemToInsert.Clone();
                    inserted = InsertToInventory(player.bank4.item, ref itemToInsert, 0, 50) | inserted;
                    AddAcquiredItem(AcquiredToVoidVault, beforeInsert.type, beforeInsert.stack - itemToInsert.stack);
                }
            }
            return inserted;
        }

        private static bool InsertToInventory(Item[] inventory, ref Item itemToInsert, int startIndex, int slotCount)
        {
            if (itemToInsert.IsAir)
            {
                return false;
            }

            bool isACoin = itemToInsert.IsACoin;
            int endIndex = Math.Min(inventory.Length, startIndex + slotCount);
            bool inserted = false;

            while (true)
            {
                int slot = FindExistingOrEmptyStack(inventory, itemToInsert.type, startIndex, endIndex);
                if (slot < 0)
                {
                    break;
                }

                inserted = true;
                if (inventory[slot].IsAir)
                {
                    inventory[slot].SetDefaults(itemToInsert.type);
                    inventory[slot].newAndShiny = true;
                    inventory[slot].stack = 0;
                }

                int moved = Math.Min(inventory[slot].maxStack - inventory[slot].stack, itemToInsert.stack);
                inventory[slot].stack += moved;
                itemToInsert.stack -= moved;

                if (isACoin)
                {
                    SortCoinsInInventory(inventory, startIndex, endIndex);
                }
                if (itemToInsert.IsAir)
                {
                    break;
                }
            }

            if (itemToInsert.IsAir)
            {
                itemToInsert.TurnToAir();
            }
            return inserted;
        }

        private static int FindExistingOrEmptyStack(Item[] inventory, int type, int startIndex, int endIndex)
        {
            int emptySlot = -1;
            for (int i = startIndex; i < endIndex; i++)
            {
                if (!inventory[i].IsAir && inventory[i].type == type && inventory[i].stack < inventory[i].maxStack)
                {
                    return i;
                }
                if (inventory[i].IsAir)
                {
                    emptySlot = i;
                }
            }
            return emptySlot;
        }

        private static void SortCoinsInInventory(Item[] inventory, int startIndex, int endIndex)
        {
            for (int i = startIndex; i < endIndex; i++)
            {
                if (inventory[i].IsAir || !inventory[i].IsACoin || inventory[i].type == ItemID.PlatinumCoin || inventory[i].stack != inventory[i].maxStack)
                {
                    continue;
                }

                inventory[i].SetDefaults(inventory[i].type + 1);
                for (int j = startIndex; j < endIndex; j++)
                {
                    if (j != i && !inventory[j].IsAir && inventory[j].type == inventory[i].type && inventory[j].stack < inventory[j].maxStack)
                    {
                        inventory[j].stack++;
                        inventory[i].TurnToAir();
                    }
                }
            }
        }

        private static void AddAcquiredItem(Dictionary<int, int> dictionary, int type, int stack)
        {
            if (type == 0 || stack == 0)
            {
                return;
            }
            dictionary.TryGetValue(type, out int current);
            dictionary[type] = current + stack;
        }

        internal static void DropQueuedItems(Player player)
        {
            if (ToDropItems.Count == 0)
            {
                return;
            }

            Vector2 center = player.Center;
            Point16 dropTile = Utils.ToPoint16(center);
            if (player.TryGetModPlayer(out PWingExtractinatorPlayer extractinatorPlayer))
            {
                dropTile = Utils.ToPoint16(Utils.ToVector2(extractinatorPlayer.CurrentOpenExtractinator.Center));
            }

            foreach (KeyValuePair<int, int> queuedItem in ToDropItems)
            {
                Item item = new Item(queuedItem.Key, queuedItem.Value);
                while (item.stack > 0)
                {
                    int stack = Math.Min(item.maxStack, item.stack);
                    item.stack -= stack;
                    int itemIndex = Item.NewItem(player.GetSource_TileInteraction(dropTile.X, dropTile.Y), center, Vector2.Zero, queuedItem.Key, stack, false, -1, true);
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemIndex, 1f);
                    }
                }
            }
            ToDropItems.Clear();
        }

        internal static void EnqueueItem(ref Item item)
        {
            if (item.IsAir)
            {
                return;
            }
            ToDropItems.TryGetValue(item.type, out int current);
            ToDropItems[item.type] = current + item.stack;
            item.TurnToAir();
        }
    }
}
