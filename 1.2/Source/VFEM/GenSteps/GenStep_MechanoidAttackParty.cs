using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace VFEMech
{
	public class GenStep_MechanoidAttackParty : GenStep
	{
		public int size = 16;

		public FloatRange defaultPawnGroupPointsRange = SymbolResolver_Settlement.DefaultPawnsPoints;

		private static List<CellRect> possibleRects = new List<CellRect>();

		public override int SeedPart => 398638181;

		public override void Generate(Map map, GenStepParams parms)
		{
			if (!MapGenerator.TryGetVar("RectOfInterest", out CellRect var))
			{
				var = CellRect.SingleCell(map.Center);
			}
			if (!MapGenerator.TryGetVar("UsedRects", out List<CellRect> var2))
			{
				var2 = new List<CellRect>();
				MapGenerator.SetVar("UsedRects", var2);
			}
			Log.Message(map.ParentFaction + " - " + map + " - " + map.ParentFaction.PlayerRelationKind);
			Faction faction = map.ParentFaction;
			ResolveParams resolveParams = default(ResolveParams);
			resolveParams.rect = GetOutpostRect(var, var2, map);
			resolveParams.faction = faction;
			resolveParams.edgeDefenseWidth = 2;
			resolveParams.edgeDefenseTurretsCount = Rand.RangeInclusive(0, 1);
			resolveParams.edgeDefenseMortarsCount = 0;
			resolveParams.settlementPawnGroupPoints = defaultPawnGroupPointsRange.RandomInRange;
			Log.Message("resolveParams.settlementPawnGroupPoints: " + resolveParams.settlementPawnGroupPoints);
			RimWorld.BaseGen.BaseGen.globalSettings.map = map;
			RimWorld.BaseGen.BaseGen.globalSettings.minBuildings = 1;
			RimWorld.BaseGen.BaseGen.globalSettings.minBarracks = 1;
			RimWorld.BaseGen.BaseGen.symbolStack.Push("settlement", resolveParams);
			if (faction != null && faction == Faction.Empire)
			{
				RimWorld.BaseGen.BaseGen.globalSettings.minThroneRooms = 1;
				RimWorld.BaseGen.BaseGen.globalSettings.minLandingPads = 1;
			}
			RimWorld.BaseGen.BaseGen.Generate();
			if (faction != null && faction == Faction.Empire && RimWorld.BaseGen.BaseGen.globalSettings.landingPadsGenerated == 0)
			{
				GenStep_Settlement.GenerateLandingPadNearby(resolveParams.rect, map, faction, out CellRect usedRect);
				var2.Add(usedRect);
			}
			var2.Add(resolveParams.rect);

			var defenders = map.mapPawns.PawnsInFaction(faction);
			foreach (var pawn in defenders)
            {
				if (pawn.RaceProps.Humanlike)
                {
					if (pawn.GetLord() != null && pawn.Faction != Faction.OfPlayer)
                    {
						map.lordManager.RemoveLord(pawn.GetLord());
                    }
				}
			}

			var mechFaction = Find.FactionManager.FirstFactionOfDef(VFEMDefOf.VFE_Mechanoid);
			PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms();
			pawnGroupMakerParms.tile = map.Tile;
			pawnGroupMakerParms.faction = mechFaction;
			pawnGroupMakerParms.points = defenders.Sum(x => x.kindDef.combatPower) * 3f;
			pawnGroupMakerParms.groupKind = PawnGroupKindDefOf.Combat;
			var mechs = PawnGroupMakerUtility.GeneratePawns(pawnGroupMakerParms);

			var spawnCenter = CellFinder.RandomClosewalkCellNear(defenders.RandomElement().Position, map, 15, (IntVec3 x) => x.Walkable(map));
			var cells = map.AllCells.Where(x => !defenders.Where(y => y.Position.DistanceTo(x) < 30f).Any() && x.DistanceTo(spawnCenter) < 60 && x.Walkable(map)).ToList();
			var mechSpawnCenter = cells.RandomElement();
			cells = map.AllCells.OrderBy(x => x.DistanceTo(mechSpawnCenter)).Where(x => x.Walkable(map)).Take(mechs.Count() * 4).ToList();
			foreach (var mech in mechs)
			{
				var cellToSpawn = cells.RandomElement();
				cells.Remove(cellToSpawn);
				GenPlace.TryPlaceThing(mech, cellToSpawn, map, ThingPlaceMode.Near);
				MechUtils.CreateOrAddToAssaultLord(mech);
				var defenderPosition = defenders.OrderBy(x => x.Position.DistanceTo(mech.Position)).FirstOrDefault().Position;
				var xPos = (defenderPosition.x + mech.Position.x) / 2;
				var zPos = (defenderPosition.z + mech.Position.z) / 2;
				var middlePosition = new IntVec3(xPos, 0, zPos);
				var cellToGoto = CellFinder.RandomClosewalkCellNear(middlePosition, map, 15, (IntVec3 x) => x.Walkable(map));
				mech.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Goto, cellToGoto));
			}

			var x_Averages = defenders.OrderBy(x => x.Position.x);
			var x_average = x_Averages.ElementAt(x_Averages.Count() / 2).Position.x;
			var z_Averages = defenders.OrderBy(x => x.Position.z);
			var z_average = z_Averages.ElementAt(z_Averages.Count() / 2).Position.z;
			var middleCell = new IntVec3(x_average, 0, z_average);

			LordMaker.MakeNewLord(faction, new LordJob_DefendPoint(middleCell, 12, false, false), map, defenders);

			var mechWarfare = map.GetComponent<MapComponent_MechWarfare>();
			mechWarfare.RegisterMechanoidAttackParty(map.mapPawns.PawnsInFaction(mechFaction), map.Parent as Site);
		}

		private CellRect GetOutpostRect(CellRect rectToDefend, List<CellRect> usedRects, Map map)
		{
			possibleRects.Add(new CellRect(rectToDefend.minX - 1 - size, rectToDefend.CenterCell.z - size / 2, size, size));
			possibleRects.Add(new CellRect(rectToDefend.maxX + 1, rectToDefend.CenterCell.z - size / 2, size, size));
			possibleRects.Add(new CellRect(rectToDefend.CenterCell.x - size / 2, rectToDefend.minZ - 1 - size, size, size));
			possibleRects.Add(new CellRect(rectToDefend.CenterCell.x - size / 2, rectToDefend.maxZ + 1, size, size));
			CellRect mapRect = new CellRect(0, 0, map.Size.x, map.Size.z);
			possibleRects.RemoveAll((CellRect x) => !x.FullyContainedWithin(mapRect));
			if (possibleRects.Any())
			{
				IEnumerable<CellRect> source = possibleRects.Where((CellRect x) => !usedRects.Any((CellRect y) => x.Overlaps(y)));
				if (!source.Any())
				{
					possibleRects.Add(new CellRect(rectToDefend.minX - 1 - size * 2, rectToDefend.CenterCell.z - size / 2, size, size));
					possibleRects.Add(new CellRect(rectToDefend.maxX + 1 + size, rectToDefend.CenterCell.z - size / 2, size, size));
					possibleRects.Add(new CellRect(rectToDefend.CenterCell.x - size / 2, rectToDefend.minZ - 1 - size * 2, size, size));
					possibleRects.Add(new CellRect(rectToDefend.CenterCell.x - size / 2, rectToDefend.maxZ + 1 + size, size, size));
				}
				if (source.Any())
				{
					return source.RandomElement();
				}
				return possibleRects.RandomElement();
			}
			return rectToDefend;
		}
	}
}