using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
			List<Gizmo> list = new List<Gizmo>();
			list.AddRange(__result);
			if (__instance.RaceProps.IsMechanoid && __instance.Faction == Faction.OfPlayer && __instance.drafter != null && CompMachine.cachedMachines.ContainsKey(__instance.Drawer.renderer))
			{
				IEnumerable<Gizmo> collection = (IEnumerable<Gizmo>)typeof(Pawn_DraftController).GetMethod("GetGizmos", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance.drafter, new object[0]);
				list.AddRange(collection);
			}
			__result = list;
		}
	}
}
