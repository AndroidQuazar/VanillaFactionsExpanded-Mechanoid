using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFEM.HarmonyPatches
{
    using System.Reflection;
    using HarmonyLib;
    using RimWorld;
    using RimWorld.BaseGen;
    using Verse;
    using VFEMech;

    [HarmonyPatch]
    public class MechSpawn_Patch
    {
        [HarmonyTargetMethod]
        public static MethodBase TargetMethod() =>
            typeof(SymbolResolver_RandomMechanoidGroup).GetNestedTypes(AccessTools.all)
                                      .First(t => t.GetMethods(AccessTools.all).Any(mi => mi.ReturnType == typeof(bool) && mi.GetParameters()[0].ParameterType == typeof(PawnKindDef))).GetMethods(AccessTools.all)
                                      .First(mi => mi.ReturnType == typeof(bool));

        [HarmonyPostfix]
        public static void Postfix(PawnKindDef kind, ref bool __result) => 
            __result &= !kind.race.defName.StartsWith("VFE_Mech");
    }
}
