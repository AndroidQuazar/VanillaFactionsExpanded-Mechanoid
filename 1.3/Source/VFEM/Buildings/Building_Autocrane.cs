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

        private Frame curTarget;

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

        private const int MaxDistanceToFrames = 20;
        private const int MinDistanceFromBase = 5;

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

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            compPower = this.GetComp<CompPowerTrader>();
            if (!respawningAfterLoad)
            {
                var cells = GenRadial.RadialCellsAround(this.Position, 10, true).Where(x => x.DistanceTo(this.Position) >= 9 && x.InBounds(map) && x.Walkable(map));
                if (cells.Any())
                {
                    endCranePosition = cells.RandomElement();
                    CurRotation = CraneDrawPos.AngleToFlat(endCranePosition.ToVector3Shifted());
                    var distance = Vector3.Distance(CraneDrawPos, endCranePosition.ToVector3Shifted());
                    curCraneSize = distance / distanceRate;
                }
            }
        }

        public override void Draw()
        {
            base.Draw();
            Matrix4x4 matrix = default(Matrix4x4);
            var topCranePos = CraneDrawPos;
            topCranePos.y += 5;
            matrix.SetTRS(topCranePos, Quaternion.Euler(0, CurRotation, 0), new Vector3(topDrawSizeX, 1f, topDrawSizeY));
            Graphics.DrawMesh(MeshPool.plane10, matrix, craneTopMat1, 0);
            Graphics.DrawMesh(MeshPool.plane10, matrix, craneTopMat2, 0);
            matrix.SetTRS(topCranePos, Quaternion.Euler(0, CurRotation, 0), new Vector3(topDrawSizeX, 1f, topDrawSizeY));
            var mesh = MeshPool.GridPlane(new Vector3(curCraneSize, 1f, 0));
            Graphics.DrawMesh(mesh, matrix, craneTopMat3, 0);
        }
        public override void Tick()
        {
            base.Tick();
            if (compPower.PowerOn && this.Faction == Faction.OfPlayer)
            {
                if (curTarget == null || curTarget.Destroyed)
                {
                    curTarget = NextTarget();
                    if (curTarget != null)
                    {
                        var angle = CraneDrawPos.AngleToFlat(curTarget.TrueCenter());
                        angle = ClampAngle(angle);
                        var anglediff = (CurRotation - angle + 180 + 360) % 360 - 180;
                        turnClockWise = anglediff < 0;
                    }
                }
                else if (curTarget != null)
                {
                    var angle = CraneDrawPos.AngleToFlat(curTarget.TrueCenter());
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
                        var distance = Vector3.Distance(CraneDrawPos, curTarget.TrueCenter());
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
                                endCranePosition = curTarget.Position;
                                DoConstruction(curTarget);
                                DoConstructionEffect();
                            }
                        }
                    }
                }
            }
        }

        private void DoConstructionEffect()
        {
            if (curTarget.WorkLeft > 0)
            {
                if (effecter == null)
                {
                    EffecterDef effecterDef = curTarget.ConstructionEffect;
                    if (effecterDef != null)
                    {
                        effecter = effecterDef.Spawn();
                        effecter.Trigger(curTarget, curTarget);
                    }
                }
                else
                {
                    effecter.EffectTick(curTarget, curTarget);
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

        private Frame NextTarget()
        {
            return GenRadial.RadialDistinctThingsAround(this.Position, Map, MaxDistanceToFrames, true).OfType<Frame>()
                .Where(x => x.Position.DistanceTo(this.Position) >= MinDistanceFromBase)
                .OrderBy(x => x.Position.DistanceTo(endCranePosition)).FirstOrDefault();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref curTarget, "curTarget");
            Scribe_Values.Look(ref curCraneSize, "curCraneSize");
            Scribe_Values.Look(ref curRotationInt, "curRotationInt");
            Scribe_Values.Look(ref endCranePosition, "endCranePosition");
            Scribe_Values.Look(ref turnClockWise, "turnClockWise");
        }
    }
}
