using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace PWing.Common.Configs
{
    public class PWingConfig : ModConfig
    {
        // "ConfigScope.ClientSide" 应用于客户端（通常涉及视觉或音频方面的调整）。
        // "ConfigScope.ServerSide" 则用于几乎所有其他情况，包括禁用某些项目或改变 NPC 的行为。
        public override ConfigScope Mode => ConfigScope.ClientSide;
        public static PWingConfig Instance => PWing.Config;
        // 括号内的内容被称为"属性"。
        [BackgroundColor(45, 50, 65, 192)]
        [Header("WingConfigs")] // 头部类似于配置文件中的标题。您只需在该头部应出现的项目上声明该头部，而无需在该类别中的每个项目上都进行声明。
                          // [Label("$Some.Key")] // 标签是显示在选项旁边的文本。通常，它应是一个对该选项功能的简短描述。默认情况下，所有模组配置字段和属性都有一个自动的标签翻译键，但模组开发者可以指定特定的翻译键。
                          // [Tooltip("$Some.Key")] // 提示是当您将鼠标悬停在选项上时显示的描述。它可以作为对该选项更详细的解释。与标签一样，也可以提供特定的键。
        [DefaultValue(false)]  // 这会设置配置的默认值。
        [ReloadRequired] // 将其标记为 [ReloadRequired] 可使 tModLoader 在选项发生更改时强制重新加载模组。此功能应用于诸如物品切换等仅在模组加载期间生效的事项。         
        public bool lowFidelityMode { get; set; }//模型精度

        [BackgroundColor(45, 50, 65, 192)]
        [Header("AutoSaveConfigs")]
        [DefaultValue(true)]
        public bool autoSaveEnabled { get; set; }//自动保存功能开关

        [DefaultValue(300)]
        [Range(60, 3600)]
        [Increment(30)]
        public int autoSaveInterval { get; set; }//自动保存时间间隔（秒）

        [BackgroundColor(45, 50, 65, 192)]
        [Header("AzafureMinerConfigs")]
        [DefaultValue(false)]
        public bool azafureMinerWhitelistEnabled { get; set; }//采矿机白名单功能开关
        
        // 采矿机白名单物品列表，用于补充无法被通用条件匹配采集的方块
        public System.Collections.Generic.List<ItemDefinition> azafureMinerWhitelistItems { get; set; } = new System.Collections.Generic.List<ItemDefinition>();

    }
}