using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Capture;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using PWing.Content.Players;
using PWing.Common;
using ReLogic.Utilities;
using static Terraria.Player;

namespace PWing.Content.FakePlayer
{
    public static class PWingFakePlayerTypeID
    {
        public const int Subspace = 0;
        public const int Hydro = 1;
        public const int Tesseract = 2;
    }
    public static class PWingTrailingID
    {
        public const int IDLE = 0;
        public const int MAGIC = 1;
        public const int RANGED = 2;
        public const int MELEE = 3;
        public const int CLOSERANGE = 4;
    }
    public static class PWingDrawStateID
    {
        public const int All = -1;
        public const int Border = 0;
        public const int Wings = 1;
        public const int HeldItemAndProjectilesBeforeBackArm = 2;
        public const int Body = 3;
        public const int HeldItemAndProjectilesBeforeFrontArm = 4;
        public const int FrontArm = 5;
        public const int HeldItemAndProjectilesAfterFrontArm = 6;
    }
    public class PWingFakePlayer
    {
        public static bool SupressNetMessage13and41 = false;
        public static bool SupressSound = false;

        public int OverrideUseSlot = -1;
        public int UniqueMouseX = -1;
        public int UniqueMouseY = -1;
        public bool ForceItemUse = false;
        public bool SkipDrawing = false;
        public bool ShouldUseWingsArmPosition = false;
        public int BonusItemAnimationTime = 0;
        public int WingFrame = 0;
        public int OverrideTrailingType = -1;
        public int TrailingType = 0;
        public const int Width = 20;
        public const int Height = 42;
        public int weaponDrawOrder = 0;
        public Vector2 Position = Vector2.Zero;
        public Vector2 Velocity = Vector2.Zero;
        public Vector2 SecondPosition = Vector2.Zero;
        public Vector2 OldPosition = Vector2.Zero;
        public Rectangle bodyFrame = Rectangle.Empty;

        public static bool IsValidUseStyle(Item item)
        {
            return item.useStyle == ItemUseStyleID.Swing ||
                item.useStyle == ItemUseStyleID.Shoot ||
                item.useStyle == ItemUseStyleID.MowTheLawn ||
                item.useStyle == ItemUseStyleID.RaiseLamp ||
                item.useStyle == ItemUseStyleID.HoldUp ||
                item.useStyle == ItemUseStyleID.Guitar ||
                item.useStyle == ItemUseStyleID.HiddenAnimation ||
                item.useStyle == ItemUseStyleID.Rapier ||
                item.useStyle == ItemUseStyleID.Thrust;
        }

        public static bool IsPlaceable(Item item)
        {
            return item.createTile != -1;
        }

        public static bool CheckItemValidityFull(Player player, Item item, Item lastUsedItem, int fakePlayerType)
        {
            PWingPlayer pwPlayer = player.GetModPlayer<PWingPlayer>();
            if (!item.active || item.IsAir)
            {
                return false;
            }
            if (fakePlayerType == PWingFakePlayerTypeID.Hydro && player.gravDir != 1)
            {
                return false;
            }
            bool canUseItem = true;
            if (!PWingFakePlayerHelper.FakePlayerItemWhitelist.Contains(item.type) || lastUsedItem == null)
            {
                if (item.ammo > 0 || item.fishingPole > 0 || (item.CountsAsClass(DamageClass.Summon) && !item.CountsAsClass(DamageClass.SummonMeleeSpeed)) || lastUsedItem == null || item.mountType != -1)
                {
                    canUseItem = false;
                }
                else if (item.damage <= 0 && !IsPlaceable(item))
                {
                    canUseItem = false;
                }
            }
            bool additionalValid = true;
            if (fakePlayerType == PWingFakePlayerTypeID.Hydro && additionalValid)
            {
                additionalValid = ValidItemForHydroPlayer(item);
            }
            if (fakePlayerType == PWingFakePlayerTypeID.Tesseract && additionalValid)
            {
                additionalValid = ValidItemForTesseractPlayer(item);
            }
            bool validItem = canUseItem && lastUsedItem.type == item.type && IsValidUseStyle(item) && additionalValid;
            return validItem;
        }

        private static bool ValidItemForTesseractPlayer(Item item)
        {
            if (item.createTile != -1)
            {
                return false;
            }
            if (item.consumable && item.stack <= 1)
            {
                return false;
            }
            return true;
        }

        private static bool ValidItemForHydroPlayer(Item item)
        {
            bool uniqueUseConditions = false;
            int trailingType = ItemTrailingType(item);
            if (trailingType == PWingTrailingID.CLOSERANGE || trailingType == PWingTrailingID.MELEE || item.type == ItemID.FlareGun)
            {
                if (item.pick > 0 || item.axe > 0 || item.createTile != -1 || PWingFakePlayerHelper.HydroPlayerItemBlacklist.Contains(item.type))
                {
                    return false;
                }
                uniqueUseConditions = true;
            }
            return uniqueUseConditions;
        }

        public static int ItemTrailingType(Item item)
        {
            int returnType = PWingTrailingID.RANGED;
            if (item.CountsAsClass(DamageClass.Melee) || item.CountsAsClass(DamageClass.SummonMeleeSpeed) || PWingFakePlayerHelper.CloseRangeItemsForFakePlayer.Contains(item.type) || IsPlaceable(item))
            {
                if (item.noMelee && !IsPlaceable(item))
                    returnType = PWingTrailingID.CLOSERANGE;
                else
                    returnType = PWingTrailingID.MELEE;
            }
            else if (item.CountsAsClass(DamageClass.Ranged))
            {
                returnType = PWingTrailingID.RANGED;
            }
            else if (item.CountsAsClass(DamageClass.Magic))
            {
                returnType = PWingTrailingID.MAGIC;
            }
            return returnType;
        }

