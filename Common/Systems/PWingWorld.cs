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
using PWing.Common.Configs;
using PWing.Content.Players;
using static PWing.PWing;

namespace PWing.Common.Systems
{
    public class PWingWorld : ModSystem
    {
        // 全局属性
        public static int GlobalCounter = 0;
        public static LightMode LastLightingMode;
        private static int MoonPhase = 0;
        private static float PrevMoonProgress = 0;
        public static float MoonPhasePercent;
        public static bool awaitTileCheck = true;

        // 自动保存相关
        private static int lastAutoSaveTime = 0;
        private static bool isSaving = false;
        private static string pendingSaveReason = "";
        private static bool pendingSave = false;
        private const int BattleCheckIntervalTicks = 300; // 战斗检测间隔5秒

        // 获取最小保存间隔（从配置读取）
        private static int GetMinSaveIntervalTicks()
        {
            return PWingConfig.Instance.minSaveInterval * 60;
        }

        // 获取玩家已击败的BOSS数量
        public static int GetBossKillCount(Player player)
        {
            PWingPlayer modPlayer = player.GetModPlayer<PWingPlayer>();
            return modPlayer.BossKillCount;
        }

        // 同步BOSS击杀记录
        public static void SyncBossKills(Player player)
        {
            PWingPlayer modPlayer = player.GetModPlayer<PWingPlayer>();

            var packet = Instance.GetPacket();
            packet.Write((byte)PWingMessageType.SyncBossKills);
            packet.Write(player.whoAmI);
            packet.Write(modPlayer.DefeatedBosses.Count);

            foreach (int bossType in modPlayer.DefeatedBosses)
            {
                packet.Write(bossType);
            }

            packet.Send();
        }

        public override void PreUpdateEntities()
        {
            Update();
        }

        public static void SyncGlobalCounter()
        {
            var packet = Instance.GetPacket();
            packet.Write((byte)PWingMessageType.SyncGlobalCounter);
            packet.Write(GlobalCounter);
            packet.Send();
        }

        // 处理网络包
        public static void HandlePacket(BinaryReader reader, int whoAmI)
        {
            PWingMessageType messageType = (PWingMessageType)reader.ReadByte();
            switch (messageType)
            {
                case PWingMessageType.SyncGlobalCounter:
                    GlobalCounter = reader.ReadInt32();
                    break;
                case PWingMessageType.SyncBossKills:
                    int playerIndex = reader.ReadInt32();
                    int bossCount = reader.ReadInt32();

                    if (playerIndex >= 0 && playerIndex < Main.player.Length)
                    {
                        Player player = Main.player[playerIndex];
                        if (player != null && player.active)
                        {
                            PWingPlayer modPlayer = player.GetModPlayer<PWingPlayer>();
                            modPlayer.DefeatedBosses.Clear();

                            for (int i = 0; i < bossCount; i++)
                            {
                                int bossType = reader.ReadInt32();
                                modPlayer.DefeatedBosses.Add(bossType);
                            }
                        }
                    }
                    break;
            }
        }

        public static void Update()
        {
            GlobalCounter++;

            // 更频繁地同步，确保客户端和服务器保持一致
            if (GlobalCounter % 120 == 0)
            {
                if (Main.netMode == NetmodeID.Server)
                    SyncGlobalCounter();
            }

            // 处理待保存请求（战斗延迟保存）
            if (pendingSave && !isSaving)
            {
                if (IsSafeToSave())
                {
                    pendingSave = false;
                    PerformAutoSave(pendingSaveReason);
                }
            }

            // 定时自动保存
            if (PWingConfig.Instance.autoSaveEnabled && !pendingSave && !isSaving)
            {
                int intervalTicks = PWingConfig.Instance.autoSaveInterval * 60;
                if (GlobalCounter - lastAutoSaveTime >= intervalTicks)
                {
                    if (IsSafeToSave())
                    {
                        PerformAutoSave("定时保存");
                    }
                    else
                    {
                        // 战斗中，延迟保存
                        pendingSave = true;
                        pendingSaveReason = "定时保存";
                    }
                }
            }

            LastLightingMode = Lighting.Mode;
            MoonPhasePercent = SantuaryMoonPhase();
        }

