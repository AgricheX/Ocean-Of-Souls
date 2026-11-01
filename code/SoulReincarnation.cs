using System;
using System.Collections.Generic;

namespace OceanOfSouls
{
    internal static class SoulReincarnation
    {
        private static readonly Random random = new Random();

        public static bool Suppress;

        private const int ReplaceNegativeMinLevel = 4;

        public static int TraitInheritCount(int level)
        {
            int minLevel = SoulConfig.InheritTraitsMinLevel;
            if (level < minLevel) return 0;
            if (level >= SoulLevels.MaxLevel) return int.MaxValue;
            return level - minLevel + 1;
        }

        public static bool CanReincarnateInto(Actor actor)
        {
            if (Suppress || actor == null || actor.data == null) return false;
            if (!SoulDefaults.CanHaveSoul(actor.asset)) return false;
            if (SoulRecord.Load(actor.data).Exists) return false;
            return SoulService.GetTraitLevel(actor) <= SoulLevels.MinLevel;
        }

        public static bool TryReincarnate(Actor actor)
        {
            if (!CanReincarnateInto(actor)) return false;

            bool protagonist = random.Next(SoulConfig.ProtagonistDenominator) == 0;
            if (!SoulStorage.TryPickRandom(SoulWorld.CurrentId(), !protagonist, out string path, out SoulData data)) return false;

            Apply(actor, path, data, false, false, protagonist);
            return true;
        }

        public static void Apply(Actor actor, string path, SoulData data, bool revive = false, bool keepSpecial = false, bool protagonist = false)
        {
            SoulRecord record = data.ToRecord();
            record.Level = SoulScoring.LevelForPoints(record.Points);
            record.PeakLevel = Math.Max(record.PeakLevel, record.Level);

            SoulStorage.Delete(path);
            SoulService.ApplyLevelTrait(actor, record.Level);
            SoulRules.CarryManual(actor, data);
            SoulRules.Evaluate(actor, record);
            record.Store(actor.data);

            actor.setName(revive ? data.Name : SoulNames.Decorate(data.Name, record.Level, record.Lives), false);
            int count = revive || protagonist ? int.MaxValue : TraitInheritCount(record.Level);
            Inherit(actor, data, count, revive || keepSpecial, protagonist || record.Level >= ReplaceNegativeMinLevel);
            if (protagonist)
            {
                GoldenFingers.MakeProtagonist(actor);
            }

            if (data.Favorite && !actor.isFavorite())
            {
                actor.switchFavorite();
            }
            actor.setStatsDirty();

            if (SoulConfig.DebugLog)
            {
                OceanOfSoulsMod.LogInfo($"{actor.getName()} reincarnated: soul level {record.Level}, lives {record.Lives}, points {record.Points}");
            }
        }

        private static void Inherit(Actor actor, SoulData data, int count, bool keepSpecial, bool replaceNegative)
        {
            if (data.Traits == null) return;

            List<ActorTrait> pool = new List<ActorTrait>();
            foreach (string id in data.Traits)
            {
                if (string.IsNullOrEmpty(id)) continue;

                ActorTrait trait = AssetManager.traits.get(id);
                if (trait == null) continue;

                if (GoldenFingers.IsSpecial(id))
                {
                    if (keepSpecial) actor.addTrait(trait);
                    continue;
                }
                if (trait.group_id == SoulLevels.GroupId) continue;
                pool.Add(trait);
            }

            if (count <= 0) return;

            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                ActorTrait swap = pool[i];
                pool[i] = pool[j];
                pool[j] = swap;
            }

            int added = 0;
            foreach (ActorTrait trait in pool)
            {
                if (added >= count) break;
                if (actor.hasTrait(trait)) continue;

                bool replace = false;
                if (actor.hasOppositeTrait(trait))
                {
                    if (!replaceNegative || !OnlyNegativeOpposites(actor, trait)) continue;
                    replace = true;
                }

                if (actor.addTrait(trait, replace)) added++;
            }
        }

        private static bool OnlyNegativeOpposites(Actor actor, ActorTrait trait)
        {
            foreach (ActorTrait opposite in trait.opposite_traits)
            {
                if (opposite.type != TraitType.Negative && actor.hasTrait(opposite)) return false;
            }
            return true;
        }
    }
}