        public void ItemCheckHack(Player player)
        {
            SupressNetMessage13and41 = true;
            PWingPlayer pwPlayer = player.GetModPlayer<PWingPlayer>();
            Item item = player.inventory[UseItemSlot(player)];
            bool saveCursorIconEnabled = player.cursorItemIconEnabled;
            int saveCursorIconID = player.cursorItemIconID;
            bool saveAutoSwing = item.autoReuse;
            bool saveUseTurn = item.useTurn;
            int savePick = item.pick;
            int saveAxe = item.axe;
            int saveHammer = item.hammer;
            try
            {
                if (Main.netMode != NetmodeID.Server)
                {
                    if (!TextureAssets.Item[item.type].IsLoaded)
                        Main.instance.LoadItem(item.type);
                }
                heldItem = item;
                TrailingType = 0;
                if (FakePlayerType == PWingFakePlayerTypeID.Tesseract)
                    item.pick = item.axe = item.hammer = 0;
                item.autoReuse = true;
                item.useTurn = true;
                bool valid = CheckItemValidityFull(player, item, lastUsedItem, FakePlayerType);
                if (item.IsAir)
                {
                    KillMyOwnedProjectiles = true;
                    RunItemCheck(player, true);
                }
                else if (valid)
                {
                    if (FakePlayerType == PWingFakePlayerTypeID.Tesseract)
                    {
                        int UniqueUsageSlot = OverrideUseSlot % 10;
                        pwPlayer.tesseractData[UniqueUsageSlot].FoundValidItem = true;
                    }
                    RunItemCheck(player, true);
                }
                else
                {
                    if (lastUsedItem != null && !lastUsedItem.IsAir && lastUsedItem.type != item.type)
                    {
                        lastUsedItem = heldItem.Clone();
                        if (FakePlayerType != PWingFakePlayerTypeID.Hydro)
                        {
                            RunItemCheck(player, true);
                        }
                        else
                        {
                            RunItemCheck(player, false);
                        }
                    }
                    else
                        RunItemCheck(player, false);
                    ResetVariables();
                }
                if (itemAnimation > 0)
                {
                    ShouldUseWingsArmPosition = false;
                    if (FakePlayerType == PWingFakePlayerTypeID.Tesseract)
                    {
                        BonusItemAnimationTime = 4;
                    }
                    else
                        BonusItemAnimationTime = 15;
                }
                else
                {
                    if (itemAnimation == 0 && (lastUsedItem == null || lastUsedItem.type != heldItem.type))
                        lastUsedItem = heldItem.Clone();
                    if (item.holdStyle == 0 || !valid)
                        ShouldUseWingsArmPosition = Velocity.Length() > 2f;
                    else
                        ShouldUseWingsArmPosition = false;
                }
                if (BonusItemAnimationTime > 0)
                {
                    BonusItemAnimationTime--;
                    TrailingType = ItemTrailingType(item);
                }
            }
            finally
            {
                // 无论中途是否抛异常，都必须还原真实物品与静态开关
                item.pick = savePick;
                item.axe = saveAxe;
                item.hammer = saveHammer;
                item.autoReuse = saveAutoSwing;
                item.useTurn = saveUseTurn;
                player.cursorItemIconEnabled = saveCursorIconEnabled;
                player.cursorItemIconID = saveCursorIconID;
                SupressNetMessage13and41 = false;
            }
        }

        public PWingSavedPlayerValues PlayerSavedProperties;
        public CompositeArmData compositeFrontArm;
        public CompositeArmData compositeBackArm;
        public float compositeFrontArmRotation;
        public float compositeBackArmRotation;
        public int projectileDrawPosition;
        public bool heldProjOverHand;
        public Vector2 bodyVect;
        public Rectangle compFrontArmFrame = Rectangle.Empty;
        public Rectangle compBackArmFrame = Rectangle.Empty;
        public Item heldItem;
        private Item lastUsedItem;
        public int FakePlayerType = 0;
        private int FakePlayerID = 0;
        public int ID => FakePlayerID;
        public bool KillMyOwnedProjectiles = false;

        public int UseItemSlot(Player player)
        {
            if (FakePlayerType == PWingFakePlayerTypeID.Hydro)
            {
                return player.selectedItem;
            }
            if (FakePlayerType == PWingFakePlayerTypeID.Subspace)
            {
                PWingPlayer pwingPlayer = player.GetModPlayer<PWingPlayer>();
                if (pwingPlayer.tesseractPlayerCount >= 10)
                {
                    return 39;
                }
                return 49;
            }
            if (OverrideUseSlot != -1)
            {
                return OverrideUseSlot;
            }
            return 49;
        }

        public bool compositeFrontArmEnabled = false;
        public bool compositeBackArmEnabled = false;

        public int AltFunctionUse;
        public int toolTime;
        public int itemAnimation;
        public int itemAnimationMax;
        public int itemTime;
        public int itemTimeMax;
        public float itemRotation;
        public Vector2 itemLocation = Vector2.Zero;

        public int itemWidth;
        public int itemHeight;
        public int direction;
        public int reuseDelay;
        public bool controlUseItem;
        public bool controlUseTile;
        public bool releaseUseItem;
        public bool justDroppedAnItem;

        public int AttackCD;
        public int ItemUsesThisAnimation;
        public int BoneGloveTimer;
        public bool Channel;
        public int HeldProj = -1;

        public void ResetVariables()
        {
            lastUsedItem = null;
            toolTime = 0;
            itemAnimation = 0;
            itemAnimationMax = 0;
            itemTime = 0;
            itemTimeMax = 0;
            itemRotation = 0;
            itemLocation = Vector2.Zero;
            itemWidth = 0;
            itemHeight = 0;
            reuseDelay = 0;
            releaseUseItem = false;
            justDroppedAnItem = false;
            AttackCD = 0;
            ItemUsesThisAnimation = 0;
            BoneGloveTimer = 0;
        }

        public PWingFakePlayer(int type = 0, int identity = -1)
        {
            FakePlayerType = type;
            PlayerSavedProperties = new PWingSavedPlayerValues();
            FakePlayerID = identity;
        }

        public void Update(Player player)
        {
            if (player.boneGloveTimer > 0)
            {
                player.boneGloveTimer--;
            }
            player.heldProj = -1;
            HeldProj = -1;
            if (player.attackCD > 0)
            {
                player.attackCD--;
            }
            if (player.itemAnimation == 0)
            {
                player.attackCD = 0;
            }
        }

        private int lastUsedItemType = -1;
        private int ChargeDuration = 0;

