using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFE.Mechanoids.HarmonyPatches
{
    [HarmonyPatch(typeof(ITab_Pawn_Character))]
    [HarmonyPatch("IsVisible", MethodType.Getter)]
    public static class NoBioForMachines
    {
        public static void Postfix(ITab_Pawn_Character __instance, ref bool __result)
        {
            Pawn pawn=(Pawn)typeof(ITab_Pawn_Character).GetProperty("PawnToShowInfoAbout", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(__instance);
            if (pawn.RaceProps.IsMechanoid)
                __result = false;
        }
    }
}
