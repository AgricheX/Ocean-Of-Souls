using System;
using System.Collections.Generic;
using System.Globalization;

namespace OceanOfSouls
{
    internal static class SoulConfig
    {
        public const int DefaultProtagonistDenominator = 1000000;
        public const int DefaultPriorityMinLevel = 5;
        public const int DefaultPriorityChance = 60;
        public const float DefaultWeight = 1f;
        public const int DefaultShortLifeGapYears = 5;
        public const float DefaultShortLifePenaltyPerYear = 2f;
        public const int DefaultShortLifePenaltyCap = 50;

        private static readonly int[] defaultThresholds = { 50, 500, 1500, 4500, 10000, 40000, 90000 };

        public static bool AllWorlds = false;
        public static bool TopOfLadder = false;
        public static bool DebugLog = false;
        public static bool DecorateName = true;
        public static int InheritTraitsMinLevel = 4;
        public static int MaxSoulsPerLevel = 0;

        public static int ProtagonistDenominator = DefaultProtagonistDenominator;
        public static int PriorityMinLevel = DefaultPriorityMinLevel;
        public static int PriorityChance = DefaultPriorityChance;
        public const int DefaultFingerWeight = 20;
        public static readonly int[] FingerWeights = { DefaultFingerWeight, DefaultFingerWeight, DefaultFingerWeight, DefaultFingerWeight, DefaultFingerWeight };

        public static float WeightLives = DefaultWeight;
        public static float WeightDeeds = DefaultWeight;
        public static float WeightAge = DefaultWeight;

        public static int ShortLifeGapYears = DefaultShortLifeGapYears;
        public static float ShortLifePenaltyPerYear = DefaultShortLifePenaltyPerYear;
        public static int ShortLifePenaltyCap = DefaultShortLifePenaltyCap;

        private static readonly bool[] saveLevel = { true, true, true, true, true, true, true, true };
        private static int[] thresholds = (int[])defaultThresholds.Clone();

        public static bool IsLevelSaved(int level)
        {
            if (level < SoulLevels.MinLevel || level > SoulLevels.MaxLevel) return false;
            return saveLevel[level];
        }

        public static int GetThreshold(int level)
        {
            if (level <= SoulLevels.MinLevel) return 0;
            if (level > SoulLevels.MaxLevel) level = SoulLevels.MaxLevel;
            return thresholds[level - 1];
        }

        internal static void AllWorldsCallBack(bool value)
        {
            AllWorlds = value;
        }

        internal static void TopOfLadderCallBack(bool value)
        {
            TopOfLadder = value;
        }

        internal static void MaxSoulsPerLevelCallBack(int value)
        {
            MaxSoulsPerLevel = value < 0 ? 0 : value;
        }

        internal static void DecorateNameCallBack(bool value)
        {
            DecorateName = value;
        }

        internal static void InheritTraitsMinLevelCallBack(int value)
        {
            InheritTraitsMinLevel = value < 1 ? 1 : value;
        }

        internal static void DebugLogCallBack(bool value)
        {
            DebugLog = value;
        }

        internal static void SaveLevel0CallBack(bool value) { saveLevel[0] = value; }
        internal static void SaveLevel1CallBack(bool value) { saveLevel[1] = value; }
        internal static void SaveLevel2CallBack(bool value) { saveLevel[2] = value; }
        internal static void SaveLevel3CallBack(bool value) { saveLevel[3] = value; }
        internal static void SaveLevel4CallBack(bool value) { saveLevel[4] = value; }
        internal static void SaveLevel5CallBack(bool value) { saveLevel[5] = value; }
        internal static void SaveLevel6CallBack(bool value) { saveLevel[6] = value; }
        internal static void SaveLevel7CallBack(bool value) { saveLevel[7] = value; }

        internal static void ProtagonistDenominatorCallBack(string text)
        {
            if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) && value > 0)
            {
                ProtagonistDenominator = value;
                return;
            }
            ProtagonistDenominator = DefaultProtagonistDenominator;
        }

        internal static void FingerAbsorbCallBack(int value) { FingerWeights[0] = Math.Max(0, value); }
        internal static void FingerSystemCallBack(int value) { FingerWeights[1] = Math.Max(0, value); }
        internal static void FingerSecondChanceCallBack(int value) { FingerWeights[2] = Math.Max(0, value); }
        internal static void FingerAuraCallBack(int value) { FingerWeights[3] = Math.Max(0, value); }
        internal static void FingerMemoryCallBack(int value) { FingerWeights[4] = Math.Max(0, value); }

        internal static void PriorityMinLevelCallBack(int value)
        {
            PriorityMinLevel = Math.Max(SoulLevels.MinLevel, Math.Min(SoulLevels.MaxLevel + 1, value));
        }

        internal static void PriorityChanceCallBack(int value)
        {
            PriorityChance = Math.Max(0, Math.Min(100, value));
        }

        internal static void LevelThresholdsCallBack(string text)
        {
            int[] parsed = ParseThresholds(text);
            thresholds = parsed ?? (int[])defaultThresholds.Clone();
        }

        internal static void WeightLivesCallBack(float value)
        {
            WeightLives = value < 0f ? 0f : value;
        }

        internal static void WeightDeedsCallBack(float value)
        {
            WeightDeeds = value < 0f ? 0f : value;
        }

        internal static void WeightAgeCallBack(float value)
        {
            WeightAge = value < 0f ? 0f : value;
        }

        internal static void ShortLifeGapYearsCallBack(int value)
        {
            ShortLifeGapYears = value < 0 ? 0 : value;
        }

        internal static void ShortLifePenaltyPerYearCallBack(float value)
        {
            ShortLifePenaltyPerYear = value < 0f ? 0f : value;
        }

        internal static void ShortLifePenaltyCapCallBack(int value)
        {
            ShortLifePenaltyCap = value < 0 ? 0 : value;
        }

        private static int[] ParseThresholds(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;

            string[] parts = text.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != SoulLevels.MaxLevel) return null;

            List<int> values = new List<int>(parts.Length);
            int previous = 0;
            foreach (string part in parts)
            {
                if (!int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value)) return null;
                if (value <= previous) return null;
                values.Add(value);
                previous = value;
            }
            return values.ToArray();
        }
    }
}
