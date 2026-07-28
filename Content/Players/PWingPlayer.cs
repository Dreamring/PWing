using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using PWing.Content.FakePlayer;
using PWing.Content.Projectiles;
using PWing.Content.Items.Wings;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace PWing.Content.Players
{
    public class TesseractMinionData
    {
        public bool FoundValidItemLastFrame;
        public bool FoundValidItem;
        public int ChargeFrames;
        public bool AltFunctionUse;

        public TesseractMinionData()
        {
            Reset();
        }

        public void Reset()
        {
            FoundValidItem = false;
            ChargeFrames = -1;
            AltFunctionUse = false;
        }
    }

    public class PWingPlayer : ModPlayer
    {
        public List<int> DefeatedBosses { get; set; } = new List<int>();

        public int BossKillCount => DefeatedBosses.Count;

        private static TesseractMinionData[] InitializeTesseractData()
        {
            TesseractMinionData[] tesseractData = new TesseractMinionData[10];
            for (int i = 0; i < tesseractData.Length; i++)
                tesseractData[i] = new TesseractMinionData();
            return tesseractData;
        }

        public TesseractMinionData[] tesseractData = InitializeTesseractData();
        public bool foundItem = false;
        public bool servantActive = false;
        public bool servantIsVanity = false;
        public bool hasHydroFakePlayer = false;
        public bool hydroVanityHidden = false;

        public int tesseractPlayerCount => Player.ownedProjectileCounts[ModContent.ProjectileType<PWingTesseractServant>()];

        public static PWingPlayer ModPlayer(Player player)
        {
            return player.GetModPlayer<PWingPlayer>();
        }

        public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath)
        {
            if (!mediumCoreDeath)
            {
                return [
                    new Item(ModContent.ItemType<GildedBladeWings>())
                ];
            }
            return [];
        }

        public override void SaveData(TagCompound tag)
        {
            if (DefeatedBosses.Count > 0)
            {
                tag["DefeatedBosses"] = DefeatedBosses;
            }
        }

        public override void LoadData(TagCompound tag)
        {
            if (tag.ContainsKey("DefeatedBosses"))
            {
                DefeatedBosses = tag.GetList<int>("DefeatedBosses").ToList();
            }
            else
            {
                DefeatedBosses = new List<int>();
            }
        }

        public override void ResetEffects()
        {
            servantIsVanity = false;
            servantActive = false;
            hasHydroFakePlayer = false;
            foundItem = false;
            hydroVanityHidden = false;
        }

        public override void PostUpdateMiscEffects()
        {
            for (int i = 0; i < tesseractData.Length; i++)
            {
                if (i >= tesseractPlayerCount)
                {
                    tesseractData[i].Reset();
                }
                else
                {
                    tesseractData[i].FoundValidItemLastFrame = tesseractData[i].FoundValidItem;
                    tesseractData[i].FoundValidItem = false;
                }
            }
        }

        public int Probe = -1;
        public int Probe2 = -1;

        public override bool Shoot(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (PWingFakePlayerProjectile.OwnerOfThisUpdateCycle != -1)
            {
                Projectile dummyProj = new Projectile();
                dummyProj.SetDefaults(item.shoot);
                if (dummyProj.aiStyle == ProjAIStyleID.Yoyo && Player.ownedProjectileCounts[item.shoot] > 1)
                {
                    return false;
                }
            }
            else
            {
                if (hasHydroFakePlayer && PWingFakePlayerProjectile.OwnerOfThisUpdateCycle == -1)
                {
                    bool isHydroPlayerUsingAnItem = PWingFakePlayer.CheckItemValidityFull(Player, item, item, PWingFakePlayerTypeID.Hydro);
                    if (isHydroPlayerUsingAnItem)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override bool CanUseItem(Item item)
        {
            if (hasHydroFakePlayer && PWingFakePlayerProjectile.OwnerOfThisUpdateCycle == -1)
            {
                bool isHydroPlayerUsingAnItem = PWingFakePlayer.CheckItemValidityFull(Player, item, item, PWingFakePlayerTypeID.Hydro);
                if (isHydroPlayerUsingAnItem)
                {
                    Player.SetDummyItemTime(2);
                    Player.itemTimeMax--;
                    return false;
                }
            }
            return true;
        }

        public override bool? CanMeleeAttackCollideWithNPC(Item item, Rectangle meleeAttackHitbox, NPC target)
        {
            if (hasHydroFakePlayer && PWingFakePlayerProjectile.OwnerOfThisUpdateCycle == -1)
            {
                bool isHydroPlayerUsingAnItem = PWingFakePlayer.CheckItemValidityFull(Player, item, item, PWingFakePlayerTypeID.Hydro);
                if (isHydroPlayerUsingAnItem)
                {
                    return false;
                }
            }
            return null;
        }
    }
}
