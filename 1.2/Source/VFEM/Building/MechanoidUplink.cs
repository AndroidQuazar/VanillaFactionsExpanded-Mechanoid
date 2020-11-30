using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFEMech
{
    public class MechanoidUplink : MechShipPart
    {
        public float communicationRadius = 50f;
        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            GenDraw.DrawRadiusRing(Position, communicationRadius);
        }
        public override void Tick()
        {
            base.Tick();
            if (this.Spawned)
            {
                foreach (var pawn in this.Map.mapPawns.AllPawns.Where(x => x.kindDef.HasModExtension<UplinkCompatible>()))// && x.Faction == this.Faction))
                {
                    if (!pawn.Dead && pawn.Spawned && pawn.Position.DistanceTo(this.Position) <= communicationRadius)
                    {
                        var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(VFEMDefOf.VFE_MechanoidUplink);
                        if (hediff == null)
                        {
                            Log.Message("Adding MechanoidUplink hediff to " + pawn);
                            var mechUplinkHediff = HediffMaker.MakeHediff(VFEMDefOf.VFE_MechanoidUplink, pawn) as Hediff_MechanoidUplink;
                            mechUplinkHediff.mechanoidUplink = this;
                            pawn.health.AddHediff(mechUplinkHediff);
                        }
                    }
                }
            }
        }
    }
}
