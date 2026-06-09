using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace PWing
{
    public class PWingGlobalNPC : GlobalNPC
    {
        public override void OnKill(NPC npc)
        {
            if (PWingUtils.IsBoss(npc))
            {
                // 遍历所有玩家，更新他们的BOSS击杀记录
                foreach (Player player in Main.player)
                {
                    if (player.active)
                    {
                        PWingPlayer modPlayer = player.GetModPlayer<PWingPlayer>();
                        // 记录BOSS击杀
                        if (!modPlayer.DefeatedBosses.Contains(npc.type))
                        {
                            modPlayer.DefeatedBosses.Add(npc.type);
                            // 仅在服务器端同步
                            if (Main.netMode == NetmodeID.Server)
                            {
                                PWingWorld.SyncBossKills(player);
                            }
                        }
                    }
                }
                
                // BOSS击败时自动保存
                PWingWorld.PerformAutoSave("击败BOSS");
            }
        }
    }
}