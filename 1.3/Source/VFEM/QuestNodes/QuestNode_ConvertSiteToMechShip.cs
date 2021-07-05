using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

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
            QuestPart_ConvertSiteToMechShip questPart_ConvertSiteToMechShip = new QuestPart_ConvertSiteToMechShip
            {
                site = slate.Get<Site>("site"),
                inSignal = (QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal"))
            };
            QuestGen.quest.AddPart(questPart_ConvertSiteToMechShip);
        }
    }
}