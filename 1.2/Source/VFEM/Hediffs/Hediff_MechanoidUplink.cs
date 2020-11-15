using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFEM
{
    public class Hediff_MechanoidUplink : HediffWithComps
    {
        public MechanoidUplink mechanoidUplink;

        public override void Tick()
        {
            base.Tick();
            if (this.pawn.Position.DistanceTo(mechanoidUplink.Position) > mechanoidUplink.communicationRadius)
            {
                this.pawn.health.RemoveHediff(this);
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref mechanoidUplink, "mechanoidUplink");
        }
    }
}
