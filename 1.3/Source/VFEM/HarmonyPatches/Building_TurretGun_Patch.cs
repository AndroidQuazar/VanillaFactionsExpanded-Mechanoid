using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
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
        public static bool accuracy;
        public static void Prefix(ref ShotReport __result, Thing caster, Verb verb, LocalTargetInfo target)
        {
            if (caster.def == VFEMDefOf.VFE_Turret_AutoTesla)
            {
                accuracy = true;
            }
        }
    }

    [HarmonyPatch(typeof(Verb_LaunchProjectile), "TryCastShot")]
    public static class Verb_LaunchProjectile_TryCastShot
    {
        public static void Prefix(Verb_LaunchProjectile __instance)
        {
            if (__instance.caster.def == VFEMDefOf.VFE_Turret_AutoTesla)
            {
                ShotReport_HitReportFor.accuracy = true;
            }
        }
        public static void Postfix()
        {
            ShotReport_HitReportFor.accuracy = false;
        }
    }

    [HarmonyPatch(typeof(ShotReport), "AimOnTargetChance_StandardTarget", MethodType.Getter)]
    public static class ShotReport_AimOnTargetChance_StandardTarget
    {
        public static void Postfix(ref float __result)
        {
            if (ShotReport_HitReportFor.accuracy)
            {
                __result = 1f;
            }
        }
    }
}
