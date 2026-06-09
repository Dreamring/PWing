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
        public List<int> DefeatedBosses { get; set; } = new List<int>();
        
        public int BossKillCount => DefeatedBosses.Count;
        
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