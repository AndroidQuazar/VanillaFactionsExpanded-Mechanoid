using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace VFEMech
{
    public class CompProperties_MechaniteReinforcer : CompProperties
    {
        public IntRange heightRange;
        public int width;
        public CompProperties_MechaniteReinforcer()
        {
            base.compClass = typeof(CompMechaniteReinforcer);
        }
    }
    public class CompMechaniteReinforcer : ThingComp
    {
        public static List<CompMechaniteReinforcer> compMechaniteReinforcers = new List<CompMechaniteReinforcer>();
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compMechaniteReinforcers.Add(this);
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            compMechaniteReinforcers.Remove(this);
        }
        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            compMechaniteReinforcers.Remove(this);
        }
        public CompProperties_MechaniteReinforcer Props => (CompProperties_MechaniteReinforcer)base.props;
        public override void PostDrawExtraSelectionOverlays()
        {
            base.PostDrawExtraSelectionOverlays();
            GenDraw.DrawFieldEdges(Utils.GetTotalAffectedCells(this.Props, this.parent.Rotation, this.parent.OccupiedRect(), this.parent.Map).ToList(), Color.white);
        }
    }
    public class PlaceWorker_ShowReinforcerAffectArea : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            CompProperties_MechaniteReinforcer compProperties = def.GetCompProperties<CompProperties_MechaniteReinforcer>();
            if (compProperties != null)
            {
                var cells = Utils.GetTotalAffectedCells(compProperties, rot, GenAdj.OccupiedRect(center, rot, def.size), Find.CurrentMap);
                GenDraw.DrawFieldEdges(cells.ToList(), Color.white);
            }
        }
    }

    [HarmonyPatch(typeof(Thing), "TakeDamage")]
    public static class Thing_TakeDamage_Patch
    {
        public static void Prefix(Thing __instance, ref DamageInfo dinfo)
        {
            if (__instance is Building && (dinfo.Weapon is null || dinfo.Weapon.race != null || dinfo.Weapon.IsMeleeWeapon || !dinfo.Def.isRanged && !dinfo.Def.isExplosive))
            {
                foreach (var reinforcer in CompMechaniteReinforcer.compMechaniteReinforcers)
                {
                    if (reinforcer.parent.Map == __instance.Map)
                    {
                        var affectedCells = Utils.GetTotalAffectedCells(reinforcer.Props, reinforcer.parent.Rotation, reinforcer.parent.OccupiedRect(), reinforcer.parent.Map);
                        if (affectedCells.Contains(__instance.Position))
                        {
                            dinfo.SetAmount(0);
                        }
                    }
                }
            }
        }
    }
    public static class Utils
    {
        private static HashSet<IntVec3> GetStartingCells(CompProperties_MechaniteReinforcer props, Rot4 rot, CellRect cellRect, Map map)
        {
            HashSet<IntVec3> startingCells = new HashSet<IntVec3>();
            foreach (var pos in cellRect.Cells)
            {
                IntVec3 startCell = IntVec3.Invalid;
                IntVec3 curCell = pos;
                while (true)
                {
                    if (curCell.DistanceTo(pos) == props.heightRange.min)
                    {
                        startCell = curCell;
                        break;
                    }
                    else
                    {
                        curCell = curCell + rot.FacingCell;
                    }
                }
                startingCells.Add(startCell);
                int i = 1;
                while (startingCells.Count < props.width)
                {
                    if (rot.IsHorizontal)
                    {
                        if (startingCells.Count < props.width)
                        {
                            startingCells.Add(startCell + (Rot4.South.FacingCell * i));
                        }
                        if (startingCells.Count < props.width)
                        {
                            startingCells.Add(startCell + (Rot4.North.FacingCell * i));
                        }
                    }
                    else
                    {
                        if (startingCells.Count < props.width)
                        {
                            startingCells.Add(startCell + (Rot4.West.FacingCell * i));
                        }
                        if (startingCells.Count < props.width)
                        {
                            startingCells.Add(startCell + (Rot4.East.FacingCell * i));
                        }
                    }
                    i++;
                }
            }
            startingCells.RemoveWhere(x => !x.InBounds(map));
            return startingCells;
        }
        private static HashSet<IntVec3> GetAffectedCells(HashSet<IntVec3> affectedCells, CompProperties_MechaniteReinforcer props, Rot4 rot, CellRect cellRect, Map map)
        {
            var newTiles = new HashSet<IntVec3>();
            foreach (var pos in affectedCells)
            {
                IntVec3 curCell = pos;
                while (true)
                {
                    curCell = curCell + rot.FacingCell;
                    var distance = curCell.DistanceTo(pos) - 1;
                    if (distance >= props.heightRange.min && distance <= props.heightRange.max)
                    {
                        newTiles.Add(curCell);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            newTiles.RemoveWhere(x => !x.InBounds(map));
            return newTiles;
        }

        public static HashSet<IntVec3> GetTotalAffectedCells(CompProperties_MechaniteReinforcer props, Rot4 rot, CellRect cellRect, Map map)
        {
            var cells = GetStartingCells(props, rot, cellRect, map);
            cells.AddRange(GetAffectedCells(cells, props, rot, cellRect, map));
            return cells;
        }
    }
}
