using PWing.Content.Vanity.Items;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace PWing.Content.Vanity
{
    public sealed class VanitySet
    {
        public string NameKey;       // "LilyAsuka" 等，用于本地化 UI 文本
        public int HeadType;
        public int BodyType;
        public int LegsType;
        public string HeadTexture;   // "PWing/Content/Vanity/Items/LilyAsukaHead_Head"
        public string BodyTexture;
        public string LegsTexture;
    }

    public static class VanitySets
    {
        private static IReadOnlyList<VanitySet> all;

        // 懒加载：属性首次访问时用 ModContent.ItemType<T>() 构建，避免在内容加载前调用
        public static IReadOnlyList<VanitySet> All
        {
            get
            {
                if (all == null)
                {
                    all = BuildSets();
                }

                return all;
            }
        }

        public static void Unload()
        {
            all = null;
        }

        private static IReadOnlyList<VanitySet> BuildSets()
        {
            return new List<VanitySet>
            {
                new VanitySet
                {
                    NameKey = "LilyAsuka",
                    HeadType = ModContent.ItemType<LilyAsukaHead>(),
                    BodyType = ModContent.ItemType<LilyAsukaBody>(),
                    LegsType = ModContent.ItemType<LilyAsukaLegs>(),
                    HeadTexture = "PWing/Content/Vanity/Items/LilyAsukaHead_Head",
                    BodyTexture = "PWing/Content/Vanity/Items/LilyAsukaBody_Body",
                    LegsTexture = "PWing/Content/Vanity/Items/LilyAsukaLegs_Legs"
                },
                new VanitySet
                {
                    NameKey = "KhoirBach",
                    HeadType = ModContent.ItemType<KhoirBachHead>(),
                    BodyType = ModContent.ItemType<KhoirBachBody>(),
                    LegsType = ModContent.ItemType<KhoirBachLegs>(),
                    HeadTexture = "PWing/Content/Vanity/Items/KhoirBachHead_Head",
                    BodyTexture = "PWing/Content/Vanity/Items/KhoirBachBody_Body",
                    LegsTexture = "PWing/Content/Vanity/Items/KhoirBachLegs_Legs"
                },
                new VanitySet
                {
                    NameKey = "RiasGremory",
                    HeadType = ModContent.ItemType<RiasGremoryHead>(),
                    BodyType = ModContent.ItemType<RiasGremoryBody>(),
                    LegsType = ModContent.ItemType<RiasGremoryLegs>(),
                    HeadTexture = "PWing/Content/Vanity/Items/RiasGremoryHead_Head",
                    BodyTexture = "PWing/Content/Vanity/Items/RiasGremoryBody_Body",
                    LegsTexture = "PWing/Content/Vanity/Items/RiasGremoryLegs_Legs"
                },
                new VanitySet
                {
                    NameKey = "Ram",
                    HeadType = ModContent.ItemType<RamHead>(),
                    BodyType = ModContent.ItemType<RamBody>(),
                    LegsType = ModContent.ItemType<RamLegs>(),
                    HeadTexture = "PWing/Content/Vanity/Items/RamHead_Head",
                    BodyTexture = "PWing/Content/Vanity/Items/RamBody_Body",
                    LegsTexture = "PWing/Content/Vanity/Items/RamLegs_Legs"
                },
                new VanitySet
                {
                    NameKey = "Rem",
                    HeadType = ModContent.ItemType<RemHead>(),
                    BodyType = ModContent.ItemType<RemBody>(),
                    LegsType = ModContent.ItemType<RemLegs>(),
                    HeadTexture = "PWing/Content/Vanity/Items/RemHead_Head",
                    BodyTexture = "PWing/Content/Vanity/Items/RemBody_Body",
                    LegsTexture = "PWing/Content/Vanity/Items/RemLegs_Legs"
                }
            };
        }
    }
}
