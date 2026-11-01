namespace OceanOfSouls
{
    internal static class SoulSpawner
    {
        private const int TileAttempts = 80;

        public static bool Revive(SoulEntry entry)
        {
            return Spawn(entry, true);
        }

        public static bool Reincarnate(SoulEntry entry)
        {
            return Spawn(entry, false);
        }

        private static bool Spawn(SoulEntry entry, bool revive)
        {
            if (entry == null || entry.Data == null) return false;

            ActorAsset asset = string.IsNullOrEmpty(entry.Data.AssetId) ? null : AssetManager.actor_library.get(entry.Data.AssetId);
            if (asset == null) return false;

            WorldTile tile = RandomTile(asset);
            if (tile == null) return false;

            Actor born;
            SoulReincarnation.Suppress = true;
            try
            {
                born = World.world.units.spawnNewUnit(asset.id, tile, true, false, 0f, null, true, revive);
            }
            finally
            {
                SoulReincarnation.Suppress = false;
            }

            if (born == null || born.data == null) return false;

            SoulReincarnation.Apply(born, entry.Path, entry.Data, revive, true);
            if (!born.isFavorite())
            {
                born.switchFavorite();
            }
            return true;
        }

        private static WorldTile RandomTile(ActorAsset asset)
        {
            WorldTile[] tiles = World.world.tiles_list;
            if (tiles == null || tiles.Length == 0) return null;

            bool ocean = asset.force_ocean_creature;
            for (int i = 0; i < TileAttempts; i++)
            {
                WorldTile tile = tiles[Randy.randomInt(0, tiles.Length)];
                if (tile == null) continue;
                if (asset.flying || tile.Type.ground != ocean) return tile;
            }
            return null;
        }
    }
}
