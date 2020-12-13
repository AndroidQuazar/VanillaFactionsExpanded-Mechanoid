using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace VFEMech
{
	public class JobGiver_DisassembleNonMechBuildings : ThinkNode_JobGiver
	{
		public float radius;
		protected override Job TryGiveJob(Pawn pawn)
		{
			var allThings = pawn.Map.listerThings.AllThings.Where(x => x.Faction != pawn.Faction && x.def.building != null);
			Predicate<Thing> validator = (Thing t) => pawn.CanReserve(t);

			var traps = allThings.Where(x => x.def.building.isTrap);
			if (traps.Any())
            {
				var trap = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, traps, PathEndMode.Touch, TraverseParms.For(TraverseMode.PassAllDestroyableThings), radius, validator);
				if (trap != null)
                {
					return JobMaker.MakeJob(VFEMDefOf.VFEM_Disassemble, trap);
                }
            }

			var turrets = allThings.Where(x => x.def.building.IsTurret && x.TryGetComp<CompPowerTrader>() is CompPowerTrader comp && comp.PowerOn);
			if (turrets.Any())
			{
				var turret = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, turrets, PathEndMode.Touch, TraverseParms.For(TraverseMode.PassAllDestroyableThings), radius, validator);
				if (turret != null)
				{
					return JobMaker.MakeJob(VFEMDefOf.VFEM_Disassemble, turret);
				}
			}

			var powerGenerators = allThings.Where(x => x.def.GetCompProperties<CompProperties_Power>()?.basePowerConsumption <= -1);
			if (powerGenerators.Any())
			{
				var powerGenerator = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, powerGenerators, PathEndMode.Touch, TraverseParms.For(TraverseMode.PassAllDestroyableThings), radius, validator);
				if (powerGenerator != null)
				{
					return JobMaker.MakeJob(VFEMDefOf.VFEM_Disassemble, powerGenerator);
				}
			}

			var walls = allThings.Where(x => x.def.defName.ToLower().Contains("wall"));
			if (walls.Any())
			{
				var wall = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, walls, PathEndMode.Touch, TraverseParms.For(TraverseMode.PassAllDestroyableThings), radius, validator);
				if (wall != null)
				{
					return JobMaker.MakeJob(VFEMDefOf.VFEM_Disassemble, wall);
				}
			}
			return null;
		}
	}
}
