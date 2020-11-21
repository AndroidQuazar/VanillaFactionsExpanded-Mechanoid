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
            if (__result.Count() == 0)
            {
				List<CompPower> compPowers = __result.ToList();;
				if (!b.parent.Spawned)
				{
					Log.Warning(string.Concat("Can't check potential connectors for ", b, " because it's unspawned."));
				}
				CellRect rect = b.parent.OccupiedRect().ExpandedBy(18).ClipInsideMap(b.parent.Map);
				for (int z = rect.minZ; z <= rect.maxZ; z++)
				{
					for (int x = rect.minX; x <= rect.maxX; x++)
					{
						IntVec3 c = new IntVec3(x, 0, z);
						List<Thing> thingList = b.parent.Map.thingGrid.ThingsListAt(c);
						for (int i = 0; i < thingList.Count; i++)
						{
							if (thingList[i].def.ConnectToPower && thingList[i].def == VFEMDefOf.VFE_ConduitPylon)
							{
								compPowers.Add(((Building)thingList[i]).PowerComp);
							}
						}
					}
				}
				__result = compPowers;
			}
		}
    }


	[HarmonyPatch(typeof(PowerConnectionMaker), "BestTransmitterForConnector")]
	internal static class BestTransmitterForConnector_Patch
	{
		public static void Postfix(ref CompPower __result, IntVec3 connectorPos, Map map, List<PowerNet> disallowedNets = null)
		{
			if (__result == null)
            {
				var conduitPylons = map.listerThings.AllThings.Where(x => x.def == VFEMDefOf.VFE_ConduitPylon);
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
							&& conduitPylons.Where(x => x.Position.DistanceTo(transmitter.Position) <= 18).Any())
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

	[HarmonyPatch(typeof(PowerConnectionMaker), "TryConnectToAnyPowerNet")]
	internal static class TryConnectToAnyPowerNet_Patch
	{
		public static void Postfix(CompPower pc, List<PowerNet> disallowedNets = null)
		{
			if (pc.connectParent == null && pc.parent.Spawned)
			{
				Log.Message("PC: " + pc);
			}
		}
	}
}
