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
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref mechShipStates, "mechShipStates", LookMode.Value, LookMode.Value, ref mechShipKeys, ref boolValues);
        }

        private List<string> mechShipKeys;
        private List<float> boolValues;

        public void DoSettingsWindowContents(Rect inRect)
        {
            var keys = mechShipStates.Keys.ToList().OrderByDescending(x => x).ToList();
            Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            Rect rect2 = new Rect(0f, 0f, inRect.width - 30f, keys.Count * 64);
            Widgets.BeginScrollView(rect, ref scrollPosition, rect2, true);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect2);
            for (int num = keys.Count - 1; num >= 0; num--)
            {
                var def = DefDatabase<IncidentDef>.GetNamedSilentFail(keys[num]);
                listingStandard.Label("Adjust base chance for incident " + def.label + ": " + mechShipStates[keys[num]]);
                mechShipStates[keys[num]] = listingStandard.Slider(mechShipStates[keys[num]], 0f, 5f);
            }
            listingStandard.End();
            Widgets.EndScrollView();
            base.Write();
        }
        private static Vector2 scrollPosition = Vector2.zero;

    }
}

