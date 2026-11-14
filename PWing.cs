using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using PWing.Common.Configs;
using PWing.Common.Systems;
using PWing.Content.AzafureMiners;
using PWing.Content.Extractinator;
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
            RequestBossKills,//请求补发BOSS击杀记录
            WorldMagnet//请求将全世界掉落物吸引到玩家身边
        }
        
        //处理网络包
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            PWingWorld.HandlePacket(reader, whoAmI);
        }

        //跨模组接口：供其他模组注册自定义提取机方块
        public override object Call(params object[] args)
        {
            if (args != null && args.Length >= 2 && args[0] is string command && int.TryParse(args[1].ToString(), out int tileType))
            {
                switch (command)
                {
                    case "AddExtractinatorTileID":
                        return PWingExtractinatorRegistry.RegisterTile(tileType, false);
                    case "AddChlorophyteExtractinatorTileID":
                        return PWingExtractinatorRegistry.RegisterTile(tileType, true);
                }
            }
            return base.Call(args);
        }
        
        public static PWingConfig Config
        {
            //加载配置
            get => ModContent.GetInstance<PWingConfig>();
        }
}}
