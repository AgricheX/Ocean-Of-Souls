using System;

namespace OceanOfSouls
{
    internal static class SoulWorld
    {
        private const string WorldIdKey = "oos_world_id";
        private const string UnknownWorld = "unknown";

        public static string CurrentId()
        {
            MapStats stats = World.world?.map_stats;
            if (stats == null) return UnknownWorld;

            if (stats.custom_data == null)
            {
                stats.custom_data = new SaveCustomData();
            }

            stats.custom_data.get(WorldIdKey, out string id, null);
            if (string.IsNullOrEmpty(id))
            {
                id = Guid.NewGuid().ToString("N");
                stats.custom_data.set(WorldIdKey, id);
            }
            return id;
        }

        public static string CurrentName()
        {
            string name = World.world?.map_stats?.name;
            return string.IsNullOrEmpty(name) ? UnknownWorld : name;
        }
    }
}
