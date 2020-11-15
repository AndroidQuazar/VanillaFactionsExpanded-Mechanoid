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
    public class TrooperStorage : Building
    {
        public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            base.PreApplyDamage(ref dinfo, out absorbed);
            ReleaseTroopers();
        }

        public void ReleaseTroopers()
        {
            var faction = Find.FactionManager.FirstFactionOfDef(VFEMDefOf.VFE_Mechanoid);
			PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms();
			pawnGroupMakerParms.tile = this.Map.Tile;
			pawnGroupMakerParms.faction = faction;
			pawnGroupMakerParms.points = 800f;
			pawnGroupMakerParms.groupKind = PawnGroupKindDefOf.Combat;
            var pawns = PawnGroupMakerUtility.GeneratePawns(pawnGroupMakerParms);
			LordMaker.MakeNewLord(faction, new LordJob_AssaultColony(faction), this.Map, pawns);
            foreach (var pawn in pawns)
            {
                GenSpawn.Spawn(pawn, this.Position, this.Map);
            }
        }
    }
}
