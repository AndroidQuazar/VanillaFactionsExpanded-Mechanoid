using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using VFEMech;

namespace VFEM
{
    public class MechShipsMod : Mod
    {
        public static MechShipsSettings settings;
        public MechShipsMod(ModContentPack pack) : base(pack)
        {
            settings = GetSettings<MechShipsSettings>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
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
            Setup();
        }

        public static void Setup()
        {
            MechShipsMod.settings.incidents = DefDatabase<IncidentDef>.AllDefsListForReading.Where(x => x.defName.StartsWith("VFEM_ShipLand")).ToList();

            if (MechShipsMod.settings.mechShipIncidentChances is null)
            {
                MechShipsMod.settings.mechShipIncidentChances = new Dictionary<string, float>();
            }
            MechShipsMod.settings.mechShipIncidentChances.RemoveAll(x => DefDatabase<IncidentDef>.GetNamed(x.Key, false) is null);

            if (MechShipsMod.settings.mechShipPresences is null)
            {
                MechShipsMod.settings.mechShipPresences = new Dictionary<string, int>();
            }

            if (MechShipsMod.settings.mechShipColonistCount is null)
            {
                MechShipsMod.settings.mechShipColonistCount = new Dictionary<string, int>();
            }

            if (MechShipsMod.settings.mechShipDistances is null)
            {
                MechShipsMod.settings.mechShipDistances = new Dictionary<string, int>();
            }

            foreach (var mechShip in MechShipsMod.settings.incidents)
            {
                if (!MechShipsMod.settings.mechShipIncidentChances.ContainsKey(mechShip.defName))
                {
                    MechShipsMod.settings.mechShipIncidentChances[mechShip.defName] = mechShip.baseChance;
                }
                mechShip.baseChance = 0;

                MechanoidBaseIncidentExtension incidentExtension = mechShip.GetModExtension<MechanoidBaseIncidentExtension>();
                if (!MechShipsMod.settings.mechShipPresences.ContainsKey(incidentExtension.baseToPlace.defName))
                {
                    int presence = incidentExtension.baseToPlace.GetModExtension<MechanoidBaseExtension>().raisesPresence;
                    MechShipsMod.settings.mechShipPresences[incidentExtension.baseToPlace.defName] = presence;
                }

                if (!MechShipsMod.settings.mechShipColonistCount.ContainsKey(mechShip.defName))
                {
                    MechShipsMod.settings.mechShipColonistCount[mechShip.defName] = incidentExtension.minimumColonistCount;
                }

                if (!MechShipsMod.settings.mechShipDistances.ContainsKey(mechShip.defName))
                {
                    MechShipsMod.settings.mechShipDistances[mechShip.defName] = incidentExtension.maxDistance;
                }
            }
        }

        public static void DoDefsAlter()
        {
            foreach (IncidentDef incidentDef in MechShipsMod.settings.incidents)
            {
                MechanoidBaseIncidentExtension incidentExtension = incidentDef.GetModExtension<MechanoidBaseIncidentExtension>();

                incidentExtension.minimumColonistCount = MechShipsMod.settings.mechShipColonistCount[incidentDef.defName];
                incidentExtension.maxDistance = MechShipsMod.settings.mechShipDistances[incidentDef.defName];


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