using System;
using HarmonyLib;

namespace OceanOfSouls
{
    internal static class SoulTracker
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Actor), "die")]
        private static bool DiePrefix(Actor __instance, bool pDestroy, bool pCountDeath)
        {
            if (!pCountDeath || __instance == null || !__instance.isAlive()) return true;

            try
            {
                if (GoldenFingers.TrySecondChance(__instance, pDestroy))
                {
                    return false;
                }

                if (SoulRebirth.ShouldRevive(__instance, pDestroy, pCountDeath))
                {
                    SoulRebirth.Revive(__instance);
                    return false;
                }

                if (!SoulService.TryRegisterDeath(__instance, out LifeResult result)) return true;

                if (SoulRebirth.ShouldRebirth(__instance) && !SoulRebirth.Rebirth(__instance))
                {
                    SoulRecord pending = SoulRecord.Load(__instance.data);
                    if (SoulConfig.IsLevelSaved(pending.Level))
                    {
                        SoulStorage.Save(SoulData.Create(__instance, pending));
                    }
                }

                if (!SoulConfig.DebugLog) return true;

                SoulRecord record = SoulRecord.Load(__instance.data);
                OceanOfSoulsMod.LogInfo(
                    $"{__instance.getName()} died: age {__instance.getAge()}, deeds {result.Deeds}, " +
                    $"gain {result.Gain}, penalty {result.Penalty}, points {record.Points}, " +
                    $"level {result.PreviousLevel} -> {result.NewLevel}, lives {record.Lives}");
                return true;
            }
            catch (Exception e)
            {
                OceanOfSoulsMod.LogWarning("Death tracking failed: " + e.Message);
                return true;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Actor), "newCreature")]
        private static void NewCreaturePostfix(Actor __instance)
        {
            TryReincarnate(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BabyMaker), nameof(BabyMaker.makeBaby))]
        private static void MakeBabyPostfix(Actor __result)
        {
            TryReincarnate(__result);
        }

        private static void TryReincarnate(Actor actor)
        {
            try
            {
                SoulReincarnation.TryReincarnate(actor);
            }
            catch (Exception e)
            {
                OceanOfSoulsMod.LogWarning("Reincarnation failed: " + e.Message);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Kingdom), nameof(Kingdom.setKing))]
        private static void SetKingPostfix(Actor pActor, bool pFromLoad)
        {
            if (pFromLoad || pActor == null) return;
            LifeSummary.Increment(pActor, LifeSummary.ReignsKey);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(City), nameof(City.setLeader))]
        private static void SetLeaderPostfix(Actor pActor, bool pNew)
        {
            if (!pNew || pActor == null) return;
            LifeSummary.Increment(pActor, LifeSummary.LeaderKey);
        }
    }
}
