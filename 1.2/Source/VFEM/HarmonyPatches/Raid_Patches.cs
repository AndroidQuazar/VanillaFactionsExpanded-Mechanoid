using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFEM.HarmonyPatches
{
    using System.Reflection;
    using HarmonyLib;
    using RimWorld;
    using Verse;
    using VFEMech;

    internal static class Raid_Patches
    {
        [HarmonyPatch]
        private static class FactionForCombatGroup_Patch
        {
            [HarmonyTargetMethod]
            public static MethodBase TargetMethod() =>
                typeof(PawnGroupMakerUtility).GetNestedTypes(AccessTools.all)
                                          .First(t => t.GetMethods(AccessTools.all).Any(mi => mi.Name.Contains(nameof(PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroup)) &&
                                                                                              mi.GetParameters()[0].ParameterType == typeof(Faction))).GetMethods(AccessTools.all)
                                          .First(mi => mi.ReturnType == typeof(bool));

            [HarmonyPrefix]
            public static bool Prefix(Faction f, ref bool __result)
            {
                if (f.def == VFEMDefOf.VFE_Mechanoid)
                    if (MechUtils.MechPresence() <= 0)
                    {
                        __result = false;
                        return false;
                    }

                return true;
            }
        }

        [HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "TryResolveRaidFaction")]
        private static class RaidEnemyResolveFaction_Patch
        {
            [HarmonyPrefix]
            public static void Prefix(ref IncidentParms parms)
            {
                if (parms.faction is null)
                    return;
                if (parms.faction.def == VFEMDefOf.VFE_Mechanoid && MechUtils.MechPresence() <= 0)
                    parms.faction = null;
            }
        }

        [HarmonyPatch(typeof(PawnGroupMaker), nameof(PawnGroupMaker.CanGenerateFrom))]
        private static class GetRandomPawnGroupMaker_Patch
        {
            [HarmonyPostfix]
            public static void Postfix(PawnGroupMaker __instance, PawnGroupMakerParms parms, ref bool __result)
            {
                if (__instance is PawnGroupMakerMech pgmm)
                    __result &= pgmm.CanGenerate(parms);
            }
        }
    }
}
