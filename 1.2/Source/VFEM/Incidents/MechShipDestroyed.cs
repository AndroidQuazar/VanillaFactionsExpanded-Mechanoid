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
    using VFEM;

    public class IncidentWorker_MechShipDestroyed : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (MechShipsMod.settings.totalWarIsDisabled)
            {
                return false;
            }
            var mechShips = Find.World.worldObjects.Settlements.Where(s => s.def.GetModExtension<MechanoidBaseExtension>() != null && !s.HasMap);
            if (!mechShips.Any())
            {
                return false;
            }
            var mechShipToDestroy = mechShips.RandomElement();
            var humanSettlements = Find.WorldObjects.SettlementBases.Where(x => x.Faction.def.humanlikeFaction);
            if (!humanSettlements.Any())
            {
                return false;
            }
            return base.CanFireNowSub(parms);
        }
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var mechShipToDestroy = Find.World.worldObjects.Settlements.Where(s => s.def.GetModExtension<MechanoidBaseExtension>() != null && !s.HasMap).RandomElement();
            var humanSettlements = Find.WorldObjects.SettlementBases.Where(x => x.Faction.def.humanlikeFaction);
            if (humanSettlements.Any())
            {
                var humanFaction = humanSettlements.OrderBy(x => Find.WorldGrid.ApproxDistanceInTiles(mechShipToDestroy.Tile, x.Tile)).First().Faction;
                this.SendStandardLetter("VFEMech.FactionDestroyedShip".Translate(humanFaction.Named("FACTION"), mechShipToDestroy.Label),
                    "VFEMech.FactionDestroyedShipDesc".Translate(humanFaction.Named("FACTION"), mechShipToDestroy.Label), def.letterDef, parms, mechShipToDestroy, humanFaction.Name);
                mechShipToDestroy.Destroy();
                return true;
            }
            return false;
        }
    }
}
