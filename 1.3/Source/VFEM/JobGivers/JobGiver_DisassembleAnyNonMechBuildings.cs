using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace VFEMech
{
	public class JobGiver_DisassembleAnyNonMechBuildings : ThinkNode_JobGiver
	{
		public float radius;

		public static bool HasDutyAndShouldStayInGroup(Pawn pawn)
        {
			if (pawn.mindState.duty != null && pawn.mindState.duty.focus.IsValid)
            {
				if (pawn.GetLord()?.LordJob is LordJob_SleepThenMechanoidsDefend)
                {
					var firstJobInGroup = pawn.GetLord().ownedPawns?.FirstOrDefault(x => x != null && x.Spawned && !x.Dead && x.def != pawn.def)?.CurJobDef;
					if (firstJobInGroup != null && (firstJobInGroup == JobDefOf.Wait_Wander || firstJobInGroup == JobDefOf.GotoWander))
                    {
						return true;
                    }
                }
            }
			return false;
        }
		protected override Job TryGiveJob(Pawn pawn)
		{
			var lord = pawn.GetLord();
			var compDormant = pawn.GetComp<CompCanBeDormant>();
			if (compDormant is null || compDormant.Awake)
            {
				if (HasDutyAndShouldStayInGroup(pawn))
                {
					var cell = CellFinder.RandomClosewalkCellNear(pawn.mindState.duty.focus.Cell, pawn.Map, 10);
					if (cell.IsValid)
					{
						return JobMaker.MakeJob(JobDefOf.Goto, cell);
					}
				}
				var allThings = pawn.Map.listerThings.AllThings.Where(x => x.Faction != null && x.Faction != pawn.Faction && x.def.building != null);
				Predicate<Thing> validator = (Thing t) => pawn.CanReserve(t);
				if (allThings.Any())
				{
					var thing = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, allThings, PathEndMode.Touch, TraverseParms.For(TraverseMode.PassAllDestroyableThings), radius, validator);
					if (thing != null)
					{
						return JobMaker.MakeJob(VFEMDefOf.VFEM_Disassemble, thing);
					}
				}
			}
			else
            {
				Job job = JobMaker.MakeJob(JobDefOf.LayDown, pawn.Position);
				job.forceSleep = true;
				return job;
			}
			return null;
		}
	}
}

