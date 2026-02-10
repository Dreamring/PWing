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
using Terraria.IO;
using static PWing.PWing;



namespace PWing
{
    public class PWingWorld:ModSystem
    {
        //定义全局属性
        public static int GlobalCounter = 0;//全局计数
        public static LightMode LastLightingMode;
        private static int MoonPhase = 0;
        private static float PrevMoonProgress = 0;
        public static float MoonPhasePercent;
        public static bool awaitTileCheck = true;
        
        // 获取已击败的BOSS数量
        public static int BossKillCount
        {
            get
            {
                int count = 0;
                // 检查游戏内置的BOSS击败记录
                if (NPC.downedBoss1) count++;
                if (NPC.downedBoss2) count++;
                if (NPC.downedBoss3) count++;
                if (NPC.downedQueenBee) count++;
                if (NPC.downedMechBoss1) count++;
                if (NPC.downedMechBoss2) count++;
                if (NPC.downedMechBoss3) count++;
                if (NPC.downedMechBossAny) count++;
                if (NPC.downedPlantBoss) count++;
                if (NPC.downedGolemBoss) count++;
                if (NPC.downedFishron) count++;
                if (NPC.downedAncientCultist) count++;
                if (NPC.downedMoonlord) count++;
                if (NPC.downedHalloweenKing) count++;
                if (NPC.downedHalloweenTree) count++;
                if (NPC.downedChristmasIceQueen) count++;
                if (NPC.downedChristmasSantank) count++;
                if (NPC.downedChristmasTree) count++;
                return count;
            }
        }
        public override void PreUpdateEntities()
        {
            //在每个游戏刻前更新
            Update();
        }

        public static void SyncGlobalCounter()
        {
            //同步GlobalCounter
            var packet = Instance.GetPacket();
            packet.Write((byte)PWingMessageType.SyncGlobalCounter);
            packet.Write(GlobalCounter);
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
        }
        

        

        
        }
}
