using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace VFE.Mechanoids.AI.WorkGivers
{
    class WorkGiver_RepairMachine : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);
        public override PathEndMode PathEndMode => PathEndMode.Touch;
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t.TryGetComp<CompMachineChargingStation>() != null && t.TryGetComp<CompMachineChargingStation>().wantsRespawn))
                return false;
            List<ThingDefCountClass> products = t.TryGetComp<CompMachineChargingStation>().Props.pawnToSpawn.race.butcherProducts.ListFullCopy();
            foreach (ThingDefCountClass thingNeeded in products)
            {
                List<Thing> thingsOfThisType = RefuelWorkGiverUtility.FindEnoughReservableThings(pawn, t.Position, new IntRange(thingNeeded.count, thingNeeded.count), (Thing thing) => thing.def == thingNeeded.thingDef);
                if (thingsOfThisType == null)
                {
                    return false;
                }
            }
            return true;
        }
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            ThingDef pawnDef = t.TryGetComp<CompMachineChargingStation>().Props.pawnToSpawn.race;
            List<ThingDefCountClass> products = pawnDef.butcherProducts.ListFullCopy();
            List<Thing> toGrab = new List<Thing>();
            List<int> toGrabCount = new List<int>();
            foreach (ThingDefCountClass thingNeeded in products)
            {
                List<Thing> thingsOfThisType=RefuelWorkGiverUtility.FindEnoughReservableThings(pawn, t.Position, new IntRange(thingNeeded.count, thingNeeded.count), (Thing thing) => thing.def == thingNeeded.thingDef);
                if(thingsOfThisType==null)
                {
                    return null;
                }
                toGrab.AddRange(thingsOfThisType);
                int totalCountNeeded = thingNeeded.count;
                foreach (Thing thingGrabbed in thingsOfThisType)
                {
                    if (thingGrabbed.stackCount >= totalCountNeeded)
                    {
                        toGrabCount.Add(totalCountNeeded);
                        totalCountNeeded = 0;
                    }
                    else
                    {
                        toGrabCount.Add(thingGrabbed.stackCount);
                        totalCountNeeded -= thingGrabbed.stackCount;
                    }
                }
            }
            Job job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("VFE_Mechanoids_RepairMachine"), t);
            job.targetQueueB = toGrab.Select((Thing f) => new LocalTargetInfo(f)).ToList();
            job.countQueue = toGrabCount.ToList();
            return job;
        }
    }
}
