using Microsoft.Xna.Framework;
using PWing.Items.Wings;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static PWing.Common.PlayerDrawing.MachinaBooster;
using static Terraria.ModLoader.ModContent;

namespace PWing
{
    public class PWingItem: GlobalItem
    {
        public override void UpdateEquip(Item item, Player player)
        {
            //当装备更新时：检查物品是否为GildedBladeWings
            if (item.type == ItemType<GildedBladeWings>())
            {
                //调用翅膀更新函数
                WingUpdate(-10, player, false);
            }
        }
        public override void UpdateVanity(Item item, Player player)
        {
            //当饰品更新时：检查物品是否为GildedBladeWings
            if (item.type == ItemType<GildedBladeWings>())
            {
                //调用翅膀更新函数
                WingUpdate(-10, player, false);
            }
        }
public override bool WingUpdate(int wings, Player player, bool inUse)
        {
            // 自定义翅膀更新逻辑
            // 检查是否为 GildedBladeWings 或调用来源是 UpdateEquip/UpdateVanity
            if (wings == EquipLoader.GetEquipSlot(Mod, "GildedBladeWings", EquipType.Wings) || wings == -10)
            {
                MachinaBoosterPlayer machinaBoosterPlayer = player.GetModPlayer<MachinaBoosterPlayer>(); // 获取玩家数据
                bool isHovering = player.controlDown && player.controlJump && Math.Abs(player.velocity.Y) < 0.1f;//检测悬停：同时按住跳跃和向下，并且垂直速度近似于0
                if (player.velocity.Y != 0f)//&& (machinaBoosterPlayer.PlayerWasLastOnASlope < 0)
                {
                    player.wingFrame = isHovering ? 2 : 1;//PWing.Config.CreateFlight?2:1; // 根据悬停状态改编翅膀形态，2：创造飞行，1：普通飞行
                    if (inUse) // 玩家主动使用翅膀
                    {
                        if (wings != -10) // 非来自 UpdateEquip/UpdateVanity
                        {
                            machinaBoosterPlayer.FlightCounter += 3; // 增加飞行计数器
                            if (PWingWorld.GlobalCounter % 18 == 0) // 每 18 帧播放一次音效
                            {
                                PWingUtils.PlaySound(SoundID.Item32, player.Center, 1.0f, -0.1f, 0.05f); // 播放音效
                            }
                        }
                    }
                    else // 玩家未主动使用
                    {
                        machinaBoosterPlayer.FlightCounter += 1; // 增加飞行计数器
                    }
                }
                // 静止时
                else
                {
                    player.wingFrame = 0; // 收起翅膀
                    if (machinaBoosterPlayer.FlightModeFloat <= 0.02f) {  // 如果飞行模式过渡完成
                        machinaBoosterPlayer.FlightCounter = 0; // 重置计数器

                    }
                }

                // 更新状态（仅当调用来源是 UpdateEquip/UpdateVanity 时）
                if (wings == -10)
                {
                    // 更新斜坡状态
                    //machinaBoosterPlayer.PlayerWasLastOnASlope = player.sloping ? 2 : machinaBoosterPlayer.PlayerWasLastOnASlope - 1;
                    // 平滑过渡动画帧
                    machinaBoosterPlayer.FlightModeFloat = MathHelper.Lerp(machinaBoosterPlayer.FlightModeFloat, player.wingFrame, 0.24f);
                }

                return true; // 表示处理成功
            }
            return false; // 表示未处理
        }
    }
}
