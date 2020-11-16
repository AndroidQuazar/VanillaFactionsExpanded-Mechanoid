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
	[StaticConstructorOnStartup]
	internal static class HarmonyInit
	{
		static HarmonyInit()
		{
			new Harmony("OskarPotocki.VanillaFactionsExpandedMechanoids").PatchAll();
		}
	}
}
