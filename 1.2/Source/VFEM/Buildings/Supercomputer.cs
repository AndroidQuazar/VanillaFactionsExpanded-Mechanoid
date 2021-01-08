using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;
using VFEM;

namespace VFEMech
{
    public class Supercomputer : Building
    {
        private CompPowerTrader compPower;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            compPower = this.GetComp<CompPowerTrader>();
        }
        public override void Tick()
        {
            base.Tick();
            if (compPower.PowerOn && this.Faction == Faction.OfPlayer && Find.TickManager.TicksGame % GenDate.TicksPerHour == 0)
            {
                var proj = Find.ResearchManager.currentProj;
                if (proj != null)
                {
                    FieldInfo fieldInfo = AccessTools.Field(typeof(ResearchManager), "progress");
                    Dictionary<ResearchProjectDef, float> dictionary = fieldInfo.GetValue(Find.ResearchManager) as Dictionary<ResearchProjectDef, float>;
                    if (dictionary.ContainsKey(proj))
                    {
                        dictionary[proj] += MechShipsMod.settings.VFEM_SuperComputerResearchPointYield;
                    }
                    if (proj.IsFinished)
                    {
                        Find.ResearchManager.FinishProject(proj, doCompletionDialog: true);
                    }
                }
            }
        }
    }
}
