using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;

namespace VFEMech
{
	public class ChoiceLetter_AcceptVisitors : ChoiceLetter
	{
		public Pawn pawn;
		public override bool CanDismissWithRightClick => false;

		public override bool CanShowInLetterStack
		{
			get
			{
				if (!base.CanShowInLetterStack)
				{
					return false;
				}
				if (quest == null)
				{
					return false;
				}
				bool result = false;
				if (CanStillAccept(pawn))
				{
					result = true;
				}
				return result;
			}
		}

		private DiaOption Option_Accept
		{
			get
			{
				DiaOption diaOption = new DiaOption("AcceptButton".Translate());
				diaOption.action = delegate
				{
					if (CanStillAccept(pawn))
                    {
						var letter = LetterMaker.MakeLetter("LetterLabelMessageRecruitSuccess".Translate() + ": " + pawn.LabelShortCap,
							"MessageRecruitJoinOfferAccepted".Translate(pawn.Named("RECRUITEE")), LetterDefOf.PositiveEvent, pawn.Faction, quest);
						Find.LetterStack.ReceiveLetter(letter);
						if (pawn.Faction != Faction.OfPlayer)
						{
							pawn.SetFaction(Faction.OfPlayer);
						}
						List<Quest> questsListForReading = Find.QuestManager.QuestsListForReading;
						for (int i = 0; i < questsListForReading.Count; i++)
						{
							List<QuestPart> partsListForReading = questsListForReading[i].PartsListForReading;
							for (int j = 0; j < partsListForReading.Count; j++)
							{
								QuestPart_ExtraFaction questPart_ExtraFaction = partsListForReading[j] as QuestPart_ExtraFaction;
								if (questPart_ExtraFaction != null && questPart_ExtraFaction.affectedPawns.Contains(pawn))
								{
									questPart_ExtraFaction.affectedPawns.Remove(pawn);
								}
							}
						}
						if (pawn.GetLord() != null)
                        {
							pawn.GetLord().ownedPawns.Remove(pawn);
                        }

					}
					Find.LetterStack.RemoveLetter(this);
				};
				diaOption.resolveTree = true;
				bool flag = false;

				if (CanStillAccept(pawn))
				{
					flag = true;
				}
				if (!flag)
				{
					diaOption.Disable(null);
				}
				return diaOption;
			}
		}

		public override IEnumerable<DiaOption> Choices
		{
			get
			{
				if (!base.ArchivedOnly)
				{
					yield return Option_Accept;
					yield return base.Option_Reject;
					yield return base.Option_Postpone;
				}
				else
				{
					yield return base.Option_Close;
				}
				if (lookTargets.IsValid())
				{
					yield return base.Option_JumpToLocationAndPostpone;
				}
				if (quest != null && !quest.hidden)
				{
					yield return Option_ViewInQuestsTab("ViewRelatedQuest", postpone: true);
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref pawn, "pawns");
		}

		private bool CanStillAccept(Pawn p)
		{
			if (p.DestroyedOrNull() || !p.SpawnedOrAnyParentSpawned)
			{
				return false;
			}
			if (p.CurJob != null && p.CurJob.exitMapOnArrival)
			{
				return false;
			}
			return true;
		}
	}
}
