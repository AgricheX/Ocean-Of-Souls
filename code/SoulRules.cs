using System;

namespace OceanOfSouls
{
    internal static class SoulRules
    {
        private const double TieBand = 0.1;

        private static readonly Random random = new Random();

        public static string ChooseTopTrait(SoulRecord record)
        {
            double lives = record.Lives * SoulScoring.PointsPerLife * SoulConfig.WeightLives;
            double deeds = record.DeedBest * SoulConfig.WeightDeeds;
            double larger = Math.Max(lives, deeds);

            if (larger <= 0d || Math.Abs(lives - deeds) <= larger * TieBand)
            {
                return random.Next(2) == 0 ? SoulLevels.InfiniteTraitId : SoulLevels.ImmortalTraitId;
            }
            return lives > deeds ? SoulLevels.InfiniteTraitId : SoulLevels.ImmortalTraitId;
        }

        public static bool HasTopTrait(Actor actor)
        {
            return actor.hasTrait(SoulLevels.ImmortalTraitId) || actor.hasTrait(SoulLevels.InfiniteTraitId);
        }

        public static void Evaluate(Actor actor, SoulRecord record)
        {
            if (actor == null || record == null) return;

            bool wanted = SoulConfig.TopOfLadder && record.Level == SoulLevels.MaxLevel;
            if (wanted)
            {
                if (HasTopTrait(actor)) return;

                string id = string.IsNullOrEmpty(record.Granted) ? ChooseTopTrait(record) : record.Granted;
                if (actor.addTrait(id))
                {
                    record.Granted = id;
                }
                return;
            }

            if (!string.IsNullOrEmpty(record.Granted))
            {
                actor.removeTrait(record.Granted);
                record.Granted = null;
            }
        }

        public static void CarryManual(Actor actor, SoulData data)
        {
            if (data.Traits == null) return;

            foreach (string id in new[] { SoulLevels.ImmortalTraitId, SoulLevels.InfiniteTraitId })
            {
                if (id == data.Granted || !data.Traits.Contains(id)) continue;
                actor.addTrait(id);
            }
        }
    }
}
