using System.Text;

namespace OceanOfSouls
{
    internal static class SoulNames
    {
        private static readonly string[] symbols = { "", "+", "*", "~", "^", "#", "&", "@" };
        private const char Mark = '\'';
        private const int MaxMarks = 3;

        public static string Symbol(int level)
        {
            if (level < 0) return string.Empty;
            if (level >= symbols.Length) level = symbols.Length - 1;
            return symbols[level];
        }

        public static string Counter(int lives)
        {
            if (lives <= 0) return string.Empty;
            if (lives <= MaxMarks) return new string(Mark, lives);
            return Mark.ToString() + lives;
        }

        public static string Strip(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;

            string result = name.Trim();

            int end = result.Length;
            while (end > 0 && char.IsDigit(result[end - 1])) end--;
            int digitsStart = end;
            while (end > 0 && result[end - 1] == Mark) end--;
            if (end < digitsStart || (end < result.Length && digitsStart == result.Length))
            {
                result = result.Substring(0, end).TrimEnd();
            }

            if (result.Length > 0)
            {
                for (int level = 1; level < symbols.Length; level++)
                {
                    if (result[0].ToString() == symbols[level])
                    {
                        result = result.Substring(1).TrimStart();
                        break;
                    }
                }
            }
            return result;
        }

        public static string Decorate(string name, int level, int lives)
        {
            string baseName = Strip(name);
            if (string.IsNullOrEmpty(baseName)) return name;
            if (!SoulConfig.DecorateName) return baseName;

            StringBuilder builder = new StringBuilder();
            builder.Append(Symbol(level));
            builder.Append(baseName);
            builder.Append(Counter(lives));
            return builder.ToString();
        }
    }
}
