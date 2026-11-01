using System;

namespace OceanOfSouls
{
    internal struct LifeResult
    {
        public int Deeds;
        public int Gain;
        public int Penalty;
        public bool ShortLife;
        public int PreviousLevel;
        public int NewLevel;
    }

    internal static class SoulScoring
    {
        public const float PointsPerLife = 5f;
        public const float AgeYearsPerPoint = 10f;
        public const float KillValue = 0.5f;
        public const float BirthValue = 2f;
        public const float RenownValue = 0.2f;
        public const float ExperienceLevelValue = 1.5f;
        public const float ReignValue = 25f;
        public const float LeaderValue = 10f;

        public static int DeedScore(LifeSummary life)
        {
            double total = life.Kills * KillValue
                           + life.Births * BirthValue
                           + life.Renown * RenownValue
                           + Math.Max(0, life.ExperienceLevel - 1) * ExperienceLevelValue
                           + life.Reigns * ReignValue
                           + life.Leaderships * LeaderValue;
            return Round(total);
        }

        public static int LevelForPoints(int points)
        {
            int level = SoulLevels.MinLevel;
            for (int candidate = SoulLevels.MinLevel + 1; candidate <= SoulLevels.MaxLevel; candidate++)
            {
                if (points >= SoulConfig.GetThreshold(candidate))
                {
                    level = candidate;
                }
            }
            return level;
        }

        public static int PointsForLevel(int level)
        {
            return SoulConfig.GetThreshold(level);
        }

        public static LifeResult Apply(SoulRecord record, LifeSummary life, int currentYear)
        {
            LifeResult result = new LifeResult
            {
                Deeds = DeedScore(life),
                PreviousLevel = record.Level
            };

            int gap = SoulConfig.ShortLifeGapYears;
            int difference = record.LastLifeAge - life.Age;
            result.ShortLife = gap > 0 && record.LastLifeAge >= 0 && difference >= gap;

            if (result.ShortLife)
            {
                int penalty = Round(SoulConfig.ShortLifePenaltyPerYear * difference);
                result.Penalty = Math.Min(SoulConfig.ShortLifePenaltyCap, penalty);
                record.Points = Math.Max(0, record.Points - result.Penalty);
            }
            else
            {
                double gain = SoulConfig.WeightLives * PointsPerLife
                              + SoulConfig.WeightDeeds * result.Deeds
                              + SoulConfig.WeightAge * life.Age / AgeYearsPerPoint;
                result.Gain = Round(gain);
                record.Points = (int)Math.Min((long)int.MaxValue, (long)record.Points + result.Gain);
            }

            record.Lives++;
            record.SoulYears = (int)Math.Min((long)int.MaxValue, (long)record.SoulYears + life.Age);
            record.LastLifeAge = life.Age;
            record.DeedTotal = (int)Math.Min((long)int.MaxValue, (long)record.DeedTotal + result.Deeds);
            record.DeedBest = Math.Max(record.DeedBest, result.Deeds);

            record.Level = LevelForPoints(record.Points);
            record.PeakLevel = Math.Max(record.PeakLevel, record.Level);
            result.NewLevel = record.Level;

            record.History.Add(new LifeEntry
            {
                Year = currentYear,
                Age = life.Age,
                Deeds = result.Deeds,
                Delta = result.ShortLife ? -result.Penalty : result.Gain,
                Level = record.Level
            });

            return result;
        }

        private static int Round(double value)
        {
            if (value > int.MaxValue) return int.MaxValue;
            if (value < int.MinValue) return int.MinValue;
            return (int)Math.Round(value, MidpointRounding.AwayFromZero);
        }
    }
}
