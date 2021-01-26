using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFEMech
{
    using RimWorld;
    using Verse;
    using Verse.AI;

    public class WorkGiver_RefuelSilo : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(VFEMDefOf.VFE_LongRangeMissileLauncher);

        public override PathEndMode PathEndMode => PathEndMode.Touch;
        
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false) =>
            t is MissileSilo silo && !silo.Satisfied && pawn.CanReserve(silo) && silo.ThingsNeeded.Any(td => pawn.Map.listerThings.ThingsOfDef(td).Any(f => !f.IsForbidden(pawn) && pawn.CanReserve(f)));

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Job job = JobMaker.MakeJob(VFEMDefOf.VFEM_RefuelSilo, t);

            if (!(t is MissileSilo silo))
                return JobMaker.MakeJob(JobDefOf.Vomit);
            List<ThingDef> defs = silo.ThingsNeeded;
            IEnumerable<LocalTargetInfo> localTargetInfos = FindEnoughReservableThings(pawn, pawn.Position, th => defs.Contains(th.def)).
                Select(th => new LocalTargetInfo(th));

            if (!localTargetInfos.Any())
                return JobMaker.MakeJob(JobDefOf.Vomit);



            job.targetB = localTargetInfos.RandomElement();

            return job;
        }


        public static List<Thing> FindEnoughReservableThings(Pawn pawn, IntVec3 rootCell, Predicate<Thing> validThing)
        {
            Predicate<Thing> validator = delegate (Thing x)
                                         {
                                             if (x.IsForbidden(pawn) || !pawn.CanReserve(x)) 
                                                 return false;
                                             return validThing(x);
                                         };
            Region               region2             = pawn.GetRegion();
            TraverseParms        traverseParams      = TraverseParms.For(pawn);
            RegionEntryPredicate entryCondition      = (Region from, Region r) => r.Allows(traverseParams, isDestination: false);
            List<Thing>          chosenThings        = new List<Thing>();
            int                  accumulatedQuantity = 0;

            List<Thing> thingList = rootCell.GetThingList(region2.Map);
            ThingListProcessor(thingList, region2);
            RegionTraverser.BreadthFirstTraverse(region2, entryCondition, RegionProcessor, 99999);
            return chosenThings;

            bool RegionProcessor(Region r)
            {
                List<Thing> things2 = r.ListerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.HaulableEver));
                return ThingListProcessor(things2, r);
            }
            bool ThingListProcessor(List<Thing> things, Region region)
            {
                for (int i = 0; i < things.Count; i++)
                {
                    Thing thing = things[i];
                    if (validator(thing) && !chosenThings.Contains(thing) && ReachabilityWithinRegion.ThingFromRegionListerReachable(thing, region, PathEndMode.ClosestTouch, pawn))
                    {
                        chosenThings.Add(thing);
                        accumulatedQuantity += thing.stackCount;
                    }
                }
                return false;
            }
        }
    }
}
