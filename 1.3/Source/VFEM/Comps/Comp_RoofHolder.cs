using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MechanoidAddon
{
	public class CompProperties_RoofHolder : CompProperties
	{
		public int roofHoldingRadius;
		public CompProperties_RoofHolder()
		{
			compClass = typeof(Comp_RoofHolder);
		}
	}


	public class Comp_RoofHolder : ThingComp
    {
		public static Dictionary<Map, HashSet<Comp_RoofHolder>> roofHolderPlaces = new Dictionary<Map, HashSet<Comp_RoofHolder>>();
		public CompProperties_RoofHolder Props => (CompProperties_RoofHolder)props;

		public static bool AnyRoofHoldersInRange(Map map, IntVec3 cell)
        {
			if (roofHolderPlaces.TryGetValue(map, out HashSet<Comp_RoofHolder> roofHolders))
            {
				foreach (var roofHolder in roofHolders)
                {
					if (roofHolder.Props.roofHoldingRadius >= cell.DistanceTo(roofHolder.parent.Position) && ConnectsToRoofHolder(roofHolder, cell, map))
                    {
						return true;
                    }
                }
            }
			return false;
        }
		private static bool ConnectsToRoofHolder(Comp_RoofHolder supporterRoof, IntVec3 c, Map map)
		{
			bool connected = false;
			map.floodFiller.FloodFill(supporterRoof.parent.Position, (IntVec3 x) => x.Roofed(map) && !connected, delegate (IntVec3 x)
			{
				if (x == c)
				{
					connected = true;
				}
			});
			return connected;
		}
		public static bool WithinRangeOfRoofHolder(IntVec3 c, Map map, bool assumeNonNoRoofCellsAreRoofed = false)
		{
			bool connected = false;
			if (roofHolderPlaces.TryGetValue(map, out HashSet<Comp_RoofHolder> roofPlaces))
            {
				var columnCandidates = roofPlaces.Where(x => x.Props.roofHoldingRadius > x.parent.Position.DistanceTo(c));
				if (columnCandidates.Any())
                {
					var buildings = columnCandidates.Select(x => x.parent).Cast<Building>().ToHashSet();
					map.floodFiller.FloodFill(c, (IntVec3 x) => (x.Roofed(map) || x == c || (assumeNonNoRoofCellsAreRoofed && !map.areaManager.NoRoof[x]))
					, delegate (IntVec3 x)
					{
						for (int i = 0; i < 5; i++)
						{
							IntVec3 c2 = x + GenAdj.CardinalDirectionsAndInside[i];
							if (c2.InBounds(map))
							{
								Building edifice = c2.GetEdifice(map);
								if (edifice != null && buildings.Contains(edifice) && roofPlaces.Where(r => r.parent == edifice).FirstOrDefault().Props.roofHoldingRadius >= c.DistanceTo(c2))
								{
									connected = true;
									return true;
								}
							}
						}
						return false;
					});
				}

			}
			return connected;
		}
		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			if (roofHolderPlaces.ContainsKey(this.parent.Map))
			{
				roofHolderPlaces[this.parent.Map].Add(this);
			}
			else
			{
				roofHolderPlaces[this.parent.Map] = new HashSet<Comp_RoofHolder> { this };
			}
		}

		public override void PostDeSpawn(Map map)
		{
			base.PostDeSpawn(map);
			if (roofHolderPlaces.ContainsKey(map))
			{
				roofHolderPlaces[map].Remove(this);
			}
		}
	}
}
