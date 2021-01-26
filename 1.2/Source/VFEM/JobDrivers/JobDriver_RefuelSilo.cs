using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFEMech
{
    using Verse;
    using Verse.AI;

    public class JobDriver_RefuelSilo : JobDriver
    {
        protected MissileSilo Silo => (MissileSilo) this.job.GetTarget(TargetIndex.A).Thing;
        protected Thing Fuel => this.job.GetTarget(TargetIndex.B).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (this.pawn.Reserve(this.Silo, this.job, 1, -1, null, errorOnFailed))
            {
                return pawn.Reserve(Fuel, job, 1, -1, null, errorOnFailed);
            }
            return false;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.AddEndCondition(() => (!this.Silo.Satisfied) ? JobCondition.Ongoing : JobCondition.Succeeded);
            this.AddFailCondition(() => !this.job.playerForced && this.Silo.Satisfied);
            yield return Toils_General.DoAtomic(delegate
                                                {
                                                    this.job.count = this.Silo.CostMissing(this.Fuel.def);
                                                });
            
            Toil reserveFuel = Toils_Reserve.Reserve(TargetIndex.B);
            yield return reserveFuel;
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B, putRemainderInQueue: false, subtractNumTakenFromJobCount: true).FailOnDestroyedNullOrForbidden(TargetIndex.B);
            yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveFuel, TargetIndex.B, TargetIndex.None, takeFromValidStorage: true);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.Wait(240).FailOnDestroyedNullOrForbidden(TargetIndex.B).FailOnDestroyedNullOrForbidden(TargetIndex.A)
                                   .FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch)
                                   .WithProgressBarToilDelay(TargetIndex.A);
            yield return new Toil
                         {
                             initAction = () =>
                                          {
                                              this.Silo.AddCost(this.Fuel);
                                          },
                             defaultCompleteMode = ToilCompleteMode.Instant
                         };
        }
    }
}

