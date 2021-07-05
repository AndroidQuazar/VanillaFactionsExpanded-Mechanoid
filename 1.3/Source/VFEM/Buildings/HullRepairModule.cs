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
    public class HullRepairModule : MechShipPart
    {
        public override void Tick()
        {
            base.Tick();
            if (this.Map != null && Find.TickManager.TicksGame % 600 == 0)
            {
                FleckMaker.Static(this.Position, this.Map, FleckDefOf.PsycastAreaEffect, 10f);
                var buildings = this.Map.listerThings.AllThings.Where(x => x.def.building?.buildingTags?.Contains("VFE_MechanoidShip") ?? false && x.Position.DistanceTo(this.Position) <= 20f).ToList();
                foreach (var building in buildings)
                {
                    var hp = building.HitPoints + 10;
                    if (hp > building.MaxHitPoints) 
                        hp = building.MaxHitPoints;
                    building.HitPoints = hp;
                }
            }
        }
    }
}
