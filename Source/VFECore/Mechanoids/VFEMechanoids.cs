using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFE.Mechanoids
{
    public class VFEMechanoids : Mod
    {
        public VFEMechanoids(ModContentPack content) : base(content)
        {
            harmonyInstance = new Harmony("OskarPotocki.VFEMechanoids");
        }

        public static Harmony harmonyInstance;
    }
}
