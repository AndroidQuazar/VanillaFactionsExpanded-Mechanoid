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
	[HarmonyPatch(typeof(RoofCollapseUtility), "WithinRangeOfRoofHolder")]
	public static class WithinRangeOfRoofHolder_Patch
	{
		public static void Postfix(ref bool __result, IntVec3 c, Map map, bool assumeNonNoRoofCellsAreRoofed = false)
		{
			if (!__result)
			{
				__result = CompRoofHolder.WithinRangeOfRoofHolder(c, map, assumeNonNoRoofCellsAreRoofed);
			}
		}
	}

	//[HarmonyPatch(typeof(RoofCollapseUtility), "ConnectedToRoofHolder")]
	//public static class ConnectedToRoofHolder_Patch
	//{
	//	public static void Postfix(ref bool __result, IntVec3 c, Map map, bool assumeRoofAtRoot)
	//	{
	//		if (!__result)
    //        {
	//			__result = Comp_RoofHolder.ConnectedToRoofHolder(c, map, assumeRoofAtRoot);
	//		}
	//	}
	//}

	[HarmonyPatch(typeof(RoofCollapseCellsFinder), "ConnectsToRoofHolder")]
	public static class ConnectsToRoofHolder_Patch
	{

		public static void Postfix(ref bool __result, IntVec3 c, Map map, HashSet<IntVec3> visitedCells)
		{
			if (!__result)
			{
				__result = CompRoofHolder.AnyRoofHoldersInRange(map, c);
			}
		}
	}
}
