using System;

namespace OceanOfSouls
{
    internal static class SoulService
    {
        public static int GetTraitLevel(Actor actor)
        {
            if (actor == null) return -1;
            for (int level = SoulLevels.MaxLevel; level >= SoulLevels.MinLevel; level--)
            {
                if (actor.hasTrait(SoulLevels.TraitId(level))) return level;
            }
            return -1;
        }

        public static bool HasSoul(Actor actor)
        {
            return GetTraitLevel(actor) >= 0;
        }

        public static void ApplyLevelTrait(Actor actor, int level)
        {
            if (actor == null) return;

            string target = SoulLevels.TraitId(level);
            for (int i = SoulLevels.MinLevel; i <= SoulLevels.MaxLevel; i++)
            {
                string id = SoulLevels.TraitId(i);
                if (id != target && actor.hasTrait(id))
                {
                    actor.removeTrait(id);
                }
            }
            if (!actor.hasTrait(target))
            {
                actor.addTrait(target);
            }
        }

        public static bool TryRegisterDeath(Actor actor, out LifeResult result)
        {
            result = default;
            if (actor == null || actor.data == null) return false;

            int traitLevel = GetTraitLevel(actor);
            if (traitLevel < 0) return false;

            SoulRecord record = SoulRecord.Load(actor.data);
            if (!record.Exists)
            {
                record.Level = traitLevel;
                record.PeakLevel = traitLevel;
                record.Points = SoulScoring.PointsForLevel(traitLevel);
                record.LastLifeAge = -1;
            }

            if (string.IsNullOrEmpty(record.SoulId))
            {
                record.SoulId = Guid.NewGuid().ToString("N");
            }

            LifeSummary life = LifeSummary.From(actor);
            int year = Date.getYear(World.world.getCurWorldTime());
            result = SoulScoring.Apply(record, life, year);
            ApplyLevelTrait(actor, record.Level);
            SoulRules.Evaluate(actor, record);
            record.Store(actor.data);

            if (SoulConfig.IsLevelSaved(record.Level) && !SoulRebirth.ShouldRebirth(actor))
            {
                SoulStorage.Save(SoulData.Create(actor, record));
            }
            return true;
        }
    }
}
