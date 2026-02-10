using ReLogic.Utilities;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
namespace PWing
{
    public static class PWingUtils {//工具类
        public static void SetResearchCost(this ModItem modItem, int amt)//设置旅行模式研究成本
        {
            modItem.Item.ResearchUnlockCount = amt;
        }
        public static SlotId PlaySound(SoundStyle style, Vector2 position, float volume = 1f, float pitch = 0f, float pitchVariance = 0f)//播放声音函数
        {
            //播放声音函数
            style = style.WithVolumeScale(volume);
            style = style.WithPitchOffset(pitch);
            style.PitchVariance = pitchVariance;
            return SoundEngine.PlaySound(style, position);
        }
    }
}
