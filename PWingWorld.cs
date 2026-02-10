using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Light;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static PWing.PWing;



namespace PWing
{
    public class PWingWorld:ModSystem
    {
        //定义全局属性
        public static int GlobalCounter = 0;//全局计数
        public static HashSet<int> DefeatedBosses = new HashSet<int>();//已击败的BOSS类型集合
        public static LightMode LastLightingMode;
        private static int MoonPhase = 0;
        private static float PrevMoonProgress = 0;
        public static float MoonPhasePercent;
        public static bool awaitTileCheck = true;
        
        // 获取已击败的BOSS数量
        public static int BossKillCount => DefeatedBosses.Count;
        public override void PreUpdateEntities()
        {
            //在每个游戏刻前更新
            Update();
        }

        public static void SyncGlobalCounter()
        {
            //同步GlobalCounter和DefeatedBosses
            var packet = Instance.GetPacket();
            packet.Write((byte)PWingMessageType.SyncGlobalCounter);
            packet.Write(GlobalCounter);
            packet.Write(DefeatedBosses.Count);
            foreach (int bossType in DefeatedBosses)
            {
                packet.Write(bossType);
            }
            packet.Send();
        }
        
        //处理网络包
        public static void HandlePacket(BinaryReader reader, int whoAmI)
        {
            PWingMessageType messageType = (PWingMessageType)reader.ReadByte();
            switch (messageType)
            {
                case PWingMessageType.SyncGlobalCounter:
                    GlobalCounter = reader.ReadInt32();
                    int count = reader.ReadInt32();
                    DefeatedBosses.Clear();
                    for (int i = 0; i < count; i++)
                    {
                        int bossType = reader.ReadInt32();
                        DefeatedBosses.Add(bossType);
                    }
                    break;
            }
        }
        
        // BOSS击败事件将在GlobalNPC中处理
        
        public static void Update()
        {
            //更新GlobalCounter
            GlobalCounter++;
            
            //更频繁地同步，确保客户端和服务器保持一致
            if (GlobalCounter % 120 == 0) // 每2秒同步一次
            {
                if (Main.netMode == NetmodeID.Server)
                    SyncGlobalCounter();
            }
            
            LastLightingMode = Lighting.Mode;
            MoonPhasePercent = SantuaryMoonPhase();
        }
        public static float SantuaryMoonPhase(int moonPhaseOffset = 0)
        {
            float moonSwitch = 19.5f;
            float progressToTheNextMoon = (Utils.GetDayTimeAs24FloatStartingFromMidnight() + moonSwitch) % 24 / 24f;
            if (moonPhaseOffset == 0)
            {
                if (PrevMoonProgress > 0.98f && progressToTheNextMoon < 0.02f)
                {
                    MoonPhase++;
                }
                else
                    MoonPhase = Main.moonPhase;
                PrevMoonProgress = progressToTheNextMoon;
            }
            return 1 - MathF.Abs(MathF.Sin((-moonPhaseOffset + MoonPhase + progressToTheNextMoon - moonSwitch / 24f) / 8f * MathF.PI));
        }
        
        // 当世界加载时触发
        public override void OnWorldLoad()
        {
            awaitTileCheck = true;
            // 初始化已击败的BOSS集合
            DefeatedBosses.Clear();
        }
        
        // 当玩家加入世界时触发
        public override void PostUpdatePlayers()
        {
            // 为所有活跃玩家检查并给予翅膀
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (player.active)
                {
                    GiveWingToPlayer(player);
                }
            }
        }
        
        // 当世界生成后触发
        public override void PostWorldGen()
        {
            // 为所有活跃玩家赠送翅膀
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (player.active)
                {
                    GiveWingToPlayer(player);
                }
            }
        }
        
        // 查找玩家物品栏中的空槽位
        private int FindEmptyItemSlot(Player player)
        {
            for (int i = 0; i < player.inventory.Length; i++)
            {
                if (player.inventory[i].type == ItemID.None)
                {
                    return i;
                }
            }
            return -1;
        }
        
        // 给予玩家翅膀的方法
        private void GiveWingToPlayer(Player player)
        {
            // 获取玩家的PWingPlayer实例
            PWingPlayer modPlayer = player.GetModPlayer<PWingPlayer>();
            
            // 检查玩家是否已经获得过翅膀
            if (modPlayer.hasReceivedWing)
            {
                return;
            }
            
            // 检查玩家是否已经拥有翅膀
            bool hasWing = false;
            for (int i = 0; i < player.inventory.Length; i++)
            {
                if (player.inventory[i].type == ModContent.ItemType<Items.Wings.GildedBladeWings>())
                {
                    hasWing = true;
                    break;
                }
            }
            
            // 如果玩家没有翅膀，则给予一个
            if (!hasWing)
            {
                // 尝试在玩家物品栏中找到空槽位
                int emptySlot = FindEmptyItemSlot(player);
                if (emptySlot != -1)
                {
                    player.inventory[emptySlot].SetDefaults(ModContent.ItemType<Items.Wings.GildedBladeWings>());
                    
                    // 标记玩家已经获得过翅膀
                    modPlayer.hasReceivedWing = true;
                    
                    // 发送消息通知玩家
                    if (Main.netMode == NetmodeID.SinglePlayer)
                    {
                        Main.NewText("你获得了一个成长翅膀！击败BOSS来让它变得更强大！", Microsoft.Xna.Framework.Color.Green);
                    }
                    else if (Main.netMode == NetmodeID.Server)
                    {
                        Terraria.Chat.ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("你获得了一个成长翅膀！击败BOSS来让它变得更强大！"), Microsoft.Xna.Framework.Color.Green, player.whoAmI);
                    }
                }
            }
            else
            {
                // 如果玩家已经拥有翅膀，也标记为已获得
                modPlayer.hasReceivedWing = true;
            }
        }
        
        }
}
