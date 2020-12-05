using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using VFE.Mechanoids.Buildings;
using VFE.Mechanoids.Needs;

namespace VFE.Mechanoids.AI.JobDrivers
{
	public class JobDriver_Recharge : JobDriver
	{
		public const TargetIndex BedOrRestSpotIndex = TargetIndex.A;

		public Building_BedMachine Bed => (Building_BedMachine)job.GetTarget(TargetIndex.A).Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (job.GetTarget(TargetIndex.A).HasThing && !pawn.Reserve(Bed, job, 1, 0, null, errorOnFailed))
			{
				return false;
			}
			return true;
		}

		public override bool CanBeginNowWhileLyingDown()
		{
			return InBedOrRestSpotNow(pawn, job.GetTarget(TargetIndex.A));
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoCell(TargetA.Cell, PathEndMode.OnCell);
			yield return LayDown(TargetIndex.A);
		}

		public static bool InBedOrRestSpotNow(Pawn pawn, LocalTargetInfo bedOrRestSpot)
		{
			if (!bedOrRestSpot.IsValid || !pawn.Spawned)
			{
				return false;
			}
			if (bedOrRestSpot.HasThing)
			{
				if (bedOrRestSpot.Thing.Map != pawn.Map)
				{
					return false;
				}
				return bedOrRestSpot.Thing.Position == pawn.Position;
			}
			return false;
		}

		public static Toil LayDown(TargetIndex bedOrRestSpotIndex) //Largely C&P'ed from vanilla LayDown toil
		{
			Toil layDown = new Toil();
			layDown.initAction = delegate
			{
				Pawn actor3 = layDown.actor;
				actor3.pather.StopDead();
				actor3.TryGetComp<CompMachine>().myBuilding.TryGetComp<CompMachineChargingStation>().CompTickRare();
			};
			layDown.tickAction = delegate
			{
				Pawn actor2 = layDown.actor;
				Job curJob = actor2.CurJob;
				JobDriver curDriver2 = actor2.jobs.curDriver;
				Building_BedMachine building_Bed = (Building_BedMachine)curJob.GetTarget(bedOrRestSpotIndex).Thing;

				if (building_Bed.TryGetComp<CompPowerTrader>().PowerOn)
				{
					actor2.needs.TryGetNeed<Need_Power>().TickResting(2f); //TODO change to individual values from document

					if (actor2.IsHashIntervalTick(100) && !actor2.Position.Fogged(actor2.Map))
					{
						MoteMaker.ThrowMetaIcon(actor2.Position, actor2.Map, ThingDefOf.Mote_SleepZ); //TODO change to lightning bolt
						if (actor2.health.hediffSet.GetNaturallyHealingInjuredParts().Any())
						{
							MoteMaker.ThrowMetaIcon(actor2.Position, actor2.Map, ThingDefOf.Mote_HealingCross); //TODO change to wrench
						}
					}
				}
				if (actor2.IsHashIntervalTick(211))
				{
					actor2.jobs.CheckForJobOverride();
				}
			};
			layDown.AddFinishAction(delegate
			{
				Pawn actor = layDown.actor;
				if (layDown.actor.needs.TryGetNeed<Need_Power>().CurLevelPercentage > 0.99f && !layDown.actor.health.hediffSet.HasNaturallyHealingInjury())
				{
					actor.TryGetComp<CompMachine>().myBuilding.TryGetComp<CompMachineChargingStation>().wantsRest = false;
				}
			});
			layDown.FailOn(() => layDown.actor.needs.TryGetNeed<Need_Power>().CurLevelPercentage > 0.99f && !layDown.actor.health.hediffSet.HasNaturallyHealingInjury());
			layDown.defaultCompleteMode = ToilCompleteMode.Never;
			return layDown;
		}
	}
}
