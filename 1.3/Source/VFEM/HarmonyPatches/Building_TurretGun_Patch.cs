using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFEMech
{
    [HarmonyPatch(typeof(Building_TurretGun), "IsValidTarget")]
    public static class Building_TurretGun_IsValidTarget
    {
        public static void Postfix(Building_TurretGun __instance, ref bool __result, Thing t)
        {
            if (__result && __instance.def == VFEMDefOf.VFE_Turret_AutoTesla)
            {
                __result = t is Pawn pawn && pawn.RaceProps.IsMechanoid && !pawn.Dead && !pawn.Downed;
            }
        }
    }

    [HarmonyPatch(typeof(ShotReport), "HitReportFor")]
    public static class ShotReport_HitReportFor
    {
        public static void Postfix(ref ShotReport __result, Thing caster, Verb verb, LocalTargetInfo target)
        {
            if (caster.def == VFEMDefOf.VFE_Turret_AutoTesla)
            {
                Traverse.Create(__result).Field("factorFromShooterAndDist").SetValue(1f);
                Traverse.Create(__result).Field("factorFromEquipment").SetValue(1f);
                Traverse.Create(__result).Field("factorFromWeather").SetValue(1f);
            }
        }
    }
}
