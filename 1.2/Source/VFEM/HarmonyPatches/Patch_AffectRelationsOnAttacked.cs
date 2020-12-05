using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace VFEMech
{
    [HarmonyPatch(typeof(SignalManager), "SendSignal")]
    internal static class Patch_SendSignal
    {
        private static void Postfix(Signal signal)
        {
            Log.Message("signal: " + signal.tag);
        }
    }

    [HarmonyPatch(typeof(SettlementUtility), "AffectRelationsOnAttacked_NewTmp")]
    internal static class Patch_AffectRelationsOnAttacked_NewTmp
    {
        private static bool Prefix(MapParent mapParent, ref TaggedString letterText)
        {
            if (mapParent is Site site && site.parts != null)
            {
                foreach (var part in site.parts)
                {
                    if (part.def == VFEMDefOf.VFE_MechanoidAttackParty)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
