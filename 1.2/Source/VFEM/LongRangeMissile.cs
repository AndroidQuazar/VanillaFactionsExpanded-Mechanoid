using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFEMech
{
    using RimWorld;
    using RimWorld.Planet;
    using UnityEngine;
    using Verse;

    public class MissileSilo : Building
    {
        public GlobalTargetInfo target = GlobalTargetInfo.Invalid;

        public List<ThingDefCountClass> costList = new List<ThingDefCountClass>();
        public List<int> delivered = new List<int>();

        public static List<ThingDefCountClass> startCostList = new List<ThingDefCountClass>()
                                                               {
                                                                   new ThingDefCountClass(ThingDefOf.Plasteel, 45),
                                                                   new ThingDefCountClass(ThingDefOf.ComponentSpacer, 3),
                                                                   new ThingDefCountClass(ThingDefOf.Uranium, 45)
                                                               };

        public bool TargetAcquired =>
            this.target != GlobalTargetInfo.Invalid;

        public bool Satisfied
        {
            get
            {
                if (!this.TargetAcquired)
                    return false;
                for (int index = 0; index < this.costList.Count; index++)
                {
                    ThingDefCountClass defCount = this.costList[index];
                    if (this.delivered[index] < defCount.count)
                        return false;
                }

                return true;
            }
        }

        public List<ThingDef> ThingsNeeded =>
            this.costList.Select(tdcc => tdcc.thingDef).Where(td => this.CostMissing(td) > 0).ToList();

        public int CostMissing(ThingDef def)
        {
            for (int index = 0; index < costList.Count; index++)
            {
                ThingDefCountClass thingDefCountClass = costList[index];
                if (thingDefCountClass.thingDef == def)
                    return Mathf.Max(0, thingDefCountClass.count - this.delivered[index]);
            }
            return 0;
        }

        public void AddCost(Thing thing)
        {
            if (this.CostMissing(thing.def) <= 0) return;

            this.AddCost(thing.def, thing.stackCount);
            thing.Destroy();
        }

        public void AddCost(ThingDef def, int count)
        {
            for (int index = 0; index < costList.Count; index++)
            {
                ThingDefCountClass thingDefCountClass = costList[index];
                if (thingDefCountClass.thingDef == def)
                    this.delivered[index] += count;
            }
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder(base.GetInspectString());

            if (!TargetAcquired)
            {
                sb.AppendLine("VFEM_SiloNoTarget".Translate());
            }
            else
            {
                WorldObject target = this.target.WorldObject;

                sb.AppendLine("VFEM_SiloTargeting".Translate(target.LabelCap, target.Faction.Name));
                sb.AppendLine((this.Satisfied ? "VFEM_SiloConditionsSatisfied" : "VFEM_SiloConditionsUnsatisfied").Translate());
                for (int i = 0; i < this.costList.Count; i++)
                {
                    ThingDefCountClass countClass = this.costList[i];
                    sb.AppendLine("VFEM_SiloCountEntry".Translate(countClass.thingDef.LabelCap, this.delivered[i], countClass.count));
                }
            }

            return sb.ToString().TrimEndNewlines();
        }

        public bool ConfigureNewTarget(GlobalTargetInfo targetInfo)
        {
            
            if (!targetInfo.IsValid || !targetInfo.HasWorldObject)
                return false;

            if (Find.QuestManager.QuestsListForReading.Any(q => !q.Historical && !q.dismissed && q.QuestLookTargets.Contains(targetInfo.WorldObject)))
            {
                Messages.Message("VFEM_SiloAimAtQuestSite".Translate(), MessageTypeDefOf.NeutralEvent);
                return false;
            }

            int distance = Find.WorldGrid.TraversalDistanceBetween(this.Map.Tile, targetInfo.Tile);
            
            if (distance > 132)
                return false;
            
            this.target = targetInfo;
            this.costList = startCostList.ListFullCopy();
            
            this.costList.Add(new ThingDefCountClass(ThingDefOf.Chemfuel, Mathf.CeilToInt(distance * 2.25f)));
            
            if(this.delivered.Count == 0)
                for (int i = 0; i < this.costList.Count; i++) 
                    this.delivered.Add(0);

            return true;
        }

        public void Fire()
        {
            if (!this.Satisfied)
                return;

            for (int i = 0; i < this.costList.Count; i++)
            {
                ThingDefCountClass entry = this.costList[i];
                this.delivered[i] -= entry.count;
            }

            this.target.WorldObject.Faction.TryAffectGoodwillWith(Faction.OfPlayer, -200);
            this.target.WorldObject.Destroy();

            this.target = GlobalTargetInfo.Invalid;

            foreach (ThingDefCountClass tdcc in this.costList) 
                tdcc.count = 0;

            Messages.Message("FIRE", MessageTypeDefOf.PositiveEvent);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos()) 
                yield return gizmo;


            Command_Action aimingGizmo = new Command_Action()
                                    {
                                        action = () =>
                                                 {
                                                     CameraJumper.TryJump(CameraJumper.GetWorldTarget(this));
                                                     Find.WorldSelector.ClearSelection();
                                                     Find.WorldTargeter.BeginTargeting_NewTemp(this.ConfigureNewTarget, false, onUpdate: () => GenDraw.DrawWorldRadiusRing(this.Map.Tile, 132), closeWorldTabWhenFinished: true);
                                                 },
                                        defaultLabel = "Aim"
                                    };
            yield return aimingGizmo;

            Command_Action fireGizmo = new Command_Action()
                                       {
                                           action = Fire,
                                           disabled = !Satisfied,
                                           disabledReason = (TargetAcquired ? "VFEM_SiloConditionsUnsatisfied" : "VFEM_SiloNoTarget").Translate(),
                                           defaultLabel = "Fire"
                                       };
            yield return fireGizmo;

            if (Prefs.DevMode)
            {
                yield return new Command_Action()
                             {
                                 action = () =>
                                          {
                                              for (int index = 0; index < this.costList.Count; index++)
                                              {
                                                  ThingDefCountClass countClass = this.costList[index];
                                                  this.delivered[index] = countClass.count;
                                              }
                                          },
                                 defaultLabel = "DEV: Fill Conditions",
                                 disabled = !this.target.IsValid
                             };
                yield return new Command_Action()
                             {
                                 action = () =>
                                          {
                                              for (int index = 0; index < this.costList.Count; index++)
                                              {
                                                  ThingDefCountClass countClass = this.costList[index];
                                                  this.delivered[index] = 100;
                                              }
                                          },
                                 defaultLabel = "DEV: Fill Conditions: 100"
                             };
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref this.costList, nameof(this.costList));
            Scribe_Collections.Look(ref this.delivered, nameof(this.delivered));
            Scribe_TargetInfo.Look(ref this.target, nameof(this.target));
        }
    }
}
