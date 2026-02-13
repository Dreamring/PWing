using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace PWing
{
    public class PWingPlayer : ModPlayer
    {
        // BOSS击杀记录
        public List<int> DefeatedBosses { get; set; } = new List<int>();
        
        // 获取已击败的BOSS数量
        public int BossKillCount
        {
            get
            {
                return DefeatedBosses.Count;
            }
        }
        
        // 检测是否为BOSS
        private bool IsBoss(NPC npc)
        {
            // 使用tModLoader的内置BOSS判断
            return npc.boss;
        }
        
        // 注意：RecordBossKill方法已移至PWingGlobalNPC.cs文件中
        
        // 注意：NPC死亡事件处理已移至PWingGlobalNPC.cs文件中
        
        // 数据保存
        public override void SaveData(TagCompound tag)
        {
            // 保存BOSS击杀记录
            if (DefeatedBosses.Count > 0)
            {
                tag["DefeatedBosses"] = DefeatedBosses;
            }
        }
        
        // 数据加载
        public override void LoadData(TagCompound tag)
        {
            // 加载BOSS击杀记录
            if (tag.ContainsKey("DefeatedBosses"))
            {
                DefeatedBosses = tag.GetList<int>("DefeatedBosses").ToList();
            }
            else
            {
                DefeatedBosses = new List<int>();
            }
        }
    }
}