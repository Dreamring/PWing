using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System.IO;
using Terraria.DataStructures;
using Terraria.IO;
using Terraria.ModLoader.IO;

namespace PWing.Content.FakePlayer
{
    public abstract class PWingFakePlayerPossessingProjectile : ModProjectile
    {
        public Vector2 ItemLocation;
        public float ItemRotation;
        public PWingFakePlayer FakePlayer = null;
        public Vector2 cursorArea;
        public int Direction = 1;
        public int itemAnimation = 0;
        public int itemAnimationMax = 0;
        public int itemTime = 0;
        public int itemTimeMax = 0;

        public sealed override void SetStaticDefaults()
        {
            PWingFakePlayerHelper.FakePlayerPossessingProjectile.Add(Type);
            Main.projPet[Projectile.type] = true;
            SafeSetStaticDefaults();
        }

        public virtual void SafeSetStaticDefaults()
        {
        }

        public sealed override bool? CanCutTiles() => false;
        public sealed override bool MinionContactDamage() => false;
        public sealed override bool ShouldUpdatePosition() => false;

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(cursorArea.X);
            writer.Write(cursorArea.Y);
            writer.Write(ItemLocation.X);
            writer.Write(ItemLocation.Y);
            writer.Write(ItemRotation);
            writer.Write(Direction);
            writer.Write(itemAnimation);
            writer.Write(itemAnimationMax);
            writer.Write(itemTime);
            writer.Write(itemTimeMax);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            cursorArea.X = reader.ReadSingle();
            cursorArea.Y = reader.ReadSingle();
            ItemLocation.X = reader.ReadSingle();
            ItemLocation.Y = reader.ReadSingle();
            ItemRotation = reader.ReadSingle();
            Direction = reader.ReadInt32();
            itemAnimation = reader.ReadInt32();
            itemAnimationMax = reader.ReadInt32();
            itemTime = reader.ReadInt32();
            itemTimeMax = reader.ReadInt32();
        }

