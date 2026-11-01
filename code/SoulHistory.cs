using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace OceanOfSouls
{
    internal struct LifeEntry
    {
        public int Year;
        public int Age;
        public int Deeds;
        public int Delta;
        public int Level;
    }

    internal static class SoulHistory
    {
        public const int MaxEntries = 20;

        public static List<LifeEntry> Parse(string text)
        {
            List<LifeEntry> entries = new List<LifeEntry>();
            if (string.IsNullOrEmpty(text)) return entries;

            foreach (string raw in text.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = raw.Split(',');
                if (parts.Length != 5) continue;
                if (!TryInt(parts[0], out int year)) continue;
                if (!TryInt(parts[1], out int age)) continue;
                if (!TryInt(parts[2], out int deeds)) continue;
                if (!TryInt(parts[3], out int delta)) continue;
                if (!TryInt(parts[4], out int level)) continue;
                entries.Add(new LifeEntry { Year = year, Age = age, Deeds = deeds, Delta = delta, Level = level });
            }
            return entries;
        }

        public static string Serialize(List<LifeEntry> entries)
        {
            StringBuilder builder = new StringBuilder();
            int start = Math.Max(0, entries.Count - MaxEntries);
            for (int i = start; i < entries.Count; i++)
            {
                LifeEntry entry = entries[i];
                if (builder.Length > 0) builder.Append(';');
                builder.Append(entry.Year.ToString(CultureInfo.InvariantCulture)).Append(',');
                builder.Append(entry.Age.ToString(CultureInfo.InvariantCulture)).Append(',');
                builder.Append(entry.Deeds.ToString(CultureInfo.InvariantCulture)).Append(',');
                builder.Append(entry.Delta.ToString(CultureInfo.InvariantCulture)).Append(',');
                builder.Append(entry.Level.ToString(CultureInfo.InvariantCulture));
            }
            return builder.ToString();
        }

        private static bool TryInt(string text, out int value)
        {
            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }
    }
}
