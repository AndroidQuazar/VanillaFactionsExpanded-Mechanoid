using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;

namespace VFEMech
{
	public class CompPylon : ThingComp
	{
        public static Dictionary<Map, List<CompPower>> compPylons = new Dictionary<Map, List<CompPower>>();
        public CompPower compPower;
        public Building transmitter;
        public CompPower transCompPower;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compPower = this.parent.TryGetComp<CompPower>();
            if (compPylons.ContainsKey(this.parent.Map))
            {
                compPylons[this.parent.Map].Add(compPower);
            }
            else
            {
                compPylons[this.parent.Map] = new List<CompPower> { compPower };
            }
            transmitter = this.parent.Position.GetTransmitter(this.parent.Map);
            if (transmitter != null)
            {
                transCompPower = transmitter.TryGetComp<CompPower>();
                parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlag.PowerGrid, regenAdjacentCells: true, regenAdjacentSections: false);
                parent.Map.powerNetManager.Notify_TransmitterSpawned(transCompPower);
                if (parent.def.ConnectToPower)
                {
                    parent.Map.powerNetManager.Notify_ConnectorWantsConnect(transCompPower);
                }
            }
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            if (compPylons.ContainsKey(map))
            {
                compPylons[map].Remove(compPower);
            }

            if (transmitter != null)
            {
                if (transCompPower.connectChildren != null)
                {
                    for (int i = 0; i < transCompPower.connectChildren.Count; i++)
                    {
                        transCompPower.connectChildren[i].LostConnectParent();
                    }
                }
                map.powerNetManager.Notify_TransmitterDespawned(transCompPower);
                map.powerNetManager.Notify_ConnectorDespawned(transCompPower);
                map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlag.PowerGrid, regenAdjacentCells: true, regenAdjacentSections: false);
            }

        }
    }
}
