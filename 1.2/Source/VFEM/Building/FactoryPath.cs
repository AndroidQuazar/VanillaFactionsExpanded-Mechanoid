using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace VFEMech
{
    public class FactoryPath : Building
    {
        public override void Tick()
        {
            base.Tick();
            foreach (var t in this.Map.thingGrid.ThingsListAtFast(this.Position))
            {
                if (t is Pawn pawn && pawn.health.hediffSet.GetFirstHediffOfDef(VFEMDefOf.VFE_FasterMovement) == null)
                {
                    var hediff = HediffMaker.MakeHediff(VFEMDefOf.VFE_FasterMovement, pawn);
                    pawn.health.AddHediff(hediff);
                    pawn.pather.ResetToCurrentPosition();
                }
            }
        }
    }
}
