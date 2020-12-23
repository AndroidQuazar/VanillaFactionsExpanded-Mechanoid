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
        public Dictionary<string, float> mechShipStates = new Dictionary<string, float>();
        public Dictionary<string, float> mechShipPresences = new Dictionary<string, float>();

        public bool totalWarIsDisabled;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref mechShipStates, "mechShipStates", LookMode.Value, LookMode.Value, ref mechShipKeys, ref floatValues);
            Scribe_Collections.Look(ref mechShipPresences, "mechShipPresences", LookMode.Value, LookMode.Value, ref mechShipKeys2, ref intValues2);
        }

        private List<string> mechShipKeys;
        private List<float> floatValues;

        private List<string> mechShipKeys2;
        private List<float> intValues2;

        public void DoSettingsWindowContents(Rect inRect)
        {
            var keys = mechShipStates.Keys.ToList().OrderByDescending(x => x).ToList();
            var keys2 = mechShipPresences.Keys.ToList().OrderByDescending(x => x).ToList();
            Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            Rect rect2 = new Rect(0f, 0f, inRect.width - 30f, (keys.Count * 64) + (keys2.Count * 64));
            Widgets.BeginScrollView(rect, ref scrollPosition, rect2, true);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect2);
            listingStandard.CheckboxLabeled("Disable Total War mechanic", ref totalWarIsDisabled);
            for (int num = keys.Count - 1; num >= 0; num--)
            {
                var incidentDef = DefDatabase<IncidentDef>.GetNamedSilentFail(keys[num]);
                listingStandard.Label("VFEMech.AdjustIncidentChanceLabel".Translate(incidentDef.label) + mechShipStates[keys[num]]);
                mechShipStates[keys[num]] = listingStandard.Slider(mechShipStates[keys[num]], 0f, 5f);
            }

            for (int num = keys2.Count - 1; num >= 0; num--)
            {
                var worldObjectDef = DefDatabase<WorldObjectDef>.GetNamedSilentFail(keys2[num]);
                listingStandard.Label("VFEMech.AdjustMechPresenceLabel".Translate(worldObjectDef.label) + mechShipPresences[keys2[num]]);
                mechShipPresences[keys2[num]] = listingStandard.Slider(mechShipPresences[keys2[num]], 0f, 5000f);
            }
            listingStandard.End();
            Widgets.EndScrollView();
            base.Write();
        }
        private static Vector2 scrollPosition = Vector2.zero;

    }
}