        /// <summary>
        /// 检查当前是否可以安全保存
        /// </summary>
        private static bool IsSafeToSave()
        {
            // 检查最小保存间隔
            if (GlobalCounter - lastAutoSaveTime < GetMinSaveIntervalTicks())
                return false;

            // 检查游戏是否暂停
            if (Main.gamePaused)
                return false;

            // 检查是否有活跃的BOSS
            if (HasActiveBoss())
                return false;

            return true;
        }

        /// <summary>
        /// 检测是否有活跃的BOSS
        /// </summary>
        private static bool HasActiveBoss()
        {
            // 每5秒检测一次，减少性能开销
            if (GlobalCounter % BattleCheckIntervalTicks != 0)
                return false;

            for (int i = 0; i < Main.npc.Length; i++)
            {
                NPC npc = Main.npc[i];
                if (npc != null && npc.active && npc.boss)
                {
                    return true;
                }
            }
            return false;
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
                {
                    MoonPhase = Main.moonPhase;
                }
                PrevMoonProgress = progressToTheNextMoon;
            }
            return 1 - MathF.Abs(MathF.Sin((-moonPhaseOffset + MoonPhase + progressToTheNextMoon - moonSwitch / 24f) / 8f * MathF.PI));
        }

        /// <summary>
        /// 执行自动保存（在主线程安全执行）
        /// </summary>
        public static void PerformAutoSave(string reason = "")
        {
            // 检查配置
            if (!PWingConfig.Instance.autoSaveEnabled)
                return;

            // 检查网络模式
            if (Main.netMode != NetmodeID.Server && Main.netMode != NetmodeID.SinglePlayer)
                return;

            // 检查是否正在保存
            if (isSaving)
                return;

            // 检查最小保存间隔
            if (GlobalCounter - lastAutoSaveTime < GetMinSaveIntervalTicks())
                return;

            isSaving = true;
            lastAutoSaveTime = GlobalCounter;

            try
            {
                WorldFile.SaveWorld();

                string message = "世界已自动保存";
                if (!string.IsNullOrEmpty(reason))
                {
                    message += " (" + reason + ")";
                }

                // 在主线程发送通知
                if (Main.netMode == NetmodeID.Server)
                {
                    foreach (Player player in Main.player)
                    {
                        if (player != null && player.active)
                        {
                            NetMessage.SendData(MessageID.ChatText, -1, -1,
                                NetworkText.FromLiteral(message), 255, 0, 255, 0);
                        }
                    }
                }
                else if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    Main.NewText(message, Color.Green);
                }
            }
            catch (Exception ex)
            {
                // 保存失败时显示错误
                string errorMsg = "自动保存失败: " + ex.Message;
                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    Main.NewText(errorMsg, Color.Red);
                }
                else if (Main.netMode == NetmodeID.Server)
                {
                    foreach (Player player in Main.player)
                    {
                        if (player != null && player.active)
                        {
                            NetMessage.SendData(MessageID.ChatText, -1, -1,
                                NetworkText.FromLiteral(errorMsg), 255, 0, 255, 0);
                        }
                    }
                }
            }
            finally
            {
                isSaving = false;
            }
        }

        // 当世界加载时触发
        public override void OnWorldLoad()
        {
            awaitTileCheck = true;
            lastAutoSaveTime = GlobalCounter;
            isSaving = false;
            pendingSave = false;
            pendingSaveReason = "";
        }

        // 世界保存时调用
        public override void OnWorldUnload()
        {
            // 确保保存状态重置
            isSaving = false;
            pendingSave = false;
        }
    }
}