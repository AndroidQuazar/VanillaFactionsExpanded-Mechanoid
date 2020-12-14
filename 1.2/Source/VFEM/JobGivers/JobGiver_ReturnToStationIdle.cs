using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using VFE.Mechanoids;

namespace VFE.Mechanoids.AI.JobGivers
{
	public class JobGiver_ReturnToStationIdle : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			var buildingPosition = pawn.TryGetComp<CompMachine>().myBuilding.Position;
			if (pawn.Position != buildingPosition)
			{
				return JobMaker.MakeJob(JobDefOf.Goto, buildingPosition);
			}
			else
            {
				pawn.Rotation = Rot4.South;
				return JobMaker.MakeJob(JobDefOf.Wait, 60);
			}

		}
	}
}
