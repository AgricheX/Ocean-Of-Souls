namespace OceanOfSouls
{
    internal static class SoulDefaults
    {
        public static void Init()
        {
            string mortal = SoulLevels.TraitId(SoulLevels.MinLevel);
            foreach (ActorAsset asset in AssetManager.actor_library.list)
            {
                if (!CanHaveSoul(asset)) continue;
                asset.addTrait(mortal);
            }
        }

        public static bool CanHaveSoul(ActorAsset asset)
        {
            if (asset == null) return false;
            if (!asset.can_receive_traits || !asset.count_as_unit) return false;
            if (asset.is_boat || asset.is_boat_transport) return false;
            return true;
        }
    }
}
