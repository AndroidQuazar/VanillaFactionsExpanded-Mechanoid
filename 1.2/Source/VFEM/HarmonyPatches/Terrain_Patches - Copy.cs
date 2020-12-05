using HarmonyLib;
using RimWorld;
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
}
