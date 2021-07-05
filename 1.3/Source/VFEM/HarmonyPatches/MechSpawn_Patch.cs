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

    [HarmonyPatch(typeof(MechClusterGenerator), nameof(MechClusterGenerator.MechKindSuitableForCluster))]
    public class MechSpawn_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(PawnKindDef __0, ref bool __result) =>
            __result &= !__0.race.defName.StartsWith("VFE_Mech");
    }
}
