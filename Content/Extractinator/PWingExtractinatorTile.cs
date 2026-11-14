using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace PWing.Content.Extractinator
{
    /// <summary>
    /// 右键原版（或注册过的）提取机时打开批量提取界面，而不是原版的一次一个。
    /// </summary>
    public class PWingExtractinatorTile : GlobalTile
    {
        public override void RightClick(int i, int j, int type)
        {
            if (!PWingExtractinatorRegistry.IsAnyExtractinator(type))
            {
                return;
            }

            TileObjectData tileData = TileObjectData.GetTileData(Main.tile[i, j]);
            if (tileData == null)
            {
                return;
            }

            Tile tile = Main.tile[i, j];
            int frameXOffset = tile.TileFrameX % tileData.CoordinateFullWidth;
            int frameYOffset = tile.TileFrameY % tileData.CoordinateFullHeight;
            int frameColumn = frameXOffset / (tileData.CoordinateWidth + tileData.CoordinatePadding);
            int frameRow = 0;
            int remainingY = frameYOffset;
            while (remainingY > 0 && frameRow < tileData.Height)
            {
                remainingY -= tileData.CoordinateHeights[frameRow] + tileData.CoordinatePadding;
                frameRow++;
            }

            Rectangle extractinatorArea = new Rectangle(i - frameColumn, j - frameRow, tileData.Width, tileData.Height);
            PWingExtractinatorType extractinatorType = PWingExtractinatorRegistry.IsExtractinator(type)
                ? PWingExtractinatorType.Normal
                : PWingExtractinatorType.Chlorophyte;

            if (!PWingExtractinatorPlayer.TryGetLocal(out PWingExtractinatorPlayer localPlayer))
            {
                return;
            }

            if (localPlayer.CurrentOpenExtractinator == extractinatorArea && localPlayer.OpenExtractinatorType == extractinatorType)
            {
                if (localPlayer.HasExtractinatorOpen)
                {
                    localPlayer.CloseExtractinator();
                }
                else
                {
                    localPlayer.OpenExtractinator();
                }
            }
            else
            {
                localPlayer.OpenExtractinatorType = extractinatorType;
                localPlayer.CurrentOpenExtractinator = extractinatorArea;
                localPlayer.OpenExtractinator();
            }
        }
    }
}
