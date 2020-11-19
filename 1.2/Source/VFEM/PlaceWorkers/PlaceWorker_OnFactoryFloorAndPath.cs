using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFEMech
{
	public class PlaceWorker_OnFactoryFloorAndPath : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			foreach (IntVec3 intVec in GenAdj.CellsOccupiedBy(loc, rot, checkingDef.Size))
			{
				if (map.terrainGrid.TerrainAt(intVec) != VFEMDefOf.VFE_FactoryFloor && map.terrainGrid.TerrainAt(intVec) != VFEMDefOf.VFE_FactoryPath)
				{
					return "VFEMech.MustPlaceOnFactoryFloor".Translate();
				}
			}
			return true;
		}
    }
}
