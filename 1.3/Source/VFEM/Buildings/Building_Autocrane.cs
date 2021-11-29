using Mono.Unix.Native;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace VFEMech
{
    [StaticConstructorOnStartup]
    public class Building_Autocrane : Building
    {
        private CompPowerTrader compPower;

        private Frame curFrameTarget;
        private Building curBuildingTarget;

        public float curCraneSize;

        private IntVec3 endCranePosition;

        private float curRotationInt;

        private static Material craneTopMat1 = MaterialPool.MatFrom("Things/Automation/AutoCrane/AutoCrane_Crane1");
        private static Material craneTopMat2 = MaterialPool.MatFrom("Things/Automation/AutoCrane/AutoCrane_Crane2");
        private static Material craneTopMat3 = MaterialPool.MatFrom("Things/Automation/AutoCrane/AutoCrane_Crane3");

        [TweakValue("00VFEM", 0, 100)] private static float topDrawSizeX = 30;
        [TweakValue("00VFEM", 0, 100)] private static float topDrawSizeY = 1;
        [TweakValue("00VFEM", -10, 10)] private static float topDrawOffsetX = -0.02f;
        [TweakValue("00VFEM", -10, 10)] private static float topDrawOffsetZ = 2.23f;

        public const int MaxDistanceToTargets = 20;
        public const int MinDistanceFromBase = 6;

        [TweakValue("00VFEM", 0, 1)] private static float rotationSpeed = 0.5f;
        [TweakValue("00VFEM", 0, 1)] private static float craneErectionSpeed = 0.005f;
        [TweakValue("00VFEM", 0, 20)] private static float distanceRate = 14.5f;

        Effecter effecter = null;

        bool turnClockWise;
        private Vector3 CraneDrawPos => this.DrawPos + Altitudes.AltIncVect + new Vector3(topDrawOffsetX, 0, topDrawOffsetZ);
        public float CurRotation
        {
            get
            {
                return curRotationInt;
            }
            set
            {
                curRotationInt = ClampAngle(value);
            }
        }

        private IntVec3 startingPosition;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            compPower = this.GetComp<CompPowerTrader>();
            startingPosition = GetStartingEndCranePosition();
            if (!respawningAfterLoad)
            {
                endCranePosition = startingPosition;
                CurRotation = CraneDrawPos.AngleToFlat(endCranePosition.ToVector3Shifted());
                var distance = Vector3.Distance(CraneDrawPos, endCranePosition.ToVector3Shifted());
                curCraneSize = distance / distanceRate;
            }
        }

        private IntVec3 GetStartingEndCranePosition()
        {
            IntVec3 curCell = this.OccupiedRect().CenterCell + (Rot4.North.FacingCell * 2);
            int num = 0;
            while ((curCell + Rot4.East.FacingCell).InBounds(Map) && num < 2)
            {
                curCell = curCell + Rot4.East.FacingCell;
                num++;
            }
            return curCell;
        }
        public override void Draw()
        {
            base.Draw();
            Matrix4x4 matrix = default(Matrix4x4);
            var topCranePos = CraneDrawPos;
            topCranePos.y += 5;
            matrix.SetTRS(topCranePos, Quaternion.Euler(0, CurRotation, 0), new Vector3(topDrawSizeX, 1f, topDrawSizeY));
            Graphics.DrawMesh(MeshPool.plane10, matrix, craneTopMat1, 0);
            topCranePos.y--;
            matrix.SetTRS(topCranePos, Quaternion.Euler(0, CurRotation, 0), new Vector3(topDrawSizeX, 1f, topDrawSizeY));
            Graphics.DrawMesh(MeshPool.plane10, matrix, craneTopMat2, 0);
            topCranePos.y--;
            matrix.SetTRS(topCranePos, Quaternion.Euler(0, CurRotation, 0), new Vector3(topDrawSizeX, 1f, topDrawSizeY));
            var mesh = MeshPool.GridPlane(new Vector3(curCraneSize, 1f, 0));
            Graphics.DrawMesh(mesh, matrix, craneTopMat3, 0);
        }
        public override void Tick()
        {
            base.Tick();
            if (this.Map != null && compPower.PowerOn && this.Faction == Faction.OfPlayer && this.IsHashIntervalTick(30))
            {
                if (curFrameTarget == null || curFrameTarget.Destroyed)
                {
                    curFrameTarget = NextFrameTarget();
                    if (curFrameTarget != null)
                    {
                        StartMovingTo(curFrameTarget);
                        compPower.powerOutputInt = -3000;
                        return;
                    }
                    else
                    {
                        compPower.powerOutputInt = -200;
                    }
                }
                else if (curFrameTarget != null)
                {
                    if (Map.reservationManager.IsReservedByAnyoneOf(curFrameTarget, Faction)) // pawn builders must be prioritized first, so the crane will not work on it
                    {
                        curFrameTarget = null;
                    }
                    else if (!TryMoveTo(curFrameTarget))
                    {
                        DoConstruction(curFrameTarget);
                        DoConstructionEffect();
                    }
                    return;
                }

                if (curBuildingTarget == null || curBuildingTarget.Destroyed || curBuildingTarget.MaxHitPoints == curBuildingTarget.HitPoints)
                {
                    curBuildingTarget = NextDamagedBuildingTarget();
                    if (curBuildingTarget != null)
                    {
                        StartMovingTo(curBuildingTarget);
                        compPower.powerOutputInt = -3000;
                        return;
                    }
                    else
                    {
                        compPower.powerOutputInt = -200;
                    }
                }
                else if (curBuildingTarget != null)
                {
                    if (Map.reservationManager.IsReservedByAnyoneOf(curBuildingTarget, Faction)) // pawn builders must be prioritized first, so the crane will not work on it
                    {
                        curBuildingTarget = null;
                    }
                    else if (!TryMoveTo(curBuildingTarget))
                    {
                        DoRepairing(curBuildingTarget);
                        DoRepairEffect();
                    }
                    return;
                }
                TryMoveTo(startingPosition, new Vector3(0, 0, 0.2f));
            }
        }

        private void StartMovingTo(LocalTargetInfo target)
        {
            var angle = CraneDrawPos.AngleToFlat(target.CenterVector3);
            angle = ClampAngle(angle);
            var anglediff = (CurRotation - angle + 180 + 360) % 360 - 180;
            turnClockWise = anglediff < 0;
        }

        private bool TryMoveTo(LocalTargetInfo target, Vector3 offset = default)
        {
            var angle = CraneDrawPos.AngleToFlat(target.CenterVector3 + offset);
            angle = ClampAngle(angle);
            var angleAbs = Mathf.Abs(angle - CurRotation);
            if (angleAbs > 0 && angleAbs < rotationSpeed)
            {
                CurRotation = angle;
            }
            else if (angle != CurRotation)
            {
                if (turnClockWise)
                {
                    CurRotation += rotationSpeed;
                }
                else
                {
                    CurRotation -= rotationSpeed;
                }
            }
            else
            {
                var distance = Vector3.Distance(CraneDrawPos, target.CenterVector3 + offset);
                var targetSize = distance / distanceRate;
                if (targetSize > (curCraneSize + craneErectionSpeed))
                {
                    curCraneSize += craneErectionSpeed;
                }
                else if (targetSize <= (curCraneSize - craneErectionSpeed))
                {
                    curCraneSize -= craneErectionSpeed;
                }
                else
                {
                    var sizeAbs = Mathf.Abs(targetSize - curCraneSize);
                    if (sizeAbs > 0 && sizeAbs < craneErectionSpeed)
                    {
                        curCraneSize = targetSize;
                    }
                    else
                    {
                        endCranePosition = target.Cell;
                        return false;
                    }
                }
            }
            return true;
        }
        private void DoConstructionEffect()
        {
            if (curFrameTarget.WorkLeft > 0)
            {
                if (effecter == null)
                {
                    EffecterDef effecterDef = curFrameTarget.ConstructionEffect;
                    if (effecterDef != null)
                    {
                        effecter = effecterDef.Spawn();
                        effecter.Trigger(curFrameTarget, curFrameTarget);
                    }
                }
                else
                {
                    effecter.EffectTick(curFrameTarget, curFrameTarget);
                }
            }
            else
            {
                if (effecter != null)
                {
                    effecter.Cleanup();
                    effecter = null;
                }
            }
        }

        private void DoConstruction(Frame frame)
        {
            float num = 1.7f;
            if (frame.Stuff != null)
            {
                num *= frame.Stuff.GetStatValueAbstract(StatDefOf.ConstructionSpeedFactor);
            }
            float workToBuild = frame.WorkToBuild;
            if (frame.def.entityDefToBuild is TerrainDef)
            {
                base.Map.snowGrid.SetDepth(frame.Position, 0f);
            }
            frame.workDone += num;
            if (frame.workDone >= workToBuild)
            {
                CompleteConstruction(frame);
            }
        }

        private void DoRepairing(Building building)
        {
            if (this.IsHashIntervalTick(20))
            {
                building.HitPoints++;
                building.HitPoints = Mathf.Min(building.HitPoints, building.MaxHitPoints);
                base.Map.listerBuildingsRepairable.Notify_BuildingRepaired(building);
            }
        }

        private void DoRepairEffect()
        {
            if (curBuildingTarget.HitPoints < curBuildingTarget.MaxHitPoints)
            {
                if (effecter == null)
                {
                    EffecterDef effecterDef = curBuildingTarget.def.repairEffect;
                    if (effecterDef != null)
                    {
                        effecter = effecterDef.Spawn();
                        effecter.Trigger(curBuildingTarget, curBuildingTarget);
                    }
                }
                else
                {
                    effecter.EffectTick(curBuildingTarget, curBuildingTarget);
                }
            }
            else
            {
                if (effecter != null)
                {
                    effecter.Cleanup();
                    effecter = null;
                }
            }
        }

        public void CompleteConstruction(Frame frame)
        {
            if (this.Faction != null)
            {
                QuestUtility.SendQuestTargetSignals(Faction.questTags, "BuiltBuilding", frame.Named("SUBJECT"));
            }
            List<CompHasSources> list = new List<CompHasSources>();
            for (int i = 0; i < frame.resourceContainer.Count; i++)
            {
                CompHasSources compHasSources = frame.resourceContainer[i].TryGetComp<CompHasSources>();
                if (compHasSources != null)
                {
                    list.Add(compHasSources);
                }
            }
            frame.resourceContainer.ClearAndDestroyContents();
            Map map = frame.Map;
            frame.Destroy();
            if (frame.GetStatValue(StatDefOf.WorkToBuild) > 150f && frame.def.entityDefToBuild is ThingDef && ((ThingDef)frame.def.entityDefToBuild).category == ThingCategory.Building)
            {
                SoundDefOf.Building_Complete.PlayOneShot(new TargetInfo(frame.Position, map));
            }
            ThingDef thingDef = frame.def.entityDefToBuild as ThingDef;
            Thing thing = null;
            if (thingDef != null)
            {
                thing = ThingMaker.MakeThing(thingDef, frame.Stuff);
                thing.SetFactionDirect(frame.Faction);
                CompQuality compQuality = thing.TryGetComp<CompQuality>();
                if (compQuality != null)
                {
                    QualityCategory q = QualityCategory.Normal;
                    compQuality.SetQuality(q, ArtGenerationContext.Colony);
                }
                CompHasSources compHasSources2 = thing.TryGetComp<CompHasSources>();
                if (compHasSources2 != null && !list.NullOrEmpty())
                {
                    for (int j = 0; j < list.Count; j++)
                    {
                        list[j].TransferSourcesTo(compHasSources2);
                    }
                }
                thing.HitPoints = Mathf.CeilToInt((float)HitPoints / (float)frame.MaxHitPoints * (float)thing.MaxHitPoints);
                GenSpawn.Spawn(thing, frame.Position, map, frame.Rotation, WipeMode.FullRefund);
                if (thingDef != null)
                {
                    Color? ideoColorForBuilding = IdeoUtility.GetIdeoColorForBuilding(thingDef, frame.Faction);
                    if (ideoColorForBuilding.HasValue)
                    {
                        thing.SetColor(ideoColorForBuilding.Value);
                    }
                }
            }
            else
            {
                map.terrainGrid.SetTerrain(frame.Position, (TerrainDef)frame.def.entityDefToBuild);
                FilthMaker.RemoveAllFilth(frame.Position, map);
            }
        }

        private static float ClampAngle(float angle)
        {
            if (angle > 360f)
            {
                angle -= 360f;
            }
            if (angle < 0f)
            {
                angle += 360f;
            }
            return angle;
        }


        private Frame NextFrameTarget()
        {
            return GenRadial.RadialDistinctThingsAround(this.Position, Map, MaxDistanceToTargets, true).OfType<Frame>()
                .Where(x => x.Position.DistanceTo(this.Position) >= MinDistanceFromBase && !Map.reservationManager.IsReservedByAnyoneOf(x, Faction))
                .OrderBy(x => x.Position.DistanceTo(endCranePosition)).FirstOrDefault();
        }

        private Building NextDamagedBuildingTarget()
        {
            return GenRadial.RadialDistinctThingsAround(this.Position, Map, MaxDistanceToTargets, true).OfType<Building>()
                .Where(x => x.HitPoints < x.MaxHitPoints && x.Position.DistanceTo(this.Position) >= MinDistanceFromBase && !Map.reservationManager.IsReservedByAnyoneOf(x, Faction))
                .OrderBy(x => x.Position.DistanceTo(endCranePosition)).FirstOrDefault();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref curFrameTarget, "curFrameTarget");
            Scribe_References.Look(ref curBuildingTarget, "curBuildingTarget");
            Scribe_Values.Look(ref curCraneSize, "curCraneSize");
            Scribe_Values.Look(ref curRotationInt, "curRotationInt");
            Scribe_Values.Look(ref endCranePosition, "endCranePosition");
            Scribe_Values.Look(ref turnClockWise, "turnClockWise");
        }
    }
}
