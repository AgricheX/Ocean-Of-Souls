using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace OceanOfSouls
{
    internal static class SoulPaths
    {
        public const string FileExtension = ".json";
        public const string WorldMetaFile = "world.json";

        private static string root;

        public static string Root
        {
            get
            {
                if (root == null)
                {
                    root = DefaultRoot();
                }
                return root;
            }
        }

        private static string DefaultRoot()
        {
            return Path.Combine(UnityEngine.Application.persistentDataPath, "OceanOfSouls", "souls");
        }

        public static string WorldFolder(string worldId)
        {
            return Path.Combine(Root, worldId);
        }

        public static string LevelFolder(string worldId, int level)
        {
            return Path.Combine(WorldFolder(worldId), level.ToString());
        }

        private const string Separator = " ~ ";
        private const int MaxNameLength = 40;

        public static string SoulFile(string worldId, int level, string name, int peak, int lives, string soulId)
        {
            return Path.Combine(LevelFolder(worldId, level), FileName(name, peak, lives, soulId));
        }

        public static string LegacyFile(string worldId, int level, string soulId)
        {
            return Path.Combine(LevelFolder(worldId, level), soulId + FileExtension);
        }

        public static string FileName(string name, int peak, int lives, string soulId)
        {
            return SafeName(name) + Separator + peak + Separator + lives + Separator + soulId + FileExtension;
        }

        private static string SafeName(string name)
        {
            string baseName = SoulNames.Strip(name);
            if (string.IsNullOrEmpty(baseName)) return "Soul";

            char[] invalid = Path.GetInvalidFileNameChars();
            StringBuilder builder = new StringBuilder(baseName.Length);
            foreach (char c in baseName)
            {
                if (c == '~' || Array.IndexOf(invalid, c) >= 0 || char.IsControl(c))
                {
                    builder.Append('-');
                }
                else
                {
                    builder.Append(c);
                }
                if (builder.Length >= MaxNameLength) break;
            }

            string result = builder.ToString().Trim();
            return result.Length == 0 ? "Soul" : result;
        }

        public static bool TryParseFile(string path, out string name, out int peak, out int lives, out string soulId)
        {
            name = null;
            peak = 0;
            lives = 0;
            soulId = null;

            string file = Path.GetFileNameWithoutExtension(path);
            int third = file.LastIndexOf(Separator, StringComparison.Ordinal);
            if (third <= 0) return false;
            string rest = file.Substring(0, third);
            soulId = file.Substring(third + Separator.Length);

            int second = rest.LastIndexOf(Separator, StringComparison.Ordinal);
            if (second <= 0) return false;
            string livesText = rest.Substring(second + Separator.Length);
            rest = rest.Substring(0, second);

            int first = rest.LastIndexOf(Separator, StringComparison.Ordinal);
            if (first < 0) return false;
            string peakText = rest.Substring(first + Separator.Length);
            name = rest.Substring(0, first);

            if (!int.TryParse(peakText, NumberStyles.Integer, CultureInfo.InvariantCulture, out peak)) return false;
            if (!int.TryParse(livesText, NumberStyles.Integer, CultureInfo.InvariantCulture, out lives)) return false;
            return soulId.Length > 0;
        }

        public static string WorldMetaPath(string worldId)
        {
            return Path.Combine(WorldFolder(worldId), WorldMetaFile);
        }
    }
}
