using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PWing.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
namespace PWing.Items.Wings
{
    //自动加载翅膀物品类型
    [AutoloadEquip(EquipType.Wings)]
    public class GildedBladeWings : ModItem
    {
        public override void SetStaticDefaults()
        {
            this.SetResearchCost(1);
            //初始设置为雏翼的基础飞行能力
            ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(60, 3f, 1f); //雏翼基础：1秒飞行时间
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
            // 清除默认工具提示
            tooltips.Clear();
            
            // 添加基础工具提示
            tooltips.Add(new TooltipLine(Mod, "BaseTooltip", "一个能够成长的神奇翅膀"));
            tooltips.Add(new TooltipLine(Mod, "GrowthTooltip", "随着你击败更多BOSS，它会变得更加强大"));
            tooltips.Add(new TooltipLine(Mod, "BonusTooltip", "每击败一个BOSS，增加0.5秒飞行时间和少量飞行速度"));
            
            int bossKills = PWingWorld.BossKillCount;
            
            //计算当前飞行能力
            float baseFlightTime = 1f;
            float currentFlightTime = baseFlightTime + bossKills * 0.5f;
            float baseFlightSpeed = 3f;
            float currentFlightSpeed = baseFlightSpeed + bossKills * 0.1f;
            
            //添加动态工具提示行
            tooltips.Add(new TooltipLine(Mod, "FlightTime", $"当前飞行时间: {currentFlightTime:F1}秒"));
            tooltips.Add(new TooltipLine(Mod, "FlightSpeed", $"当前飞行速度: {currentFlightSpeed:F1}"));
            tooltips.Add(new TooltipLine(Mod, "BossKills", $"已击败BOSS数量: {bossKills}"));
            
            //添加成长提示
            if (bossKills == 0)
            {
                tooltips.Add(new TooltipLine(Mod, "GrowthHint", "击败第一个BOSS后开始成长"));
            }
            else
            {
                float nextFlightTime = currentFlightTime + 0.5f;
                float nextFlightSpeed = currentFlightSpeed + 0.1f;
                tooltips.Add(new TooltipLine(Mod, "NextGrowth", $"下一级: {nextFlightTime:F1}秒飞行时间, {nextFlightSpeed:F1}飞行速度"));
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
            // 创建微光转化配方
            CreateRecipe(1)
                .AddIngredient(ItemID.FairyWings) // 仙女翅膀
                .AddTile(TileID.LunarCraftingStation) // 月球制造站
                .Register();
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            //根据BOSS击败数量动态调整飞行能力
            int bossKills = PWingWorld.BossKillCount;
            
            //每击败一个BOSS，增加0.5秒飞行时间和少量飞行速度
            float flightTimeIncrease = bossKills * 0.5f; // 每BOSS增加0.5秒
            float speedIncrease = bossKills * 0.1f; // 每BOSS增加少量速度
            
            //计算当前飞行能力
            float baseFlightTime = 1f; // 基础1秒
            float currentFlightTime = baseFlightTime + flightTimeIncrease;
            float baseFlightSpeed = 3f; // 基础飞行速度
            float currentFlightSpeed = baseFlightSpeed + speedIncrease;
            float baseAcceleration = 1f; // 基础加速度
            float currentAcceleration = baseAcceleration + speedIncrease * 0.5f;
            
            //更新翅膀统计
            ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats((int)(currentFlightTime * 60), currentFlightSpeed, currentAcceleration);
            
            //添加额外的视觉效果
            if (bossKills > 0)
            {
                //每击败一个BOSS，增加一点发光效果
                // 暂时移除MachinaBooster引用，后续可添加自定义粒子效果
            }
            
            // 添加按住down和Up键进行水平移动的功能（类似天界星盘）
            if (player.wingsLogic > 0 && player.controlDown && !player.controlUp)
            {
                // 按住向下键时的水平移动
                player.position.X += player.velocity.X * 0.1f;
            }
            else if (player.wingsLogic > 0 && player.controlUp && !player.controlDown)
            {
                // 按住向上键时的水平移动
                player.position.X += player.velocity.X * 0.1f;
            }
        }
        public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
        {
            int bossKills = PWingWorld.BossKillCount;
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
            int bossKills = PWingWorld.BossKillCount;
            float bonus = bossKills * 0.1f; // 每BOSS增加的额外水平速度
            
            // 设置上限
            float maxBonus = 3.0f; // 最大额外水平速度
            bonus = Math.Min(bonus, maxBonus);
            
            speed = 8f + bonus; // 基础水平速度 + 额外速度
            acceleration = 2f + bonus * 0.5f; // 基础加速度 + 额外加速度
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
                Main.spriteBatch.Draw(WingTexture, position, null, baseColor * alpha, rotation, origin, scale, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(WingGlowTexture2, position, null, ColorHelper.Pastel(MathHelper.ToRadians(PWingWorld.GlobalCounter), true) * 0.8f * alpha, rotation, origin, scale, SpriteEffects.None, 0f);
            }
            else
            {
                //高精度模式：完整绘制
                DrawHalo(spriteBatch, position, scale, rotation);
                Main.spriteBatch.Draw(WingTexture, position, null, baseColor * alpha, rotation, origin, scale, SpriteEffects.None, 0f);
                for (int k = 0; k < 8; k++)
                {
                    Vector2 offset = new Vector2(2, 0).RotatedBy(k * MathHelper.PiOver4 + MathHelper.ToRadians(PWingWorld.GlobalCounter * 2));
                    Main.spriteBatch.Draw(WingGlowTexture, position + offset, null, color * alpha, rotation, origin, scale, SpriteEffects.None, 0f);
                }
                Main.spriteBatch.Draw(WingGlowTexture2, position, null, ColorHelper.Pastel(MathHelper.ToRadians(PWingWorld.GlobalCounter), true) * 1.2f * alpha, rotation, origin, scale, SpriteEffects.None, 0f);
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
            DrawWing(spriteBatch, position, origin, scale, 0f, drawColor);
            return false;
        }
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            //世界中绘制翅膀图标
            Vector2 drawOrigin = new Vector2(Terraria.GameContent.TextureAssets.Item[Item.type].Value.Width * 0.5f, Item.height * 0.5f);
            Vector2 drawPos = new Vector2((float)(Item.Center.X - (int)Main.screenPosition.X), (float)(Item.Center.Y - (int)Main.screenPosition.Y));
            float alpha = (1f - (Item.alpha / 255f));
            
            //使用通用绘制方法
            DrawWing(spriteBatch, drawPos, drawOrigin, scale, rotation, lightColor, alpha);
            return false;
        }
        //缓存纹理资源
        private Texture2D _wingTexture;
        private Texture2D _wingGlowTexture;
        private Texture2D _wingGlowTexture2;
        private Texture2D _pixelTexture;
        
        private Texture2D WingTexture
        {
            get
            {
                if (_wingTexture == null)
                {
                    _wingTexture = Mod.Assets.Request<Texture2D>("Items/Wings/GildedBladeWings").Value;
                }
                return _wingTexture;
            }
        }
        
        private Texture2D WingGlowTexture
        {
            get
            {
                if (_wingGlowTexture == null)
                {
                    _wingGlowTexture = Mod.Assets.Request<Texture2D>("Items/Wings/GildedBladeWingsGlow").Value;
                }
                return _wingGlowTexture;
            }
        }
        
        private Texture2D WingGlowTexture2
        {
            get
            {
                if (_wingGlowTexture2 == null)
                {
                    _wingGlowTexture2 = Mod.Assets.Request<Texture2D>("Items/Wings/GildedBladeWingsGlow2").Value;
                }
                return _wingGlowTexture2;
            }
        }
        
        private Texture2D PixelTexture
        {
            get
            {
                if (_pixelTexture == null)
                {
                    _pixelTexture = Mod.Assets.Request<Texture2D>("Items/Secrets/WhitePixel", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                }
                return _pixelTexture;
            }
        }
        
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
    
}
