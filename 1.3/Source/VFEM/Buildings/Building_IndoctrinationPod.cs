using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace VFEMech
{
    public class Building_IndoctrinationPod : Building_CryptosleepCasket
    {
        private Ideo ideoConversionTarget;

        public const int IndocrtinationTickRate = 2500;
        public static readonly FloatRange IndoctrinationPower = new FloatRange(0.01f, 0.02f);
        public Pawn InnerPawn => ContainedThing as Pawn;

        public CompPowerTrader compPower;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            compPower = this.TryGetComp<CompPowerTrader>();
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad && this.Faction != null)
            {
                ideoConversionTarget = this.Faction.ideos.PrimaryIdeo;
            }
        }
        public override void Tick()
        {
            base.Tick();
            var pawn = InnerPawn;
            if (ideoConversionTarget != null && this.Faction != null && !this.Faction.ideos.AllIdeos.Contains(ideoConversionTarget))
            {
                ideoConversionTarget = null;
            }
            if (compPower.PowerOn && ideoConversionTarget != null && pawn != null && pawn.Ideo != null)
            {
                if (pawn.IsHashIntervalTick(IndocrtinationTickRate))
                {
                    if (pawn.Ideo != ideoConversionTarget)
                    {
                        var certaintyReduce = -IndoctrinationPower.RandomInRange;
                        pawn.ideo.Reassure(certaintyReduce);
                        if (pawn.ideo.Certainty == 0f)
                        {
                            pawn.ideo.SetIdeo(ideoConversionTarget);
                            Traverse.Create(pawn.ideo).Field("certainty").SetValue(0);
                        }
                    }
                    else
                    {
                        var certaintyGain = IndoctrinationPower.RandomInRange;
                        pawn.ideo.Reassure(certaintyGain);
                    }
                }

                if (pawn.Ideo == ideoConversionTarget && pawn.ideo.Certainty == 1f)
                {
                    this.EjectContents();
                }
            }
        }

        public override string GetInspectString()
        {
            var sb = new StringBuilder(base.GetInspectString() + "\n");
            sb.AppendLine("VFEMech.IdeologyConversionTarget".Translate(ideoConversionTarget?.name ?? "None".Translate()));
            if (InnerPawn != null)
            {
                sb.AppendLine("VFEMech.CurrentCertainty".Translate(InnerPawn.Named("PAWN"), InnerPawn.Ideo.name, InnerPawn.ideo.Certainty * 100f));
            }
            return sb.ToString().TrimEndNewlines();
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            foreach (var opt in base.GetFloatMenuOptions(myPawn))
            {
                if (opt.Label == "EnterCryptosleepCasket".Translate())
                {
                    JobDef jobDef = VFEMDefOf.VFEM_EnterIndoctrinationPod;
                    string label = "VFEMech.EnterIndoctrinationPod".Translate();
                    Action action = delegate
                    {
                        Job job = JobMaker.MakeJob(jobDef, this);
                        myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    };
                    yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action), myPawn, this);
                }
                else
                {
                    yield return opt;
                }
            }
        }
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }
            if (this.Faction == Faction.OfPlayer)
            {
                if (this.ideoConversionTarget is null)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "VFEMech.SetIdeologyConversionTarget".Translate(),
                        defaultDesc = "VFEMech.SetIdeologyConversionTargetDesc".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/IdeologyConversionTarget"),
                        action = () =>
                        {
                            var floatList = new List<FloatMenuOption>();
                            foreach (var ideo in this.Faction.ideos.AllIdeos)
                            {
                                if (ideo != ideoConversionTarget)
                                {
                                    floatList.Add(new FloatMenuOption(ideo.name, () =>
                                    {
                                        this.ideoConversionTarget = ideo;
                                    }, ideo.Icon, ideo.Color));
                                }
                            }
                            Find.WindowStack.Add(new FloatMenu(floatList));
                        }
                    };
                }
                else
                {
                    if (this.Faction.ideos.AllIdeos.Count() > 1)
                    {
                        var command = new Command_Action
                        {
                            defaultLabel = ideoConversionTarget.name,
                            defaultDesc = "VFEMech.SetIdeologyConversionTargetDesc".Translate(),
                            icon = ideoConversionTarget.Icon
                        };
                        command.action = delegate
                        {
                            var floatList = new List<FloatMenuOption>();
                            foreach (var ideo in this.Faction.ideos.AllIdeos)
                            {
                                if (ideo != ideoConversionTarget)
                                {
                                    floatList.Add(new FloatMenuOption(ideo.name, () =>
                                    {
                                        this.ideoConversionTarget = ideo;
                                    }, ideo.Icon, ideo.Color));
                                }
                            }
                            Find.WindowStack.Add(new FloatMenu(floatList));
                        };
                        yield return command;
                    }
                }
            }
        }

        public override void EjectContents()
        {
            var pawn = InnerPawn;
            if (pawn != null)
            {
                if (pawn.Ideo != ideoConversionTarget || pawn.ideo.Certainty < 1f)
                {
                    var hediff = HediffMaker.MakeHediff(VFEMDefOf.VFE_BrainWashedNotFully, pawn);
                    pawn.health.AddHediff(hediff);
                    pawn.needs.mood.thoughts.memories.TryGainMemory(VFEMDefOf.VFE_Thought_BrainWashedNotFully);
                }
                else
                {
                    var hediff = HediffMaker.MakeHediff(VFEMDefOf.VFE_BrainWashedFully, pawn);
                    pawn.health.AddHediff(hediff);
                    pawn.needs.mood.thoughts.memories.TryGainMemory(VFEMDefOf.VFE_Thought_BrainWashedFully);
                }
            }
            base.EjectContents();
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref ideoConversionTarget, "ideoConversionTarget");
        }
    }
}
