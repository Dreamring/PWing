using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using PWing.Common.Configs;
using PWing.Common.Systems;
using PWing.Content.AzafureMiners;
using PWing.Content.FakePlayer;
using PWing.Content.Vanity;

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
        public override void PostSetupContent()
        {
            //物品与图块数据在此阶段已全部就绪，一次性建立矿石查找表
            PWingAzMinerTP.SetUpList();
        }
        public override void Unload()
        {
            PWingFakePlayer.ClearTextureCache();
            PWingFakePlayerDrawing.Unload();
            PWingFakePlayerHelper.Unload();
            PlayerInventorySlotsManager.Unload();
            PWingAzMinerTP.UnloadStaticData();
            PWingVanityUI.UnloadStaticData();
            VanitySets.Unload();
            //卸载操作
            Instance = null;
        }
        internal enum PWingMessageType : int
        {
            //mod消息类型
            SyncBossKills,//同步BOSS击杀记录
            RequestBossKills//请求补发BOSS击杀记录
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
