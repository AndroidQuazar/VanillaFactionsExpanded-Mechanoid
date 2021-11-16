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
}
