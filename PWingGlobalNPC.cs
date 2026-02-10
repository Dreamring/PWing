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
                // 将BOSS类型添加到已击败集合中
                bool added = PWingWorld.DefeatedBosses.Add(npc.type);
                
                // 如果是新击败的BOSS且是服务器，同步BOSS击败信息
                if (added && Main.netMode == NetmodeID.Server)
                {
                    PWingWorld.SyncGlobalCounter();
                }
            }
        }
    }
}