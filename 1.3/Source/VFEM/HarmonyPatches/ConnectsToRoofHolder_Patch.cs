using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;

namespace VFEMech
{
	[HarmonyPatch(typeof(RoofCollapseUtility), "WithinRangeOfRoofHolder")]
	public static class WithinRangeOfRoofHolder_Patch
	{
		public static void Postfix(ref bool __result, IntVec3 c, Map map, bool assumeNonNoRoofCellsAreRoofed = false)
		{
			if (!__result)
			{
				__result = CompRoofHolder.WithinRangeOfRoofHolder(c, map, assumeNonNoRoofCellsAreRoofed);
			}
		}
	}

	[HarmonyPatch(typeof(RoofCollapseCellsFinder), "ConnectsToRoofHolder")]
	public static class ConnectsToRoofHolder_Patch
	{

		public static void Postfix(ref bool __result, IntVec3 c, Map map, HashSet<IntVec3> visitedCells)
		{
			if (!__result)
			{
				__result = CompRoofHolder.AnyRoofHoldersInRange(map, c);
			}
		}
	}


	[HarmonyPatch(typeof(AutoBuildRoofAreaSetter), "TryGenerateAreaNow")]
	public static class AutoBuildRoofAreaSetter_TryGenerateAreaNow
	{
		private static List<Room> queuedGenerateRooms = new List<Room>();

		private static HashSet<IntVec3> cellsToRoof = new HashSet<IntVec3>();

		private static HashSet<IntVec3> innerCells = new HashSet<IntVec3>();

		private static List<IntVec3> justRoofedCells = new List<IntVec3>();
		public static void Postfix(Room room)
		{
			if (room.CellCount > 320)
{
				if (CompRoofHolder.roofHolderPlaces.TryGetValue(room.Map, out HashSet<CompRoofHolder> roofHolders))
				{
					if (roofHolders.Any(x => x.parent.GetRoom() == room))
                    {
						var map = room.Map;
						if (room.Dereferenced || room.TouchesMapEdge || room.RegionCount > 26 || room.IsDoorway)
						{
							return;
						}
						bool flag = false;
						foreach (IntVec3 borderCell in room.BorderCells)
						{
							Thing roofHolderOrImpassable = borderCell.GetRoofHolderOrImpassable(map);
							if (roofHolderOrImpassable != null)
							{
								if ((roofHolderOrImpassable.Faction != null && roofHolderOrImpassable.Faction != Faction.OfPlayer) || (roofHolderOrImpassable.def.building != null && !roofHolderOrImpassable.def.building.allowAutoroof))
								{
									return;
								}
								if (roofHolderOrImpassable.Faction == Faction.OfPlayer)
								{
									flag = true;
								}
							}
						}
						if (!flag)
						{
							return;
						}
						innerCells.Clear();
						foreach (IntVec3 cell in room.Cells)
						{
							if (!innerCells.Contains(cell))
							{
								innerCells.Add(cell);
							}
							for (int i = 0; i < 8; i++)
							{
								IntVec3 c = cell + GenAdj.AdjacentCells[i];
								if (!c.InBounds(map))
								{
									continue;
								}
								Thing roofHolderOrImpassable2 = c.GetRoofHolderOrImpassable(map);
								if (roofHolderOrImpassable2 == null || (roofHolderOrImpassable2.def.size.x <= 1 && roofHolderOrImpassable2.def.size.z <= 1))
								{
									continue;
								}
								CellRect cellRect = roofHolderOrImpassable2.OccupiedRect();
								cellRect.ClipInsideMap(map);
								for (int j = cellRect.minZ; j <= cellRect.maxZ; j++)
								{
									for (int k = cellRect.minX; k <= cellRect.maxX; k++)
									{
										IntVec3 item = new IntVec3(k, 0, j);
										if (!innerCells.Contains(item))
										{
											innerCells.Add(item);
										}
									}
								}
							}
						}
						cellsToRoof.Clear();
						foreach (IntVec3 innerCell in innerCells)
						{
							for (int l = 0; l < 9; l++)
							{
								IntVec3 intVec = innerCell + GenAdj.AdjacentCellsAndInside[l];
								if (intVec.InBounds(map) && (l == 8 || intVec.GetRoofHolderOrImpassable(map) != null) && !cellsToRoof.Contains(intVec))
								{
									cellsToRoof.Add(intVec);
								}
							}
						}
						justRoofedCells.Clear();
						foreach (IntVec3 item2 in cellsToRoof)
						{
							if (map.roofGrid.RoofAt(item2) == null && !justRoofedCells.Contains(item2) && !map.areaManager.NoRoof[item2] && RoofCollapseUtility.WithinRangeOfRoofHolder(item2, map, assumeNonNoRoofCellsAreRoofed: true))
							{
								map.areaManager.BuildRoof[item2] = true;
								justRoofedCells.Add(item2);
							}
						}
					}
				}
			}
		}
	}
}
