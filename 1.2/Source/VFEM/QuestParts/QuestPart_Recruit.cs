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

		public float randomChance = 1f;

		public Site site;
		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (signal.tag == inSignal)
			{
				var remainingPawns = new List<Pawn>();
				foreach (var p in site.Map.mapPawns.PawnsInFaction(site.Map.ParentFaction).Where(x => !x.Dead && x.RaceProps.Humanlike))
				{
					if (Rand.Chance(randomChance))
                    {
						var letter = (ChoiceLetter_AcceptVisitors)LetterMaker.MakeLetter("LetterJoinOfferLabel".Translate(p.Named("PAWN"))
							, "LetterJoinOfferTitle".Translate(p.Named("PAWN"))
							, LetterDefOf.AcceptVisitors, null, quest);
						letter.title = "LetterJoinOfferText".Translate(p.Named("PAWN"));
						letter.pawns.Add(p);
						letter.quest = quest;
						letter.acceptedSignal = QuestGen.GenerateNewSignal("Accepted");
						letter.lookTargets = new LookTargets(p);
						Find.LetterStack.ReceiveLetter(letter);
						Action action = delegate ()
						{
							quest.Letter(LetterDefOf.PositiveEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, 
								null, filterDeadPawnsFromLookTargets: false, label: "LetterLabelMessageRecruitSuccess".Translate() + ": " + p.LabelShortCap, 
								text: "MessageRecruitJoinOfferAccepted".Translate(p.Named("RECRUITEE")));

							//quest.SignalPass(null, null, null);
						};
						QuestGenUtility.RunInner(action, letter.acceptedSignal);

						//
						//if (p.Faction != Faction.OfPlayer)
						//{
						//	p.SetFaction(Faction.OfPlayer);
						//}
						//List<Quest> questsListForReading = Find.QuestManager.QuestsListForReading;
						//for (int i = 0; i < questsListForReading.Count; i++)
						//{
						//	List<QuestPart> partsListForReading = questsListForReading[i].PartsListForReading;
						//	for (int j = 0; j < partsListForReading.Count; j++)
						//	{
						//		QuestPart_ExtraFaction questPart_ExtraFaction = partsListForReading[j] as QuestPart_ExtraFaction;
						//		if (questPart_ExtraFaction != null && questPart_ExtraFaction.affectedPawns.Contains(p))
						//		{
						//			questPart_ExtraFaction.affectedPawns.Remove(p);
						//		}
						//	}
						//}
					}
					else
                    {
						remainingPawns.Add(p);
                    }
				}

				if (remainingPawns.Any())
                {
					Pawn pawn = remainingPawns.First();
					LordJob_ExitMapBest lordJob = new LordJob_ExitMapBest(LocomotionUrgency.Walk, canDig: true, canDefendSelf: true);
					LordMaker.MakeNewLord(pawn.Faction, lordJob, pawn.MapHeld, remainingPawns);
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_Values.Look(ref randomChance, "randomChance");
			Scribe_References.Look(ref site, "site");
			Scribe_Values.Look(ref sendStandardLetter, "sendStandardLetter", defaultValue: true);
			Scribe_Values.Look(ref leaveOnCleanup, "leaveOnCleanup", defaultValue: false);
			Scribe_Values.Look(ref inSignalRemovePawn, "inSignalRemovePawn");
		}
	}
}