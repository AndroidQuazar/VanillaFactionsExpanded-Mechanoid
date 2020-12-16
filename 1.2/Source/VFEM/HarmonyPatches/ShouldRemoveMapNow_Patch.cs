using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace VFEM.HarmonyPatches
{
    using System.Linq;
    using VFEMech;

    [HarmonyPatch(typeof(Site), "ShouldRemoveMapNow")]
    internal static class ShouldRemoveMapNow_Patch
    {
        private static void Postfix(Site __instance, ref bool __result)
        {
            if (__result && __instance.parts != null)
            {
                foreach (var part in __instance.parts)
                {
                    if (part.def == VFEMDefOf.VFE_MechanoidAttackParty)
                    {
                        if (__instance.Map.mapPawns.AllPawns.Where(x => x.RaceProps.IsMechanoid && x.HostileTo(Faction.OfPlayer) && !x.Dead && !x.Destroyed).Any())
                        {
                            __result = false;
                            return;
                        }
                    }
                    else if (part.def == VFEMDefOf.VFE_MechanoidShipLanding)
                    {
                        if (__instance.Map.listerThings.AllThings.Where(x => x.def == VFEMDefOf.VFE_MechLandingBeacon && !x.DestroyedOrNull()).Any())
                        {
                            __result = false;
                            return;
                        }
                    }
                    else if (part.def == VFEMDefOf.VFE_MechanoidStorage)
                    {
                        if (__instance.Map.mapPawns.AllPawns.Where(x => x.RaceProps.IsMechanoid && x.HostileTo(Faction.OfPlayer) && !x.Dead && !x.Destroyed).Any())
                        {
                            __result = false;
                            return;
                        }
                    }
                }
            }
        }
    }
}