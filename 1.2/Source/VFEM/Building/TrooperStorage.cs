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
    public class TrooperStorage : MechShipPart
    {
        private bool troopersAreReleased;
        public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            base.PreApplyDamage(ref dinfo, out absorbed);
            if (CanSpawnTroopers)
            {
                ReleaseTroopers();
            }
        }
        public bool CanSpawnTroopers => !troopersAreReleased && this.Map != null;
        public void ReleaseTroopers()
        {
            var faction = Find.FactionManager.FirstFactionOfDef(VFEMDefOf.VFE_Mechanoid);
			PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms();
			pawnGroupMakerParms.tile = this.Map.Tile;
			pawnGroupMakerParms.faction = faction;
			pawnGroupMakerParms.points = 800f;
			pawnGroupMakerParms.groupKind = PawnGroupKindDefOf.Combat;
            var pawns = PawnGroupMakerUtility.GeneratePawns(pawnGroupMakerParms);
            foreach (var pawn in pawns)
            {
                GenPlace.TryPlaceThing(pawn, this.Position, this.Map, ThingPlaceMode.Near);
                MechUtils.CreateOrAddToAssaultLord(pawn);
            }
            troopersAreReleased = true;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref troopersAreReleased, "troopersAreReleased");
        }
    }
}
