using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace VFEMech
{
    public class SitePartWorker_MechanoidAttackParty : SitePartWorker
    {
        public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
        {
            base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);
            var asker = slate.Get<Pawn>("asker");
            var site = part.ParentHolder as Site;
            site.SetFaction(asker.Faction);
        }
    }
}