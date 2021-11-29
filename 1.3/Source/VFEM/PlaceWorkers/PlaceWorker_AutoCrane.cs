using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using VFE.Mechanoids.Buildings;
using VFEMech;

namespace VFE.Mechanoids.PlaceWorkers
{
    class PlaceWorker_AutoCrane : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            var unreachableCells = GenRadial.RadialCellsAround(center, Building_Autocrane.MinDistanceFromBase, true).ToList();
            GenDraw.DrawFieldEdges(unreachableCells, Color.red);
            var reachableCells = GenRadial.RadialCellsAround(center, Building_Autocrane.MaxDistanceToTargets, true).ToList();
            GenDraw.DrawFieldEdges(reachableCells);
        }
    }
}
