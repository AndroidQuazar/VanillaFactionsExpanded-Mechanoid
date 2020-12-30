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
    class mechShipsSettings : ModSettings
    {
        public Dictionary<string, float> mechShipIncidentChances = new Dictionary<string, float>();
        public Dictionary<string, int> mechShipPresences = new Dictionary<string, int>();
        public Dictionary<string, int> mechShipColonistCount = new Dictionary<string, int>();
        public Dictionary<string, int> mechShipDistances = new Dictionary<string, int>();
        public const float VFEM_factorySpeedMultiplierBase = 1;
        public float VFEM_factorySpeedMultiplier = VFEM_factorySpeedMultiplierBase;



        public bool totalWarIsDisabled;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref totalWarIsDisabled, "totalWarIsDisabled");
            Scribe_Collections.Look(ref mechShipIncidentChances, "mechShipStates", LookMode.Value, LookMode.Value, ref mechShipKeys, ref floatValues);
            Scribe_Collections.Look(ref mechShipPresences, "mechShipPresences", LookMode.Value, LookMode.Value, ref mechShipKeys2, ref intValues2);
            Scribe_Collections.Look(ref mechShipColonistCount, "mechShipColonistCount", LookMode.Value, LookMode.Value, ref mechShipKeys3, ref intValues3);
            Scribe_Collections.Look(ref mechShipDistances, "mechShipDistances", LookMode.Value, LookMode.Value, ref mechShipKeys4, ref intValues4);
            Scribe_Values.Look(ref VFEM_factorySpeedMultiplier, "VFEM_factorySpeedMultiplier", VFEM_factorySpeedMultiplierBase, true);

        }

        private List<string> mechShipKeys;
        private List<float> floatValues;

        private List<string> mechShipKeys2;
        private List<int> intValues2;

        private List<string> mechShipKeys3;
        private List<int> intValues3;

        private List<string> mechShipKeys4;
        private List<int> intValues4;

        public void DoSettingsWindowContents(Rect inRect)
        {
            var keys = mechShipIncidentChances.Keys.ToList().OrderByDescending(x => x).ToList();
            var keys2 = mechShipPresences.Keys.ToList().OrderByDescending(x => x).ToList();
            var keys3 = mechShipColonistCount.Keys.ToList().OrderByDescending(x => x).ToList();
            var keys4 = mechShipDistances.Keys.ToList().OrderByDescending(x => x).ToList();

            Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            Rect rect2 = new Rect(0f, 0f, inRect.width - 30f, (keys.Count * 30) + (keys2.Count * 30) + (keys3.Count * 30) + (keys4.Count * 30)+100);
            Widgets.BeginScrollView(rect, ref scrollPosition, rect2, true);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect2);
            listingStandard.CheckboxLabeled("VFEMech.DisableTotalWarMechanic".Translate(), ref totalWarIsDisabled);
            listingStandard.Label("VFEMech.AdjustIncidentChanceLabel".Translate());
            for (int num = keys.Count - 1; num >= 0; num--)
            {
                var incidentDef = DefDatabase<IncidentDef>.GetNamedSilentFail(keys[num]);
                if (incidentDef != null)
                {
                    var incidentChance = mechShipIncidentChances[keys[num]];
                    listingStandard.SliderLabeled(incidentDef.label, ref incidentChance, incidentChance.ToStringDecimalIfSmall(), 0f, 5f);
                    mechShipIncidentChances[keys[num]] = incidentChance;
                }
            }
            listingStandard.GapLine();
            listingStandard.Label("VFEMech.AdjustMechPresenceLabel".Translate());
            for (int num = keys2.Count - 1; num >= 0; num--)
            {
                var worldObjectDef = DefDatabase<WorldObjectDef>.GetNamedSilentFail(keys2[num]);
                if (worldObjectDef != null)
                {
                    var mechPresence = mechShipPresences[keys2[num]];
                    listingStandard.SliderLabeled(worldObjectDef.label, ref mechPresence, mechPresence.ToString(), 0, 5000);
                    mechShipPresences[keys2[num]] = mechPresence;
                }
            }
            listingStandard.GapLine();
            listingStandard.Label("VFEMech.AdjustMinimunColonistCountLabel".Translate());
            for (int num = keys3.Count - 1; num >= 0; num--)
            {
                var incidentDef = DefDatabase<IncidentDef>.GetNamedSilentFail(keys3[num]);
                if (incidentDef != null)
                {
                    var minimumCount = mechShipColonistCount[keys3[num]];
                    listingStandard.SliderLabeled(incidentDef.label, ref minimumCount, minimumCount.ToString(), 0, 100);
                    mechShipColonistCount[keys3[num]] = minimumCount;
                }
            }
            listingStandard.GapLine();

            listingStandard.Label("VFEMech.AdjustMaximumDistanceForShipsLabel".Translate());
            for (int num = keys4.Count - 1; num >= 0; num--)
            {
                var incidentDef = DefDatabase<IncidentDef>.GetNamedSilentFail(keys4[num]);
                if (incidentDef != null)
                {
                    var maximumDistance = mechShipDistances[keys4[num]];
                    listingStandard.SliderLabeled(incidentDef.label, ref maximumDistance, maximumDistance.ToString(), 0, 1000);
                    mechShipDistances[keys4[num]] = maximumDistance;
                }
            }
            listingStandard.GapLine();
            listingStandard.Label("VFEM_factorySpeedMultiplier".Translate() + ": " + VFEM_factorySpeedMultiplier, -1, "VFEM_factorySpeedMultiplierTooltip".Translate());
            VFEM_factorySpeedMultiplier = listingStandard.Slider(VFEM_factorySpeedMultiplier, 0.1f, 5f);
            listingStandard.GapLine();

            listingStandard.End();
            Widgets.EndScrollView();
            base.Write();
        }
        private static Vector2 scrollPosition = Vector2.zero;

    }
}