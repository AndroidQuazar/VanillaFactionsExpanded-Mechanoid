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
    [StaticConstructorOnStartup]
    public static class TerrainPatches 
    {
        public static Dictionary<Map, HashSet<IntVec3>> factoryFloors;
        static TerrainPatches()
        {
            factoryFloors = new Dictionary<Map, HashSet<IntVec3>>();
        }

        [HarmonyPatch(typeof(TerrainGrid), "RemoveTopLayer")]
        internal static class RemoveTopLayer_Patch
        {
            public static void Prefix(IntVec3 c, Map ___map, bool doLeavings)
            {
                if (c.GetTerrain(___map) == VFEMDefOf.VFE_FactoryPath)
                {
                    if (factoryFloors.ContainsKey(___map))
                    {
                        factoryFloors[___map].Remove(c);
                    }
                }
            }
        }


        [HarmonyPatch(typeof(TerrainGrid), "SetTerrain")]
        internal static class Patch_SetTerrain
        {
            private static void Postfix(IntVec3 c, TerrainDef newTerr, Map ___map)
            {
                if (newTerr == VFEMDefOf.VFE_FactoryPath)
                {
                    if (factoryFloors.ContainsKey(___map))
                    {
                        factoryFloors[___map].Add(c);
                    }
                    else
                    {
                        factoryFloors[___map] = new HashSet<IntVec3> { c };
                    }
                }
            }
        }
    }



}
