using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using VFE.Mechanoids.Buildings;

namespace VFE.Mechanoids.PlaceWorkers
{
    class PlaceWorker_AutoPlant : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            int range = 7;
            if (thing != null)
                range = ((Building_AutoPlant)thing).range;

            List<IntVec3> cells = new List<IntVec3>();
            for(int offset=0;offset<range-1;offset++)
            {
                if(rot==Rot4.North)
                {
                    for (int x = -3; x <= 3; x++)
                        cells.Add(new IntVec3(center.x + x, 0, center.z + 2 + offset));
                }
                else if(rot==Rot4.South)
                {
                    for (int x = -3; x <= 3; x++)
                        cells.Add(new IntVec3(center.x + x, 0, center.z - 2 - offset));
                }
                else if(rot==Rot4.East)
                {
                    for (int z = -3; z <= 3; z++)
                        cells.Add(new IntVec3(center.x + 2 + offset, 0, center.z + z));
                }
                else
                {
                    for (int z = -3; z <= 3; z++)
                        cells.Add(new IntVec3(center.x - 2 - offset, 0, center.z + z));
                }
            }

            GenDraw.DrawFieldEdges(cells);
        }
    }
}
