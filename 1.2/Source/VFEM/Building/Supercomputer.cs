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

namespace VFEMech
{
    public class Supercomputer : Building
    {
        public override void Tick()
        {
            base.Tick();
            if (this.Faction == Faction.OfPlayer && Find.TickManager.TicksGame % GenDate.TicksPerHour == 0)
            {
                var proj = Find.ResearchManager.currentProj;
                if (proj != null)
                {
                    FieldInfo fieldInfo = AccessTools.Field(typeof(ResearchManager), "progress");
                    Dictionary<ResearchProjectDef, float> dictionary = fieldInfo.GetValue(Current.Game.researchManager) as Dictionary<ResearchProjectDef, float>;
                    if (dictionary.ContainsKey(proj))
                    {
                        dictionary[proj] += 1f;
                    }
                }
            }
        }
    }
}
