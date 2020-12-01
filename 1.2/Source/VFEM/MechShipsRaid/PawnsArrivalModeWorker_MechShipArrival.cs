using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VFEM
{
    internal class PawnsArrivalModeWorker_MechShipArrival : PawnsArrivalModeWorker
    {
        public override void Arrive(List<Pawn> pawns, IncidentParms parms)
        {
            PawnsArrivalModeWorkerUtility.DropInDropPodsNearSpawnCenter(parms, pawns);
        }

        public override void TravelingTransportPodsArrived(List<ActiveDropPodInfo> dropPods, Map map)
        {
            IntVec3 near;
            if (!DropCellFinder.TryFindRaidDropCenterClose(out near, map, true, true, true, -1))
            {
                near = DropCellFinder.FindRaidDropCenterDistant_NewTemp(map, false);
            }
            TransportPodsArrivalActionUtility.DropTravelingTransportPods(dropPods, near, map);
        }

        public override bool TryResolveRaidSpawnCenter(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            parms.spawnCenter = this.FindRect(map);
            parms.spawnRotation = Rot4.Random;
            return true;
        }

        public IntVec3 FindRect(Map map)
        {
            CellRect rect;
            bool shre = true;
            while (shre)
            {
                rect = CellRect.CenteredOn(CellFinder.RandomNotEdgeCell(33, map), 33, 33);
                if (rect.Cells.ToList().Any(i => !i.Walkable(map) || !i.GetTerrain(map).affordances.Contains(TerrainAffordanceDefOf.Medium))) { }
                else return rect.CenterCell;
            }
            return IntVec3.Invalid;
        }
    }
}