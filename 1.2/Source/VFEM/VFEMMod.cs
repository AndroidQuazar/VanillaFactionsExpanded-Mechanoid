using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using VFEMech;

namespace VFEM
{
    class MechShipsMod : Mod
    {
        public static mechShipsSettings settings;
        public MechShipsMod(ModContentPack pack) : base(pack)
        {
            settings = GetSettings<mechShipsSettings>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            var mechShips = DefDatabase<IncidentDef>.AllDefsListForReading.Where(x => x.defName.StartsWith("VFEM_ShipLand"));

            foreach (var mechShip in mechShips)
            {
                if (settings.mechShipStates is null)
                {
                    settings.mechShipStates = new Dictionary<string, float>();
                }
                if (settings.mechShipPresences is null)
                {
                    settings.mechShipPresences = new Dictionary<string, float>();
                }
                if (!settings.mechShipStates.ContainsKey(mechShip.defName))
                {
                    settings.mechShipStates[mechShip.defName] = mechShip.baseChance;
                }
                var def = DefDatabase<IncidentDef>.GetNamedSilentFail(mechShip.defName);
                MechanoidBaseIncidentExtension incidentExtension = def.GetModExtension<MechanoidBaseIncidentExtension>();
                if (!settings.mechShipPresences.ContainsKey(incidentExtension.baseToPlace.defName))
                {
                    var presence = incidentExtension.baseToPlace.GetModExtension<MechanoidBaseExtension>().raisesPresence;
                    settings.mechShipPresences[incidentExtension.baseToPlace.defName] = presence;
                }
            }

            settings.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Vanilla Factions Expanded - Mechanoids";
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            DefsAlterer.DoDefsAlter();
        }
    }
    [StaticConstructorOnStartup]
    public static class DefsAlterer
    {
        static DefsAlterer()
        {
            DoDefsAlter();
        }
        public static void DoDefsAlter()
        {
            if (MechShipsMod.settings.mechShipStates is null)
            {
                MechShipsMod.settings.mechShipStates = new Dictionary<string, float>();
            }
            foreach (var mechShipState in MechShipsMod.settings.mechShipStates)
            {
                var defToAlter = DefDatabase<IncidentDef>.GetNamedSilentFail(mechShipState.Key);
                if (defToAlter != null)
                {
                    defToAlter.baseChance = mechShipState.Value;
                }
            }
            if (MechShipsMod.settings.mechShipPresences is null)
            {
                MechShipsMod.settings.mechShipPresences = new Dictionary<string, float>();
            }
            foreach (var mechShipPresence in MechShipsMod.settings.mechShipPresences)
            {
                var defToAlter = DefDatabase<WorldObjectDef>.GetNamedSilentFail(mechShipPresence.Key);
                if (defToAlter != null)
                {
                    defToAlter.GetModExtension<MechanoidBaseExtension>().raisesPresence = mechShipPresence.Value;
                }
            }
        }
    }
}