        public void RunItemCheck(Player player, bool canUseItem = false)
        {
            PWingPlayer pwPlayer = player.GetModPlayer<PWingPlayer>();
            PWingFakePlayerProjectile.OwnerOfThisUpdateCycle = FakePlayerID;
            int whoAmI = player.whoAmI;
            bool holdingTesseract = !player.HeldItem.IsAir && player.HeldItem.type == ModContent.ItemType<Content.Items.Summon.PWingTesseract>();
            bool ownersControlUseItem = player.controlUseItem;
            bool ownersControlUseTile = player.controlUseTile;
            SaveRealPlayerValues(player);
            CopyFakeToReal(player);
            Update(player);
            if (FakePlayerType == PWingFakePlayerTypeID.Tesseract)
                CheckTesseractShouldDraw(player, OverrideTrailingType != -1 ? OverrideTrailingType : TrailingType);
            bool valid = CheckItemValidityFull(player, player.HeldItem, lastUsedItem, FakePlayerType);
            int UniqueUsageSlot = OverrideUseSlot % 10;
            if (canUseItem || player.channel)
            {
                if (lastUsedItemType == -1)
                    lastUsedItemType = heldItem.type;
                if (heldItem.type != lastUsedItemType)
                {
                    ChargeDuration = player.itemAnimation = player.itemTime = player.itemAnimationMax = -1;
                    player.channel = false;
                    canUseItem = false;
                    KillMyOwnedProjectiles = true;
                    if (FakePlayerType == PWingFakePlayerTypeID.Tesseract)
                    {
                        if (UniqueUsageSlot >= 0)
                        {
                            pwPlayer.tesseractData[UniqueUsageSlot].Reset();
                        }
                    }
                }
                lastUsedItemType = heldItem.type;
                if (FakePlayerType == PWingFakePlayerTypeID.Tesseract && UniqueUsageSlot >= 0)
                {
                    int automaticUseTimer = pwPlayer.tesseractData[UniqueUsageSlot].ChargeFrames;
                    bool NextToBeRegistered = TesseractServantNextInLine(pwPlayer, UniqueUsageSlot) || automaticUseTimer >= 0;
                    if (automaticUseTimer < 0 && NextToBeRegistered)
                    {
                        if (!player.controlUseItem && !holdingTesseract)
                            player.controlUseItem = ownersControlUseItem;
                        if (!holdingTesseract)
                            player.controlUseTile = ownersControlUseTile;
                    }
                    else if (ForceItemUse && NextToBeRegistered)
                    {
                        if (ChargeDuration < 0 && (player.ItemAnimationEndingOrEnded || player.ItemAnimationJustStarted))
                        {
                            ChargeDuration++;
                        }
                        if (ChargeDuration >= 0)
                        {
                            ChargeDuration++;
                            if (ChargeDuration <= automaticUseTimer)
                            {
                                if (pwPlayer.tesseractData[UniqueUsageSlot].AltFunctionUse)
                                {
                                    SupressSound = true;
                                    player.mouseInterface = Main.HoveringOverAnNPC = CaptureManager.Instance.Active = Main.SmartInteractShowingGenuine = false;
                                    SupressSound = false;
                                    player.controlUseTile = ForceItemUse;
                                    player.controlUseItem = false;
                                }
                                else
                                {
                                    player.controlUseItem = ForceItemUse;
                                    player.controlUseTile = false;
                                }
                            }
                            else
                            {
                                player.channel = false;
                                player.controlUseItem = player.controlUseTile = false;
                                ChargeDuration = -2;
                            }
                        }
                    }
                    else
                    {
                        ChargeDuration = -2;
                    }
                }
                else
                {
                    if (!player.controlUseItem)
                        player.controlUseItem = ownersControlUseItem;
                    if (!player.controlUseTile)
                        player.controlUseTile = ownersControlUseTile;
                }
                if (Main.myPlayer == player.whoAmI)
                {
                    if (player.controlUseTile && !Main.mouseLeft)
                    {
                        player.releaseUseItem = true;
                        player.controlUseItem = false;
                    }
                    player.ItemCheck_ManageRightClickFeatures();
                }
                if (canUseItem || player.channel)
                {
                    if (!player.HeldItem.IsAir && (player.ItemAnimationJustStarted || !player.ItemAnimationActive))
                        player.StartChanneling(player.HeldItem);
                    player.ItemCheck();
                    if (FakePlayerType == PWingFakePlayerTypeID.Tesseract)
                    {
                        if (player.controlUseItem || player.altFunctionUse == 2)
                        {
                            if (ChargeDuration < 0)
                                ChargeDuration = 0;
                            if (pwPlayer.tesseractData[UniqueUsageSlot].ChargeFrames == -1 && !player.HeldItem.IsAir)
                                ChargeDuration++;
                        }
                        if (player.altFunctionUse == 2)
                        {
                            if (!pwPlayer.tesseractData[UniqueUsageSlot].AltFunctionUse)
                            {
                                pwPlayer.tesseractData[UniqueUsageSlot].AltFunctionUse = true;
                            }
                        }
                    }
                }
            }
            if (!player.controlUseItem && player.altFunctionUse == 0 && ChargeDuration > 0 && FakePlayerType == PWingFakePlayerTypeID.Tesseract && !player.HeldItem.IsAir && player.itemAnimation <= 0)
            {
                if (pwPlayer.tesseractData[UniqueUsageSlot].ChargeFrames == -1)
                {
                    pwPlayer.tesseractData[UniqueUsageSlot].ChargeFrames = ChargeDuration;
                    if (!player.HeldItem.IsAir && !player.HeldItem.channel)
                    {
                        pwPlayer.tesseractData[UniqueUsageSlot].ChargeFrames = 7200;
                    }
                }
                ChargeDuration = 0;
            }
            UpdateMyProjectiles(player);
            player.oldPosition = Position;
            SetupBodyFrame(player);
            player.controlUseItem = false;
            player.controlUseTile = false;
            CopyRealToFake(player);
            LoadRealPlayerValues(player);
            if (FakePlayerType == PWingFakePlayerTypeID.Hydro)
            {
                if (valid)
                    HydroServantPostUpdate(player);
                else if (itemAnimation <= 0 && (BonusItemAnimationTime <= 0 || !valid))
                    SkipDrawing = true;
            }
            ForceItemUse = false;
            KillMyOwnedProjectiles = false;
            PWingFakePlayerProjectile.OwnerOfThisUpdateCycle = -1;
        }

        public void CheckTesseractShouldDraw(Player player, int trailingType)
        {
            if (FakePlayerType == PWingFakePlayerTypeID.Tesseract)
            {
                if (itemAnimation > 0 || BonusItemAnimationTime > 0 || trailingType != PWingTrailingID.IDLE)
                {
                    SkipDrawing = false;
                }
                else if (lastUsedItemType != player.HeldItem.type)
                {
                    SkipDrawing = true;
                }
                else if (player.HeldItem.holdStyle != ItemUseStyleID.None)
                {
                    SkipDrawing = false;
                }
                else
                {
                    Player tempPlayer = new Player();
                    float tempPlayerItemRotation = tempPlayer.itemRotation;
                    Vector2 tempPlayerItemLocation = tempPlayer.itemLocation;
                    Rectangle frame = new Rectangle(0, 0, 0, 0);
                    ItemLoader.HoldStyle(player.HeldItem, tempPlayer, frame);
                    bool wasAnythingEffected = false;
                    if (tempPlayer.itemRotation != tempPlayerItemRotation || !tempPlayer.itemLocation.Equals(tempPlayerItemLocation))
                    {
                        wasAnythingEffected = true;
                    }
                    if (wasAnythingEffected)
                    {
                        SkipDrawing = false;
                    }
                    else
                    {
                        SkipDrawing = true;
                    }
                }
            }
        }

