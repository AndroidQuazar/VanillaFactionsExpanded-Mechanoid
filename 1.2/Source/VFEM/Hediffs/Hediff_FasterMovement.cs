using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFEMech
{
    public class Hediff_FasterMovement : Hediff
    {
        public override void Tick()
        {
            base.Tick();
            if (this.pawn.Map == null || this.pawn.Position.GetFirstThing(this.pawn.Map, VFEMDefOf.VFE_FactoryPath) == null)
            {
                this.pawn.health.RemoveHediff(this);
            }
        }
    }
}
