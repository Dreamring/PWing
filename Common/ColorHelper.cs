using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace PWing.Common
{
    public class ColorHelper
    {
        public static Color AcediaColor = new Color(213, 68, 255);
        public static Color Pastel(float radians, bool pinkify = false)
        {
            Color color = Color.White;
            if (pinkify)
                color = new Color(253, 198, 234);
            return PastelGradient(radians, color);
        }
        public static Color PastelGradient(float radians, Color overrideColor)
        {
            float newAi = radians;
            double center = 190;
            Vector2 circlePalette = new Vector2(1, 0).RotatedBy(newAi);
            double width = 65 * circlePalette.Y;
            int red = (int)(center + width);
            circlePalette = new Vector2(1, 0).RotatedBy(newAi + MathHelper.ToRadians(120));
            width = 65 * circlePalette.Y;
            int grn = (int)(center + width);
            circlePalette = new Vector2(1, 0).RotatedBy(newAi + MathHelper.ToRadians(240));
            width = 65 * circlePalette.Y;
            int blu = (int)(center + width);
            if (overrideColor == Color.White)
                return new Color(red, grn, blu);
            else
                return new Color(red, grn, blu).MultiplyRGB(overrideColor);
        }

        public static Color AmethystColor
        {
            get
            {
                Color color = new Color(133, 13, 191);
                Color other = new Color(158, 0, 244);
                return Color.Lerp(color, other, (float)(0.5f + 0.5f * Math.Sin(MathHelper.ToRadians(Main.GameUpdateCount))));
            }
        }

        public static Color TesseractColor(float radians, float lerp = 0.5f)
        {
            return Color.Lerp(AmethystColor, Pastel(radians + Main.GameUpdateCount * 9 / 160f, true), lerp);
        }

        public static Color[] ConvertToSingleColor(Texture2D texture, Color color)
        {
            Color[] data = new Color[texture.Width * texture.Height];
            texture.GetData(data);
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].A > 0)
                {
                    data[i] = color * (data[i].A / 255f);
                }
            }
            return data;
        }
    }
}