        public bool TesseractServantNextInLine(PWingPlayer pwPlayer, int index)
        {
            for (int i = index - 1; i >= 0; i--)
            {
                if (pwPlayer.tesseractData[i].ChargeFrames <= 0 && pwPlayer.tesseractData[i].FoundValidItemLastFrame)
                {
                    return false;
                }
            }
            return true;
        }

        public void HydroServantPostUpdate(Player player)
        {
            bool isPlayerUsingAHydroCapableItem = CheckItemValidityFull(player, player.HeldItem, player.HeldItem, 1);
            player.SetDummyItemTime(itemAnimation);
            player.itemTimeMax--;
            player.reuseDelay = itemAnimation;
            if (isPlayerUsingAHydroCapableItem)
            {
                Vector2 fromOwnerToMe = Position - player.position;
                float cyclicOffset = (float)(8f * MathHelper.TwoPi / 180f * Math.Sin(MathHelper.ToRadians(Main.GameUpdateCount * 2f))) * player.direction;
                player.SetCompositeArmFront(true, CompositeArmStretchAmount.Full, fromOwnerToMe.ToRotation() + cyclicOffset - MathHelper.PiOver2);
                player.SetCompositeArmBack(true, CompositeArmStretchAmount.Full, fromOwnerToMe.ToRotation() - cyclicOffset - MathHelper.PiOver2);
                if (itemAnimation > 0 || BonusItemAnimationTime > 0)
                {
                    SkipDrawing = false;
                }
                else if (player.HeldItem.holdStyle != ItemUseStyleID.None)
                {
                    SkipDrawing = false;
                }
                else
                {
                    Player tempPlayer = new Player();
                    float tempPlayerItemRotation = tempPlayer.itemRotation;
                    Vector2 tempPlayerItemLocation = tempPlayer.itemLocation;
                    Rectangle frame = new Rectangle(0, 0, 0, 0);
                    ItemLoader.HoldStyle(player.HeldItem, tempPlayer, frame);
                    bool wasAnythingEffected = false;
                    if (tempPlayer.itemRotation != tempPlayerItemRotation || !tempPlayer.itemLocation.Equals(tempPlayerItemLocation))
                    {
                        wasAnythingEffected = true;
                    }
                    if (wasAnythingEffected)
                    {
                        SkipDrawing = false;
                    }
                    else
                    {
                        SkipDrawing = true;
                    }
                }
            }
            else
            {
                SkipDrawing = true;
            }
        }

        public void SaveRealPlayerValues(Player player)
        {
            PlayerSavedProperties.saveMouseInterface = player.mouseInterface;
            PlayerSavedProperties.saveHoveringOverAnNPC = Main.HoveringOverAnNPC;
            PlayerSavedProperties.saveCaptureManagerActive = CaptureManager.Instance.Active;
            PlayerSavedProperties.saveSmartInteractShowingGenuine = Main.SmartInteractShowingGenuine;

            PlayerSavedProperties.SaveMouseX = Main.mouseX;
            PlayerSavedProperties.SaveMouseY = Main.mouseY;

            PlayerSavedProperties.SavePosition = player.position;
            PlayerSavedProperties.SaveOldPos = player.oldPosition;
            PlayerSavedProperties.SaveVelocity = player.velocity;
            PlayerSavedProperties.SavedSelectedItem = player.selectedItem;
            PlayerSavedProperties.PlayerOwnedLastVisualizedSelectedItem = player.lastVisualizedSelectedItem.Clone();
            PlayerSavedProperties.saveChannel = player.channel;
            PlayerSavedProperties.saveFrozen = player.frozen;
            PlayerSavedProperties.saveWebbed = player.webbed;
            PlayerSavedProperties.saveStoned = player.stoned;
            PlayerSavedProperties.saveWet = player.wet;
            PlayerSavedProperties.saveMount = player.mount.Active;
            PlayerSavedProperties.saveAltFunctionUse = player.altFunctionUse;
            PlayerSavedProperties.savePulley = player.pulley;
            PlayerSavedProperties.savePettingAnimal = player.isPettingAnimal;
            PlayerSavedProperties.saveHeldProj = player.heldProj;
            PlayerSavedProperties.saveStealth = player.stealth;
            PlayerSavedProperties.saveGravDir = player.gravDir;
            PlayerSavedProperties.saveInvis = player.invis;
            PlayerSavedProperties.saveGFXOffY = player.gfxOffY;
            PlayerSavedProperties.saveSelectItemOnNextUse = player.selectItemOnNextUse;

            PlayerSavedProperties.savecompositeFrontArmEnabled = player.compositeFrontArm.enabled;
            PlayerSavedProperties.savecompositeBackArmEnabled = player.compositeBackArm.enabled;
            PlayerSavedProperties.saveToolTime = player.toolTime;
            PlayerSavedProperties.saveItemAnimation = player.itemAnimation;
            PlayerSavedProperties.saveItemAnimationMax = player.itemAnimationMax;
            PlayerSavedProperties.saveItemTime = player.itemTime;
            PlayerSavedProperties.saveItemTimeMax = player.itemTimeMax;
            PlayerSavedProperties.saveItemRotation = player.itemRotation;
            PlayerSavedProperties.saveItemLocation = player.itemLocation;
            PlayerSavedProperties.saveItemWidth = player.itemWidth;
            PlayerSavedProperties.saveItemHeight = player.itemHeight;
            PlayerSavedProperties.saveDirection = player.direction;
            PlayerSavedProperties.saveReuseDelay = player.reuseDelay;
            PlayerSavedProperties.saveReleaseUseItem = player.releaseUseItem;
            PlayerSavedProperties.saveControlUseItem = player.controlUseItem;
            PlayerSavedProperties.saveControlUseTile = player.controlUseTile;
            PlayerSavedProperties.saveJustDroppedAnItem = player.JustDroppedAnItem;
            PlayerSavedProperties.saveAttackCD = player.attackCD;
            PlayerSavedProperties.saveItemUsesThisAnimation = player.ItemUsesThisAnimation;
            PlayerSavedProperties.saveBoneGloveTimer = player.boneGloveTimer;
            PlayerSavedProperties.saveFrontArm = player.compositeFrontArm;
            PlayerSavedProperties.saveBackArm = player.compositeBackArm;
            PlayerSavedProperties.saveBodyFrame = player.bodyFrame;
            PlayerSavedProperties.saveMale = player.Male;
        }

