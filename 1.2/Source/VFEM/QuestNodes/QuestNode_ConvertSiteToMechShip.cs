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
	public class QuestNode_ConvertSiteToMechShip : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> inSignal;
		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			QuestPart_ConvertSiteToMechShip questPart_ConvertSiteToMechShip = new QuestPart_ConvertSiteToMechShip();
			questPart_ConvertSiteToMechShip.site = slate.Get<Site>("site");
			questPart_ConvertSiteToMechShip.inSignal = (QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal"));
			QuestGen.quest.AddPart(questPart_ConvertSiteToMechShip);
		}
	}
}