using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using PWing.Common.Configs;
using PWing.Common.Systems;

namespace PWing
{
	// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
	public class PWing : Mod
    {
        internal static PWing Instance;//用例
        public override void Load()
        {
            //向游戏中加载内容
            Instance = ModContent.GetInstance<PWing>();
        }
        public override void Unload()
        {
            //卸载操作
            Instance = null;
        }
        internal enum PWingMessageType : int
        {
            //mod消息类型
            SyncGlobalCounter,//同步全局计数器
            SyncCreativeFlight,//同步创造飞行
            SyncTileLocations,//同步位置信息
            RequestTileLocations,//请求定位图块位置
            SyncBossKills//同步BOSS击杀记录
        }
        
        //处理网络包
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            PWingWorld.HandlePacket(reader, whoAmI);
        }
        
        public static PWingConfig Config
        {
            //加载配置
            get => ModContent.GetInstance<PWingConfig>();
        }
}}