        public void CopyFakeToReal(Player player)
        {
            if (UniqueMouseX != -1 && UniqueMouseY != -1)
            {
                Main.mouseX = UniqueMouseX;
                Main.mouseY = UniqueMouseY;
            }
            player.selectedItem = UseItemSlot(player);
            player.lastVisualizedSelectedItem = lastUsedItem;
            if (lastUsedItem == null)
            {
                player.lastVisualizedSelectedItem = player.HeldItem;
            }
            player.channel = Channel;
            player.frozen = player.stoned = player.webbed = player.wet = false;
            player.mount._active = false;
            player.pulley = false;
            player.isPettingAnimal = false;
            player.heldProj = HeldProj;
            player.stealth = 1f;
            if (FakePlayerType != PWingFakePlayerTypeID.Hydro)
                player.gravDir = 1f;
            player.invis = false;
            player.bodyFrame = bodyFrame;
            player.gfxOffY = 0;
            player.selectItemOnNextUse = false;

            player.controlUseItem = controlUseItem;
            player.controlUseTile = controlUseTile;

            player.position = Position;
            player.oldPosition = OldPosition;
            player.velocity = Velocity;
            player.compositeFrontArm.enabled = compositeFrontArmEnabled;
            player.compositeBackArm.enabled = compositeBackArmEnabled;
            player.toolTime = toolTime;
            player.itemAnimation = itemAnimation;
            player.itemAnimationMax = itemAnimationMax;
            player.itemTime = itemTime;
            player.itemTimeMax = itemTimeMax;
            player.itemWidth = itemWidth;
            player.itemHeight = itemHeight;
            player.direction = direction;
            player.reuseDelay = reuseDelay;
            player.releaseUseItem = releaseUseItem;
            player.JustDroppedAnItem = justDroppedAnItem;
            player.itemRotation = itemRotation;
            player.itemLocation = itemLocation;
            player.attackCD = AttackCD;
            player.altFunctionUse = AltFunctionUse;
            player.boneGloveTimer = BoneGloveTimer;
            player.compositeFrontArm = compositeFrontArm;
            player.compositeBackArm = compositeBackArm;
            SetPlayerItemUsesThisAnimationViaReflection(player, ItemUsesThisAnimation);
        }

        public void CopyRealToFake(Player player)
        {
            controlUseItem = player.controlUseItem;
            controlUseTile = player.controlUseTile;

            HeldProj = player.heldProj;
            Channel = player.channel;
            Position = player.position;
            OldPosition = player.oldPosition;
            Velocity = player.velocity;
            compositeFrontArmEnabled = player.compositeFrontArm.enabled;
            compositeBackArmEnabled = player.compositeBackArm.enabled;
            toolTime = player.toolTime;
            itemAnimation = player.itemAnimation;
            itemAnimationMax = player.itemAnimationMax;
            itemTime = player.itemTime;
            itemTimeMax = player.itemTimeMax;
            itemWidth = player.itemWidth;
            itemHeight = player.itemHeight;
            itemRotation = player.itemRotation;
            itemLocation = player.itemLocation;
            direction = player.direction;
            reuseDelay = player.reuseDelay;
            releaseUseItem = player.releaseUseItem;
            justDroppedAnItem = player.JustDroppedAnItem;
            AttackCD = player.attackCD;
            AltFunctionUse = player.altFunctionUse;
            ItemUsesThisAnimation = player.ItemUsesThisAnimation;
            BoneGloveTimer = player.boneGloveTimer;
            compositeFrontArm = player.compositeFrontArm;
            compositeBackArm = player.compositeBackArm;
        }

        public void LoadRealPlayerValues(Player player)
        {
            player.mouseInterface = PlayerSavedProperties.saveMouseInterface;
            Main.HoveringOverAnNPC = PlayerSavedProperties.saveHoveringOverAnNPC;
            SupressSound = true;
            CaptureManager.Instance.Active = PlayerSavedProperties.saveCaptureManagerActive;
            SupressSound = false;
            Main.SmartInteractShowingGenuine = PlayerSavedProperties.saveSmartInteractShowingGenuine;

            player.position = PlayerSavedProperties.SavePosition;
            player.oldPosition = PlayerSavedProperties.SaveOldPos;
            player.velocity = PlayerSavedProperties.SaveVelocity;
            player.selectedItem = PlayerSavedProperties.SavedSelectedItem;
            player.lastVisualizedSelectedItem = PlayerSavedProperties.PlayerOwnedLastVisualizedSelectedItem;
            player.channel = PlayerSavedProperties.saveChannel;
            player.frozen = PlayerSavedProperties.saveFrozen;
            player.webbed = PlayerSavedProperties.saveWebbed;
            player.stoned = PlayerSavedProperties.saveStoned;
            player.wet = PlayerSavedProperties.saveWet;
            player.mount._active = PlayerSavedProperties.saveMount;
            player.altFunctionUse = PlayerSavedProperties.saveAltFunctionUse;
            player.pulley = PlayerSavedProperties.savePulley;
            player.isPettingAnimal = PlayerSavedProperties.savePettingAnimal;
            player.heldProj = PlayerSavedProperties.saveHeldProj;
            player.stealth = PlayerSavedProperties.saveStealth;
            player.gravDir = PlayerSavedProperties.saveGravDir;
            player.invis = PlayerSavedProperties.saveInvis;
            player.gfxOffY = PlayerSavedProperties.saveGFXOffY;
            player.selectItemOnNextUse = PlayerSavedProperties.saveSelectItemOnNextUse;

            player.compositeFrontArm.enabled = PlayerSavedProperties.savecompositeFrontArmEnabled;
            player.compositeBackArm.enabled = PlayerSavedProperties.savecompositeBackArmEnabled;
            player.toolTime = PlayerSavedProperties.saveToolTime;
            player.itemAnimation = PlayerSavedProperties.saveItemAnimation;
            player.itemAnimationMax = PlayerSavedProperties.saveItemAnimationMax;
            player.itemTime = PlayerSavedProperties.saveItemTime;
            player.itemTimeMax = PlayerSavedProperties.saveItemTimeMax;
            player.itemRotation = PlayerSavedProperties.saveItemRotation;
            player.itemLocation = PlayerSavedProperties.saveItemLocation;
            player.itemWidth = PlayerSavedProperties.saveItemWidth;
            player.itemHeight = PlayerSavedProperties.saveItemHeight;
            player.direction = PlayerSavedProperties.saveDirection;
            player.reuseDelay = PlayerSavedProperties.saveReuseDelay;
            player.releaseUseItem = PlayerSavedProperties.saveReleaseUseItem;
            player.controlUseItem = PlayerSavedProperties.saveControlUseItem;
            player.controlUseTile = PlayerSavedProperties.saveControlUseTile;
            player.JustDroppedAnItem = PlayerSavedProperties.saveJustDroppedAnItem;
            player.attackCD = PlayerSavedProperties.saveAttackCD;
            player.boneGloveTimer = PlayerSavedProperties.saveBoneGloveTimer;
            player.compositeFrontArm = PlayerSavedProperties.saveFrontArm;
            player.compositeBackArm = PlayerSavedProperties.saveBackArm;
            player.bodyFrame = PlayerSavedProperties.saveBodyFrame;
            SetPlayerItemUsesThisAnimationViaReflection(player, PlayerSavedProperties.saveItemUsesThisAnimation);
            player.Male = PlayerSavedProperties.saveMale;

            Main.mouseX = PlayerSavedProperties.SaveMouseX;
            Main.mouseY = PlayerSavedProperties.SaveMouseY;
        }

