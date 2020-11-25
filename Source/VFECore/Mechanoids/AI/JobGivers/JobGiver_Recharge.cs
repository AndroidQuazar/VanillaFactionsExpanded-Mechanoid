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
	public class JobGiver_Recharge : ThinkNode_JobGiver
	{
		private RestCategory minCategory=RestCategory.VeryTired;

		private float maxLevelPercentage = 0.99f;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_Recharge obj = (JobGiver_Recharge)base.DeepCopy(resolve);
			obj.minCategory = minCategory;
			obj.maxLevelPercentage = maxLevelPercentage;
			return obj;
		}

		public override float GetPriority(Pawn pawn)
		{
			Need_Rest rest = pawn.needs.rest;
			if (rest == null)
			{
				return 0f;
			}
			if ((int)rest.CurCategory < (int)minCategory)
			{
				return 0f;
			}
			if (rest.CurLevelPercentage > maxLevelPercentage)
			{
				return 0f;
			}
			return 8f;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			Need_Rest rest = pawn.needs.rest;
			if (rest == null || rest.CurLevelPercentage > maxLevelPercentage)
				return null;
			if (pawn.CurJobDef != RimWorld.JobDefOf.LayDown && rest.CurCategory <= minCategory)
				return null;
			return JobMaker.MakeJob(RimWorld.JobDefOf.LayDown, pawn.TryGetComp<CompMachine>().myBuilding);
		}
	}
}
