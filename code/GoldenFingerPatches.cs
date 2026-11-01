using System;
using HarmonyLib;

namespace OceanOfSouls
{
    internal static class GoldenFingerPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Actor), "newKillAction")]
        private static void KillPostfix(Actor __instance, Actor pDeadUnit)
        {
            try
            {
                GoldenFingers.OnKill(__instance, pDeadUnit);
            }
            catch (Exception e)
            {
                OceanOfSoulsMod.LogWarning("Absorption failed: " + e.Message);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Actor), "levelUp")]
        private static void LevelUpPostfix(Actor __instance)
        {
            try
            {
                GoldenFingers.OnLevelUp(__instance);
            }
            catch (Exception e)
            {
                OceanOfSoulsMod.LogWarning("Evolution failed: " + e.Message);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Actor), "addSpecialEffectAugmentations")]
        private static void StatsPostfix(Actor __instance)
        {
            try
            {
                GoldenFingers.ApplyStats(__instance, __instance.stats);
            }
            catch (Exception e)
            {
                OceanOfSoulsMod.LogWarning("Golden finger stats failed: " + e.Message);
            }
        }
    }
}
