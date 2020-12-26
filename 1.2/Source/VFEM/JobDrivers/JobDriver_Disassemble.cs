using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace VFEMech
{
	public class JobDriver_Disassemble : JobDriver
	{
		protected Thing Target => job.targetA.Thing;
		protected Building Building => (Building)Target.GetInnerIfMinified();

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(Target, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			Toil doWork = new Toil().FailOnDestroyedOrNull(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			doWork.tickAction = delegate
			{
				this.Target.HitPoints -= 1;
				if (this.Target.HitPoints <= 0f)
				{
					doWork.actor.jobs.curDriver.ReadyForNextToil();
				}
			};
			doWork.defaultCompleteMode = ToilCompleteMode.Never;
			doWork.WithProgressBar(TargetIndex.A, () => this.Target.HitPoints / this.Target.MaxHitPoints);
			yield return doWork;
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				FinishedRemoving();
				base.Map.designationManager.RemoveAllDesignationsOn(Target);
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return toil;
		}

		protected void FinishedRemoving()
		{
			this.Target.Destroy(DestroyMode.Refund);
		}
	}
}

