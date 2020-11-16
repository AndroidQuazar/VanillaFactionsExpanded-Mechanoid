using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFEMech
{
	public class DeathActionWorker_BigBombExplosion : DeathActionWorker
	{
		public override RulePackDef DeathRules => RulePackDefOf.Transition_DiedExplosive;

		public override bool DangerousInMelee => true;

		public override void PawnDied(Corpse corpse)
		{
			GenExplosion.DoExplosion(radius: (corpse.InnerPawn.ageTracker.CurLifeStageIndex == 0) ? 1.9f : ((corpse.InnerPawn.ageTracker.CurLifeStageIndex != 1) ? 4.9f : 2.9f), 
				center: corpse.Position, map: corpse.Map, damType: DamageDefOf.Bomb, instigator: corpse.InnerPawn);
		}
	}
}
