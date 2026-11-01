using System;
using System.Collections.Generic;

namespace OceanOfSouls
{
    internal static class GoldenFingers
    {
        public const string ProtagonistId = "oos_protagonist";
        public const string AbsorbId = "oos_gf_absorb";
        public const string SystemId = "oos_gf_system";
        public const string SecondChanceId = "oos_gf_second_chance";
        public const string AuraId = "oos_gf_aura";
        public const string MemoryId = "oos_gf_memory";

        public static readonly string[] All = { AbsorbId, SystemId, SecondChanceId, AuraId, MemoryId };

        private const string AbsorbedKey = "oos_gf_absorbed";
        private const string SecondChanceKey = "oos_gf_sc_year";
        private const int AbsorbStealChance = 50;
        private const int SecondChanceYears = 50;
        private const int AuraRadius = 8;
        private const int AbsorbCap = 600;
        private const int MemoryCap = 100;
        private const float AbsorbHeal = 0.25f;
        private const float AuraStatusSeconds = 10f;

        private static readonly string[] blockedSteals = { "immortal" };
        private static readonly Random random = new Random();

        public static bool IsSpecial(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            return id == ProtagonistId || Array.IndexOf(All, id) >= 0;
        }

        public static string RandomFinger()
        {
            int total = 0;
            foreach (int weight in SoulConfig.FingerWeights) total += weight;
            if (total <= 0) return null;

            int roll = random.Next(total);
            for (int i = 0; i < All.Length; i++)
            {
                roll -= SoulConfig.FingerWeights[i];
                if (roll < 0) return All[i];
            }
            return null;
        }

        public static void MakeProtagonist(Actor actor)
        {
            actor.addTrait("blessed", true);
            actor.addTrait("chosen_one", true);
            actor.addTrait("strong_minded", true);
            actor.addTrait("genius", true);
            actor.addTrait(ProtagonistId);
            if (!HasFinger(actor))
            {
                string finger = RandomFinger();
                if (finger != null) actor.addTrait(finger);
            }
            actor.setStatsDirty();
        }

        public static bool HasFinger(Actor actor)
        {
            foreach (string id in All)
            {
                if (actor.hasTrait(id)) return true;
            }
            return false;
        }

        public static void ApplyStats(Actor actor, BaseStats stats)
        {
            if (actor == null || actor.data == null || stats == null) return;

            if (actor.hasTrait(AbsorbId))
            {
                actor.data.get(AbsorbedKey, out int absorbed);
                absorbed = Math.Min(absorbed, AbsorbCap);
                stats["health"] += absorbed * 1.5f;
                stats["damage"] += absorbed * 0.15f;
            }

            if (actor.hasTrait(MemoryId))
            {
                float lives = Math.Min(SoulRecord.Load(actor.data).Lives, MemoryCap);
                stats["health"] += lives * 15f;
                stats["damage"] += lives * 1f;
                stats["armor"] += lives * 0.3f;
                stats["lifespan"] += lives * 2f;
            }
        }

        public static void OnKill(Actor killer, Actor dead)
        {
            if (killer == null || dead == null || killer.data == null) return;
            if (!killer.isAlive() || !killer.hasTrait(AbsorbId)) return;

            killer.data.get(AbsorbedKey, out int absorbed);
            killer.data.set(AbsorbedKey, absorbed + 1);
            killer.restoreHealthPercent(AbsorbHeal);

            if (random.Next(100) < AbsorbStealChance)
            {
                StealTrait(killer, dead);
            }
            killer.setStatsDirty();
        }

        private static void StealTrait(Actor killer, Actor dead)
        {
            List<ActorTrait> pool = new List<ActorTrait>();
            foreach (ActorTrait trait in dead.getTraits())
            {
                if (IsStealable(killer, trait)) pool.Add(trait);
            }
            if (pool.Count == 0) return;

            killer.addTrait(pool[random.Next(pool.Count)]);
        }

        private static bool IsStealable(Actor actor, ActorTrait trait)
        {
            if (trait == null || trait.type != TraitType.Positive) return false;
            if (trait.group_id == SoulLevels.GroupId) return false;
            if (Array.IndexOf(blockedSteals, trait.id) >= 0) return false;
            return !actor.hasTrait(trait);
        }

        public static void OnLevelUp(Actor actor)
        {
            if (actor == null || !actor.hasTrait(SystemId)) return;

            List<ActorTrait> pool = new List<ActorTrait>();
            foreach (ActorTrait trait in AssetManager.traits.list)
            {
                if (IsStealable(actor, trait) && !actor.hasOppositeTrait(trait)) pool.Add(trait);
            }

            for (int attempt = 0; attempt < 6 && pool.Count > 0; attempt++)
            {
                int index = random.Next(pool.Count);
                ActorTrait pick = pool[index];
                pool.RemoveAt(index);
                if (actor.addTrait(pick)) return;
            }
        }

        public static bool TrySecondChance(Actor actor, bool destroy)
        {
            if (destroy || actor == null || actor.data == null) return false;
            if (!actor.hasTrait(SecondChanceId)) return false;

            int year = Date.getYear(World.world.getCurWorldTime()) + 1;
            actor.data.get(SecondChanceKey, out int last);
            if (last != 0 && year - last < SecondChanceYears) return false;

            actor.data.set(SecondChanceKey, year);
            actor.restoreHealthPercent(1f);
            actor.addStatusEffect("invincible", 5f);
            return true;
        }

        public static bool AuraEffect(BaseSimObject self, WorldTile tile = null)
        {
            Actor owner = self?.a;
            if (owner == null || !owner.isAlive() || tile == null) return false;

            foreach (Actor ally in Finder.getUnitsFromChunk(tile, 1, AuraRadius))
            {
                if (ally == owner || !ally.isAlive()) continue;
                if (ally.kingdom != owner.kingdom || owner.areFoes(ally)) continue;

                ally.addStatusEffect("inspired", AuraStatusSeconds);
                ally.addStatusEffect("powerup", AuraStatusSeconds);
            }

            if (random.Next(5) == 0)
            {
                owner.addRenown(1);
            }
            return true;
        }
    }
}
