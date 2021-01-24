using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace VFEMech
{
	[HarmonyPatch(typeof(PowerConnectionMaker), "PotentialConnectorsForTransmitter")]
    internal static class PotentialConnectorsForTransmitter_Patch
    {
        public static void Postfix(ref IEnumerable<CompPower> __result, CompPower b)
        {
			if (CompPylon.compPylons.TryGetValue(b.parent.Map, out List<CompPower> compPylons) && b.parent.Spawned)
			{
				var nearbyPylons = compPylons.Where(x => x.parent.Position.DistanceTo(b.parent.Position) <= 18);
				foreach (var nearbyPylon in nearbyPylons)
				{
					__result.AddItem(nearbyPylon);
				}
			}

			//if (__result.Count() == 0)
            //{
			//	List<CompPower> compPowers = __result.ToList();
			//	if (b.parent.Spawned && CompPylon.compPylons.TryGetValue(b.parent.Map, out List<CompPower> compPylons))
			//	{
			//		var nearbyPylons = compPylons.Where(x => x.parent.Position.DistanceTo(b.parent.Position) <= 18);
			//		if (nearbyPylons.Any())
			//	    {
			//			compPowers.AddRange(nearbyPylons);
			//	    }
			//	}
			//
			//	// old vanilla non-performant code...
			//	//if (!b.parent.Spawned)
			//	//{
			//	//	Log.Warning(string.Concat("Can't check potential connectors for ", b, " because it's unspawned."));
			//	//}
			//	//CellRect rect = b.parent.OccupiedRect().ExpandedBy(18).ClipInsideMap(b.parent.Map);
			//	//for (int z = rect.minZ; z <= rect.maxZ; z++)
			//	//{
			//	//	for (int x = rect.minX; x <= rect.maxX; x++)
			//	//	{
			//	//		IntVec3 c = new IntVec3(x, 0, z);
			//	//		List<Thing> thingList = b.parent.Map.thingGrid.ThingsListAt(c);
			//	//		for (int i = 0; i < thingList.Count; i++)
			//	//		{
			//	//			if (thingList[i].def == VFEMDefOf.VFE_ConduitPylon && thingList[i].TryGetComp<CompFlickable>().SwitchIsOn)
			//	//			{
			//	//				var compPower = thingList[i].TryGetComp<CompPower>();
			//	//				if (compPower != null)
			//	//				{
			//	//					compPowers.Add(compPower);
			//	//				}
			//	//			}
			//	//		}
			//	//	}
			//	//}
			//	__result = compPowers;
			//}
		}
    }


	[HarmonyPatch(typeof(PowerConnectionMaker), "BestTransmitterForConnector")]
	internal static class BestTransmitterForConnector_Patch
	{
		public static void Postfix(ref CompPower __result, IntVec3 connectorPos, Map map, List<PowerNet> disallowedNets = null)
		{
			if (__result == null)
            {
				CellRect cellRect = CellRect.SingleCell(connectorPos).ExpandedBy(18).ClipInsideMap(map);
				cellRect.ClipInsideMap(map);
				float num = 999999f;
				CompPower result = null;
				for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
				{
					for (int j = cellRect.minX; j <= cellRect.maxX; j++)
					{
						Building transmitter = new IntVec3(j, 0, i).GetTransmitter(map);
						if (transmitter == null || transmitter.Destroyed)
						{
							continue;
						}
						CompPower powerComp = transmitter.PowerComp;
						if (powerComp != null && powerComp.TransmitsPowerNow && (transmitter.def.building == null || transmitter.def.building.allowWireConnection) 
							&& (disallowedNets == null || !disallowedNets.Contains(powerComp.transNet))
							&& transmitter.Position.GetThingList(map).Where(x => x.def == VFEMDefOf.VFE_ConduitPylon && x.TryGetComp<CompFlickable>().SwitchIsOn).Any())
						{
							float num2 = (transmitter.Position - connectorPos).LengthHorizontalSquared;
							if (num2 < num)
							{
								num = num2;
								result = powerComp;
							}
						}
					}
				}
				__result = result;
			}
		}
	}

	[HarmonyPatch(typeof(CompPowerTrader), "PowerOn", MethodType.Setter)]
	internal static class PowerOn_Patch
	{
		public static void Postfix(CompPowerTrader __instance)
		{
			if (__instance.parent.def == VFEMDefOf.VFE_ConduitPylon && __instance.parent.Map != null)
			{
				var transmitter = __instance.parent.Position.GetTransmitter(__instance.parent.Map);
				if (transmitter != null)
                {
					CompPower powerComp = transmitter.PowerComp;
					for (int num = powerComp.PowerNet.powerComps.Count - 1; num >= 0; num--)
					{
						var powerUser = powerComp.PowerNet.powerComps[num];
						if (powerUser.parent.def != VFEMDefOf.VFE_ConduitPylon && powerUser.connectParent?.parent?.Position == __instance.parent.Position && powerUser.connectParent != null)
						{
							PowerConnectionMaker.DisconnectFromPowerNet(powerUser);
							var flickableComp = powerUser.parent.TryGetComp<CompFlickable>();
							if (flickableComp != null && !flickableComp.SwitchIsOn)
							{
								powerUser.parent.Map.overlayDrawer.DrawOverlay(powerUser.parent, OverlayTypes.PowerOff);
							}
							else if (FlickUtility.WantsToBeOn(powerUser.parent) && !powerUser.PowerOn)
							{
								powerUser.parent.Map.overlayDrawer.DrawOverlay(powerUser.parent, OverlayTypes.NeedsPower);
							}
							powerUser.PowerOn = false;
							powerUser.parent.Map.powerNetManager.Notify_ConnectorWantsConnect(powerUser);
							PowerConnectionMaker.TryConnectToAnyPowerNet(powerUser);
						}
					}
				}

				if (__instance.PowerOn)
                {
					CellRect cellRect = CellRect.SingleCell(__instance.parent.Position).ExpandedBy(18).ClipInsideMap(__instance.parent.Map);
					cellRect.ClipInsideMap(__instance.parent.Map);
					for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
					{
						for (int j = cellRect.minX; j <= cellRect.maxX; j++)
						{
							foreach (var building in new IntVec3(j, 0, i).GetThingList(__instance.parent.Map))
							{
								if (building.TryGetComp<CompPowerTrader>() is CompPowerTrader powerUser && !powerUser.PowerOn)
								{
									powerUser.parent.Map.mapDrawer.MapMeshDirty(powerUser.parent.Position, MapMeshFlag.PowerGrid, regenAdjacentCells: true, regenAdjacentSections: false);
									if (powerUser.Props.transmitsPower)
									{
										powerUser.parent.Map.powerNetManager.Notify_TransmitterSpawned(powerUser);
									}
									if (powerUser.parent.def.ConnectToPower)
									{
										powerUser.parent.Map.powerNetManager.Notify_ConnectorWantsConnect(powerUser);
									}
									powerUser.SetUpPowerVars();
								}
							}
						}
					}
				}
			}
		}
	}
}
