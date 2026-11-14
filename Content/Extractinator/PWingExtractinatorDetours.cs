using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.Localization;
using Terraria.ModLoader;

namespace PWing.Content.Extractinator
{
    /// <summary>
    /// 拦截批量提取期间原版提取机产生的掉落物与网络包，并把它们交给批量提取逻辑处理。
    /// </summary>
    internal static class PWingExtractinatorDetours
    {
        internal static void Initialize()
        {
            On_Player.DropItemFromExtractinator += HookDropItemFromExtractinator;
            On_NetMessage.SendData += HookSendData;
            On_CreativeUI.ToggleMenu += HookCreativeToggleMenu;
            On_CreativeUI.DrawToggleButton += HookCreativeDrawToggleButton;
        }

        internal static void Unload()
        {
            On_Player.DropItemFromExtractinator -= HookDropItemFromExtractinator;
            On_NetMessage.SendData -= HookSendData;
            On_CreativeUI.ToggleMenu -= HookCreativeToggleMenu;
            On_CreativeUI.DrawToggleButton -= HookCreativeDrawToggleButton;
        }

        private static void HookDropItemFromExtractinator(On_Player.orig_DropItemFromExtractinator orig, Player self, int itemType, int stack)
        {
            if (PWingExtractinatorHelper.ShouldInterceptNewItem)
            {
                PWingExtractinatorHelper.OnNewItemIntercept(itemType, stack);
            }
            else
            {
                orig(self, itemType, stack);
            }
        }

        private static void HookSendData(On_NetMessage.orig_SendData orig, int msgType, int remoteClient, int ignoreClient, NetworkText text, int number, float number2, float number3, float number4, int number5, int number6, int number7)
        {
            if (!PWingExtractinatorHelper.ShouldInterceptNewItem)
            {
                orig(msgType, remoteClient, ignoreClient, text, number, number2, number3, number4, number5, number6, number7);
            }
        }

        private static void HookCreativeToggleMenu(On_CreativeUI.orig_ToggleMenu orig, CreativeUI self)
        {
            if (!PWingExtractinatorPlayer.ExtractinatorOpenLocally)
            {
                orig(self);
            }
        }

        private static void HookCreativeDrawToggleButton(On_CreativeUI.orig_DrawToggleButton orig, CreativeUI self, SpriteBatch spriteBatch, Vector2 location)
        {
            if (!PWingExtractinatorPlayer.ExtractinatorOpenLocally)
            {
                orig(self, spriteBatch, location);
            }
        }
    }
}
