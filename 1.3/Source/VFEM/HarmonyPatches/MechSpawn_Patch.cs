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

    //[HarmonyPatch]
    //public class MechSpawn_Patch
    //{
    //    [HarmonyTargetMethods]
    //    public static IEnumerable<MethodBase> TargetMethods()
    //    {
    //        yield return typeof(SymbolResolver_RandomMechanoidGroup).GetNestedTypes(AccessTools.all)
    //                                                             .First(t => t.GetMethods(AccessTools.all)
    //                                                                       .Any(mi => mi.ReturnType == typeof(bool) && mi.GetParameters()[0].ParameterType == typeof(PawnKindDef)))
    //                                                             .GetMethods(AccessTools.all)
    //                                                             .First(mi => mi.ReturnType == typeof(bool));
    //        
    //        yield return typeof(MechClusterGenerator).GetNestedTypes(AccessTools.all).MaxBy(t => t.GetMethods(AccessTools.all).Length).GetMethods(AccessTools.all)
    //                                              .First(mi => mi.ReturnType == typeof(bool) && mi.GetParameters()[0].ParameterType == typeof(PawnKindDef));
    //    }
    //
    //    [HarmonyPostfix]
    //    public static void Postfix(PawnKindDef __0, ref bool __result) => 
    //        __result &= !__0.race.defName.StartsWith("VFE_Mech");
    //}
}
