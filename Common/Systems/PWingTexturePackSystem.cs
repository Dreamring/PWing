using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace PWing.Common.Systems
{
    // 将材质包贴图替换到原版纹理索引上，卸载时逐个恢复
    public class PWingTexturePackSystem : ModSystem
    {
        private const string TexturePackRoot = "Assets/TexturePack/";
        private const string RawImageSuffix = ".rawimg";

        private static readonly Regex PlayerRegex = new Regex(@"^Player_(\d+)_(\d+)$", RegexOptions.Compiled);
        private static readonly Regex PlayerHairRegex = new Regex(@"^Player_Hair_(\d+)$", RegexOptions.Compiled);
        private static readonly Regex WingsRegex = new Regex(@"^Wings_(\d+)$", RegexOptions.Compiled);
        private static readonly Regex AccBackRegex = new Regex(@"^Acc_Back_(\d+)$", RegexOptions.Compiled);
        private static readonly Regex ExtraRegex = new Regex(@"^Extra_(\d+)$", RegexOptions.Compiled);
        private static readonly Regex GlowRegex = new Regex(@"^Glow_(\d+)$", RegexOptions.Compiled);
        private static readonly Regex ProjectileRegex = new Regex(@"^Projectile_(\d+)$", RegexOptions.Compiled);

        private readonly List<Action> restores = new List<Action>();

        public override void PostSetupContent()
        {
            // 专用服务器不加载任何客户端贴图
            if (Main.dedServ)
                return;

            foreach (string rawName in Mod.GetFileNames())
            {
                try
                {
                    ApplyFile(rawName);
                }
                catch
                {
                    // 单个文件处理失败不影响整体启动
                }
            }
        }

        private void ApplyFile(string rawName)
        {
            if (string.IsNullOrEmpty(rawName))
                return;

            string fileName = rawName.Replace('\\', '/');
            if (!fileName.StartsWith(TexturePackRoot, StringComparison.OrdinalIgnoreCase))
                return;
            if (!fileName.EndsWith(RawImageSuffix, StringComparison.OrdinalIgnoreCase))
                return;

            // 去掉 .rawimg 后的完整路径，如 Assets/TexturePack/Player/Player_0_0
            string assetPath = fileName.Substring(0, fileName.Length - RawImageSuffix.Length);
            string shortName = assetPath.Substring(assetPath.LastIndexOf('/') + 1);

            Asset<Texture2D> texture;
            try
            {
                texture = ModContent.Request<Texture2D>("PWing/" + assetPath, AssetRequestMode.ImmediateLoad);
            }
            catch
            {
                return;
            }

            if (texture == null)
                return;

            ApplyTexture(shortName, texture);
        }

        private void ApplyTexture(string name, Asset<Texture2D> texture)
        {
            Match match = PlayerRegex.Match(name);
            if (match.Success)
            {
                int variant = int.Parse(match.Groups[1].Value);
                int part = int.Parse(match.Groups[2].Value);
                ReplaceTexture(TextureAssets.Players, variant, part, texture);
                return;
            }

            match = PlayerHairRegex.Match(name);
            if (match.Success)
            {
                ReplaceTexture(TextureAssets.PlayerHair, int.Parse(match.Groups[1].Value), texture);
                return;
            }

            match = WingsRegex.Match(name);
            if (match.Success)
            {
                ReplaceTexture(TextureAssets.Wings, int.Parse(match.Groups[1].Value), texture);
                return;
            }

            match = AccBackRegex.Match(name);
            if (match.Success)
            {
                ReplaceTexture(TextureAssets.AccBack, int.Parse(match.Groups[1].Value), texture);
                return;
            }

            match = ExtraRegex.Match(name);
            if (match.Success)
            {
                ReplaceTexture(TextureAssets.Extra, int.Parse(match.Groups[1].Value), texture);
                return;
            }

            match = GlowRegex.Match(name);
            if (match.Success)
            {
                ReplaceTexture(TextureAssets.GlowMask, int.Parse(match.Groups[1].Value), texture);
                return;
            }

            match = ProjectileRegex.Match(name);
            if (match.Success)
            {
                ReplaceTexture(TextureAssets.Projectile, int.Parse(match.Groups[1].Value), texture);
            }
        }

        // 一维纹理数组替换
        private void ReplaceTexture(Asset<Texture2D>[] array, int index, Asset<Texture2D> texture)
        {
            if (array == null || index < 0 || index >= array.Length)
                return;

            Asset<Texture2D> original = array[index];
            array[index] = texture;
            restores.Add(() => array[index] = original);
        }

        // 二维纹理数组替换（玩家部件）
        private void ReplaceTexture(Asset<Texture2D>[,] array, int index0, int index1, Asset<Texture2D> texture)
        {
            if (array == null)
                return;
            if (index0 < 0 || index0 >= array.GetLength(0))
                return;
            if (index1 < 0 || index1 >= array.GetLength(1))
                return;

            Asset<Texture2D> original = array[index0, index1];
            array[index0, index1] = texture;
            restores.Add(() => array[index0, index1] = original);
        }

        public override void Unload()
        {
            // 倒序恢复，避免重复替换时覆盖错误的原值
            for (int i = restores.Count - 1; i >= 0; i--)
            {
                try
                {
                    restores[i]();
                }
                catch
                {
                    // 单个恢复失败不影响其余恢复
                }
            }

            restores.Clear();
        }
    }
}
