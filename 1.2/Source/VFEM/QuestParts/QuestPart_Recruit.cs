using KCSG;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Grammar;

namespace VFEMech
{
	public class QuestPart_Recruit : QuestPart
	{
		public string inSignal;

		public bool sendStandardLetter = true;

		public bool leaveOnCleanup = true;

		public string inSignalRemovePawn;

		public float recruitChance = 1f;

		public Site site;
		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (signal.tag == inSignal)
			{
				var remainingPawns = new List<Pawn>();
				var recruitCandidatePawns = site.Map.mapPawns.PawnsInFaction(site.Map.ParentFaction).Where(x => !x.Dead && x.RaceProps.Humanlike).InRandomOrder().ToList();
				var pawnsToRecruit = new List<Pawn>();
				var firstPawnRecruit = recruitCandidatePawns.FirstOrDefault();
				if (firstPawnRecruit != null)
                {
					pawnsToRecruit.Add(firstPawnRecruit);
					recruitCandidatePawns.Remove(firstPawnRecruit);
				}
				foreach (var p in recruitCandidatePawns)
				{
					if (Rand.Chance(recruitChance))
                    {
						pawnsToRecruit.Add(p);
					}
					else
                    {
						remainingPawns.Add(p);
                    }
				}

				foreach (var p in pawnsToRecruit)
                {
					var letter = (ChoiceLetter_AcceptVisitors)LetterMaker.MakeLetter("VFEMech.LetterJoinOfferLabel".Translate(p.Named("PAWN"))
					, "VFEMech.LetterJoinOfferTitle".Translate(p.Named("PAWN"))
					, VFEMDefOf.VFEMech_AcceptVisitors, null, quest);
					letter.title = "VFEMech.LetterJoinOfferText".Translate(p.Named("PAWN"));
					letter.pawn = p;
					letter.quest = quest;
					letter.lookTargets = new LookTargets(p);
					Find.LetterStack.ReceiveLetter(letter);
				}
				//foreach (var pawn in remainingPawns)
				//{
				//	if (pawn.GetLord() != null)
				//	{
				//		pawn.GetLord().ownedPawns.Remove(pawn);
				//	}
				//}
				//
				//if (remainingPawns.Any())
				//{
				//	Pawn pawn = remainingPawns.First();
				//	LordJob_ExitMapBest lordJob = new LordJob_ExitMapBest(LocomotionUrgency.Walk, canDig: true, canDefendSelf: true);
				//	LordMaker.MakeNewLord(pawn.Faction, lordJob, pawn.MapHeld, remainingPawns);
				//}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_Values.Look(ref recruitChance, "recruitChance");
			Scribe_References.Look(ref site, "site");
			Scribe_Values.Look(ref sendStandardLetter, "sendStandardLetter", defaultValue: true);
			Scribe_Values.Look(ref leaveOnCleanup, "leaveOnCleanup", defaultValue: false);
			Scribe_Values.Look(ref inSignalRemovePawn, "inSignalRemovePawn");
		}
	}
}