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

        public const int IndocrtinationTickRate = 60;
        public static readonly FloatRange IndoctrinationPower = new FloatRange(0.01f, 0.02f);
        public Pawn InnerPawn => ContainedThing as Pawn;
        public override void Tick()
        {
            base.Tick();
            var pawn = InnerPawn;
            if (ideoConversionTarget != null && pawn != null && pawn.Ideo != null)
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
                yield return new Command_Action
                {
                    defaultLabel = "VFEMech.SetIdeologyConversionTarget".Translate(),
                    defaultDesc = "VFEMech.SetIdeologyConversionTargetDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/IdeologyConversionTarget"),
                    action = () =>
                    {
                        var floatList = new List<FloatMenuOption>();
                        foreach (var ideo in Find.IdeoManager.IdeosListForReading)
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
