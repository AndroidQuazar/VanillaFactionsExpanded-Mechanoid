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
    [HarmonyPatch(typeof(FloatMenuMakerMap), "CanTakeOrder")]
    public static class MechanoidsObeyOrders
    {
        public static void Postfix(Pawn pawn, ref bool __result)
        {
            if (pawn.drafter != null)
                __result = true;
        }
    }
}
