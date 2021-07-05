using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace VFEMech
{
	class PlaceWorker_ApiaryAndFactoryFloorAndPath : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			foreach (IntVec3 c in GenAdj.OccupiedRect(center, rot, def.Size).ExpandedBy(1))
			{
				List<Thing> list = map.thingGrid.ThingsListAt(c);
				for (int i = 0; i < list.Count; i++)
				{
					Thing thing2 = list[i];
					if (thing2 != thingToIgnore && ((thing2.def.category == ThingCategory.Building && thing2.def.defName == "VFEV_Apiary") || ((thing2.def.IsBlueprint || thing2.def.IsFrame) && thing2.def.entityDefToBuild is ThingDef && ((ThingDef)thing2.def.entityDefToBuild).defName == "VFEV_Apiary")))
					{
						return "APlaceWorker".Translate();
					}
				}
				if (map.terrainGrid.TerrainAt(c) != VFEMDefOf.VFE_FactoryFloor && map.terrainGrid.TerrainAt(c) != VFEMDefOf.VFE_FactoryPath)
				{
					return "VFEMech.MustPlaceOnFactoryFloor".Translate();
				}
			}
			if (center.Roofed(map)) return "APlaceWorkerNoRoof".Translate();
			return true;
		}
	}
}
