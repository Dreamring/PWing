using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PWing.Content.Dusts;
using PWing.Common;
using PWing.Common.Systems;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static PWing.Common.PlayerDrawing.MachinaBooster;
using static PWing.PWing;

namespace PWing.Common.PlayerDrawing
{
    public class MachinaBooster : PlayerDrawLayer
    {
        public Texture2D[] wingAssets = null;// 用于存储翅膀相关的纹理资源
        public bool HasBladeWings(PlayerDrawSet drawInfo) => drawInfo.drawPlayer.wings == EquipLoader.GetEquipSlot(Mod, "GildedBladeWings", EquipType.Wings);
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Wings);
        // 可见性
        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            return HasBladeWings(drawInfo);
        }
        public static Color ChangeColorBasedOnStealth(Color color, PlayerDrawSet drawInfo)
        {
            // 颜色调整逻辑...
            Player drawPlayer = drawInfo.drawPlayer;
            var shadow = drawInfo.shadow;
            if (drawPlayer.inventory[drawPlayer.selectedItem].type == ItemID.PsychoKnife)
            {
                var num23 = drawPlayer.stealth;
                if (num23 < 0.03)
                    num23 = 0.03f;
                var num24 = (float)((1.0 + num23 * 10.0) / 11.0);
                if (num23 < 0.0)
                    num23 = 0.0f;
                if (num23 >= 1.0 - shadow && shadow > 0.0)
                    num23 = shadow * 0.5f;
                color = new Color((int)(byte)(color.R * num23),
                    (byte)(color.G * num23),
                    (byte)(color.B * num24),
                    (byte)(color.A * num23));
            }
            else if (drawPlayer.shroomiteStealth)
            {
                var num23 = drawPlayer.stealth;
                if (num23 < 0.03)
                    num23 = 0.03f;
                var num24 = (float)((1.0 + num23 * 10.0) / 11.0);
                if (num23 < 0.0)
                    num23 = 0.0f;
                if (num23 >= 1.0 - shadow && shadow > 0.0)
                    num23 = shadow * 0.5f;
                color = new Color((byte)(color.R * (double)num23),
                    (byte)(color.G * num23),
                    (byte)(color.B * num24),
                    (byte)(color.A * num23));
            }
            else if (drawPlayer.setVortex)
            {
                var num23 = drawPlayer.stealth;
                if (num23 < 0.03)
                    num23 = 0.03f;
                if (num23 < 0.0)
                    num23 = 0.0f;
                if (num23 >= 1.0 - shadow && shadow > 0.0)
                    num23 = shadow * 0.5f;
                Color secondColor = new Color(Vector4.Lerp(Vector4.One, new Vector4(0.0f, 0.12f, 0.16f, 0.0f), 1f - num23));
                color = color.MultiplyRGBA(secondColor);
            }
            return color;
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            if (Main.dresserInterfaceDummy == drawInfo.drawPlayer)// 如果是衣柜界面则不绘制
                return;
            if (drawInfo.drawPlayer.dead || drawInfo.drawPlayer.mount.Active)// 如果玩家死亡或骑乘也不绘制
            {
                return;
            }
            if (HasBladeWings(drawInfo))
            {
                DrawBladeWings(ref drawInfo);// 如果装备了镀金刀翼，则调用其绘制方法
            }
        }
        private void DrawBladeWings(ref PlayerDrawSet drawInfo)
        {
            bool DrawCycleIsPartOfStarlightRiverStupidPlayerTargetSystem = Main.screenPosition == Vector2.Zero;
            //if(drawInfo.shadow != 0f)
            //{
            //    return;
            //}a
            Player drawPlayer = drawInfo.drawPlayer;
            MachinaBoosterPlayer mbPlayer = drawPlayer.GetModPlayer<MachinaBoosterPlayer>();
            List<DrawData> dataTrails = new List<DrawData>();
            List<DrawData> dataBack = new List<DrawData>();
            List<DrawData> dataMiddle = new List<DrawData>();
            List<DrawData> dataFront = new List<DrawData>();
            Texture2D blade = Mod.Assets.Request<Texture2D>("Assets/Items/Wings/BladeWing", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Texture2D bladeF = Mod.Assets.Request<Texture2D>("Assets/Items/Wings/BladeWingFlipped", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Texture2D bladeOutline = Mod.Assets.Request<Texture2D>("Assets/Items/Wings/BladeWingOutline", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Texture2D bladeOutlineF = Mod.Assets.Request<Texture2D>("Assets/Items/Wings/BladeWingOutlineFlipped", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Texture2D bladeHandle = Mod.Assets.Request<Texture2D>("Assets/Items/Wings/BladeWingHandle", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Texture2D bladeHandleF = Mod.Assets.Request<Texture2D>("Assets/Items/Wings/BladeWingHandleFlipped", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Texture2D pixel = Mod.Assets.Request<Texture2D>("Assets/Items/Secrets/WhitePixel", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

            float drawX = (int)drawInfo.Position.X + drawPlayer.width / 2;
            float drawY = (int)drawInfo.Position.Y + drawPlayer.height / 2 + (drawPlayer.portableStoolInfo.IsInUse ? 14 : 0);
            drawX -= 2 * drawPlayer.direction;
            float alpha = 1 - drawInfo.shadow;
            alpha *= (255 - drawPlayer.immuneAlpha) / 255f;
            Color color = Color.White.MultiplyRGBA(Lighting.GetColor((int)drawX / 16, (int)drawY / 16)); //apply lighting to wings
            color = ChangeColorBasedOnStealth(color, drawInfo);
            //Rectangle frame = new Rectangle(0, Main.wingsTexture[drawPlayer.wings].Height / 4 * drawPlayer.wingFrame, Main.wingsTexture[drawPlayer.wings].Width, Main.wingsTexture[drawPlayer.wings].Height / 4);
            float rotation = drawPlayer.bodyRotation;
            SpriteEffects spriteEffects = drawInfo.playerEffect;
            Vector2 bladeOrigin = blade.Size() / 2;
            float counter = mbPlayer.FlightCounter + 40;
            int bladeID = drawPlayer.direction == 1 ? 9 : 0;
            for (int i = -1; i <= 1; i += 2)
            {
                bool Front = i * drawPlayer.gravDir == 1;
                float direction = drawPlayer.direction * i;
                for (int j = -4; j <= 4; j++)
                {
                    Vector2 position = new Vector2(drawX, drawY) - Main.screenPosition;
                    int proper = j * i * (int)drawPlayer.direction;
                    float scale = (Front ? 0.7f : 0.625f) + 0.05f * j;
                    float bonusDist = 0;
                    if (Math.Abs(j) % 2 == 1)
                    {
                        scale *= 0.75f;
                        bonusDist = 20;
                    }

                    //Creative Flight
                    float sinusoid = MathF.Sin(MathHelper.ToRadians(counter * 2 + 10 * j)) * 5.5f * direction;
                    Vector2 creativeOffset = new Vector2(-58 * direction, 0).RotatedBy(MathHelper.ToRadians(proper * 18.5f + sinusoid + 0.25f * direction)) * scale;
                    float creativeRotation = -MathHelper.ToRadians(46 * direction - 21 * proper - sinusoid);
                    creativeOffset.Y += 2;

                    //Normal Flight
                    sinusoid = MathF.Sin(MathHelper.ToRadians(counter * 2 + 12 * j));
                    sinusoid *= (34 - j) * direction;
                    sinusoid += 12 * direction;
                    Vector2 normalOffset = new Vector2(-(54 + bonusDist) * direction, 4).RotatedBy(MathHelper.ToRadians(proper * 6.5f + sinusoid)) * scale;
                    float normalRotation = -MathHelper.ToRadians(76 * direction - 13 * proper - sinusoid);

                    //Grounded
                    proper -= 6 * (int)direction;
                    sinusoid = MathF.Sin(MathHelper.ToRadians(PWingWorld.GlobalCounter * 2 + 12 * j)) * (16 - j) * direction;
                    Vector2 groundedOffset = new Vector2(-(64 + bonusDist * scale) * direction, 4).RotatedBy(MathHelper.ToRadians(proper * 6 + sinusoid * 0.2f)) * scale;
                    groundedOffset.X *= 0.4f;
                    if (!Front)
                    {
                        groundedOffset.Y *= 1.1f;
                    }
                    float groundedRotation = -MathHelper.ToRadians(72 * direction - 6 * proper - sinusoid);
                    float groundedScale = scale * 0.75f;

                    Vector2 finalOffset;
                    float finalRotation;
                    float finalScale;

                    if (mbPlayer.FlightModeFloat > 1)
                    {
                        float lerpAmt = mbPlayer.FlightModeFloat - 1;
                        finalOffset = Vector2.Lerp(normalOffset, creativeOffset, lerpAmt);
                        finalRotation = MathHelper.Lerp(normalRotation, creativeRotation, lerpAmt);
                        finalScale = scale;
                        position.Y += 6 * lerpAmt * drawPlayer.gravDir;
                        finalScale += lerpAmt * 0.05f;
                    }
                    else
                    {
                        float lerpAmt = mbPlayer.FlightModeFloat;
                        finalOffset = Vector2.Lerp(groundedOffset, normalOffset, lerpAmt);
                        finalRotation = MathHelper.Lerp(groundedRotation, normalRotation, lerpAmt);
                        finalScale = MathHelper.Lerp(groundedScale, scale, lerpAmt);
                        position.Y -= 24 * (1 - lerpAmt) * drawPlayer.gravDir;
                    }
                    Color finalColor1 = Color.Lerp(Color.Black, ColorHelper.Pastel(MathHelper.ToRadians(PWingWorld.GlobalCounter + j * 20), true), 0.85f);
                    Color finalColor2 = finalColor1;
                    finalColor2.A = 0;
                    Vector2 bladePosition = position + finalOffset * drawPlayer.gravDir;
                    dataBack.Add(new DrawData(Front ? blade : bladeF, bladePosition, null, finalColor1 * alpha * alpha * 0.8f, rotation + finalRotation, bladeOrigin, finalScale, spriteEffects, 0));
                    dataMiddle.Add(new DrawData(Front ? bladeOutline : bladeOutlineF, bladePosition, null, finalColor2 * alpha * alpha * 1.2f, rotation + finalRotation, bladeOrigin, finalScale, spriteEffects, 0));
                    dataFront.Add(new DrawData(Front ? bladeHandle : bladeHandleF, bladePosition, null, color * alpha, rotation + finalRotation, bladeOrigin, finalScale, spriteEffects, 0));

                    if (!Main.gamePaused && !Main.gameMenu && !DrawCycleIsPartOfStarlightRiverStupidPlayerTargetSystem)
                    {
                        mbPlayer.WingsBeingVisualized = true;
                        if (drawInfo.shadow == 0f && mbPlayer.BladeWingTrails != null && mbPlayer.BladeWingTrails[bladeID] != null) // Add dust to end of blades
                        {
                            Vector2 offset = ((-bladeOrigin + new Vector2(2, 2)) * finalScale * drawPlayer.gravDir).RotatedBy(rotation + finalRotation + (direction == -1 ? MathHelper.ToRadians(73) : 0));
                            Vector2 dustPosition = bladePosition + offset + Main.screenPosition;
                            mbPlayer.BladeWingTrails[bladeID].Insert(0, dustPosition);
                        }
                    }
                    bladeID++;
                    bladeID %= 18;
                }
            }
            if (mbPlayer.BladeWingTrails != null && !Main.gameMenu && drawInfo.shadow == 0f)
            {
                Vector2 trailOrigin = new Vector2(0, 1);
                float trailSpriteLength = pixel.Width;
                for (int i = 0; i < mbPlayer.BladeWingTrails.Length; i++)
                {
                    int bladeNum = i % 9;
                    float scaleF = 0.5f + bladeNum * 0.0625f;
                    List<Vector2> list = mbPlayer.BladeWingTrails[i];
                    if (list.Count > 0)
                    {
                        Vector2 previous = list[0];
                        for (int j = 1; j < list.Count; j++)
                        {
                            float alpha2 = 1 - (j / (float)list.Count);
                            Vector2 current = list[j];
                            Vector2 toPrevious = previous - current;
                            if (toPrevious.LengthSquared() < 1000000) //Length squared is a faster calculation than length. sqrt 1000000 = 1000
                            {
                                Color finalColor1 = Color.Lerp(Color.Black, ColorHelper.Pastel(MathHelper.ToRadians(PWingWorld.GlobalCounter + (bladeNum - 4) * 20 + j * 6), true), 0.85f);
                                finalColor1.A = 0;
                                dataTrails.Add(new DrawData(pixel, current - Main.screenPosition, null, finalColor1 * alpha * alpha2 * scaleF * 0.8f, toPrevious.ToRotation(), trailOrigin, new Vector2(toPrevious.Length() / trailSpriteLength, 0.5f * scaleF + 1f * alpha2), SpriteEffects.None, 0));
                                previous = current;
                            }
                            else
                                break;
                        }
                    }
                }
            }
            for (int i = 0; i < dataTrails.Count; i++)
            {
                DrawData data = dataTrails[i];
                data.shader = drawInfo.cWings;
                drawInfo.DrawDataCache.Add(data);
            }
            for (int i = 0; i < dataBack.Count; i++)
            {
                DrawData data = dataBack[i];
                data.shader = drawInfo.cWings;
                drawInfo.DrawDataCache.Add(data);
                data = dataMiddle[i];
                data.shader = drawInfo.cWings;
                drawInfo.DrawDataCache.Add(data);
                if (dataFront.Count > i)
                {
                    data = dataFront[i];
                    data.shader = drawInfo.cWings;
                    drawInfo.DrawDataCache.Add(data);
                }
            }
        }
        // MachinaBoosterPlayer 是 ModPlayer 的子类，用于管理玩家与“Machina Booster 翅膀”相关的逻辑（飞行状态、动画、网络同步等）
        public class MachinaBoosterPlayer : ModPlayer
        {
            // 发送网络包以同步创意飞行状态（creativeFlight 和 SlowFlight）
            public void SendPacket()
            {
                var packet = Mod.GetPacket(); // 创建 Mod 的网络包
                packet.Write((byte)PWingMessageType.SyncCreativeFlight); // 写入消息类型
                packet.Write((byte)Player.whoAmI); // 写入玩家 ID
                packet.Send(); // 发送包到服务器/客户端
                netUpdate = false; // 标记已同步
            }
            // 翅膀的默认飞行速度（7f）
            public float wingSpeed = 7f;



            // 翅膀类型（默认或 Blocky）
            public int epicWingType = 0;
            // 是否激活陀螺仪效果
            public bool gyro = false;
            // 是否需要网络更新
            public bool netUpdate = false;
            // 动画过渡浮点值（用于翅膀动画的平滑切换）
            public float FlightModeFloat = 0f;
            // 动画计数器（控制翅膀旋转和位置偏移）
            public float FlightCounter = 0f;
            // 用于修复斜坡移动时的动画问题
            //public int PlayerWasLastOnASlope = 0;

            // 翅膀类型枚举
            public enum EpicWingType : int
            {
                Default = 0,
                Blocky = 1,
            }

            // 粒子爆炸效果（用于飞行特效）
            public void DustExplosion()
            {
                for (int i = 0; i < 360; i += 4)
                {
                    
                        if (epicWingType == 0)
                        {
                            int index = Dust.NewDust(Player.Center + new Vector2(-4, -4), 0, 0, ModContent.DustType<CopyDust>(), 0, 0, 0, Color.White);
                            Dust dust = Main.dust[index];
                            dust.noGravity = true;
                            dust.fadeIn = 0.1f;
                            dust.velocity *= 0.8f;
                            dust.scale += 1.5f;
                            dust.velocity += new Vector2(12, 0).RotatedBy(MathHelper.ToRadians(i));
                            dust.shader = GameShaders.Armor.GetSecondaryShader(Player.cWings, Player);
                        }
                        else
                        {
                            int index = Dust.NewDust(Player.Center + new Vector2(-5, -5), 0, 0, ModContent.DustType<CopyDust>());
                            Dust dust = Main.dust[index];
                            dust.noGravity = true;
                            dust.fadeIn = 0.5f;
                            dust.velocity *= 0.8f;
                            dust.scale = 1.5f;
                            dust.velocity += new Vector2(12, 0).RotatedBy(MathHelper.ToRadians(i));
                            dust.shader = GameShaders.Armor.GetSecondaryShader(Player.cWings, Player);
                        
                    }
                }
                Terraria.Audio.SoundEngine.PlaySound(SoundID.MenuTick, Player.Center); // 播放音效
            }

            // 移动状态标志（水平/垂直方向）
            // bool movingHori = false;
            // bool movingVert = false;


            // 检测玩家下方是否有实体方块（用于判断是否站在地面上）
            public bool TilesBelow()
            {
                // 检查多个位置的 Tile 状态
                int i = (int)Player.Center.X / 16;
                int i2 = (int)(Player.Center.X + 10) / 16;
                int i3 = (int)(Player.Center.X - 10) / 16;
                int j = (int)(Player.position.Y + Player.height + 1) / 16;
                int j2 = (int)(Player.position.Y + Player.height + 15) / 16;
                Tile tile = Framing.GetTileSafely(i, j);
                Tile tile2 = Framing.GetTileSafely(i, j2);
                Tile tile3 = Framing.GetTileSafely(i2, j);
                Tile tile4 = Framing.GetTileSafely(i2, j2);
                Tile tile5 = Framing.GetTileSafely(i3, j);
                Tile tile6 = Framing.GetTileSafely(i3, j2);
                return (tile2 == tile && tile.HasTile && !tile.IsActuated && (Main.tileSolid[tile.TileType] || Main.tileTable[tile.TileType])) ||
                       (tile3 == tile4 && tile4.HasTile && !tile4.IsActuated && (Main.tileSolid[tile4.TileType] || Main.tileTable[tile4.TileType])) ||
                       (tile5 == tile6 && tile6.HasTile && !tile6.IsActuated && (Main.tileSolid[tile6.TileType] || Main.tileTable[tile6.TileType]));
            }

            // 粒子追踪系统相关变量
            int dustIter = 0;
            int[] dustID = new int[180];
            int[] dustOwnerID = new int[180];
            Vector2[] dustPos = new Vector2[180];
            // private bool AutoCancelFlight = false;

            // 生成环绕玩家的光环粒子
            public void HaloDust()
            {
                

                for (int i = 0; i < 3; i++)
                {
                    dustIter++;
                    Vector2 center = new Vector2(Player.Center.X - 12 * Player.direction, Player.position.Y - 4);
                    Vector2 circularLocation = new Vector2(10, 0).RotatedBy(MathHelper.ToRadians(randCounter * 6 + i * 120));
                    Vector2 circularLocation2 = new Vector2(circularLocation.X / 2, circularLocation.Y).RotatedBy(MathHelper.ToRadians(45 * Player.direction));
                    Vector2 loc = center + circularLocation2;
                    int index = Dust.NewDust(loc + new Vector2(-4, -4), 0, 0, ModContent.DustType<CopyDust2>(), 0, 0, 0, new Color(255, 255, 255));
                    Dust dust = Main.dust[index];
                    dust.noGravity = true;
                    dust.velocity = Player.velocity;
                    dust.scale = 0.65f;
                    dustPos[dustIter % 180] = dust.position - Player.Center;
                    dustID[dustIter % 180] = index;
                    dustOwnerID[dustIter % 180] = Player.whoAmI;
                    dust.shader = GameShaders.Armor.GetSecondaryShader(Player.cWings, Player);
                    dust.alpha = (int)(255 - (255 * Player.stealth));
                }

                for (int i = 0; i < (dustIter > 180 ? 180 : dustIter); i++)
                {
                    if (dustID[i] != -1 && dustOwnerID[i] != -1)
                    {
                        Player owner = Main.player[dustOwnerID[i]];
                        Dust dust = Main.dust[dustID[i]];
                        if (dust.type == ModContent.DustType<CopyDust2>() && dust.active && dustPos[i] != new Vector2(-1, -1))
                        {
                            dust.position = dustPos[i] + owner.Center;
                            if (dustPos[i].X > 0 && owner.direction == 1)
                            {
                                dust.active = false;
                                dustID[i] = -1;
                                dustOwnerID[i] = -1;
                                dustPos[i] = new Vector2(-1, -1);
                            }
                            if (dustPos[i].X < 0 && owner.direction == -1)
                            {
                                dust.active = false;
                                dustID[i] = -1;
                                dustOwnerID[i] = -1;
                                dustPos[i] = new Vector2(-1, -1);
                            }
                        }
                        else
                        {
                            dustID[i] = -1;
                            dustOwnerID[i] = -1;
                            dustPos[i] = new Vector2(-1, -1);
                        }
                    }
                }
            }

           

            // 随机计数器（用于粒子效果）
            public int randCounter = 0;
            float boost = 0f;
            // bool runOnce = true;

            // 重置效果（每次玩家更新时调用）
            public override void ResetEffects()
            {
                UpdateWingTrails(); // 更新翅膀轨迹

                if (gyro && !TilesBelow())
                {
                    if (Player.controlDown )
                    {
                        if (boost < 300)
                        {
                            boost++;
                            boost *= 1.025f;
                        }
                        else
                        {
                            boost = 300;
                        }
                        if (Player.velocity.Y != 0 && boost > 40)
                        {
                            Dust dust = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.Electric, 0, -1);
                            dust.velocity *= 0.2f;
                            dust.noGravity = true;
                        }
                    }
                    else
                    {
                        boost = 0f;
                    }
                    Player.gravity += 0.0025f * boost;
                    Player.maxFallSpeed += 0.075f * boost;
                }
                else
                {
                    boost = 0f;
                }
                gyro = false;
                randCounter++;
                
            }

            // 翅膀轨迹数据（用于绘制粒子轨迹）
            public List<Vector2>[] BladeWingTrails = null;
            public static int MaxBladeTrailLength => PWing.Config.lowFidelityMode ? 10 : 24;
            public bool WingsBeingVisualized = false;

            // 初始化翅膀轨迹列表
            private void InitWingTrails()
            {
                BladeWingTrails = new List<Vector2>[18];
                for (int i = 0; i < BladeWingTrails.Length; i++)
                {
                    BladeWingTrails[i] = new List<Vector2>();
                }
            }

            // 更新翅膀轨迹（平滑处理并限制长度）
            private void UpdateWingTrails()
            {
                if (BladeWingTrails == null)
                    InitWingTrails();
                for (int i = 0; i < BladeWingTrails.Length; i++)
                {
                    List<Vector2> list = BladeWingTrails[i];
                    for (int j = 1; j < list.Count - 1; j++)
                    {
                        Vector2 previous = list[j - 1];
                        Vector2 next = list[j + 1];
                        Vector2 supposedCenter = previous * 0.5f + next * 0.5f;
                        Vector2 me = list[j];
                        list[j] = me * 0.5f + supposedCenter * 0.5f;
                    }
                    if (list.Count > MaxBladeTrailLength || !WingsBeingVisualized)
                    {
                        bool runOnce = true;
                        while (runOnce)
                        {
                            if (list.Count > 0)
                                list.RemoveAt(list.Count - 1);
                            runOnce = list.Count > MaxBladeTrailLength;
                        }
                    }
                }
                WingsBeingVisualized = false;
            }
        }
    }
    public class Halo : PlayerDrawLayer
    {
        public bool HasBladeWings(PlayerDrawSet drawInfo) => drawInfo.drawPlayer.wings == EquipLoader.GetEquipSlot(Mod, "GildedBladeWings", EquipType.Wings);
        public override Position GetDefaultPosition() => PlayerDrawLayers.AfterLastVanillaLayer;
        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            return HasBladeWings(drawInfo);
        }
        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            if (Main.dresserInterfaceDummy == drawInfo.drawPlayer)
                return;
            if (drawInfo.drawPlayer.dead || drawInfo.drawPlayer.mount.Active)
            {
                return;
            }
            if (HasBladeWings(drawInfo))
            {
                DrawHalo(ref drawInfo);
            }
        }
        
        private void DrawHalo(ref PlayerDrawSet drawInfo)
        {
            //if (drawInfo.shadow != 0)
            //    return;
            Player drawPlayer = drawInfo.drawPlayer;
            _ = drawPlayer.GetModPlayer<MachinaBoosterPlayer>();
            List<DrawData> drawData0 = new List<DrawData>();
            List<DrawData> drawData1 = new List<DrawData>();
            List<DrawData> drawData2 = new List<DrawData>();
            Texture2D pixel = Mod.Assets.Request<Texture2D>("Assets/Items/Secrets/WhitePixel", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            int repeats = 40;
            Vector2 center = new Vector2((int)drawPlayer.Center.X, (int)drawPlayer.Center.Y + drawPlayer.gfxOffY) + new Vector2(-10 * drawPlayer.direction, -(drawPlayer.height / 2 + 6) * drawPlayer.gravDir);
            Vector2[] points = new Vector2[repeats];
            float[] scales = new float[repeats];
            Vector2[] points2 = new Vector2[repeats / 2];
            float[] scales2 = new float[repeats / 2];
            Vector2 bonusPoint = Vector2.Zero;
            float bonusScale = 0f;
            float tilt = 35f + MathF.Sin(MathHelper.ToRadians(PWingWorld.GlobalCounter * 1.5f)) * 5f;
            float zTilt = 1f + MathF.Sin(MathHelper.ToRadians(PWingWorld.GlobalCounter * 0.75f)) * 0.2f;
            for (int i = 0; i < repeats; i++)
            {
                float rotation = i / (float)repeats * MathHelper.TwoPi + MathHelper.ToRadians(PWingWorld.GlobalCounter * 0.75f);
                float offset = 11;
                int pointN = i % repeats;
                float offsetBonus = 0;
                if (pointN == 39)
                    offsetBonus += 18;
                if (pointN == 2)
                    offsetBonus += 14;
                if (pointN == 5)
                    offsetBonus += 14;
                if (pointN == 10)
                    offsetBonus += 10;
                if (pointN == 14)
                    offsetBonus += 20;
                if (pointN == 17)
                    offsetBonus += 14;
                if (pointN == 20)
                    offsetBonus += 8;
                if (pointN == 27)
                    offsetBonus += 10;
                if (pointN == 31)
                    offsetBonus += 16;
                offset += offsetBonus * (0.85f + 0.15f * MathF.Sin(MathHelper.ToRadians(PWingWorld.GlobalCounter * (2 + i / 40f) + i * 24))) * 0.95f;

                if (i % 2 == 0)
                {
                    Vector2 innerCircular = new Vector2(7, 0).RotatedBy(rotation);
                    innerCircular.Y *= 0.45f * drawPlayer.gravDir * zTilt;
                    innerCircular.X *= drawPlayer.direction;
                    innerCircular = innerCircular.RotatedBy(MathHelper.ToRadians(tilt * -drawPlayer.direction * drawPlayer.gravDir));
                    points2[i / 2] = center + innerCircular;
                    scales2[i / 2] = 0.7f + MathF.Sin(rotation) * 0.1f / zTilt;
                }

                Vector2 circular = new Vector2(offset, 0).RotatedBy(rotation);
                circular.Y *= 0.55f * drawPlayer.gravDir * zTilt;
                circular.X *= drawPlayer.direction;
                circular = circular.RotatedBy(MathHelper.ToRadians(tilt * -drawPlayer.direction * drawPlayer.gravDir));
                points[i] = center + circular;
                scales[i] = 0.8f + MathF.Sin(rotation) * 0.2f / zTilt;
                if (pointN == 37)
                {
                    Vector2 bonusPos = new Vector2(5, 0).RotatedBy(rotation);
                    bonusPos.Y *= 0.55f * drawPlayer.gravDir * zTilt;
                    bonusPos.X *= drawPlayer.direction;
                    bonusPos = bonusPos.RotatedBy(MathHelper.ToRadians(tilt * -drawPlayer.direction * drawPlayer.gravDir));
                    bonusPoint = points[i] + bonusPos;
                    bonusScale = scales[i];
                }
            }
            Vector2 previous = points[points.Length - 1];
            for (int i = 0; i < points.Length; i++)
            {
                Color color = Color.Lerp(Color.Black, ColorHelper.Pastel(MathHelper.ToRadians(PWingWorld.GlobalCounter * 2) + i / (float)repeats * MathHelper.TwoPi, true), 0.8f);
                Color color2 = color * 1.1f;
                color.A = 0;
                Vector2 current = points[i];
                Vector2 toPrevious = previous - current;
                drawData0.Add(new DrawData(pixel, points[i] - Main.screenPosition, null, color, toPrevious.ToRotation(), new Vector2(0, 1), new Vector2(toPrevious.Length() / 2, scales[i] * 0.8f), SpriteEffects.None, 0));
                drawData2.Add(new DrawData(pixel, points[i] - Main.screenPosition, null, color2, toPrevious.ToRotation(), new Vector2(0, 1), new Vector2(toPrevious.Length() / 2, scales[i]) + Vector2.One * 0.6f, SpriteEffects.None, 0));
                previous = current;
            }
            previous = points2[points2.Length - 1];
            for (int i = 0; i < points2.Length; i++)
            {
                Color color = Color.Lerp(Color.Black, ColorHelper.Pastel(MathHelper.ToRadians(PWingWorld.GlobalCounter * -1) + i / (float)repeats * MathHelper.TwoPi * 2, true), 0.8f);
                Color color2 = color * 1.1f;
                color.A = 0;
                Vector2 current = points2[i];
                Vector2 toPrevious = previous - current;
                drawData1.Add(new DrawData(pixel, points2[i] - Main.screenPosition, null, color, toPrevious.ToRotation(), new Vector2(0, 1), new Vector2(toPrevious.Length() / 2, scales2[i] * 0.8f), SpriteEffects.None, 0));
                drawData2.Add(new DrawData(pixel, points2[i] - Main.screenPosition, null, color2, toPrevious.ToRotation(), new Vector2(0, 1), new Vector2(toPrevious.Length() / 2, scales2[i]) + Vector2.One * 0.6f, SpriteEffects.None, 0));
                previous = current;
            }
            Color finalColor2 = Color.Lerp(Color.Black, ColorHelper.Pastel(MathHelper.ToRadians(PWingWorld.GlobalCounter * 2) + 37f / (float)repeats * MathHelper.TwoPi, true), 0.8f);
            Color finalColor3 = finalColor2 * 1.1f;
            finalColor2.A = 0;
            drawData0.Add(new DrawData(pixel, bonusPoint - Main.screenPosition, null, finalColor2, 0f, new Vector2(1, 1), bonusScale + 0.2f, SpriteEffects.None, 0));
            drawData2.Add(new DrawData(pixel, bonusPoint - Main.screenPosition, null, finalColor3, 0f, new Vector2(1, 1), bonusScale + 0.8f, SpriteEffects.None, 0));
            for (int i = 0; i < drawData2.Count; i++)
            {
                DrawData data = drawData2[i];
                data.shader = drawInfo.cWings;
                drawInfo.DrawDataCache.Add(data);
            }
            for (int i = 0; i < drawData1.Count; i++)
            {
                DrawData data = drawData1[i];
                data.shader = drawInfo.cWings;
                drawInfo.DrawDataCache.Add(data);
            }
            for (int i = 0; i < drawData0.Count; i++)
            {
                DrawData data = drawData0[i];
                data.shader = drawInfo.cWings;
                drawInfo.DrawDataCache.Add(data);
            }
        }
    }
}
