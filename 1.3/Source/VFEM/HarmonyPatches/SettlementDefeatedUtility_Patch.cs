using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace VFEM.HarmonyPatches
{
    using VFEMech;

    [HarmonyPatch(typeof(SettlementDefeatUtility), "IsDefeated")]
    [HarmonyAfter("vanillaexpanded.achievements")]
    internal static class SettlementDefeatedUtility_Patch
    {
        [HarmonyPostfix]
        private static void Postfix(Map map, Faction faction, ref bool __result)
        {
            if (map.listerThings.ThingsOfDef(VFEMDefOf.VFEM_MissileIncoming).Any()) __result = true;
            else if (map.mapPawns.SpawnedPawnsInFaction(faction).Any(p => p.Faction?.def.defName == "VFE_Mechanoid" && GenHostility.IsActiveThreatToPlayer(p))) __result = false;
            else if (map.listerBuildings.allBuildingsNonColonist.Any(b => b.Faction?.def.defName == "VFE_Mechanoid" && b.def.defName.Contains("_Turret_"))) __result     = false;
        }
    }
}