namespace OceanOfSouls
{
    internal static class SoulTraitGroup
    {
        public static void Init()
        {
            AssetManager.trait_groups.add(new ActorTraitGroupAsset
            {
                id = SoulLevels.GroupId,
                name = "trait_group_" + SoulLevels.GroupId,
                color = "#7FD8FF"
            });
        }
    }
}
