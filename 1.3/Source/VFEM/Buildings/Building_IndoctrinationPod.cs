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
using static UnityEngine.Random;
using Verse.Noise;
using Verse.Sound;

namespace VFEMech
{
    public class Building_IndoctrinationPod : Building_Casket, ISuspendableThingHolder
    {
        private Ideo ideoConversionTarget;

        public const int IndocrtinationTickRate = 2500;
        public static readonly FloatRange IndoctrinationPower = new FloatRange(0.01f, 0.02f);
        public Pawn InnerPawn => ContainedThing as Pawn;

        public CompPowerTrader compPower;
        public bool IsContentsSuspended => InnerPawn != null;
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
            else if (pawn != null)
            {
                this.EjectContents();
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
            if (myPawn.IsQuestLodger())
            {
                yield return new FloatMenuOption("CannotUseReason".Translate("CryptosleepCasketGuestsNotAllowed".Translate()), null);
                yield break;
            }
            foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(myPawn))
            {
                yield return floatMenuOption;
            }
            if (innerContainer.Count != 0)
            {
                yield break;
            }
            if (!myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly))
            {
                yield return new FloatMenuOption("CannotUseNoPath".Translate(), null);
                yield break;
            }
            JobDef jobDef = VFEMDefOf.VFEM_EnterIndoctrinationPod;
            var reason = CannotUseNowReason(myPawn);
            if (reason != null)
            {
                yield return new FloatMenuOption("VFEM.CannotEnter".Translate(reason), null);
            }
            else
            {
                string label = "VFEMech.EnterIndoctrinationPod".Translate();
                Action action = delegate
                {
                    Job job = JobMaker.MakeJob(jobDef, this);
                    myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                };
                yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action), myPawn, this);
            }
        }



        public string CannotUseNowReason(Pawn myPawn)
        {
            if (!compPower.PowerOn)
            {
                return "NoPower".Translate();
            }
            else if (myPawn.Ideo == ideoConversionTarget)
            {
                return "VFEM.MustHaveAnotherIdeology".Translate();
            }
            return null;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            if (base.Faction == Faction.OfPlayer)
            {
                if (innerContainer.Count > 0 && def.building.isPlayerEjectable)
                {
                    Command_Action command_Action = new Command_Action();
                    command_Action.action = EjectContents;
                    command_Action.defaultLabel = "CommandPodEject".Translate();
                    command_Action.defaultDesc = "CommandPodEjectDesc".Translate();
                    if (innerContainer.Count == 0)
                    {
                        command_Action.Disable("CommandPodEjectFailEmpty".Translate());
                    }
                    command_Action.hotKey = KeyBindingDefOf.Misc8;
                    command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/PodEject");
                    yield return command_Action;
                }

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
                        },
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
                        command.defaultIconColor = this.ideoConversionTarget.Color;
                        yield return command;
                    }
                }
            }
        }


        public override void EjectContents()
        {
            ThingDef filth_Slime = ThingDefOf.Filth_Slime;
            foreach (Thing item in (IEnumerable<Thing>)innerContainer)
            {
                Pawn pawn2 = item as Pawn;
                if (pawn2 != null)
                {
                    PawnComponentsUtility.AddComponentsForSpawn(pawn2);
                    pawn2.filth.GainFilth(filth_Slime);
                    if (pawn2.RaceProps.IsFlesh)
                    {
                        pawn2.health.AddHediff(HediffDefOf.CryptosleepSickness);
                    }
                }
            }

            if (!base.Destroyed)
            {
                SoundDefOf.CryptosleepCasket_Eject.PlayOneShot(SoundInfo.InMap(new TargetInfo(base.Position, base.Map)));
            }
            var pawn = InnerPawn;
            if (pawn != null)
            {
                if (pawn.Ideo != ideoConversionTarget || pawn.ideo.Certainty < 1f)
                {
                    var hediff = HediffMaker.MakeHediff(VFEMDefOf.VFE_BrainWashedNotFully, pawn);
                    pawn.health.AddHediff(hediff);
                }
                else
                {
                    var hediff = HediffMaker.MakeHediff(VFEMDefOf.VFE_BrainWashedFully, pawn);
                    pawn.health.AddHediff(hediff);
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
