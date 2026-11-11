using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace PWing.Common.Configs
{
    public class PWingConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [BackgroundColor(45, 50, 65, 192)]
        [Header("WingConfigs")]
        [DefaultValue(false)]
        [ReloadRequired]
        public bool lowFidelityMode { get; set; }

        [BackgroundColor(45, 50, 65, 192)]
        [Header("AzafureMinerConfigs")]
        [DefaultValue(false)]
        public bool azafureMinerWhitelistEnabled { get; set; }

        public System.Collections.Generic.List<ItemDefinition> azafureMinerWhitelistItems { get; set; } = new System.Collections.Generic.List<ItemDefinition>();

        [BackgroundColor(45, 50, 65, 192)]
        [Header("TesseractConfigs")]
        [DefaultValue(0.8f)]
        [Range(0f, 2f)]
        [Increment(0.1f)]
        public float tesseractGlowIntensity { get; set; }

        [DefaultValue(2f)]
        [Range(0.5f, 5f)]
        [Increment(0.5f)]
        public float tesseractLineThickness { get; set; }

        [DefaultValue(1f)]
        [Range(0f, 2f)]
        [Increment(0.1f)]
        public float tesseractColorSaturation { get; set; }

        [DefaultValue(1f)]
        [Range(0.1f, 3f)]
        [Increment(0.1f)]
        public float tesseractAnimationSpeed { get; set; }

        [DefaultValue(true)]
        public bool tesseractShowPlanetTexture { get; set; }

        [DefaultValue(true)]
        public bool tesseractShowWings { get; set; }

        [DefaultValue(true)]
        public bool tesseractShowWeapon { get; set; }

    }
}