using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VFEMech
{
	public class GenStep_MechanoidStorage : GenStep_Scatterer
	{
		public ThingSetMakerDef thingSetMakerDef;

		private const int Size = 7;

		public override int SeedPart => 913432591;

		protected override bool CanScatterAt(IntVec3 c, Map map)
		{
			if (!base.CanScatterAt(c, map))
			{
				return false;
			}
			if (!c.SupportsStructureType(map, TerrainAffordanceDefOf.Heavy))
			{
				return false;
			}
			if (!map.reachability.CanReachMapEdge(c, TraverseParms.For(TraverseMode.PassDoors)))
			{
				return false;
			}
			CellRect rect = CellRect.CenteredOn(c, 7, 7);
			if (MapGenerator.TryGetVar("UsedRects", out List<CellRect> var) && var.Any((CellRect x) => x.Overlaps(rect)))
			{
				return false;
			}
			foreach (IntVec3 item in rect)
			{
				if (!item.InBounds(map) || item.GetEdifice(map) != null)
				{
					return false;
				}
			}
			return true;
		}

		protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
		{
			if (parms.sitePart != null && parms.sitePart.things != null && parms.sitePart.things.Any)
			{
				var list = parms.sitePart.things.ToList();
				var spawnCenter = CellFinder.RandomClosewalkCellNear(map.Center, map, 25, (IntVec3 x) => x.Walkable(map) && !x.GetTerrain(map).IsWater);
				var enemyPoints = list.Sum(x => x.MarketValue + x.stackCount);

				foreach (var t in list)
			    {
					GenPlace.TryPlaceThing(t, spawnCenter, map, ThingPlaceMode.Near);
			    }

				var faction = Find.FactionManager.FirstFactionOfDef(VFEMDefOf.VFE_Mechanoid);
				PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms();
				pawnGroupMakerParms.tile = map.Tile;
				pawnGroupMakerParms.faction = faction;
				pawnGroupMakerParms.points = enemyPoints;
				pawnGroupMakerParms.groupKind = PawnGroupKindDefOf.Combat;
				var pawns = PawnGroupMakerUtility.GeneratePawns(pawnGroupMakerParms);

				foreach (var pawn in pawns)
				{
					var cell = CellFinder.RandomClosewalkCellNear(spawnCenter, map, 25, (IntVec3 x) => x.Walkable(map) && !x.GetTerrain(map).IsWater);
					GenPlace.TryPlaceThing(pawn, cell, map, ThingPlaceMode.Near);
					MechUtils.CreateOrAddToAssaultLord(pawn);
				}
			}
		}
	}
}