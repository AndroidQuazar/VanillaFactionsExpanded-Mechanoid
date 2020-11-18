using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;

namespace VFEMech
{
    public static class MechUtils
    {
        public static int MechPresence() => 
            Find.World.worldObjects.Settlements.Sum(s => s.def.GetModExtension<MechanoidBaseExtension>()?.raisesPresence ?? 0);

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
