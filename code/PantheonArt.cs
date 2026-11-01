using System.IO;
using NeoModLoader.utils;
using UnityEngine;

namespace OceanOfSouls
{
    internal static class PantheonArt
    {
        public static Sprite Banner;
        public static Sprite RowBackground;
        public static Sprite Wave;
        public static Sprite Star;
        public static Sprite Wisp;
        public static readonly Sprite[] Orbs = new Sprite[SoulLevels.Count];

        private static readonly Color32[] levelColors =
        {
            new Color32(120, 134, 150, 255),
            new Color32(96, 170, 210, 255),
            new Color32(84, 190, 190, 255),
            new Color32(96, 200, 140, 255),
            new Color32(230, 200, 90, 255),
            new Color32(240, 150, 70, 255),
            new Color32(230, 100, 150, 255),
            new Color32(250, 245, 255, 255)
        };

        public static void Load(string modFolder)
        {
            string ui = Path.Combine(modFolder, "GameResources", "ui");
            Banner = Read(Path.Combine(ui, "banner.png"), true);
            RowBackground = Read(Path.Combine(ui, "row_bg.png"), true);
            Wave = Read(Path.Combine(ui, "wave.png"), true);
            Star = Read(Path.Combine(ui, "star.png"), true);
            Wisp = Read(Path.Combine(ui, "wisp.png"), true);

            string traits = Path.Combine(modFolder, "GameResources", "trait");
            for (int level = SoulLevels.MinLevel; level <= SoulLevels.MaxLevel; level++)
            {
                Orbs[level] = Read(Path.Combine(traits, SoulLevels.TraitId(level) + ".png"), false);
            }
        }

        public static Color LevelColor(int level)
        {
            if (level < SoulLevels.MinLevel) level = SoulLevels.MinLevel;
            if (level > SoulLevels.MaxLevel) level = SoulLevels.MaxLevel;
            return levelColors[level];
        }

        public static string LevelHex(int level)
        {
            Color light = Color.Lerp(LevelColor(level), Color.white, 0.55f);
            return ColorUtility.ToHtmlStringRGB(light);
        }

        private static Sprite Read(string path, bool pixel)
        {
            if (!File.Exists(path)) return null;

            Sprite sprite = SpriteLoadUtils.LoadSingleSprite(path);
            if (sprite != null && sprite.texture != null)
            {
                sprite.texture.filterMode = pixel ? FilterMode.Point : FilterMode.Bilinear;
            }
            return sprite;
        }
    }
}
