using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PWing.Common;
using PWing.Common.Systems;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;
using Terraria.Localization;
namespace PWing.Content.Items.Wings
{
    //自动加载翅膀物品类型
    [AutoloadEquip(EquipType.Wings)]
    public class GildedBladeWings : ModItem
    {
        //创造飞行解锁所需击杀的BOSS数量
        public const int CreativeFlightUnlockBossKills = 5;
        //创造飞行模式下飞行性能相对正常模式的倍率
        public const float CreativeFlightPerformanceMultiplier = 0.6f;

        private int lastAppliedBossKills = -1;

        public override void SetStaticDefaults()
        {
            this.SetResearchCost(1);
            //初始设置为雏翼的基础飞行能力，并开启按住下键悬浮（同天界星盘）
            ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(60, 1.5f, 1f, true, 6f, 1.5f); //雏翼基础：1秒飞行时间
        }
        public override void SetDefaults()
        {
            //宽度，高度，价值，稀有度等
            Item.width = 38;
            Item.height = 30;
            Item.value = Item.sellPrice(0, 20, 0, 0);//使用函数定义价格
            Item.rare = ItemRarityID.Red;//稀有度——红色
            Item.expert = true;//是否为专家
            Item.accessory = true;//这时一个饰品

            // 工具提示将在ModifyTooltips中设置
        }
        
        // 设置基础工具提示
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color goldenColor = new Color(255, 215, 0);
            const string keyPrefix = "Mods.PWing.Items.GildedBladeWings.";

            // 保存物品名称提示行（名称文本由本地化 DisplayName 提供）
            TooltipLine nameLine = null;
            foreach (var line in tooltips)
            {
                if (line.Name == "ItemName")
                {
                    nameLine = line;
                    break;
                }
            }

            tooltips.Clear();

            if (nameLine != null)
            {
                //名字使用流动的彩色
                nameLine.OverrideColor = ColorHelper.Pastel(MathHelper.ToRadians(PWingWorld.GlobalCounter), true);
                tooltips.Add(nameLine);
            }

            int bossKills = 0;
            if (Main.LocalPlayer != null)
            {
                bossKills = PWingWorld.GetBossKillCount(Main.LocalPlayer);
            }

            //三段描述合并为一行
            tooltips.Add(new TooltipLine(Mod, "Lore", Language.GetTextValue(keyPrefix + "Lore")));

            //创造飞行解锁状态提示
            bool creativeFlightUnlocked = bossKills >= CreativeFlightUnlockBossKills;
            TooltipLine creativeFlightLine = new TooltipLine(Mod, "CreativeFlight",
                Language.GetTextValue(keyPrefix + (creativeFlightUnlocked ? "CreativeFlightUnlocked" : "CreativeFlightLocked")));
            if (creativeFlightUnlocked)
            {
                creativeFlightLine.OverrideColor = ColorHelper.Pastel(MathHelper.ToRadians(PWingWorld.GlobalCounter * 2), true);
            }
            else
            {
                creativeFlightLine.OverrideColor = new Color(150, 150, 150);
            }
            tooltips.Add(creativeFlightLine);

            tooltips.Add(new TooltipLine(Mod, "EmptyLine1", ""));

