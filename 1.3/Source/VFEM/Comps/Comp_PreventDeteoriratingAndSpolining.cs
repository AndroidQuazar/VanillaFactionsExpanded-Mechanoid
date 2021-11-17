using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MechanoidAddon
{
	public class CompProperties_PreventDeteoriratingAndSpolining : CompProperties
	{
		public CompProperties_PreventDeteoriratingAndSpolining()
		{
			compClass = typeof(Comp_PreventDeteoriratingAndSpolining);
		}
	}
	public class Comp_PreventDeteoriratingAndSpolining : ThingComp
    {
		public static Dictionary<Map, HashSet<IntVec3>> safePlaces = new Dictionary<Map, HashSet<IntVec3>>();
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
			if (safePlaces.ContainsKey(this.parent.Map))
			{
				safePlaces[this.parent.Map].Add(this.parent.Position);
			}
			else
			{
				safePlaces[this.parent.Map] = new HashSet<IntVec3> { this.parent.Position };
			}
		}

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
			if (safePlaces.ContainsKey(map))
			{
				safePlaces[map].Remove(this.parent.Position);
			}
		}
    }

}
