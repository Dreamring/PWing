using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using PWing.Content.Players;
using static PWing.PWing;

namespace PWing.Common.Systems
{
    public class PWingWorld : ModSystem
    {
        // 全局属性
        public static int GlobalCounter = 0;

        // 获取玩家已击败的BOSS数量
        public static int GetBossKillCount(Player player)
        {
            PWingPlayer modPlayer = player.GetModPlayer<PWingPlayer>();
            return modPlayer.BossKillCount;
        }

        // 同步BOSS击杀记录
        public static void SyncBossKills(Player player, int toClient = -1)
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

            packet.Send(toClient);
        }

        public override void PreUpdateEntities()
        {
            Update();
        }

        // 处理网络包
        public static void HandlePacket(BinaryReader reader, int whoAmI)
        {
            PWingMessageType messageType = (PWingMessageType)reader.ReadByte();
            switch (messageType)
            {
                case PWingMessageType.RequestBossKills:
                    // 客户端进入世界时请求服务器补发击杀记录，避免后进服进度为空
                    if (Main.netMode == NetmodeID.Server && whoAmI >= 0 && whoAmI < Main.player.Length && Main.player[whoAmI].active)
                    {
                        SyncBossKills(Main.player[whoAmI], whoAmI);
                    }
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
            // 仅用作本地动画与音效的时间基准，不再承担自动保存与跨端同步
            GlobalCounter++;
        }
    }
}
