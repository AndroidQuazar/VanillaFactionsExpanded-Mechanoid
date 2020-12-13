using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using VFE.Mechanoids;
using VFEMech;

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
			var compMachine = pawn.TryGetComp<CompMachine>();
			var compMachineChargingStation = compMachine.myBuilding.TryGetComp<CompMachineChargingStation>();
			if (compMachineChargingStation.wantsRest && compMachine.myBuilding.TryGetComp<CompPowerTrader>().PowerOn)
				return JobMaker.MakeJob(VFEMDefOf.VFE_Mechanoids_Recharge, compMachine.myBuilding);
			return null;
		}
	}
}
