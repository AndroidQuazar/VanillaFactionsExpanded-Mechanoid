using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFE.Mechanoids.HarmonyPatches
{
    [HarmonyPatch(typeof(PawnRenderer), "CarryWeaponOpenly")]
    public static class MobileTurretsShowTurrets
    {
        static FieldInfo pawnField = typeof(PawnRenderer).GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(PawnRenderer __instance, ref bool __result)
        {
            Pawn pawn = (Pawn)pawnField.GetValue(__instance);
            if (pawn.TryGetComp<CompMachine>()?.turretAttached!=null)
            {
                __result=true;
            }
        }
    }
}
