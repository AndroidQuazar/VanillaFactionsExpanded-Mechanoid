using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFEMech
{
    public class Hediff_MechanoidUplink : HediffWithComps
    {
        public MechanoidUplink mechanoidUplink;
        public override void Tick()
        {
            base.Tick();
            if (mechanoidUplink != null && this.pawn.Position.DistanceTo(mechanoidUplink.Position) > mechanoidUplink.communicationRadius)
            {
                this.pawn.health.RemoveHediff(this);
            }
            if (mechanoidUplink == null)
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
