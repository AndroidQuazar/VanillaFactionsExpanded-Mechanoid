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
                if (settings.mechShipIncidentChances is null)
                {
                    settings.mechShipIncidentChances = new Dictionary<string, float>();
                }
                if (settings.mechShipPresences is null)
                {
                    settings.mechShipPresences = new Dictionary<string, int>();
                }
                if (!settings.mechShipIncidentChances.ContainsKey(mechShip.defName))
                {
                    settings.mechShipIncidentChances[mechShip.defName] = mechShip.baseChance;
                }
                var def = DefDatabase<IncidentDef>.GetNamedSilentFail(mechShip.defName);
                MechanoidBaseIncidentExtension incidentExtension = def.GetModExtension<MechanoidBaseIncidentExtension>();
                if (!settings.mechShipPresences.ContainsKey(incidentExtension.baseToPlace.defName))
                {
                    var presence = incidentExtension.baseToPlace.GetModExtension<MechanoidBaseExtension>().raisesPresence;
                    settings.mechShipPresences[incidentExtension.baseToPlace.defName] = presence;
                }

                if (!settings.mechShipColonistCount.ContainsKey(def.defName))
                {
                    settings.mechShipColonistCount[def.defName] = incidentExtension.minimumColonistCount;
                }
                if (!settings.mechShipDistances.ContainsKey(def.defName))
                {
                    settings.mechShipDistances[def.defName] = incidentExtension.maxDistance;
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
            if (MechShipsMod.settings.mechShipIncidentChances is null)
            {
                MechShipsMod.settings.mechShipIncidentChances = new Dictionary<string, float>();
            }
            foreach (var mechShipState in MechShipsMod.settings.mechShipIncidentChances)
            {
                var defToAlter = DefDatabase<IncidentDef>.GetNamedSilentFail(mechShipState.Key);
                if (defToAlter != null)
                {
                    defToAlter.baseChance = mechShipState.Value;
                }
            }
            if (MechShipsMod.settings.mechShipPresences is null)
            {
                MechShipsMod.settings.mechShipPresences = new Dictionary<string, int>();
            }
            foreach (var mechShipPresence in MechShipsMod.settings.mechShipPresences)
            {
                var defToAlter = DefDatabase<WorldObjectDef>.GetNamedSilentFail(mechShipPresence.Key);
                if (defToAlter != null)
                {
                    defToAlter.GetModExtension<MechanoidBaseExtension>().raisesPresence = mechShipPresence.Value;
                }
            }
            if (MechShipsMod.settings.mechShipColonistCount is null)
            {
                MechShipsMod.settings.mechShipColonistCount = new Dictionary<string, int>();
            }
            foreach (var mechShipColonistCount in MechShipsMod.settings.mechShipColonistCount)
            {
                var defToAlter = DefDatabase<IncidentDef>.GetNamedSilentFail(mechShipColonistCount.Key);
                if (defToAlter != null)
                {
                    MechanoidBaseIncidentExtension incidentExtension = defToAlter.GetModExtension<MechanoidBaseIncidentExtension>();
                    incidentExtension.minimumColonistCount = mechShipColonistCount.Value;
                }
            }
            if (MechShipsMod.settings.mechShipDistances is null)
            {
                MechShipsMod.settings.mechShipDistances = new Dictionary<string, int>();
            }
            foreach (var mechShipMaxDistance in MechShipsMod.settings.mechShipDistances)
            {
                var defToAlter = DefDatabase<IncidentDef>.GetNamedSilentFail(mechShipMaxDistance.Key);
                if (defToAlter != null)
                {
                    MechanoidBaseIncidentExtension incidentExtension = defToAlter.GetModExtension<MechanoidBaseIncidentExtension>();
                    incidentExtension.maxDistance = mechShipMaxDistance.Value;
                }
            }
        }
    }
}