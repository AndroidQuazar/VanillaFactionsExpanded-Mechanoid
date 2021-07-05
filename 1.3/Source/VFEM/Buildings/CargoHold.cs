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
    public class CargoHold : MechShipPart
    {
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            var resourceDef = DefDatabase<ThingDef>.AllDefs.Where(x => x.IsStuff && x.BaseMarketValue <= 10);
            var resource = ThingMaker.MakeThing(resourceDef.RandomElement());
            resource.stackCount = 130;
            var map = this.Map;
            base.Destroy(mode);
            GenPlace.TryPlaceThing(resource, this.Position, map, ThingPlaceMode.Near);
        }
    }
}
