using Microsoft.Xna.Framework;
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
    }
}
