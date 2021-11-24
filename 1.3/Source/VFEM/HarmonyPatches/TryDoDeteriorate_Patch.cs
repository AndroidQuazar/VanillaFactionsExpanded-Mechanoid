using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFEMech
{
	[HarmonyPatch(typeof(SteadyEnvironmentEffects), "TryDoDeteriorate")]
	public static class TryDoDeteriorate_Patch
	{
		public static bool Prefix(Thing t, bool roofed, bool roomUsesOutdoorTemperature, bool protectedByEdifice, TerrainDef terrain)
		{
			if (t?.Map != null && CompPreventDeteoriratingAndSpolining.safePlaces.TryGetValue(t.Map, out HashSet<IntVec3> safePositions) && safePositions.Contains(t.Position))
            {
				return false;
            }
			return true;
		}
	}

	[HarmonyPatch(typeof(CompRottable), "Active", MethodType.Getter)]
	public static class Active_Patch
	{
		public static bool Prefix(CompRottable __instance, ref bool __result)
		{
			if (__instance.parent?.Map != null && CompPreventDeteoriratingAndSpolining.safePlaces.TryGetValue(__instance.parent.Map, out HashSet<IntVec3> safePositions) && safePositions.Contains(__instance.parent.Position))
			{
				__result = false;
				return false;
			}
			return true;
		}
	}
}
