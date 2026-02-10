using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace PWing
{
    public class PWingGlobalNPC : GlobalNPC
    {
        // 处理BOSS击败事件
        public override void OnKill(NPC npc)
        {
            // 检查是否是BOSS
            if (npc.boss)
            {
                // 游戏会自动记录BOSS击败状态，不需要手动处理
                // 仅在服务器端同步全局计数器
                if (Main.netMode == NetmodeID.Server)
                {
                    PWingWorld.SyncGlobalCounter();
                }
            }
        }
    }
}