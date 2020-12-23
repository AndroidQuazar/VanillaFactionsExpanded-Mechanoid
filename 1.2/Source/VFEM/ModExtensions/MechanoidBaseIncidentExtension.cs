using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFEMech
{
    using RimWorld;
    using Verse;

    public class MechanoidBaseIncidentExtension : DefModExtension
    {
        public WorldObjectDef baseToPlace;
        public int minDistance = 5;
        public int maxDistance = 120;
        public int minimumColonistCount;
    }
}
