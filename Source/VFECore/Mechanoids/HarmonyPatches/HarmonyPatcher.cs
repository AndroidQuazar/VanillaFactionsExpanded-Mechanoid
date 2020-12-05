using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFE.Mechanoids
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatcher
    {
        static HarmonyPatcher()
        {
            VFEMechanoids.harmonyInstance.PatchAll();
        }
    }
}
