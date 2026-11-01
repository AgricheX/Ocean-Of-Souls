using System.Collections.Generic;

namespace OceanOfSouls
{
    internal static class SoulTraits
    {
        private static readonly Rarity[] levelRarity =
        {
            Rarity.R0_Normal,
            Rarity.R0_Normal,
            Rarity.R1_Rare,
            Rarity.R1_Rare,
            Rarity.R2_Epic,
            Rarity.R2_Epic,
            Rarity.R3_Legendary,
            Rarity.R3_Legendary
        };

        public static void Init()
        {
            List<ActorTrait> levelTraits = new List<ActorTrait>(SoulLevels.Count);

            for (int level = SoulLevels.MinLevel; level <= SoulLevels.MaxLevel; level++)
            {
                ActorTrait trait = Create(SoulLevels.TraitId(level), levelRarity[level]);
                AssetManager.traits.add(trait);
                levelTraits.Add(trait);
            }

            LinkExclusive(levelTraits);

            AssetManager.traits.add(Create(SoulLevels.ImmortalTraitId, Rarity.R3_Legendary));
            AssetManager.traits.add(Create(SoulLevels.InfiniteTraitId, Rarity.R3_Legendary));

            ActorTrait protagonist = Create(GoldenFingers.ProtagonistId, Rarity.R3_Legendary);
            AssetManager.traits.add(protagonist);
            protagonist.base_stats["health"] = 400f;
            protagonist.base_stats["damage"] = 20f;
            protagonist.base_stats["armor"] = 10f;
            protagonist.base_stats["lifespan"] = 150f;
            protagonist.base_stats["intelligence"] = 10f;
            protagonist.base_stats["warfare"] = 10f;
            protagonist.base_stats["stewardship"] = 10f;
            protagonist.base_stats["diplomacy"] = 10f;
            protagonist.base_stats["critical_chance"] = 0.1f;

            foreach (string id in GoldenFingers.All)
            {
                ActorTrait finger = Create(id, Rarity.R3_Legendary);
                if (id == GoldenFingers.AuraId)
                {
                    finger.action_special_effect = GoldenFingers.AuraEffect;
                    finger.special_effect_interval = 4f;
                }
                AssetManager.traits.add(finger);
            }
        }

        private static ActorTrait Create(string id, Rarity rarity)
        {
            return new ActorTrait
            {
                id = id,
                path_icon = "trait/" + id,
                group_id = SoulLevels.GroupId,
                type = TraitType.Other,
                rarity = rarity,
                needs_to_be_explored = false,
                spawn_random_trait_allowed = false,
                spawn_random_rate = 0,
                rate_birth = 0,
                rate_acquire_grow_up = 0,
                rate_inherit = 0,
                is_mutation_box_allowed = false,
                can_be_in_book = false
            };
        }

        private static void LinkExclusive(List<ActorTrait> traits)
        {
            foreach (ActorTrait trait in traits)
            {
                trait.opposite_list = new List<string>();
                trait.opposite_traits = new HashSet<ActorTrait>();
                foreach (ActorTrait other in traits)
                {
                    if (other == trait) continue;
                    trait.opposite_list.Add(other.id);
                    trait.opposite_traits.Add(other);
                }
            }
        }
    }
}
