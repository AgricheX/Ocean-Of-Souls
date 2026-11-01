using HarmonyLib;
using NeoModLoader.api;

namespace OceanOfSouls
{
    internal class OceanOfSoulsMod : BasicMod<OceanOfSoulsMod>
    {
        public const string HarmonyId = "oceanofsouls.mod";

        protected override void OnModLoad()
        {
            SoulTraitGroup.Init();
            SoulTraits.Init();
            SoulDefaults.Init();
            SoulStorage.Init();

            try
            {
                PantheonButton.Init(GetDeclaration().FolderPath, GetConfig);
            }
            catch (System.Exception e)
            {
                LogWarning("Pantheon setup failed: " + e.Message);
            }

            Harmony harmony = new Harmony(HarmonyId);
            harmony.PatchAll(typeof(SoulTracker));
            harmony.PatchAll(typeof(GoldenFingerPatches));
        }
    }
}
