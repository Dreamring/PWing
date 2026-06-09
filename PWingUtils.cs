using ReLogic.Utilities;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
namespace PWing
{
    public static class PWingUtils
    {
        public static void SetResearchCost(this ModItem modItem, int amt)//设置旅行模式研究成本
        {
            modItem.Item.ResearchUnlockCount = amt;
        }
        public static SlotId PlaySound(SoundStyle style, Vector2 position, float volume = 1f, float pitch = 0f, float pitchVariance = 0f)
        {
            style = style.WithVolumeScale(volume);
            style = style.WithPitchOffset(pitch);
            style.PitchVariance = pitchVariance;
            return SoundEngine.PlaySound(style, position);
        }
        
        public static bool IsBoss(NPC npc)
        {
            return npc.boss;
        }
    }
}
