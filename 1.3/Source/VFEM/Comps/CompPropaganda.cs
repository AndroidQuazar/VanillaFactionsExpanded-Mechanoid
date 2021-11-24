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
        public float recruitmentPower;
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

        private void DoRelaxingMusic()
        {
            foreach (var pawn in GenRadial.RadialDistinctThingsAround(this.parent.Position, this.parent.Map, Props.relaxingMusicRadius, true).OfType<Pawn>())
            {
                if (pawn.RaceProps.Humanlike && pawn.needs?.mood?.thoughts?.memories != null)
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemory(Props.relaxingThought);
                }
            }
        }
        private void DoInspirationalMusic()
        {
            foreach (var pawn in GenRadial.RadialDistinctThingsAround(this.parent.Position, this.parent.Map, Props.inspirationalMusicRadius, true).OfType<Pawn>())
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
                foreach (var pawn in GenRadial.RadialDistinctThingsAround(this.parent.Position, this.parent.Map, Props.relaxingMusicRadius, true).OfType<Pawn>())
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
