using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFEMech
{
    public class VFEMechanoid : Pawn
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                var age = (long)(Rand.Range(1, 10) * 3600000f);
                this.ageTracker.AgeBiologicalTicks = age;
                this.ageTracker.AgeChronologicalTicks = age;
            }
        }
    }
}
