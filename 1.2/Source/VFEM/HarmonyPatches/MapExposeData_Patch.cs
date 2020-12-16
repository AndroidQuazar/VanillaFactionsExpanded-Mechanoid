using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFEM
{
    [HarmonyPatch(typeof(Map), "ExposeData")]
    public static class MapExposeData_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(Map __instance)
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                List<Thing> vFEM_Skyfallers = __instance.listerThings.AllThings.FindAll(t => t is VFEM_Skyfaller);
                if (vFEM_Skyfallers.Count > 0)
                {
                    foreach (var item in vFEM_Skyfallers)
                    {
                        if (item is VFEM_Skyfaller skyfaller && skyfaller != null)
                        {
                            skyfaller.SaveImpact();
                        }
                    }
                }
            }
        }
    }
}