        public void UpdateItems(Player player)
        {
            FakePlayer.UniqueMouseX = Main.mouseX;
            FakePlayer.UniqueMouseY = Main.mouseY;

            FakePlayer.itemLocation = ItemLocation;
            FakePlayer.itemRotation = ItemRotation;
            FakePlayer.WingFrame = (int)(Projectile.ai[1] / 4);
            FakePlayer.SecondPosition = Projectile.position + new Vector2(-5 * Projectile.ai[0], 2);
            Projectile.position += Projectile.velocity;
            Projectile.Center = new Vector2(MathHelper.Clamp(Projectile.Center.X, 160f, (float)(Main.maxTilesX * 16 - 160)), MathHelper.Clamp(Projectile.Center.Y, 160f, (float)(Main.maxTilesY * 16 - 160)));
            if (FakePlayer.BonusItemAnimationTime <= 0 || FakePlayer.itemAnimation > 0)
                FakePlayer.direction = Direction;
            FakePlayer.Position = Projectile.position;
            FakePlayer.Velocity = Projectile.velocity;

            if (FakePlayer.itemTimeMax != itemTimeMax || FakePlayer.itemAnimationMax != itemAnimationMax || FakePlayer.itemTime != itemTime || (FakePlayer.itemAnimation != itemAnimation && FakePlayer.itemAnimation + 1 != itemAnimation && itemAnimation != 0))
            {
                FakePlayer.itemAnimation = itemAnimation;
                FakePlayer.itemAnimationMax = itemAnimationMax;
                FakePlayer.itemTime = itemTime;
                FakePlayer.itemTimeMax = itemTimeMax;
            }

            FakePlayer.ItemCheckHack(player);
            if (!FakePlayer.SkipDrawing)
                Direction = FakePlayer.direction;

            ItemLocation = FakePlayer.itemLocation;
            ItemRotation = FakePlayer.itemRotation;
            itemAnimation = FakePlayer.itemAnimation;
            itemAnimationMax = FakePlayer.itemAnimationMax;
            itemTime = FakePlayer.itemTime;
            itemTimeMax = FakePlayer.itemTimeMax;
            Projectile.position = FakePlayer.Position;
        }
    }

    public static class PWingFakePlayerHelper
    {
        public static HashSet<int> FakePlayerPossessingProjectile;
        public static HashSet<int> HydroPlayerItemBlacklist;
        public static HashSet<int> CloseRangeItemsForFakePlayer;
        public static HashSet<int> FakePlayerItemWhitelist;

        public static void Initialize()
        {
            FakePlayerPossessingProjectile = new HashSet<int>();
            FakePlayerItemWhitelist = new HashSet<int> {
                ItemID.LawnMower,
                ItemID.CarbonGuitar,
                ItemID.IvyGuitar,
                ItemID.DrumStick,
                ItemID.Harp,
                ItemID.Bell
            };
            HydroPlayerItemBlacklist = new HashSet<int> {
                ItemID.LawnMower
            };
            CloseRangeItemsForFakePlayer = new HashSet<int> {
                ItemID.Toxikarp,
                ItemID.SpiritFlame,
                ItemID.LawnMower,
                ItemID.FairyQueenMagicItem
            };
        }

        public static void Unload()
        {
            FakePlayerPossessingProjectile = null;
            FakePlayerItemWhitelist = null;
            HydroPlayerItemBlacklist = null;
            CloseRangeItemsForFakePlayer = null;
            PWingFakePlayerProjectile.OwnerOfThisUpdateCycle = -1;
            PWingFakePlayerProjectile.OwnerOfThisDrawCycle = -1;
            PWingFakePlayerProjectile.BeginStartNetUpdate = 0;
            PWingFakePlayer.SupressNetMessage13and41 = false;
            PWingFakePlayer.SupressSound = false;
        }
    }

    public class PWingFakePlayerProjectile : GlobalProjectile
    {
        public static int OwnerOfThisUpdateCycle = -1;
        public static int OwnerOfThisDrawCycle = -1;
        public static int BeginStartNetUpdate = 0;

        public override bool InstancePerEntity => true;
        public int FakeOwnerIdentity = -1;

        public override void SetDefaults(Projectile entity)
        {
            FakeOwnerIdentity = -1;
        }

        public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            binaryWriter.Write(FakeOwnerIdentity);
        }

        public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
        {
            FakeOwnerIdentity = binaryReader.ReadInt32();
        }

        public override void PostAI(Projectile projectile)
        {
            if (FakeOwnerIdentity == -1)
                return;
            if (BeginStartNetUpdate % 5 == 0 && BeginStartNetUpdate < 20)
            {
                projectile.netUpdate = true;
            }
            BeginStartNetUpdate++;
        }

        public void UpdateFakeOwner(Projectile projectile)
        {
            if (FakeOwnerIdentity == -1)
                return;
            projectile.ownerHitCheck = false;
            Projectile fakePlayer = FindProjectileByIdentity(FakeOwnerIdentity);
            if (fakePlayer == null)
            {
                return;
            }
            bool killMe = !PWingFakePlayerHelper.FakePlayerPossessingProjectile.Contains(fakePlayer.type) || !fakePlayer.active || fakePlayer.owner != projectile.owner;
            if (fakePlayer.ModProjectile is PWingFakePlayerPossessingProjectile fppp)
            {
                if (fppp.FakePlayer != null && fppp.FakePlayer.KillMyOwnedProjectiles)
                {
                    killMe = true;
                }
            }
            if (killMe)
            {
                FakeOwnerIdentity = -1;
                projectile.active = false;
            }
        }

        //按 identity 查找宿主弹幕，替代 LINQ 全表扫描，避免每帧产生枚举器与闭包分配
        private static Projectile FindProjectileByIdentity(int identity)
        {
            for (int i = 0; i < Main.projectile.Length; i++)
            {
                if (Main.projectile[i].identity == identity)
                {
                    return Main.projectile[i];
                }
            }
            return null;
        }

        public PWingFakePlayerPossessingProjectile WhoOwnsMe(Projectile projectile)
        {
            if (FakeOwnerIdentity == -1)
                return null;
            Projectile fakePlayer = FindProjectileByIdentity(FakeOwnerIdentity);
            if (fakePlayer == null)
                return null;
            if (PWingFakePlayerHelper.FakePlayerPossessingProjectile.Contains(fakePlayer.type) && fakePlayer.active)
            {
                if (fakePlayer.ModProjectile is PWingFakePlayerPossessingProjectile fppp)
                {
                    return fppp;
                }
            }
            return null;
        }

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (OwnerOfThisUpdateCycle == -1 || projectile.owner != Main.myPlayer || projectile.hostile)
            {
                FakeOwnerIdentity = -1;
            }
            else
            {
                FakeOwnerIdentity = OwnerOfThisUpdateCycle;
            }
        }
    }
}