            //未按住Shift时折叠属性区，仅显示展开提示
            bool shiftHeld = Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift)
                || Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightShift);
            if (!shiftHeld)
            {
                TooltipLine expandHintLine = new TooltipLine(Mod, "ExpandHint", Language.GetTextValue(keyPrefix + "ExpandHint"));
                expandHintLine.OverrideColor = new Color(180, 180, 180);
                tooltips.Add(expandHintLine);
                return;
            }

            float baseFlightTime = 1f;
            float currentFlightTime = baseFlightTime + bossKills * 0.5f;
            float baseFlightSpeed = 1.5f;
            float currentFlightSpeed = baseFlightSpeed + bossKills * 0.1f;

            TooltipLine flightTimeLine = new TooltipLine(Mod, "FlightTime", Language.GetTextValue(keyPrefix + "FlightTime", currentFlightTime.ToString("F1")));
            flightTimeLine.OverrideColor = new Color(135, 206, 250);
            tooltips.Add(flightTimeLine);

            TooltipLine flightSpeedLine = new TooltipLine(Mod, "FlightSpeed", Language.GetTextValue(keyPrefix + "FlightSpeed", currentFlightSpeed.ToString("F1")));
            flightSpeedLine.OverrideColor = new Color(144, 238, 144);
            tooltips.Add(flightSpeedLine);

            TooltipLine bossKillsLine = new TooltipLine(Mod, "BossKills", Language.GetTextValue(keyPrefix + "BossKills", bossKills));
            bossKillsLine.OverrideColor = goldenColor;
            tooltips.Add(bossKillsLine);

            tooltips.Add(new TooltipLine(Mod, "EmptyLine2", ""));

            // 以下数值必须与 UpdateAccessory 中的实际效果保持一致
            int lifeBonus = bossKills * 5;
            float enduranceBonus = bossKills * 0.5f;
            int defenseBonus = bossKills;
            float attackBonusPercent = bossKills * 20f;
            int lifeRegenBonus = (int)(bossKills * 0.1f * 60);
            int manaBonus = bossKills * 5;
            int extraSlots = bossKills / 5;

            TooltipLine lifeBonusLine = new TooltipLine(Mod, "LifeBonus", Language.GetTextValue(keyPrefix + "LifeBonus", lifeBonus));
            lifeBonusLine.OverrideColor = new Color(255, 99, 71);
            tooltips.Add(lifeBonusLine);

            TooltipLine enduranceBonusLine = new TooltipLine(Mod, "EnduranceBonus", Language.GetTextValue(keyPrefix + "EnduranceBonus", enduranceBonus.ToString("F1")));
            enduranceBonusLine.OverrideColor = new Color(255, 165, 0);
            tooltips.Add(enduranceBonusLine);

            TooltipLine defenseBonusLine = new TooltipLine(Mod, "DefenseBonus", Language.GetTextValue(keyPrefix + "DefenseBonus", defenseBonus));
            defenseBonusLine.OverrideColor = new Color(50, 205, 50);
            tooltips.Add(defenseBonusLine);

            TooltipLine attackBonusLine = new TooltipLine(Mod, "AttackBonus", Language.GetTextValue(keyPrefix + "AttackBonus", attackBonusPercent.ToString("F1")));
            attackBonusLine.OverrideColor = new Color(0, 191, 255);
            tooltips.Add(attackBonusLine);

            TooltipLine lifeRegenBonusLine = new TooltipLine(Mod, "LifeRegenBonus", Language.GetTextValue(keyPrefix + "LifeRegenBonus", lifeRegenBonus));
            lifeRegenBonusLine.OverrideColor = new Color(255, 182, 193);
            tooltips.Add(lifeRegenBonusLine);

            TooltipLine manaBonusLine = new TooltipLine(Mod, "ManaBonus", Language.GetTextValue(keyPrefix + "ManaBonus", manaBonus));
            manaBonusLine.OverrideColor = new Color(148, 0, 211);
            tooltips.Add(manaBonusLine);

            if (extraSlots > 0)
            {
                TooltipLine slotBonusLine = new TooltipLine(Mod, "SlotBonus", Language.GetTextValue(keyPrefix + "SlotBonus", extraSlots));
                slotBonusLine.OverrideColor = goldenColor;
                tooltips.Add(slotBonusLine);
            }

            tooltips.Add(new TooltipLine(Mod, "EmptyLine3", ""));

            if (bossKills == 0)
            {
                TooltipLine growthHintLine = new TooltipLine(Mod, "GrowthHint", Language.GetTextValue(keyPrefix + "GrowthHint"));
                growthHintLine.OverrideColor = new Color(255, 165, 0);
                tooltips.Add(growthHintLine);
            }
            else
            {
                float nextFlightTime = currentFlightTime + 0.5f;
                float nextFlightSpeed = currentFlightSpeed + 0.1f;
                TooltipLine nextGrowthLine = new TooltipLine(Mod, "NextGrowth", Language.GetTextValue(keyPrefix + "NextGrowth", nextFlightTime.ToString("F1"), nextFlightSpeed.ToString("F1")));
                nextGrowthLine.OverrideColor = new Color(255, 182, 193);
                tooltips.Add(nextGrowthLine);
            }
        }
        public override bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player)
        {
            //翅膀同时装备
            return true;
        }
        // 添加配方，将雏翼丢入微光转化为成长翅膀
        public override void AddRecipes()
        {
            // 创建工作台合成配方
            CreateRecipe(1)
                .AddIngredient(ItemID.IronBar, 20) // 20个铁锭
                .AddIngredient(ItemID.Star, 20) // 20个落星
                .AddTile(TileID.WorkBenches) // 工作台
                .Register();
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            GildedBladeWingsPlayer wingPlayer = player.GetModPlayer<GildedBladeWingsPlayer>();

            //装备在饰品栏中时，右键点击翅膀切换创造飞行模式
            if (player.whoAmI == Main.myPlayer && Main.mouseRight && Main.mouseRightRelease && Main.HoverItem.type == Item.type)
            {
                if (PWingWorld.GetBossKillCount(player) >= CreativeFlightUnlockBossKills)
                {
                    wingPlayer.CreativeFlight = !wingPlayer.CreativeFlight;
                    //关闭创造飞行时恢复翅膀飞行时间，避免空中切换后直接坠落
                    if (!wingPlayer.CreativeFlight)
                    {
                        player.wingTime = player.wingTimeMax;
                    }
                }
                //消耗这次右键点击，避免同一次点击触发其他逻辑
                Main.mouseRightRelease = false;
            }

            //根据BOSS击败数量动态调整飞行能力
            int bossKills = PWingWorld.GetBossKillCount(player);
            
            //每击败一个BOSS，增加0.5秒飞行时间和少量飞行速度
            float flightTimeIncrease = bossKills * 0.5f; // 每BOSS增加0.5秒
            float speedIncrease = bossKills * 0.1f; // 每BOSS增加少量速度
            
            //计算当前飞行能力
            float baseFlightTime = 1f; // 基础1秒
            float currentFlightTime = baseFlightTime + flightTimeIncrease;
            float baseFlightSpeed = 1.5f; // 基础飞行速度
            float currentFlightSpeed = baseFlightSpeed + speedIncrease;
            float baseAcceleration = 1f; // 基础加速度
            float currentAcceleration = baseAcceleration + speedIncrease * 0.5f;
            
            //更新翅膀统计（仅在成长层数变化时写入全局表，避免每帧覆盖）
            //悬浮参数：按住下键+跳跃键悬浮时可水平移动，速度与水平飞行速度一致
            if (lastAppliedBossKills != bossKills)
            {
                lastAppliedBossKills = bossKills;
                float hoverSpeed = 6f + Math.Min(bossKills * 0.1f, 3f);
                ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats((int)(currentFlightTime * 60), currentFlightSpeed, currentAcceleration, true, hoverSpeed, 1.5f);
            }
            
            //添加新的成长属性增益
            //每击败一个BOSS，增加以下属性：
            //生命+5，全伤害+1%，暴击率+1%，防御力+1，基础攻击力+1，生命恢复+0.1，魔力上限+5
            player.statLifeMax2 += bossKills * 5; // 生命+5 per BOSS
            player.endurance += bossKills * 0.005f; // 伤害减免+0.5% per BOSS (作为全伤害的替代)
            player.statDefense += bossKills; // 防御力+1 per BOSS
            player.GetDamage(DamageClass.Generic) += bossKills * 0.2f; // 基础攻击力+0.2 per BOSS
            player.lifeRegen += (int)(bossKills * 0.1f * 60); // 生命恢复+0.1 per BOSS (转换为每分钟恢复值)
            player.statManaMax2 += bossKills * 5; // 魔力上限+5 per BOSS
            
            //每击败5个BOSS增加一个召唤栏和哨兵栏
            int extraSlots = bossKills / 5;
            player.maxMinions += extraSlots; // 增加召唤栏
            player.maxTurrets += extraSlots; // 增加哨兵栏
            
            // 按住上或下键时附带水平位移（类似天界星盘），创造飞行由专门的移动逻辑处理
            if (!wingPlayer.CreativeFlight && player.wingsLogic > 0 && player.controlDown != player.controlUp)
            {
                player.position.X += player.velocity.X * 0.1f;
            }
        }
        public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
        {
            int bossKills = PWingWorld.GetBossKillCount(player);
            float bonus = bossKills * 0.05f; // 每BOSS增加的额外能力
            
            // 设置上限
            float maxBonus = 2.0f; // 最大额外能力
            bonus = Math.Min(bonus, maxBonus);
            
            ascentWhenFalling = 0.85f + bonus; // 下落时的上升速度
            ascentWhenRising = 0.15f + bonus * 0.5f; // 上升时的上升速度
            maxCanAscendMultiplier = 1.0f + bonus; // 最大可上升乘数
            maxAscentMultiplier = 2.0f + bonus * 2; // 最大上升乘数
            constantAscend = 0.1f + bonus * 0.5f; // 恒定上升速度
        }
        
        public override void HorizontalWingSpeeds(Player player, ref float speed, ref float acceleration)
        {
            int bossKills = PWingWorld.GetBossKillCount(player);
            float bonus = bossKills * 0.1f; // 每BOSS增加的额外水平速度
            
            // 设置上限
            float maxBonus = 3.0f; // 最大额外水平速度
            bonus = Math.Min(bonus, maxBonus);
            
            speed = 6f + bonus; // 基础水平速度 + 额外速度 (从8f降低到6f)
            acceleration = 1.5f + bonus * 0.5f; // 基础加速度 + 额外加速度 (从2f降低到1.5f)
        }

        //通用绘制方法
        private void DrawWing(SpriteBatch spriteBatch, Vector2 position, Vector2 origin, float scale, float rotation, Color baseColor, float alpha = 1f)
        {
            //定义颜色
            Color color = new Color(110, 110, 110, 0) * 0.25f;
            
            //根据低精度模式调整绘制逻辑
            if (PWing.Config.lowFidelityMode)
            {
                //低精度模式：简化绘制
                spriteBatch.Draw(WingTexture, position, null, baseColor * alpha, rotation, origin, scale, SpriteEffects.None, 0f);
                spriteBatch.Draw(WingGlowTexture2, position, null, ColorHelper.Pastel(MathHelper.ToRadians(PWingWorld.GlobalCounter), true) * 0.8f * alpha, rotation, origin, scale, SpriteEffects.None, 0f);
            }
            else
            {
                //高精度模式：完整绘制
                DrawHalo(spriteBatch, position, scale, rotation);
                spriteBatch.Draw(WingTexture, position, null, baseColor * alpha, rotation, origin, scale, SpriteEffects.None, 0f);
                for (int k = 0; k < 8; k++)
                {
                    Vector2 offset = new Vector2(2, 0).RotatedBy(k * MathHelper.PiOver4 + MathHelper.ToRadians(PWingWorld.GlobalCounter * 2));
                    spriteBatch.Draw(WingGlowTexture, position + offset, null, color * alpha, rotation, origin, scale, SpriteEffects.None, 0f);
                }
                spriteBatch.Draw(WingGlowTexture2, position, null, ColorHelper.Pastel(MathHelper.ToRadians(PWingWorld.GlobalCounter), true) * 1.2f * alpha, rotation, origin, scale, SpriteEffects.None, 0f);
            }
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            /*
             允许您在该物品所在的物品栏中绘制其后方的其他内容。
            返回 false 可以阻止游戏绘制该物品（如果您是手动绘制该物品则此功能很有用）。 
            请注意，position 表示的是库存槽的中心位置，而 origin 则是将要绘制的纹理框的中心位置。
            因此，所提供的参数可以传递给 SpriteBatch.DrawString（SpriteFont，string，Vector2，Color，float，Vector2，float，SpriteEffects，float）函数，以按照常规方式绘制纹理。 仅向本地客户端发出请求。 默认情况下返回值为真。
             */
            //在物品栏中绘制图标逻辑
            try
            {
                DrawWing(spriteBatch, position, origin, scale, 0f, drawColor);
            }
            catch (Exception ex)
            {
                LogDrawError("PreDrawInInventory", ex);
            }
            return false;
        }
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            //世界中绘制翅膀图标
            Vector2 drawOrigin = new Vector2(Terraria.GameContent.TextureAssets.Item[Item.type].Value.Width * 0.5f, Item.height * 0.5f);
            Vector2 drawPos = new Vector2((float)(Item.Center.X - (int)Main.screenPosition.X), (float)(Item.Center.Y - (int)Main.screenPosition.Y));
            float alpha = (1f - (Item.alpha / 255f));
            
            //使用通用绘制方法
            try
            {
                DrawWing(spriteBatch, drawPos, drawOrigin, scale, rotation, lightColor, alpha);
            }
            catch (Exception ex)
            {
                LogDrawError("PreDrawInWorld", ex);
            }
            return false;
        }

        //限流记录绘制异常，避免贴图缺失时完全静默
        private static uint lastDrawErrorTick;
        private void LogDrawError(string stage, Exception ex)
        {
            if (Main.GameUpdateCount - lastDrawErrorTick < 60)
            {
                return;
            }
            lastDrawErrorTick = Main.GameUpdateCount;
            Mod.Logger.Error("GildedBladeWings " + stage + " error: " + ex);
        }
        //纹理使用静态缓存：物品克隆实例各自缓存纹理时可能导致部分实例贴图缺失
        private static Texture2D _wingTexture;
        private static Texture2D _wingGlowTexture;
        private static Texture2D _wingGlowTexture2;
        private static Texture2D _pixelTexture;

        private static Texture2D RequestTexture(string path) => PWing.Instance.Assets.Request<Texture2D>(path, AssetRequestMode.ImmediateLoad).Value;

        private static Texture2D WingTexture => _wingTexture ??= RequestTexture("Assets/Items/Wings/GildedBladeWings");

        private static Texture2D WingGlowTexture => _wingGlowTexture ??= RequestTexture("Assets/Items/Wings/GildedBladeWingsGlow");

        private static Texture2D WingGlowTexture2 => _wingGlowTexture2 ??= RequestTexture("Assets/Items/Wings/GildedBladeWingsGlow2");

        private static Texture2D PixelTexture => _pixelTexture ??= RequestTexture("Assets/Items/Secrets/WhitePixel");
        
        private void DrawHalo(SpriteBatch spriteBatch, Vector2 position, float scale, float s_rotation)
        {
            //根据低精度模式调整绘制逻辑
            if (PWing.Config.lowFidelityMode)
            {
                //低精度模式：简化光环绘制
                int repeats = 20; // 减少重复次数
                Vector2 center = position;
                
                //预计算公共值
                float globalCounterRad = MathHelper.ToRadians(PWingWorld.GlobalCounter * 0.75f);
                
                //直接绘制，避免内存分配
                Vector2 previous = Vector2.Zero;
                for (int i = 0; i < repeats; i++)
                {
                    float rotation = i / (float)repeats * MathHelper.TwoPi + globalCounterRad + s_rotation;
                    float offset = 15;
                    Vector2 circular = new Vector2(offset * scale, 0).RotatedBy(rotation);
                    Vector2 currentPoint = center + circular;
                    
                    if (i == 0)
                    {
                        previous = currentPoint;
                        continue;
                    }
                    
                    //计算颜色
                    Color color = Color.Lerp(Color.Black, ColorHelper.Pastel(MathHelper.ToRadians(PWingWorld.GlobalCounter * 2) + i / (float)repeats * MathHelper.TwoPi, true), 0.6f);
                    color.A = 0;
                    
                    //计算线段
                    Vector2 toPrevious = previous - currentPoint;
                    float lineLength = toPrevious.Length();
                    float lineRotation = toPrevious.ToRotation();
                    
                    //直接绘制线段
                    spriteBatch.Draw(PixelTexture, currentPoint, null, color, lineRotation, new Vector2(0, 1), new Vector2(lineLength / 2, 0.5f * scale), SpriteEffects.None, 0f);
                    
                    previous = currentPoint;
                }
            }
            else
            {
                //高精度模式：完整光环绘制
                int repeats = 40;
                Vector2 center = position;
                
                //预计算公共值
                float globalCounterRad = MathHelper.ToRadians(PWingWorld.GlobalCounter * 0.75f);
                float globalCounterRad2 = MathHelper.ToRadians(PWingWorld.GlobalCounter * 2);
                float globalCounterRadNeg = MathHelper.ToRadians(PWingWorld.GlobalCounter * -1);
                
                //直接绘制，避免使用List<DrawData>
                Vector2 previous = Vector2.Zero;
                Vector2 previousInner = Vector2.Zero;
                Vector2 bonusPoint = Vector2.Zero;
                
                //绘制外圈
                for (int i = 0; i < repeats; i++)
                {
                    float rotation = i / (float)repeats * MathHelper.TwoPi + globalCounterRad + s_rotation;
                    float offset = 20;
                    int pointN = i % repeats;
                    float offsetBonus = 0;
                    
                    //快速计算offsetBonus
                    switch (pointN)
                    {
                        case 39: offsetBonus += 18; break;
                        case 2: offsetBonus += 14; break;
                        case 5: offsetBonus += 14; break;
                        case 10: offsetBonus += 10; break;
                        case 14: offsetBonus += 20; break;
                        case 17: offsetBonus += 14; break;
                        case 20: offsetBonus += 8; break;
                        case 27: offsetBonus += 10; break;
                        case 31: offsetBonus += 16; break;
                    }
                    
                    offset += offsetBonus * (0.7f + 0.3f * MathF.Sin(MathHelper.ToRadians(PWingWorld.GlobalCounter * (2 + i / 40f) + i * 24))) * 0.4f;
                    
                    Vector2 circular = new Vector2(offset * scale, 0).RotatedBy(rotation);
                    Vector2 currentPoint = center + circular;
                    
                    //处理内圈点
                    if (i % 2 == 0)
                    {
                        Vector2 innerCircular = new Vector2(16 * scale, 0).RotatedBy(rotation);
                        Vector2 innerPoint = center + innerCircular;
                        
                        if (i > 0)
                        {
                            //绘制内圈线段
                            Color innerColor = Color.Lerp(Color.Black, ColorHelper.Pastel(globalCounterRadNeg + i / (float)repeats * MathHelper.TwoPi * 2, true), 0.8f);
                            Color innerColor2 = innerColor * 1.1f;
                            innerColor.A = 0;
                            
                            Vector2 toPreviousInner = previousInner - innerPoint;
                            float innerLineLength = toPreviousInner.Length();
                            float innerLineRotation = toPreviousInner.ToRotation();
                            
                            //绘制外发光
                            spriteBatch.Draw(PixelTexture, innerPoint, null, innerColor2, innerLineRotation, new Vector2(0, 1), new Vector2(innerLineLength / 2, scale) + Vector2.One * 0.6f, SpriteEffects.None, 0f);
                            //绘制内圈
                            spriteBatch.Draw(PixelTexture, innerPoint, null, innerColor, innerLineRotation, new Vector2(0, 1), new Vector2(innerLineLength / 2, 0.8f * scale), SpriteEffects.None, 0f);
                        }
                        
                        previousInner = innerPoint;
                    }
                    
                    //处理外圈线段
                    if (i > 0)
                    {
                        Color color = Color.Lerp(Color.Black, ColorHelper.Pastel(globalCounterRad2 + i / (float)repeats * MathHelper.TwoPi, true), 0.8f);
                        Color color2 = color * 1.1f;
                        color.A = 0;
                        
                        Vector2 toPrevious = previous - currentPoint;
                        float lineLength = toPrevious.Length();
                        float lineRotation = toPrevious.ToRotation();
                        
                        //绘制外发光
                        spriteBatch.Draw(PixelTexture, currentPoint, null, color2, lineRotation, new Vector2(0, 1), new Vector2(lineLength / 2, scale) + Vector2.One * 0.6f, SpriteEffects.None, 0f);
                        //绘制外圈
                        spriteBatch.Draw(PixelTexture, currentPoint, null, color, lineRotation, new Vector2(0, 1), new Vector2(lineLength / 2, 0.8f * scale), SpriteEffects.None, 0f);
                    }
                    
                    //处理 bonus point
                    if (pointN == 37)
                    {
                        Vector2 bonusPos = new Vector2(5 * scale, 0).RotatedBy(rotation);
                        bonusPoint = currentPoint + bonusPos;
                    }
                    
                    previous = currentPoint;
                }
                
                //绘制 bonus point
                if (bonusPoint != Vector2.Zero)
                {
                    Color finalColor2 = Color.Lerp(Color.Black, ColorHelper.Pastel(globalCounterRad2 + 37f / (float)repeats * MathHelper.TwoPi, true), 0.8f);
                    Color finalColor3 = finalColor2;
                    finalColor2.A = 0;
                    
                    //绘制外发光
                    spriteBatch.Draw(PixelTexture, bonusPoint, null, finalColor3, 0f, new Vector2(1, 1), 1.8f * scale, SpriteEffects.None, 0f);
                    //绘制点
                    spriteBatch.Draw(PixelTexture, bonusPoint, null, finalColor2, 0f, new Vector2(1, 1), 1.2f * scale, SpriteEffects.None, 0f);
                }
            }
        }
    }

    //创造飞行与按住下键+跳跃键悬浮（悬浮效果同原版顶级飞行翅膀）
    public class GildedBladeWingsPlayer : ModPlayer
    {
        //创造飞行模式是否开启
        public bool CreativeFlight { get; set; }

        //创造飞行解锁条件：击败指定数量的BOSS
        public bool CreativeFlightUnlocked => PWingWorld.GetBossKillCount(Player) >= GildedBladeWings.CreativeFlightUnlockBossKills;

        public override void PostUpdateEquips()
        {
            if (!CreativeFlight)
            {
                return;
            }

            //失去翅膀、骑乘或不再满足解锁条件时自动退出创造飞行
            if (!CanUseCreativeFlight())
            {
                CreativeFlight = false;
                return;
            }

            //原版每帧开始时都会把重力重置为默认值，因此必须在移动逻辑执行前清零
            Player.gravity = 0f;
            Player.noFallDmg = true;
            //禁用原版翅膀推进，避免与创造飞行叠加
            Player.wingTime = -1f;
        }

        public override void PreUpdateMovement()
        {
            if (Player.whoAmI != Main.myPlayer)
            {
                return;
            }

            if (CreativeFlight)
            {
                CreativeFlightMovement();
                return;
            }

            //仅在本地玩家装备梦环时生效
            if (Player.mount.Active || Player.wingTime <= 0f)
            {
                return;
            }
            if (Player.wingsLogic != EquipLoader.GetEquipSlot(Mod, nameof(GildedBladeWings), EquipType.Wings))
            {
                return;
            }
            if (!Player.controlJump || !Player.TryingToHoverDown || Player.TryingToHoverUp)
            {
                return;
            }
            //在地面时不触发，避免抵消起跳
            if (HasTilesBelow())
            {
                return;
            }

            //悬浮：保留极小非零垂直速度
            //若设为0，原版会把玩家当作处于“落地”状态，触发自动跳跃分支并在每帧把飞行时间重置为上限，导致无限悬停
            Player.velocity.Y = -0.001f;

            //飞行时间由原版飞行逻辑正常扣除，此处不再手动扣除，避免重复消耗
        }

        private bool CanUseCreativeFlight()
        {
            return !Player.mount.Active && CreativeFlightUnlocked &&
                Player.wingsLogic == EquipLoader.GetEquipSlot(Mod, nameof(GildedBladeWings), EquipType.Wings);
        }

        private void CreativeFlightMovement()
        {
            if (!CanUseCreativeFlight())
            {
                CreativeFlight = false;
                return;
            }

            //创造飞行性能取正常模式速度与加速度的60%
            int bossKills = PWingWorld.GetBossKillCount(Player);
            float normalSpeed = 6f + Math.Min(bossKills * 0.1f, 3f);
            float normalAcceleration = 1.5f + Math.Min(bossKills * 0.1f, 3f) * 0.5f;
            float maxSpeed = normalSpeed * GildedBladeWings.CreativeFlightPerformanceMultiplier;
            float acceleration = normalAcceleration * GildedBladeWings.CreativeFlightPerformanceMultiplier;

            bool up = (Player.controlUp || Player.controlJump) && !Player.controlDown;
            bool down = Player.controlDown && !(Player.controlUp || Player.controlJump) && !HasTilesBelow();
            bool left = Player.controlLeft && !Player.controlRight;
            bool right = Player.controlRight && !Player.controlLeft;

            //不具备惯性：有方向输入时加速，松开后立即停止
            ApplyCreativeFlightVelocity(ref Player.velocity.X, left, right, maxSpeed, acceleration);
            ApplyCreativeFlightVelocity(ref Player.velocity.Y, up, down, maxSpeed, acceleration);
        }

        private static void ApplyCreativeFlightVelocity(ref float velocity, bool negative, bool positive, float maxSpeed, float acceleration)
        {
            if (!negative && !positive)
            {
                velocity = 0f;
                return;
            }

            float target = positive ? maxSpeed : -maxSpeed;
            velocity = MathHelper.Clamp(velocity + Math.Sign(target) * acceleration, -maxSpeed, maxSpeed);
        }

        private bool HasTilesBelow()
        {
            Vector2 groundCheckPos = new Vector2(Player.position.X, Player.position.Y + Player.height);
            return Collision.SolidCollision(groundCheckPos, Player.width, 8);
        }
    }
    
}