        public void SetPlayerItemUsesThisAnimationViaReflection(Player player, int setUses)
        {
            itemUsesThisAnimationProperty ??= typeof(Player).GetProperty("ItemUsesThisAnimation");
            itemUsesThisAnimationProperty?.SetValue(player, setUses, null);
        }
        private static PropertyInfo itemUsesThisAnimationProperty;

        public SpriteEffects playerEffect;
        public SpriteEffects itemEffect;

        public void HijackItemDrawing(ref PlayerDrawSet drawInfo, bool green)
        {
            if (green)
            {
                ConvertItemTextureToSolid(drawInfo.heldItem);
                try
                {
                    Draw27_HeldItem(ref drawInfo, new Vector2(1, 0));
                    Draw27_HeldItem(ref drawInfo, new Vector2(-1, 0));
                    Draw27_HeldItem(ref drawInfo, new Vector2(0, 1));
                    Draw27_HeldItem(ref drawInfo, new Vector2(0, -1));
                }
                finally
                {
                    ConvertItemTextureBackToNormal(drawInfo.heldItem);
                }
            }
            else
            {
                Draw27_HeldItem(ref drawInfo, Vector2.Zero);
            }
        }

        public void Draw27_HeldItem(ref PlayerDrawSet drawInfo, Vector2 offset)
        {
            Player player = drawInfo.drawPlayer;
            Vector2 savePlayerPos = player.itemLocation;
            Vector2 saveDrawinfoPos = drawInfo.ItemLocation;

            player.itemLocation += offset;
            drawInfo.ItemLocation += offset;

            PWingFakeItem.overrideLightColor = true;
            PWingFakeItem.runOnce = true;
            PlayerDrawLayers.DrawPlayer_27_HeldItem(ref drawInfo);
            PWingFakeItem.overrideLightColor = false;

            player.itemLocation = savePlayerPos;
            drawInfo.ItemLocation = saveDrawinfoPos;
        }

        public bool PrepareDrawing(ref PlayerDrawSet drawInfo, Player player, int DrawState)
        {
            if (bodyFrame.IsEmpty)
            {
                return false;
            }
            PWingFakePlayerProjectile.OwnerOfThisDrawCycle = FakePlayerID;
            SaveRealPlayerValues(player);
            CopyFakeToReal(player);
            try
            {
                PWingFakePlayerDrawing.SetupCompositeDrawing(ref drawInfo, this, player);
                drawInfo.shadow = 0f;
                drawInfo.heldItem = heldItem;
                drawInfo.ItemLocation = itemLocation;
                drawInfo.playerEffect = playerEffect;
                drawInfo.itemEffect = itemEffect;
                drawInfo.Position = Position;

                SetupSpriteDirection(ref drawInfo, player);

                drawInfo.compFrontArmFrame = this.compFrontArmFrame;
                drawInfo.compBackArmFrame = this.compBackArmFrame;
                drawInfo.compositeFrontArmRotation = this.compositeFrontArmRotation;
                drawInfo.compositeBackArmRotation = this.compositeBackArmRotation;
                drawInfo.projectileDrawPosition = this.projectileDrawPosition;
                drawInfo.heldProjOverHand = this.heldProjOverHand;
                drawInfo.bodyVect = this.bodyVect;

                if (!SkipDrawing)
                {
                    if (DrawState == PWingDrawStateID.All || DrawState == PWingDrawStateID.Border)
                    {
                        PWingFakePlayerDrawing.DrawTail(this, ref drawInfo, true);
                        HijackItemDrawing(ref drawInfo, true);
                        PWingFakePlayerDrawing.DrawBackArm(this, ref drawInfo, true);
                        PWingFakePlayerDrawing.DrawBody(this, ref drawInfo, true);
                        PWingFakePlayerDrawing.DrawFrontArm(this, ref drawInfo, true);
                        PWingFakePlayerDrawing.DrawWings(this, ref drawInfo, WingFrame, true);
                    }
                    if (DrawState == PWingDrawStateID.All || DrawState == PWingDrawStateID.Wings)
                    {
                        PWingFakePlayerDrawing.DrawWings(this, ref drawInfo, WingFrame, false);
                    }
                    if (DrawState == PWingDrawStateID.All || DrawState == PWingDrawStateID.HeldItemAndProjectilesBeforeBackArm)
                    {
                        if (weaponDrawOrder == 0)
                            HijackItemDrawing(ref drawInfo, false);
                    }
                    if (DrawState == PWingDrawStateID.All || DrawState == PWingDrawStateID.Body)
                    {
                        PWingFakePlayerDrawing.DrawBackArm(this, ref drawInfo, false);
                        PWingFakePlayerDrawing.DrawTail(this, ref drawInfo, false);
                        PWingFakePlayerDrawing.DrawBody(this, ref drawInfo, false);
                    }
                    if (DrawState == PWingDrawStateID.All || DrawState == PWingDrawStateID.HeldItemAndProjectilesBeforeFrontArm)
                    {
                        if (weaponDrawOrder == 1)
                            HijackItemDrawing(ref drawInfo, false);
                    }
                    if (DrawState == PWingDrawStateID.All || DrawState == PWingDrawStateID.FrontArm)
                    {
                        if (player.heldProj == -1 || heldProjOverHand)
                        {
                            PWingFakePlayerDrawing.DrawFrontArm(this, ref drawInfo, false);
                        }
                    }
                    if (DrawState == PWingDrawStateID.All || DrawState == PWingDrawStateID.HeldItemAndProjectilesAfterFrontArm)
                    {
                        if (weaponDrawOrder == 2)
                            HijackItemDrawing(ref drawInfo, false);
                    }
                }

                compFrontArmFrame = drawInfo.compFrontArmFrame;
                compBackArmFrame = drawInfo.compBackArmFrame;
                compositeFrontArmRotation = drawInfo.compositeFrontArmRotation;
                compositeBackArmRotation = drawInfo.compositeBackArmRotation;
                projectileDrawPosition = drawInfo.projectileDrawPosition;
                heldProjOverHand = drawInfo.heldProjOverHand;
                bodyVect = drawInfo.bodyVect;
                itemLocation = drawInfo.ItemLocation;
                heldItem = drawInfo.heldItem;
                playerEffect = drawInfo.playerEffect;
                itemEffect = drawInfo.itemEffect;
                Position = drawInfo.Position;

                if (DrawState == PWingDrawStateID.All || DrawState == PWingDrawStateID.Border)
                {
                    DrawMyProjectiles(player);
                }
                return true;
            }
            finally
            {
                CopyRealToFake(player);
                LoadRealPlayerValues(player);
                PWingFakePlayerProjectile.OwnerOfThisDrawCycle = -1;
            }
        }

