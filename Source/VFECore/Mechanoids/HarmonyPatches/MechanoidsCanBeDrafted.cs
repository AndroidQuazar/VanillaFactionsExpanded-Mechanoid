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
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public static class MechanoidsCanBeDrafted
    {
        public static void Postfix(Pawn __instance, ref IEnumerable<Gizmo> __result)
        {
            List<Gizmo> gizmos = new List<Gizmo>();
            gizmos.AddRange(__result);
            if(__instance.RaceProps.IsMechanoid && __instance.Faction==Faction.OfPlayer)
            {
                if (__instance.drafter != null)
                {
                    IEnumerable<Gizmo> draftGizmos = (IEnumerable<Gizmo>)typeof(Pawn_DraftController).GetMethod("GetGizmos", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(__instance.drafter, new object[] { });
                    gizmos.AddRange(draftGizmos);
                }
            }
            __result = gizmos;
        }
    }
}
