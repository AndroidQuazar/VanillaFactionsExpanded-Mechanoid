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
            var mechShips = DefDatabase<IncidentDef>.AllDefsListForReading.Where(x => x.defName.StartsWith("VFEM_"));
            foreach (var mechShip in mechShips)
            {
                if (settings.mechShipStates == null) settings.mechShipStates = new Dictionary<string, float>();
                if (!settings.mechShipStates.ContainsKey(mechShip.defName))
                {
                    settings.mechShipStates[mechShip.defName] = mechShip.baseChance;
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
            foreach (var mechShipState in MechShipsMod.settings.mechShipStates)
            {
                var defToAlter = DefDatabase<IncidentDef>.GetNamedSilentFail(mechShipState.Key);
                defToAlter.baseChance = mechShipState.Value;
            }
        }
    }
}