        public void DrawFrontHandAndHeldProj(SpriteBatch spriteBatch, Player player, int DrawState)
        {
            if (HeldProj != -1)
            {
                SaveRealPlayerValues(player);
                CopyFakeToReal(player);
                try
                {
                    if (!heldProjOverHand)
                    {
                        if (DrawState == PWingDrawStateID.All || DrawState == PWingDrawStateID.HeldItemAndProjectilesBeforeFrontArm)
                        {
                            DrawMyHeldProjectile(player);
                        }
                    }
                    else
                    {
                        if (DrawState == PWingDrawStateID.All || DrawState == PWingDrawStateID.HeldItemAndProjectilesAfterFrontArm)
                        {
                            DrawMyHeldProjectile(player);
                        }
                    }
                    if (!heldProjOverHand && (DrawState == PWingDrawStateID.All || DrawState == PWingDrawStateID.FrontArm))
                    {
                        PWingFakePlayerDrawing.DrawFrontArm(this, spriteBatch);
                    }
                }
                finally
                {
                    CopyRealToFake(player);
                    LoadRealPlayerValues(player);
                }
            }
        }

        public Texture2D saveNormalTexture;
        // 单色描边贴图按 (物品, 分身类型, 槽位) 缓存，避免每次换武器都新建 GPU 贴图
        private static readonly Dictionary<(int, int, int), Texture2D> solidTextureCache = new();
        private static readonly Dictionary<Type, FieldInfo> ownValueFieldCache = new();

        public void ConvertItemTextureToSolid(Item item)
        {
            saveNormalTexture = TextureAssets.Item[item.type].Value;
            int slotKey = FakePlayerType == PWingFakePlayerTypeID.Tesseract ? OverrideUseSlot % 10 : 0;
            var key = (item.type, FakePlayerType, slotKey);
            if (!solidTextureCache.TryGetValue(key, out Texture2D solidTexture) || solidTexture == null || solidTexture.IsDisposed)
            {
                Texture2D itemTexture = TextureAssets.Item[item.type].Value;
                Color[] colors = ColorHelper.ConvertToSingleColor(itemTexture, MyBorderColor());
                solidTexture = new Texture2D(Main.graphics.GraphicsDevice, itemTexture.Width, itemTexture.Height);
                solidTexture.SetData(0, null, colors, 0, itemTexture.Width * itemTexture.Height);
                solidTextureCache[key] = solidTexture;
            }
            SetTextureValueViaReflection(TextureAssets.Item[item.type], solidTexture);
        }

        public Color MyBorderColor()
        {
            if (FakePlayerType == PWingFakePlayerTypeID.Subspace)
            {
                return new Color(0, 255, 0);
            }
            if (FakePlayerType == PWingFakePlayerTypeID.Hydro)
            {
                return new Color(255, 255, 0);
            }
            if (FakePlayerType == PWingFakePlayerTypeID.Tesseract)
            {
                return ColorHelper.TesseractColor(MathHelper.TwoPi * (OverrideUseSlot % 10) / 10f, 0.4f);
            }
            return new Color(255, 255, 255);
        }

        public void ConvertItemTextureBackToNormal(Item item)
        {
            SetTextureValueViaReflection(TextureAssets.Item[item.type], saveNormalTexture);
        }

        public void SetTextureValueViaReflection(object texture, Texture2D newValue)
        {
            Type type = texture.GetType();
            if (!ownValueFieldCache.TryGetValue(type, out FieldInfo fieldInfo))
            {
                fieldInfo = type.GetField("ownValue", BindingFlags.NonPublic | BindingFlags.Instance);
                ownValueFieldCache[type] = fieldInfo;
            }
            if (fieldInfo == null)
                return;
            fieldInfo.SetValue(texture, newValue);
        }

        //释放缓存贴图，供模组卸载时调用
        public static void ClearTextureCache()
        {
            foreach (Texture2D texture in solidTextureCache.Values)
            {
                if (texture != null && !texture.IsDisposed)
                    texture.Dispose();
            }
            solidTextureCache.Clear();
            ownValueFieldCache.Clear();
        }

        public void SetupSpriteDirection(ref PlayerDrawSet drawInfo, Player player)
        {
            drawInfo.playerEffect = SpriteEffects.None;
            drawInfo.itemEffect = SpriteEffects.FlipHorizontally;
            if (player.gravDir == 1f)
            {
                if (player.direction == 1)
                {
                    drawInfo.playerEffect = SpriteEffects.None;
                    drawInfo.itemEffect = SpriteEffects.None;
                }
                else
                {
                    drawInfo.playerEffect = SpriteEffects.FlipHorizontally;
                    drawInfo.itemEffect = SpriteEffects.FlipHorizontally;
                }
            }
            else
            {
                if (player.direction == 1)
                {
                    drawInfo.playerEffect = SpriteEffects.FlipVertically;
                    drawInfo.itemEffect = SpriteEffects.FlipVertically;
                }
                else
                {
                    drawInfo.playerEffect = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
                    drawInfo.itemEffect = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
                }
            }
        }

