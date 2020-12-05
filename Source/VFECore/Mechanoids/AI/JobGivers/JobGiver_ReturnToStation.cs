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
	public class JobGiver_ReturnToStation : ThinkNode_JobGiver
	{

		public override float GetPriority(Pawn pawn)
		{
			return 8f;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			if(pawn.TryGetComp<CompMachine>().myBuilding.TryGetComp<CompMachineChargingStation>().wantsRest && pawn.TryGetComp<CompMachine>().myBuilding.TryGetComp<CompPowerTrader>().PowerOn)
				return JobMaker.MakeJob(JobGiver_Recharge.Recharge, pawn.TryGetComp<CompMachine>().myBuilding);
			return null;
		}
	}
}
