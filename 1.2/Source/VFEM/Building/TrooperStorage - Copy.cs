using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;

namespace VFEMech
{
    public class HullRepairModule : Building
    {
        public override void Tick()
        {
            base.Tick();
            if (Find.TickManager.TicksGame % 600 == 0)
            {
                MoteMaker.MakeStaticMote(this.Position, this.Map, ThingDefOf.Mote_PsycastAreaEffect, 10f);
            }
        }
    }
}
