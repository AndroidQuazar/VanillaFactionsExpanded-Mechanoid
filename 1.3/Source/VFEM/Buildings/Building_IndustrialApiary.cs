using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VFEMech
{
    public class Building_IndustrialApiary : Building
    {
        int ApiaryProgress;
        int duration = 240000;      
        public float neededFlower;
        float progressPercentage;
        private int flowerCount;
        private int flowerNeeded;
        private List<IntVec3> cellsAround = new List<IntVec3>();
        CompPowerTrader compPower;

        public bool HoneyReady
        {
            get
            {
                return ApiaryProgress >= duration;
            }
        }

        private bool PowerOn
        {
            get
            {
               
                return compPower != null && compPower.PowerOn;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            compPower = this.GetComp<CompPowerTrader>();
        }

        private int EstimatedTicksLeft
        {
            get
            {
                return this.duration - this.ApiaryProgress;
            }
        }

        private bool IsthereFlowerAround;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.ApiaryProgress, "ApiaryProgress", 0, false);           
            Scribe_Values.Look<int>(ref this.flowerCount, "flowerCount", 0, false);
            Scribe_Values.Look<float>(ref this.neededFlower, "neededFlower", 0, false);
        }

        public override void TickRare()
        {
            base.TickRare();
            if (PowerOn) {
                progressPercentage = (float)this.ApiaryProgress / this.duration;
                IsthereFlowerAround = FlowerAround();
                flowerNeeded = FlowerNeeded();

                if (!(this.AmbientTemperature < 10) && IsthereFlowerAround)
                {

                    this.ApiaryProgress += 250;

                }

                if (HoneyReady)
                {
                    Thing thing = ThingMaker.MakeThing(ThingDef.Named("VFEV_Honey"), null);
                    thing.stackCount = 75;
                    GenPlace.TryPlaceThing(thing, this.InteractionCell, this.Map, ThingPlaceMode.Direct);
                    this.Reset();

                }
            }
            
        }

        private void Reset()
        {
            this.ApiaryProgress = 0;
        }

        

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            if (stringBuilder.Length != 0)
            {
                stringBuilder.AppendLine();
            }

            if (this.HoneyReady)
            {
                stringBuilder.AppendLine("AReady".Translate());
            }
            else
            {
                if (!IsthereFlowerAround)
                {
                    stringBuilder.AppendLine("ANeedFlower".Translate(flowerNeeded));
                }
                else
                {
                    if (this.AmbientTemperature < 10)
                    {
                        stringBuilder.AppendLine("AResting".Translate());
                    }
                    else
                    {
                        stringBuilder.AppendLine("FermentationProgress".Translate(this.progressPercentage.ToStringPercent(), this.EstimatedTicksLeft.ToStringTicksToPeriod()));
                        
                    }
                }
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }

       

        private int FlowerNeeded()
        {
            int i = (cellsAround.Count - 8) / 2;
            i -= flowerCount;
            return i;
        }

        public bool FlowerAround()
        {
            flowerCount = 0;
            cellsAround = CellsAroundA(this.TrueCenter().ToIntVec3(), this.Map);
            foreach (IntVec3 cell in cellsAround)
            {
                foreach (Thing item in cell.GetThingList(this.Map))
                {
                    if (item.def.plant != null && item.def.plant.purpose == PlantPurpose.Beauty)
                    {
                        flowerCount++;
                    }
                }
            }
            if (flowerCount >= (int)((cellsAround.Count - 8) / 2))
            {
                return true;
            }
            return false;
        }

        public List<IntVec3> CellsAroundA(IntVec3 pos, Map map)
        {
            List<IntVec3> cellsAround = new List<IntVec3>();
            if (!pos.InBounds(map))
            {
                return cellsAround;
            }
            IEnumerable<IntVec3> cells = CellRect.CenteredOn(this.Position, 7).Cells;
            foreach (IntVec3 item in cells)
            {
                if (item.InHorDistOf(pos, 6.9f))
                {
                    cellsAround.Add(item);
                }
            }
            return cellsAround;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }
            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Debug: Set progress to max",
                    action = delegate ()
                    {
                        this.ApiaryProgress = this.duration;
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "Debug: Add progress",
                    action = delegate ()
                    {
                        this.ApiaryProgress += 12000;
                      
                    }
                };
            }
            yield break;
        }
    }
}
