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
	public class GenStep_MechanoidShipLanding : GenStep
	{
		public override int SeedPart => 391638181;

		public override void Generate(Map map, GenStepParams parms)
		{
			var cells = map.AllCells.Where(x => x.GetEdifice(map) == null).OrderBy(x => x.DistanceTo(map.Center)).ToHashSet();
			CreateLandingZone(cells, map);
		}

		public void CreateLandingZone(HashSet<IntVec3> cells, Map map)
        {
			foreach (var cell in cells)
            {
				var cellRectCandidate = new CellRect(cell.x - 10, cell.x - 10, 20, 20);
				if (!cellRectCandidate.Cells.Where(x => !cells.Contains(x)).Any())
                {
					SpawnMechsAndBeacons(cellRectCandidate, map);
					return;
				}
            }

			var cellRectCandidate2 = new CellRect(map.Center.x - 10, map.Center.x - 10, 20, 20);
			foreach (var cell in cellRectCandidate2)
            {
				var thingList = cell.GetThingList(map);
				for (int num = thingList.Count - 1; num >= 0; num--)
                {
					thingList[num].DeSpawn();
                }
			}
			SpawnMechsAndBeacons(cellRectCandidate2, map);
		}

		public void SpawnMechsAndBeacons(CellRect rect, Map map)
        {
			List<Thing> mechLandPads = new List<Thing>();

			var bottomLeft = new IntVec3(rect.minX, 0, rect.minZ);
			var bottomRigth = new IntVec3(rect.maxX, 0, rect.minZ);
			var topRight = new IntVec3(rect.maxX, 0, rect.maxZ);
			var topLeft = new IntVec3(rect.minX, 0, rect.maxZ);

			mechLandPads.Add(GenSpawn.Spawn(VFEMDefOf.VFE_MechLandingBeacon, bottomRigth, map));
			mechLandPads.Add(GenSpawn.Spawn(VFEMDefOf.VFE_MechLandingBeacon, topRight, map));
			mechLandPads.Add(GenSpawn.Spawn(VFEMDefOf.VFE_MechLandingBeacon, bottomLeft, map));
			mechLandPads.Add(GenSpawn.Spawn(VFEMDefOf.VFE_MechLandingBeacon, topLeft, map));

			var mechFaction = Find.FactionManager.FirstFactionOfDef(VFEMDefOf.VFE_Mechanoid);
			PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms();
			pawnGroupMakerParms.tile = map.Tile;
			pawnGroupMakerParms.faction = mechFaction;
			pawnGroupMakerParms.points = MechUtils.MechPresence();
			pawnGroupMakerParms.groupKind = PawnGroupKindDefOf.Combat;
			var mechs = PawnGroupMakerUtility.GeneratePawns(pawnGroupMakerParms);

			foreach (var mech in mechs)
			{
				GenPlace.TryPlaceThing(mech, rect.Cells.RandomElement(), map, ThingPlaceMode.Near);
				MechUtils.CreateOrAddToAssaultLord(mech);
			}

			var mechWarfare = map.GetComponent<MapComponent_MechWarfare>();
			mechWarfare.RegisterMechLandingSite(mechLandPads, map.Parent as Site);
		}
	}
}