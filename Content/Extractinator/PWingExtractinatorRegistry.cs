using System.Collections.Generic;
using Terraria.ID;

namespace PWing.Content.Extractinator
{
    /// <summary>
    /// 提取机方块注册表：默认包含原版提取机与叶绿提取机，其他模组可通过 PWing 的 Call 接口注册自定义方块。
    /// </summary>
    public static class PWingExtractinatorRegistry
    {
        public static readonly List<int> ExtractinatorTiles = new List<int> { TileID.Extractinator };
        public static readonly List<int> ChlorophyteExtractinatorTiles = new List<int> { TileID.ChlorophyteExtractinator };

        /// <summary>确保原版方块始终在注册表中，不会清掉其他模组提前注册的自定义方块。</summary>
        public static void Initialize()
        {
            if (!ExtractinatorTiles.Contains(TileID.Extractinator))
            {
                ExtractinatorTiles.Add(TileID.Extractinator);
            }
            if (!ChlorophyteExtractinatorTiles.Contains(TileID.ChlorophyteExtractinator))
            {
                ChlorophyteExtractinatorTiles.Add(TileID.ChlorophyteExtractinator);
            }
        }

        public static void Unload()
        {
            ExtractinatorTiles.Clear();
            ChlorophyteExtractinatorTiles.Clear();
        }

        public static bool RegisterTile(int tileType, bool chlorophyte)
        {
            if (tileType <= 0)
            {
                return false;
            }

            List<int> tiles = chlorophyte ? ChlorophyteExtractinatorTiles : ExtractinatorTiles;
            if (!tiles.Contains(tileType))
            {
                tiles.Add(tileType);
            }
            return true;
        }

        public static bool IsExtractinator(int tileType)
        {
            return ExtractinatorTiles.Contains(tileType);
        }

        public static bool IsChlorophyteExtractinator(int tileType)
        {
            return ChlorophyteExtractinatorTiles.Contains(tileType);
        }

        public static bool IsAnyExtractinator(int tileType)
        {
            return IsExtractinator(tileType) || IsChlorophyteExtractinator(tileType);
        }
    }
}
