using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFEMech
{
    using System.Diagnostics;
    using System.Reflection;
    using HarmonyLib;
    using RimWorld;
    using RimWorld.Planet;
    using UnityEngine;
    using Verse;
    using Verse.AI.Group;
    using Verse.Sound;

    public class MissileSilo : Building
    {
        public GlobalTargetInfo target = GlobalTargetInfo.Invalid;

        public List<ThingDefCountClass> costList = new List<ThingDefCountClass>();
        public List<int> delivered = new List<int>();

        public static List<ThingDefCountClass> startCostList = new List<ThingDefCountClass>()
                                                               {
                                                                   new ThingDefCountClass(ThingDefOf.Plasteel, 45),
                                                                   new ThingDefCountClass(ThingDefOf.ComponentSpacer, 3),
                                                                   new ThingDefCountClass(ThingDefOf.Uranium, 45)
                                                               };

        public bool TargetAcquired =>
            this.target != GlobalTargetInfo.Invalid;

        public bool Satisfied
        {
            get
            {
                if (!this.TargetAcquired)
                    return false;
                for (int index = 0; index < this.costList.Count; index++)
                {
                    ThingDefCountClass defCount = this.costList[index];
                    if (this.delivered[index] < defCount.count)
                        return false;
                }

                return true;
            }
        }

        public List<ThingDef> ThingsNeeded =>
            this.costList.Select(tdcc => tdcc.thingDef).Where(td => this.CostMissing(td) > 0).ToList();

        public int CostMissing(ThingDef def)
        {
            for (int index = 0; index < this.costList.Count; index++)
            {
                ThingDefCountClass thingDefCountClass = this.costList[index];
                if (thingDefCountClass.thingDef == def)
                    return Mathf.Max(0, thingDefCountClass.count - this.delivered[index]);
            }
            return 0;
        }

        public void AddCost(Thing thing)
        {
            if (this.CostMissing(thing.def) <= 0) return;

            this.AddCost(thing.def, thing.stackCount);
            thing.Destroy();
        }

        public void AddCost(ThingDef def, int count)
        {
            for (int index = 0; index < this.costList.Count; index++)
            {
                ThingDefCountClass thingDefCountClass = this.costList[index];
                if (thingDefCountClass.thingDef == def)
                    this.delivered[index] += count;
            }
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder(base.GetInspectString());

            if (!this.TargetAcquired)
            {
                sb.AppendLine("VFEM_SiloNoTarget".Translate());
            }
            else
            {
                WorldObject target = this.target.WorldObject;

                sb.AppendLine("VFEM_SiloTargeting".Translate(target.LabelCap, target.Faction.Name));
                sb.AppendLine((this.Satisfied ? "VFEM_SiloConditionsSatisfied" : "VFEM_SiloConditionsUnsatisfied").Translate());
                for (int i = 0; i < this.costList.Count; i++)
                {
                    ThingDefCountClass countClass = this.costList[i];
                    sb.AppendLine("VFEM_SiloCountEntry".Translate(countClass.thingDef.LabelCap, this.delivered[i], countClass.count));
                }
            }

            return sb.ToString().TrimEndNewlines();
        }

        public bool ConfigureNewTarget(GlobalTargetInfo targetInfo)
        {
            
            if (!targetInfo.IsValid || !targetInfo.HasWorldObject)
                return false;

            if (Find.QuestManager.QuestsListForReading.Any(q => !q.Historical && !q.dismissed && q.QuestLookTargets.Contains(targetInfo.WorldObject)))
            {
                Messages.Message("VFEM_SiloAimAtQuestSite".Translate(), MessageTypeDefOf.NeutralEvent);
                return false;
            }

            int distance = Find.WorldGrid.TraversalDistanceBetween(this.Map.Tile, targetInfo.Tile);
            
            if (distance > 132)
                return false;
            
            this.target = targetInfo;
            this.costList = startCostList.ListFullCopy();
            
            this.costList.Add(new ThingDefCountClass(ThingDefOf.Chemfuel, Mathf.CeilToInt(distance * 2.25f)));
            
            if(this.delivered.Count == 0)
                for (int i = 0; i < this.costList.Count; i++) 
                    this.delivered.Add(0);

            return true;
        }

        public void Fire()
        {
            if (!this.Satisfied)
                return;

            for (int i = 0; i < this.costList.Count; i++)
            {
                ThingDefCountClass entry = this.costList[i];
                this.delivered[i] -= entry.count;
            }

            foreach (ThingDefCountClass tdcc in this.costList) 
                tdcc.count = 0;
            
            MissileLeaving obj = (MissileLeaving)SkyfallerMaker.MakeSkyfaller(VFEMDefOf.VFEM_MissileLeaving);
            obj.destinationTile = this.target.Tile;
            GenSpawn.Spawn(obj, this.TrueCenter().ToIntVec3(), this.Map);
            this.target = GlobalTargetInfo.Invalid;

            SoundDefOf.ShipTakeoff.PlayOneShot(SoundInfo.OnCamera());
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos()) 
                yield return gizmo;


            Command_Action aimingGizmo = new Command_Action()
                                    {
                                        action = () =>
                                                 {
                                                     CameraJumper.TryJump(CameraJumper.GetWorldTarget(this));
                                                     Find.WorldSelector.ClearSelection();
                                                     Find.WorldTargeter.BeginTargeting_NewTemp(this.ConfigureNewTarget, false, onUpdate: () => GenDraw.DrawWorldRadiusRing(this.Map.Tile, 132), closeWorldTabWhenFinished: true);
                                                 },
                                        defaultLabel = "Aim"
                                    };
            yield return aimingGizmo;

            Command_Action fireGizmo = new Command_Action()
                                       {
                                           action = this.Fire,
                                           disabled = !this.Satisfied,
                                           disabledReason = (this.TargetAcquired ? "VFEM_SiloConditionsUnsatisfied" : "VFEM_SiloNoTarget").Translate(),
                                           defaultLabel = "Fire"
                                       };
            yield return fireGizmo;

            if (Prefs.DevMode)
            {
                yield return new Command_Action()
                             {
                                 action = () =>
                                          {
                                              for (int index = 0; index < this.costList.Count; index++)
                                              {
                                                  ThingDefCountClass countClass = this.costList[index];
                                                  this.delivered[index] = countClass.count;
                                              }
                                          },
                                 defaultLabel = "DEV: Fill Conditions",
                                 disabled = !this.target.IsValid
                             };
                yield return new Command_Action()
                             {
                                 action = () =>
                                          {
                                              for (int index = 0; index < this.costList.Count; index++)
                                              {
                                                  ThingDefCountClass countClass = this.costList[index];
                                                  this.delivered[index] = 100;
                                              }
                                          },
                                 defaultLabel = "DEV: Fill Conditions: 100"
                             };
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref this.costList, nameof(this.costList));
            Scribe_Collections.Look(ref this.delivered, nameof(this.delivered));
            Scribe_TargetInfo.Look(ref this.target, nameof(this.target));
        }
    }

    public class MissileLeaving : Skyfaller, IActiveDropPod
    {
        public int destinationTile = -1;

        public bool createWorldObject = true;

        private bool alreadyLeft;

        public ActiveDropPodInfo Contents
        {
            get
            {
                return ((ActiveDropPod) this.innerContainer[0]).Contents;
            }
            set
            {
                ((ActiveDropPod) this.innerContainer[0]).Contents = value;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.angle = 0f;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.destinationTile, "destinationTile", 0);
            Scribe_Values.Look(ref this.alreadyLeft, "alreadyLeft", defaultValue: false);
        }

        public override void Tick()
        {
            base.Tick();

            Vector3 drawPos = this.GetDrawPosForMotes();

            if (this.Map == null || !drawPos.InBounds(this.Map))
                return;



            MoteMaker.ThrowSmoke(drawPos, this.Map, 2f);

            MoteThrown heatGlow = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_HeatGlow);
            heatGlow.Scale         = Rand.Range(4f, 6f) * 2f;
            heatGlow.rotationRate  = Rand.Range(-3f, 3f);
            heatGlow.exactPosition = drawPos;
            heatGlow.SetVelocity(Rand.Range(0, 360), 0.12f);
            GenSpawn.Spawn(heatGlow, drawPos.ToIntVec3(), this.Map);

        }

        private Vector3 GetDrawPosForMotes()
        {
            int ticksToImpactPrediction = this.ticksToImpact + GenTicks.TicksPerRealSecond/2;


            float timeInAnim = (float) ticksToImpactPrediction / 220f;


            float currentSpeed = this.def.skyfaller.speedCurve.Evaluate(timeInAnim) * this.def.skyfaller.speed;

            switch (this.def.skyfaller.movementType)
            {
                case SkyfallerMovementType.Accelerate:
                    return SkyfallerDrawPosUtility.DrawPos_Accelerate(base.DrawPos, ticksToImpactPrediction, this.angle, currentSpeed);
                case SkyfallerMovementType.ConstantSpeed:
                    return SkyfallerDrawPosUtility.DrawPos_ConstantSpeed(base.DrawPos, ticksToImpactPrediction, this.angle, currentSpeed);
                case SkyfallerMovementType.Decelerate:
                    return SkyfallerDrawPosUtility.DrawPos_Decelerate(base.DrawPos, ticksToImpactPrediction, this.angle, currentSpeed);
                default:
                    Log.ErrorOnce("SkyfallerMovementType not handled: " + this.def.skyfaller.movementType, this.thingIDNumber ^ 0x7424EBC7);
                    return SkyfallerDrawPosUtility.DrawPos_Accelerate(base.DrawPos, ticksToImpactPrediction, this.angle, currentSpeed);
            }
        }

        protected override void LeaveMap()
        {
            if (this.alreadyLeft || !this.createWorldObject)
            {
                base.LeaveMap();
                return;
            }
            if (this.destinationTile < 0)
            {
                Log.Error("Missile left the map, but its destination tile is " + this.destinationTile);
                this.Destroy();
                return;
            }
            TravelingMissile travelingMissile = (TravelingMissile)WorldObjectMaker.MakeWorldObject(VFEMDefOf.VFEM_TravelingMissile);
            travelingMissile.Tile = this.Map.Tile;
            travelingMissile.SetFaction(Faction.OfPlayer);
            travelingMissile.destinationTile = this.destinationTile;
			Find.WorldObjects.Add(travelingMissile);
            base.LeaveMap();
        }
    }

	public class TravelingMissile : WorldObject, IThingHolder
	{
		public int destinationTile = -1;

        private bool arrived;

		private int initialTile = -1;

		private float traveledPct;

		private const float TravelSpeed = 0.00025f;

		private Vector3 Start => Find.WorldGrid.GetTileCenter(this.initialTile);

		private Vector3 End => Find.WorldGrid.GetTileCenter(this.destinationTile);

		public override Vector3 DrawPos => Vector3.Slerp(this.Start, this.End, this.traveledPct);

		public override bool ExpandingIconFlipHorizontal => GenWorldUI.WorldToUIPosition(this.Start).x > GenWorldUI.WorldToUIPosition(this.End).x;

		public override float ExpandingIconRotation
		{
			get
			{
				if (!this.def.rotateGraphicWhenTraveling)
				{
					return base.ExpandingIconRotation;
				}
				Vector2 vector = GenWorldUI.WorldToUIPosition(this.Start);
				Vector2 vector2 = GenWorldUI.WorldToUIPosition(this.End);
				float num = Mathf.Atan2(vector2.y - vector.y, vector2.x - vector.x) * 57.29578f;
				if (num > 180f)
				{
					num -= 180f;
				}
				return num + 90f;
			}
		}

		private float TraveledPctStepPerTick
		{
			get
			{
				Vector3 start = this.Start;
				Vector3 end = this.End;
				if (start == end)
				{
					return 1f;
				}
				float num = GenMath.SphericalDistance(start.normalized, end.normalized);
				if (num == 0f)
				{
					return 1f;
				}
				return 0.00025f / num;
			}
		}

		
		public override void ExposeData()
		{
			base.ExposeData();
            Scribe_Values.Look(ref this.destinationTile, "destinationTile", 0);
            Scribe_Values.Look(ref this.arrived, "arrived", defaultValue: false);
			Scribe_Values.Look(ref this.initialTile, "initialTile", 0);
			Scribe_Values.Look(ref this.traveledPct, "traveledPct", 0f);
        }

		public override void PostAdd()
		{
			base.PostAdd();
            this.initialTile = this.Tile;
		}

		public override void Tick()
		{
			base.Tick();
            this.traveledPct += this.TraveledPctStepPerTick;
			if (this.traveledPct >= 1f)
			{
                this.traveledPct = 1f;
                this.Arrived();
			}
		}

		private void Arrived()
		{
			if (this.arrived) return;
            this.arrived = true;

            WorldObject worldObject = Find.World.worldObjects.WorldObjectAt<WorldObject>(this.destinationTile);
            if (worldObject is MapParent mp && mp.HasMap)
            {
                GenSpawn.Spawn(SkyfallerMaker.MakeSkyfaller(VFEMDefOf.VFEM_MissileIncoming), mp.Map.Center, mp.Map);
            }
            else
            {
                Find.World.grid[this.destinationTile].hilliness = Hilliness.Impassable;
                worldObject?.Faction.TryAffectGoodwillWith(Faction.OfPlayer, -200);
                worldObject?.Destroy();
            }
            this.Destroy();
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            
        }

        public ThingOwner GetDirectlyHeldThings() => null;
    }

    public class MissileIncoming : Skyfaller, IActiveDropPod
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.angle = 0f;
        }

        public ActiveDropPodInfo Contents
        {
            get => ((ActiveDropPod) this.innerContainer[0]).Contents;
            set => ((ActiveDropPod) this.innerContainer[0]).Contents = value;
        }

        protected override void SpawnThings()
        {
        }

        protected override void Impact()
        {
            IntVec3 loc = this.Map.Center;

            //Find.CameraDriver.SetRootPosAndSize(rootPos: Find.CurrentMap.rememberedCameraPos.rootPos, rootSize: 50f);
            //Find.CameraDriver.JumpToCurrentMapLoc(cell: loc);

            int radius = Mathf.CeilToInt(this.Map.Size.x / 2f - 5f);

            CellRect cells = CellRect.CenteredOn(loc, radius);

            if (Find.CurrentMap == this.Map)
                Find.CameraDriver.shaker.DoShake(mag: 20f);

            Map?.Parent.Faction.TryAffectGoodwillWith(Faction.OfPlayer, -200);

            AccessTools.FieldRef<MoteCounter, int> moteCount = AccessTools.FieldRefAccess<MoteCounter, int>(fieldName: "moteCount");

            DamageInfo destroyInfo = new DamageInfo(DamageDefOf.Bomb, float.MaxValue, float.MaxValue, instigator: this);
            DamageInfo damageInfo  = new DamageInfo(DamageDefOf.Bomb, 500,            1f,             instigator: this);


            
            int       x  = 0;
            foreach (IntVec3 intVec3 in cells)
            {
                x++;
                if (x % 50 == 0)
                {
                    moteCount(this.Map.moteCounter) = 0;
                        Vector3 vc = intVec3.ToVector3();
                    MoteMaker.ThrowMicroSparks(vc, this.Map);
                    MoteMaker.ThrowHeatGlow(intVec3, this.Map, size: 3f);
                    MoteMaker.ThrowFireGlow(intVec3, this.Map, size: 5f);
                    MoteMaker.ThrowLightningGlow(vc, this.Map, size: 10f);
                    MoteMaker.ThrowMetaPuff(vc, this.Map);
                }
                List<Thing> things = this.Map.thingGrid.ThingsListAtFast(intVec3);
                
                for (int i = 0; i < things.Count; i++)
                {
                    Thing thing = things[i];
                    if (thing is Pawn || thing.def.IsEdifice() && !(thing.def.building?.isNaturalRock ?? false))
                        thing.TakeDamage(damageInfo);
                }
            }

            FloodFillerFog.FloodUnfog(loc, this.Map);
            GenExplosion.DoExplosion(loc, this.Map, radius, DamageDefOf.Bomb, damAmount: 500, applyDamageToExplosionCellsNeighbors: true, chanceToStartFire: 1f, instigator: this);
            this.Map.weatherDecider.DisableRainFor(GenDate.TicksPerQuadrum);
            this.Map.TileInfo.hilliness = Hilliness.Impassable;


            this.Destroy();
        }
    }
}
