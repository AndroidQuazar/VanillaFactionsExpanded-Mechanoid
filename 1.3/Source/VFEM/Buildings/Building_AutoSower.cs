using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFE.Mechanoids.Buildings
{
    class Building_AutoSower : Building_AutoPlant, IPlantToGrowSettable
    {
        ThingDef plantToPlant = ThingDefOf.Plant_Potato;

        static List<IntVec3> emptyList = new List<IntVec3>();

        public Building_AutoSower()
        {
            blockedByTree = true;
        }

        public IEnumerable<IntVec3> Cells => emptyList;

        protected override void DoWorkOnCell(IntVec3 cell)
        {
            base.DoWorkOnCell(cell);
            List<Thing> thingList = cell.GetThingList(this.Map);
            List<Thing> toDestroy = new List<Thing>();
            foreach(Thing t in thingList)
            {
                if (t.def.plant != null && t.def.plant.harvestedThingDef == null)
                    toDestroy.Add(t);
                else if (t != this && ((t.def.plant != null && t.def.plant.harvestedThingDef != null) || t.def.BlocksPlanting(true)))
                    return;
            }
            foreach (Thing t in toDestroy)
                t.Kill();
            if (plantToPlant.plant.interferesWithRoof && cell.Roofed(this.Map))
                return;
            Thing otherPlant = PlantUtility.AdjacentSowBlocker(plantToPlant, cell, this.Map);
            if (otherPlant != null)
                return;
            if (cell.GetTerrain(this.Map).fertility < plantToPlant.plant.fertilityMin)
                return;

            Plant plant = (Plant)GenSpawn.Spawn(plantToPlant, cell, this.Map);
            plant.Growth = 0.05f;
            plant.sown = true;
            this.Map.mapDrawer.MapMeshDirty(cell, MapMeshFlag.Things);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look<ThingDef>(ref plantToPlant, "plantToPlant");
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            List<Gizmo> gizmos = new List<Gizmo>();
            gizmos.AddRange(base.GetGizmos());

            gizmos.Add(PlantToGrowSettableUtility.SetPlantToGrowCommand(this));

            return gizmos;
        }

        public ThingDef GetPlantDefToGrow()
        {
            return plantToPlant;
        }

        public void SetPlantDefToGrow(ThingDef plantDef)
        {
            plantToPlant = plantDef;
        }

        public bool CanAcceptSowNow()
        {
            return true;
        }
    }
}
