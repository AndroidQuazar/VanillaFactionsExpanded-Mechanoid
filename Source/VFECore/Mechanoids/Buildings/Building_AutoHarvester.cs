using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Sound;

namespace VFE.Mechanoids.Buildings
{
    class Building_AutoHarvester : Building_AutoPlant
    {
        protected override void DoWorkOnCell(IntVec3 cell)
        {
            base.DoWorkOnCell(cell);
            List<Thing> thingList = cell.GetThingList(this.Map);
            List<Plant> toHarvest = new List<Plant>();
            foreach(Thing t in thingList)
            {
                if(t is Plant)
                {
                    Plant plant = (Plant)t;
                    if(plant.HarvestableNow)
                    {
                        toHarvest.Add(plant);
                    }
                }
            }
            foreach(Plant plant in toHarvest)
            {
                int num = plant.YieldNow();
                if (num > 0)
                {
                    Thing thing = ThingMaker.MakeThing(plant.def.plant.harvestedThingDef);
                    thing.stackCount = num;
                    GenPlace.TryPlaceThing(thing, cell, this.Map, ThingPlaceMode.Near);
                    plant.def.plant.soundHarvestFinish.PlayOneShot(this);
                    plant.PlantCollected();
                }
            }
        }
    }
}
