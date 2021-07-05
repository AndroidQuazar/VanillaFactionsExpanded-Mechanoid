using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace VFEMech
{
    public class SitePartWorker_MechanoidShipLanding : SitePartWorker
    {
        public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
        {
            base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);
            var mechanoidFaction = Find.FactionManager.FirstFactionOfDef(VFEMDefOf.VFE_Mechanoid);
            var site = part.ParentHolder as Site;
            site.SetFaction(mechanoidFaction);
            site.desiredThreatPoints = slate.Get<float>("points", 1000f);
        }
    }
}