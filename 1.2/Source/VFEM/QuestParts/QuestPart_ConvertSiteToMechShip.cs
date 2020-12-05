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
	public class QuestPart_ConvertSiteToMechShip : QuestPart
	{
		public string inSignal;

		public bool sendStandardLetter = true;

		public Site site;
		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (signal.tag == inSignal)
			{
                WorldObjectDef objectDef = DefDatabase<WorldObjectDef>.AllDefs.Where(x => x.GetModExtension<MechanoidBaseExtension>() is MechanoidBaseExtension options 
                    && options.raisesPresence > 100).RandomElement();
                Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(objectDef);
                Faction faction = Find.FactionManager.FirstFactionOfDef(VFEMDefOf.VFE_Mechanoid);
                settlement.SetFaction(faction);
				settlement.Tile = site.Tile;
				if (site.HasMap)
                {
					var pawns = site.Map.mapPawns.PawnsInFaction(Faction.OfPlayer);
					List<int> neigbors = new List<int>();
					Find.WorldGrid.GetTileNeighbors(site.Tile, neigbors);
					var dest = neigbors.RandomElement();
					CaravanFormingUtility.FormAndCreateCaravan(pawns, Faction.OfPlayer, site.Tile, dest, dest);
				}
				site.Destroy();
				settlement.Name = SettlementNameGenerator.GenerateSettlementName(settlement, objectDef.GetModExtension<MechanoidBaseExtension>().nameMaker);
                Find.WorldObjects.Add(settlement);
            }
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_References.Look(ref site, "site");
			Scribe_Values.Look(ref sendStandardLetter, "sendStandardLetter", defaultValue: true);
		}
	}
}