        public void SetupBodyFrame(Player player)
        {
            Player PlayerToFrame = new Player();
            PlayerToFrame.itemAnimation = player.itemAnimation;
            PlayerToFrame.itemAnimationMax = player.itemAnimationMax;
            PlayerToFrame.selectedItem = UseItemSlot(player);
            PlayerToFrame.itemRotation = player.itemRotation;
            PlayerToFrame.direction = player.direction;
            PlayerToFrame.inventory[UseItemSlot(player)] = player.inventory[UseItemSlot(player)];
            if (CheckItemValidityFull(player, PlayerToFrame.inventory[UseItemSlot(player)], lastUsedItem, FakePlayerType) || PlayerToFrame.inventory[UseItemSlot(player)].IsAir)
                PlayerToFrame.PlayerFrame();
            bodyFrame = PlayerToFrame.bodyFrame;
            if (ShouldUseWingsArmPosition && !player.ItemAnimationActive)
            {
                bodyFrame.Y = bodyFrame.Height * 6;
            }
        }

        public void UpdateMyProjectiles(Player player)
        {
            bool hasFoundYOYO = false;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile projectile = Main.projectile[i];
                if (projectile.active)
                {
                    PWingFakePlayerProjectile fPPInstance;
                    bool canGetGlobal = projectile.TryGetGlobalProjectile(out fPPInstance);
                    if (canGetGlobal)
                    {
                        if (fPPInstance.FakeOwnerIdentity == FakePlayerID)
                        {
                            projectile.Update(i);
                            if (projectile.aiStyle == ProjAIStyleID.Yoyo && !hasFoundYOYO)
                            {
                                hasFoundYOYO = true;
                                if (player.direction == 1)
                                    player.itemRotation = MathHelper.WrapAngle((projectile.Center - player.Center).ToRotation());
                                else
                                    player.itemRotation = MathHelper.WrapAngle((player.Center - projectile.Center).ToRotation());
                            }
                        }
                    }
                }
            }
        }

        public void DrawMyProjectiles(Player player)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile projectile = Main.projectile[i];
                if (projectile.active && projectile.type > ProjectileID.None && !projectile.hide && projectile.whoAmI != HeldProj)
                {
                    PWingFakePlayerProjectile fPPInstance;
                    bool canGetGlobal = projectile.TryGetGlobalProjectile(out fPPInstance);
                    if (canGetGlobal)
                    {
                        if (fPPInstance.FakeOwnerIdentity == FakePlayerID)
                        {
                            Main.instance.DrawProj(i);
                        }
                    }
                }
            }
        }

        public void DrawMyHeldProjectile(Player player)
        {
            PWingFakePlayerProjectile.OwnerOfThisDrawCycle = FakePlayerID;
            try
            {
                DrawHeldProj(player, Main.projectile[player.heldProj]);
            }
            finally
            {
                PWingFakePlayerProjectile.OwnerOfThisDrawCycle = -1;
            }
        }

        public static void DrawHeldProj(Player player, Projectile proj)
        {
            if (!proj.active)
                return;
            float saveGFX = player.gfxOffY;
            player.gfxOffY = 0;
            proj.gfxOffY = 0;
            try
            {
                Main.instance.DrawProjDirect(proj);
            }
            catch
            {
                proj.active = false;
            }
            player.gfxOffY = saveGFX;
        }
    }

    public class PWingSavedPlayerValues
    {
        public bool saveMouseInterface;
        public bool saveCaptureManagerActive;
        public bool saveHoveringOverAnNPC;
        public bool saveSmartInteractShowingGenuine;

        public int SaveMouseX;
        public int SaveMouseY;
        public Vector2 SavePosition;
        public Vector2 SaveOldPos;
        public Vector2 SaveVelocity;
        public int SavedSelectedItem;
        public Item PlayerOwnedLastVisualizedSelectedItem;
        public bool saveChannel;
        public bool saveFrozen;
        public bool saveWebbed;
        public bool saveStoned;
        public bool saveWet;
        public bool saveMount;
        public int saveAltFunctionUse;
        public bool savePulley;
        public bool savePettingAnimal;
        public int saveHeldProj;
        public float saveStealth;
        public float saveGravDir;
        public bool saveInvis;

        public bool savecompositeFrontArmEnabled;
        public bool savecompositeBackArmEnabled;
        public int saveToolTime;
        public int saveItemAnimation;
        public int saveItemAnimationMax;
        public int saveItemTime;
        public int saveItemTimeMax;
        public float saveItemRotation;
        public Vector2 saveItemLocation;
        public int saveItemWidth;
        public int saveItemHeight;
        public int saveDirection;
        public int saveReuseDelay;
        public bool saveReleaseUseItem;
        public bool saveControlUseItem;
        public bool saveControlUseTile;
        public bool saveJustDroppedAnItem;
        public int saveAttackCD;
        public int saveItemUsesThisAnimation;
        public int saveBoneGloveTimer;
        public CompositeArmData saveFrontArm;
        public CompositeArmData saveBackArm;
        public Rectangle saveBodyFrame;
        public float saveGFXOffY;
        public bool saveSelectItemOnNextUse;
        public bool saveMale;
    }

    public class PWingFakeItem : GlobalItem
    {
        public static bool overrideLightColor = false;
        public static bool runOnce = false;

        public override Color? GetAlpha(Item item, Color lightColor)
        {
            if (overrideLightColor)
            {
                lightColor = Color.White;
                if (runOnce)
                {
                    runOnce = false;
                    Color color = item.GetAlpha(lightColor);
                    return color;
                }
            }
            return base.GetAlpha(item, lightColor);
        }

        public override bool ConsumeItem(Item item, Player player)
        {
            PWingPlayer pwPlayer = player.GetModPlayer<PWingPlayer>();
            if (pwPlayer.hasHydroFakePlayer && PWingFakePlayerProjectile.OwnerOfThisUpdateCycle == -1)
            {
                bool isHydroPlayerUsingAnItem = PWingFakePlayer.CheckItemValidityFull(player, item, item, 1);
                if (isHydroPlayerUsingAnItem)
                {
                    return false;
                }
            }
            return true;
        }

        public override void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            PWingPlayer pwPlayer = player.GetModPlayer<PWingPlayer>();
            if (pwPlayer.hasHydroFakePlayer && PWingFakePlayerProjectile.OwnerOfThisUpdateCycle == -1)
            {
                bool isHydroPlayerUsingAnItem = PWingFakePlayer.CheckItemValidityFull(player, item, item, 1);
                if (isHydroPlayerUsingAnItem)
                {
                    noHitbox = true;
                }
            }
        }

        public override void HoldStyle(Item item, Player player, Rectangle heldItemFrame)
        {
            return;
        }
    }
}