using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace VFEMech
{
    public enum PropagandaMode
    {
        RelaxingMusic,
        InspirationalMusic,
        IdeoPropaganda,
        Recruitment
    };
	public class CompProperties_Propaganda : CompProperties
	{
        public float relaxingMusicRadius;
        public ThoughtDef relaxingThought;

        public float inspirationalMusicRadius;
        public HediffDef inspirationalHediff;

        public float propagandaRadius;
        public float propagandaPower;
        public int propagandaRate;

        public float recruitmentRadius;
        public float resistanceSuppressPower;
        public float willSuppressPower;
        public int recruitmentRate;

        public CompProperties_Propaganda()
		{
			compClass = typeof(CompPropaganda);
		}
	}
	public class CompPropaganda : ThingComp
    {
        public Pawn Pawn => this.parent as Pawn;
        public CompProperties_Propaganda Props => base.props as CompProperties_Propaganda;

        public PropagandaMode curPropagandaMode;

        public Ideo curIdeo;

        public static HashSet<CompPropaganda> compPropagandas = new HashSet<CompPropaganda>();
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            curIdeo = this.parent.Faction.ideos.PrimaryIdeo;
            compPropagandas.Add(this);
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            compPropagandas.Remove(this);
        }

        public List<Hediff_Propaganda> givenHediffs = new List<Hediff_Propaganda>();
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var g in base.CompGetGizmosExtra())
            {
                yield return g;
            }
            yield return new Command_Action
            {
                defaultLabel = "VFEM.PropagandaMode".Translate(("VFEM." + curPropagandaMode.ToString()).Translate()),
                defaultDesc = ("VFEM." + curPropagandaMode.ToString() + "Desc").Translate(),
                action = () =>
                {
                    var floatList = new List<FloatMenuOption>();
                    foreach (var propagandaMode in Enum.GetValues(typeof(PropagandaMode)).Cast<PropagandaMode>())
                    {
                        if (propagandaMode != curPropagandaMode)
                        {
                            floatList.Add(new FloatMenuOption(("VFEM." + propagandaMode.ToString()).Translate(), delegate
                            {
                                curPropagandaMode = propagandaMode;
                            }));
                        }
                    }
                    Find.WindowStack.Add(new FloatMenu(floatList));
                },
                icon = ContentFinder<Texture2D>.Get("UI/Propaganda/" + curPropagandaMode.ToString())
            };
            if (curPropagandaMode == PropagandaMode.IdeoPropaganda)
            {
                if (this.parent.Faction.ideos.AllIdeos.Count() > 1)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "VFEM.SelectIdeology".Translate(),
                        defaultDesc = "VFEM.SelectIdeologyDesc".Translate(),
                        action = () =>
                        {
                            var floatList = new List<FloatMenuOption>();
                            foreach (var ideo in this.parent.Faction.ideos.AllIdeos)
                            {
                                if (ideo != curIdeo)
                                {
                                    floatList.Add(new FloatMenuOption(ideo.name, delegate
                                    {
                                        curIdeo = ideo;
                                    }, ideo.Icon, ideo.Color));
                                }
                            }
                            Find.WindowStack.Add(new FloatMenu(floatList));
                        },
                        icon = curIdeo.Icon,
                        defaultIconColor = curIdeo.Color
                    };
                }
            }
        }

        [HarmonyPatch(typeof(Pawn_IdeoTracker), "SetIdeo")]
        public static class SetIdeo_Patch
        {
            public static void Postfix(Pawn ___pawn)
            {
                if (___pawn.IsColonist)
                {
                    RecheckBots();
                }
            }
        }

        public static void RecheckBots()
        {
            foreach (var comp in compPropagandas)
            {
                if (!comp.parent.Faction.ideos.AllIdeos.Any(x => x == comp.curIdeo))
                {
                    comp.curIdeo = comp.parent.Faction.ideos.PrimaryIdeo;
                }
            }
        }
        public override void CompTick()
        {
            base.CompTick();
            bool shouldWork = true;
            var pawn = Pawn;
            if (pawn != null && pawn.CurJobDef != JobDefOf.GotoWander && pawn.CurJobDef != JobDefOf.Wait_Wander)
            {
                shouldWork = false;
            }
            if (shouldWork)
            {
                switch (curPropagandaMode)
                {
                    case PropagandaMode.RelaxingMusic:
                        DoRelaxingMusic();
                        break;
                    case PropagandaMode.InspirationalMusic:
                        DoInspirationalMusic();
                        break;
                    case PropagandaMode.IdeoPropaganda:
                        DoIdeoPropaganda();
                        break;
                    case PropagandaMode.Recruitment:
                        DoPrisonerPropaganda();
                        break;
                }
            }
            for (var i = givenHediffs.Count - 1; i >= 0; i--)
            {
                var hediff = givenHediffs[i];
                if (!shouldWork || !this.parent.Spawned || this.parent.Destroyed || hediff.pawn.Position.DistanceTo(this.parent.Position) > this.Props.propagandaRadius)
                {
                    hediff.pawn.health.RemoveHediff(hediff);
                    givenHediffs.RemoveAt(i);
                }
            }
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            for (var i = givenHediffs.Count - 1; i >= 0; i--)
            {
                var hediff = givenHediffs[i];
                hediff.pawn.health.RemoveHediff(hediff);
                givenHediffs.RemoveAt(i);
            }
        }

        private IEnumerable<Pawn> PawnsAround(float radius)
        {
            return GenRadial.RadialDistinctThingsAround(this.parent.Position, this.parent.Map, radius, true).OfType<Pawn>();
        }
        private void DoRelaxingMusic()
        {
            foreach (var pawn in PawnsAround(Props.relaxingMusicRadius))
            {
                if (pawn.RaceProps.Humanlike && pawn.needs?.mood?.thoughts?.memories != null)
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemory(Props.relaxingThought);
                }
            }
        }
        private void DoInspirationalMusic()
        {
            foreach (var pawn in PawnsAround(Props.inspirationalMusicRadius))
            {
                if (pawn.RaceProps.Humanlike)
                {
                    var hediff = HediffMaker.MakeHediff(Props.inspirationalHediff, pawn) as Hediff_Propaganda;
                    pawn.health.AddHediff(hediff);
                    if (givenHediffs is null)
                    {
                        givenHediffs = new List<Hediff_Propaganda>();
                    }
                    givenHediffs.Add(hediff);
                }
            }
        }

        private void DoIdeoPropaganda()
        {
            if (this.parent.IsHashIntervalTick(Props.propagandaRate))
            {
                foreach (var pawn in PawnsAround(Props.propagandaRadius))
                {
                    if (pawn.RaceProps.Humanlike)
                    {
                        if (curIdeo != pawn.Ideo)
                        {
                            pawn.ideo.Reassure(-Props.propagandaPower);
                            if (pawn.ideo.Certainty <= 0)
                            {
                                pawn.ideo.SetIdeo(curIdeo);
                            }
                        }
                        else
                        {
                            pawn.ideo.Reassure(Props.propagandaPower);
                        }
                    }
                }
            }
        }

        private void DoPrisonerPropaganda()
        {
            if (this.parent.IsHashIntervalTick(Props.recruitmentRate))
            {
                foreach (var recipient in PawnsAround(Props.recruitmentRadius))
                {
                    if (recipient.RaceProps.Humanlike && recipient.guest != null)
                    {
                        if (recipient.guest.interactionMode == PrisonerInteractionModeDefOf.ReduceResistance || recipient.guest.interactionMode == PrisonerInteractionModeDefOf.AttemptRecruit)
                        {
                            float resistance = recipient.guest.resistance;
                            if (resistance > 0)
                            {
                                float num2 = Props.resistanceSuppressPower;
                                float resistanceReduce = 0f;
                                recipient.guest.resistance = Mathf.Max(0f, recipient.guest.resistance - num2);
                                resistanceReduce = resistance - recipient.guest.resistance;
                                string text = "TextMote_ResistanceReduced".Translate(resistance.ToString("F1"), recipient.guest.resistance.ToString("F1"));
                                if (recipient.needs.mood != null && recipient.needs.mood.CurLevelPercentage < 0.4f)
                                {
                                    text += "\n(" + "lowMood".Translate() + ")";
                                }
                                MoteMaker.ThrowText(recipient.DrawPos, this.parent.Map, text, 8f);
                                if (recipient.guest.resistance == 0f)
                                {
                                    TaggedString taggedString2 = "MessagePrisonerResistanceBroken".Translate(recipient.LabelShort, this.parent.LabelShort, this.parent.Named("WARDEN"), recipient.Named("PRISONER"));
                                    if (recipient.guest.interactionMode == PrisonerInteractionModeDefOf.AttemptRecruit)
                                    {
                                        taggedString2 += " " + "MessagePrisonerResistanceBroken_RecruitAttempsWillBegin".Translate();
                                    }
                                    Messages.Message(taggedString2, recipient, MessageTypeDefOf.PositiveEvent);
                                }
                                if (this.parent is Pawn pawn)
                                {
                                    recipient.guest.SetLastRecruiterData(pawn, resistanceReduce);
                                }
                            }
                        }
                        else if (recipient.guest.interactionMode == PrisonerInteractionModeDefOf.ReduceWill || recipient.guest.interactionMode == PrisonerInteractionModeDefOf.Enslave)
                        {
                            if (recipient.guest.will > 0)
                            {
                                float num = Props.willSuppressPower;
                                num = Mathf.Min(num, recipient.guest.will);
                                float will = recipient.guest.will;
                                recipient.guest.will = Mathf.Max(0f, recipient.guest.will - num);
                                _ = recipient.guest.will;
                                string text = "TextMote_WillReduced".Translate(will.ToString("F1"), recipient.guest.will.ToString("F1"));
                                if (recipient.needs.mood != null && recipient.needs.mood.CurLevelPercentage < 0.4f)
                                {
                                    text += "\n(" + "lowMood".Translate() + ")";
                                }
                                MoteMaker.ThrowText(recipient.DrawPos, parent.Map, text, 8f);
                                if (recipient.guest.will == 0f)
                                {
                                    TaggedString taggedString = "MessagePrisonerWillBroken".Translate(parent, recipient);
                                    if (recipient.guest.interactionMode == PrisonerInteractionModeDefOf.AttemptRecruit)
                                    {
                                        taggedString += " " + "MessagePrisonerWillBroken_RecruitAttempsWillBegin".Translate();
                                    }
                                    Messages.Message(taggedString, recipient, MessageTypeDefOf.PositiveEvent);
                                }
                            }
                         }
                    }
                }
            }
        }

        public override string CompInspectStringExtra()
        {
            if (curPropagandaMode == PropagandaMode.IdeoPropaganda)
            {
                return "VFEM.CurrentIdeology".Translate(curIdeo.name);
            }
            return null;
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref curPropagandaMode, "curPropagandaMode");
            Scribe_Collections.Look(ref givenHediffs, "givenHediffs", LookMode.Reference);
            Scribe_References.Look(ref curIdeo, "curIdeo");
        }
    }
}
