using Terraria;
using Terraria.ID;

namespace PWing.WorldgenHelpers
{
    public class PWingWorldgenHelper
    {
        public static bool TrueTileSolid(int i, int j, bool includeActuated = false)
        {
            return (!WorldGen.InWorld(i, j, 20) || Main.tile[i, j].HasTile && Main.tileSolidTop[Main.tile[i, j].TileType] == false
                && Main.tileSolid[Main.tile[i, j].TileType] == true && (Main.tile[i, j].HasUnactuatedTile || includeActuated)) && Main.tile[i, j].TileType != TileID.Bubble;
        }
    }
}
