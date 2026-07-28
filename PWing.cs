using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using PWing.Common.Configs;
using PWing.Common.Systems;
using PWing.Content.FakePlayer;

namespace PWing
{
	public class PWing : Mod
    {
        internal static PWing Instance;
        public override void Load()
        {
            Instance = ModContent.GetInstance<PWing>();
            PWingFakePlayerHelper.Initialize();
            PWingDetours.Initialize();
            PlayerInventorySlotsManager.Load();
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
