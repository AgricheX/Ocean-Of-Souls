namespace OceanOfSouls
{
    internal static class SoulLevels
    {
        public const int MinLevel = 0;
        public const int MaxLevel = 7;
        public const int Count = MaxLevel + 1;

        public const string GroupId = "OceanOfSouls";
        public const string ImmortalTraitId = "oos_immortal";
        public const string InfiniteTraitId = "oos_infinite_reincarnation";

        private static readonly string[] levelTraitIds = BuildIds();

        private static string[] BuildIds()
        {
            string[] ids = new string[Count];
            for (int i = 0; i < Count; i++)
            {
                ids[i] = "oos_soul_" + i;
            }
            return ids;
        }

        public static string TraitId(int level)
        {
            if (level < MinLevel) level = MinLevel;
            if (level > MaxLevel) level = MaxLevel;
            return levelTraitIds[level];
        }

        public static bool TryGetLevel(string traitId, out int level)
        {
            for (int i = 0; i < Count; i++)
            {
                if (levelTraitIds[i] == traitId)
                {
                    level = i;
                    return true;
                }
            }
            level = -1;
            return false;
        }

        public static bool IsLevelTrait(string traitId)
        {
            return TryGetLevel(traitId, out _);
        }
    }
}
