using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFEMech
{
    using RimWorld;
    using RimWorld.Planet;
    using Verse;


    public class IncidentWorker_ShipLanding : IncidentWorker
    {

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            MechanoidBaseIncidentExtension incidentExtension = this.def.GetModExtension<MechanoidBaseIncidentExtension>();
            if (incidentExtension != null && incidentExtension.minimumColonistCount > 0)
            {
                return PawnsFinder.AllMaps_FreeColonistsSpawned.Count >= incidentExtension.minimumColonistCount;
            }
            return base.CanFireNowSub(parms);
        }
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            MechanoidBaseIncidentExtension incidentExtension = this.def.GetModExtension<MechanoidBaseIncidentExtension>();
            WorldObjectDef objectDef = incidentExtension.baseToPlace;

            Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(objectDef);
            Faction faction = Find.FactionManager.FirstFactionOfDef(VFEMDefOf.VFE_Mechanoid);
            settlement.SetFaction(faction);

            try
            {
                if (TileFinder.TryFindPassableTileWithTraversalDistance(Find.AnyPlayerHomeMap.Tile, incidentExtension.minDistance, incidentExtension.maxDistance, out int tile,
                                                                        i => TileFinder.IsValidTileForNewSettlement(i)))
                    settlement.Tile = tile;
            }
            catch
            {
                // ignored
            }

            if(settlement.Tile < 0)
                settlement.Tile = TileFinder.RandomSettlementTileFor(faction);

            settlement.Name = SettlementNameGenerator.GenerateSettlementName(settlement, objectDef.GetModExtension<MechanoidBaseExtension>().nameMaker);
            Find.WorldObjects.Add(settlement);

            string letterLabel = this.def.letterLabel;
            string letterText = this.def.letterText;

            letterText += "\n\n" + objectDef.description;


            this.SendStandardLetter(letterLabel, letterText, def.letterDef, parms, settlement, faction.Name);

            int raisesPresence = objectDef.GetModExtension<MechanoidBaseExtension>().raisesPresence;
            int presence       = MechUtils.MechPresence();
            foreach (MechUpgradeWarningDef warningDef in DefDatabase<MechUpgradeWarningDef>.AllDefsListForReading)
            {
                if (presence >= warningDef.presenceRequired && (presence - raisesPresence) < warningDef.presenceRequired)
                {
                    Messages.Message(warningDef.description, MessageTypeDefOf.CautionInput);
                    if (warningDef.sendLetter)
                        Find.LetterStack.ReceiveLetter(warningDef.label, warningDef.description, LetterDefOf.NegativeEvent);
                }
            }


            return true;
        }
    }
}
