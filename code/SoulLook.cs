using System;

namespace OceanOfSouls
{
    internal sealed class SoulLook
    {
        public int Sex;
        public int Head = -1;
        public int PhenotypeIndex;
        public int PhenotypeShade;
        public string KingdomColor;
        public string Mutation;
        public bool King;
        public bool Wise;
        public bool Adult = true;

        public static SoulLook From(Actor actor)
        {
            SoulLook look = new SoulLook();
            if (actor == null || actor.data == null) return look;

            ActorData data = actor.data;
            look.Sex = (int)data.sex;
            look.Head = data.head;
            look.PhenotypeIndex = data.phenotype_index;
            look.PhenotypeShade = data.phenotype_shade;
            look.King = actor.isKing();
            look.Wise = actor.hasTrait("wise");
            look.Adult = actor.isAdult();

            ColorAsset color = actor.kingdom?.getColor();
            look.KingdomColor = color?.id;
            look.Mutation = actor.subspecies?.mutation_skin_asset?.id;
            return look;
        }

        public static ActorAvatarData ToAvatarData(SoulLook look, ActorAsset asset, string soulId)
        {
            SoulLook source = look ?? new SoulLook();

            SubspeciesTrait mutation = string.IsNullOrEmpty(source.Mutation) ? null : AssetManager.subspecies_traits.get(source.Mutation);
            ColorAsset color = string.IsNullOrEmpty(source.KingdomColor) ? null : AssetManager.kingdom_colors_library.get(source.KingdomColor);
            if (color == null)
            {
                string kingdomId = string.IsNullOrEmpty(asset.kingdom_id_civilization) ? asset.kingdom_id_wild : asset.kingdom_id_civilization;
                KingdomAsset kingdom = string.IsNullOrEmpty(kingdomId) ? null : AssetManager.kingdoms.get(kingdomId);
                color = kingdom?.default_kingdom_color;
            }

            long seed = string.IsNullOrEmpty(soulId) ? 0 : Math.Abs((long)soulId.GetHashCode());
            int phenotype = source.PhenotypeIndex;
            int shade = source.PhenotypeShade;
            if (phenotype == 0 && asset.phenotypes_list != null && asset.phenotypes_list.Count > 0)
            {
                PhenotypeAsset fallback = AssetManager.phenotype_library.get(asset.phenotypes_list[(int)(seed % asset.phenotypes_list.Count)]);
                if (fallback != null)
                {
                    phenotype = fallback.phenotype_index;
                    shade = (int)((seed / 7) % 4);
                }
            }
            if (shade < 0 || shade > 3) shade = 0;

            ActorAvatarData avatar = new ActorAvatarData();
            avatar.setData(
                asset, mutation, (ActorSex)source.Sex, seed, source.Head, null,
                phenotype, shade, color,
                false, source.King, false, source.Wise, null, source.Adult,
                false, false, false, false, false, false, false,
                null, (int)(seed & 0x7FFFFFFF), null, null);
            return avatar;
        }
    }
}
