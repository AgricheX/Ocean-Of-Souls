namespace OceanOfSouls
{
    internal static class SoulRebirth
    {
        public static bool ShouldRevive(Actor actor, bool pDestroy, bool pCountDeath)
        {
            return pCountDeath && !pDestroy && actor.hasTrait(SoulLevels.ImmortalTraitId);
        }

        public static void Revive(Actor actor)
        {
            actor.restoreHealthPercent(1f);
        }

        public static bool ShouldRebirth(Actor actor)
        {
            return actor.hasTrait(SoulLevels.InfiniteTraitId);
        }

        public static bool Rebirth(Actor dying)
        {
            if (dying == null || dying.data == null || dying.asset == null) return false;

            SoulRecord record = SoulRecord.Load(dying.data);
            if (!record.Exists) return false;

            WorldTile tile = FindTile(dying);
            if (tile == null) return false;

            SoulData data = SoulData.Create(dying, record);
            Subspecies subspecies = dying.hasSubspecies() && !dying.subspecies.isRekt() ? dying.subspecies : null;

            Actor born;
            SoulReincarnation.Suppress = true;
            try
            {
                born = World.world.units.spawnNewUnit(dying.asset.id, tile, false, false, 0f, subspecies);
            }
            finally
            {
                SoulReincarnation.Suppress = false;
            }

            if (born == null || born.data == null) return false;

            SoulReincarnation.Apply(born, null, data, false, true);
            return true;
        }

        private static WorldTile FindTile(Actor actor)
        {
            WorldTile tile = actor.current_tile;
            if (tile == null) return null;
            if (actor.asset.flying || actor.asset.force_ocean_creature || tile.Type.ground) return tile;

            foreach (WorldTile neighbour in tile.neighboursAll)
            {
                if (neighbour != null && neighbour.Type.ground) return neighbour;
            }
            return null;
        }
    }
}
