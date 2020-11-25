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
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            WorldObjectDef objectDef = this.def.GetModExtension<MechanoidBaseIncidentExtension>().baseToPlace;

            Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(objectDef);
            Faction faction = Find.FactionManager.FirstFactionOfDef(VFEMDefOf.VFE_Mechanoid);
            settlement.SetFaction(faction);
            settlement.Tile = TileFinder.RandomSettlementTileFor(faction);
            settlement.Name = SettlementNameGenerator.GenerateSettlementName(settlement, objectDef.GetModExtension<MechanoidBaseExtension>().nameMaker);
            Find.WorldObjects.Add(settlement);


            this.SendStandardLetter(def.letterLabel, this.def.letterText + "\n\n" + objectDef.description, def.letterDef, parms, settlement, faction.Name);
            return true;
        }
    }
}
