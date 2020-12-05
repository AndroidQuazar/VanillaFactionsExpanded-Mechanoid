using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFEMech
{
    public class MapComponent_MechWarfare : MapComponent
    {
        public MapComponent_MechWarfare(Map map) : base(map)
        {

        }

        private bool mechAttackQuestIsActive;
        private List<Pawn> mechanoidsFromAttackParty;
        private List<string> mechQuestTags;
        public override void FinalizeInit()
        {
            base.FinalizeInit();
            TerrainPatches.factoryFloors[map] = new HashSet<IntVec3>();
            foreach (var cell in map.AllCells)
            {
                if (cell.GetTerrain(map) == VFEMDefOf.VFE_FactoryPath)
                {
                    TerrainPatches.factoryFloors[map].Add(cell);
                }
            }
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if (Find.TickManager.TicksGame % 10 == 0 && TerrainPatches.factoryFloors.TryGetValue(map, out HashSet<IntVec3> factoryFloors))
            {
                foreach (var cell in factoryFloors)
                {
                    foreach (var t in map.thingGrid.ThingsListAtFast(cell))
                    {
                        if (t is Pawn pawn && pawn.health.hediffSet.GetFirstHediffOfDef(VFEMDefOf.VFE_FasterMovement) == null)
                        {
                            var hediff = HediffMaker.MakeHediff(VFEMDefOf.VFE_FasterMovement, pawn);
                            pawn.health.AddHediff(hediff);
                        }
                    }
                }
            }

            if (mechAttackQuestIsActive)
            {
                if (!OnePawnIsLive(mechanoidsFromAttackParty))
                {
                    QuestUtility.SendQuestTargetSignals(mechQuestTags, "AllMechsDefeated");
                    mechAttackQuestIsActive = false;
                    mechanoidsFromAttackParty = null;
                    mechQuestTags = null;
                }
            }
        }

        public void RegisterMechanoidAttackParty(List<Pawn> pawns, Site site)
        {
            mechAttackQuestIsActive = true;
            mechanoidsFromAttackParty = pawns;
            mechQuestTags = site.questTags;
        }

        public bool OnePawnIsLive(List<Pawn> pawns)
        {
            foreach (var pawn in pawns)
            {
                if (pawn.Spawned && !pawn.Dead)
                {
                    return true;
                }
            }
            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref mechAttackQuestIsActive, "mechAttackQuestIsActive");
            Scribe_Collections.Look(ref mechanoidsFromAttackParty, "mechanoidsFromAttackParty", LookMode.Reference);
            Scribe_Collections.Look(ref mechQuestTags, "mechQuestTags", LookMode.Value);
        }
    }
}
