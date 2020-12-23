using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace VFEMech
{
    [StaticConstructorOnStartup]
    public static class MechUtils
    {
        public static Mesh plane20Flip = MeshMakerPlanes.NewPlaneMesh(2f, flipped: true);
        static MechUtils()
        {
            DefDatabase<ThingDef>.GetNamed("PsychicDroner", false)?.killedLeavings.Add(new ThingDefCountClass(VFEMDefOf.VFE_ComponentMechanoid, 4));
            DefDatabase<ThingDef>.GetNamed("PsychicSuppressor", false)?.killedLeavings.Add(new ThingDefCountClass(VFEMDefOf.VFE_ComponentMechanoid, 4));
            DefDatabase<ThingDef>.GetNamed("WeatherController", false)?.killedLeavings.Add(new ThingDefCountClass(VFEMDefOf.VFE_ComponentMechanoid, 4));

            DefDatabase<ThingDef>.GetNamed("SmokeSpewer", false)?.killedLeavings.Add(new ThingDefCountClass(VFEMDefOf.VFE_ComponentMechanoid, 3));
            DefDatabase<ThingDef>.GetNamed("ToxicSpewer", false)?.killedLeavings.Add(new ThingDefCountClass(VFEMDefOf.VFE_ComponentMechanoid, 3));
            DefDatabase<ThingDef>.GetNamed("SunBlocker", false)?.killedLeavings.Add(new ThingDefCountClass(VFEMDefOf.VFE_ComponentMechanoid, 3));
            DefDatabase<ThingDef>.GetNamed("EMIDynamo", false)?.killedLeavings.Add(new ThingDefCountClass(VFEMDefOf.VFE_ComponentMechanoid, 3));
            DefDatabase<ThingDef>.GetNamed("ClimateAdjuster", false)?.killedLeavings.Add(new ThingDefCountClass(VFEMDefOf.VFE_ComponentMechanoid, 3));

            DefDatabase<ThingDef>.GetNamed("Defoliator", false)?.killedLeavings.Add(new ThingDefCountClass(VFEMDefOf.VFE_ComponentMechanoid, 2));
            DefDatabase<ThingDef>.GetNamed("MechAssembler", false)?.killedLeavings.Add(new ThingDefCountClass(VFEMDefOf.VFE_ComponentMechanoid, 2));

            DefDatabase<ThingDef>.GetNamed("MechCapsule", false)?.killedLeavings.Add(new ThingDefCountClass(VFEMDefOf.VFE_ComponentMechanoid, 1));
        }

        public static int MechPresence() => 
            (int)Find.World.worldObjects.Settlements.Sum(s => s.def.GetModExtension<MechanoidBaseExtension>()?.raisesPresence ?? 0f);

        public static void CreateOrAddToAssaultLord(Pawn pawn, Lord lord = null, bool canKidnap = false, bool canTimeoutOrFlee = false, bool sappers = false,
                                                    bool useAvoidGridSmart = false, bool canSteal = false)
        {
            if (lord == null && pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction).Any((Pawn p) => p != pawn))
            {
                lord = ((Pawn)GenClosest.ClosestThing_Global(pawn.Position, pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction), 99999f, 
                    (Thing p) => p != pawn && ((Pawn)p).GetLord() != null, null)).GetLord();
            }
            if (lord == null)
            {
                var lordJob = new LordJob_AssaultColony(pawn.Faction, canKidnap, canTimeoutOrFlee, sappers, useAvoidGridSmart, canSteal);
                lord = LordMaker.MakeNewLord(pawn.Faction, lordJob, pawn.Map, null);
            }
            lord.AddPawn(pawn);
        }
    }
}
