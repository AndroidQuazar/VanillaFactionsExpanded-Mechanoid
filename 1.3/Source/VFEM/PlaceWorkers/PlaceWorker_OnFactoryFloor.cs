using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFEMech
{
	public class PlaceWorker_OnFactoryFloor : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			if (loc.GetTerrain(map) != VFEMDefOf.VFE_FactoryFloor)
			{
				return "VFEMech.MustPlaceOnFactoryFloor".Translate();
			}
			return true;
		}
    }
}
