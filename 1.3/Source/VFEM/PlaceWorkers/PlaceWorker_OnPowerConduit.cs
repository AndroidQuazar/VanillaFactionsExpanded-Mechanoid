using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFEMech
{
	public class PlaceWorker_OnPowerConduit : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			var buildings = loc.GetThingList(map);
			if (!buildings.Where(x => x.def.defName.ToLower().Contains("conduit") && x.TryGetComp<CompPowerTransmitter>() is CompPowerTransmitter comp && comp.Props.transmitsPower).Any())
			{
				return "VFEMech.MustPlaceOnPowerConduit".Translate();
			}
			return true;
		}
    }
}
