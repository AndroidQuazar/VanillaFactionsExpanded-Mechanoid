using KCSG;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Grammar;

namespace VFEMech
{
	public class QuestNode_Recruit : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> inSignal;

		[NoTranslate]
		public SlateRef<string> inSignalRemovePawn;

		public SlateRef<bool?> sendStandardLetter;

		public SlateRef<bool?> leaveOnCleanup;

		public SlateRef<float?> randomChance;

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			QuestPart_Recruit questPart_Recruit = new QuestPart_Recruit();
			questPart_Recruit.site = slate.Get<Site>("site");
			questPart_Recruit.inSignal = (QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal"));
			questPart_Recruit.sendStandardLetter = (sendStandardLetter.GetValue(slate) ?? questPart_Recruit.sendStandardLetter);
			questPart_Recruit.leaveOnCleanup = (leaveOnCleanup.GetValue(slate) ?? questPart_Recruit.leaveOnCleanup);
			questPart_Recruit.inSignalRemovePawn = inSignalRemovePawn.GetValue(slate);
			questPart_Recruit.randomChance = randomChance.GetValue(slate) ?? questPart_Recruit.randomChance;
			QuestGen.quest.AddPart(questPart_Recruit);
		}
	}
}