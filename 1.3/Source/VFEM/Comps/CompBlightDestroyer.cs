using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace VFEMech
{

	public class CompProperties_BlightDestroyer : CompProperties
	{
		public float effectiveRadius;
		public int tickRate;
		public CompProperties_BlightDestroyer()
		{
			compClass = typeof(CompBlightDestroyer);
		}
	}
	public class CompBlightDestroyer : ThingComp
    {
		public CompProperties_BlightDestroyer Props => base.props as CompProperties_BlightDestroyer;
        public override void CompTick()
        {
            base.CompTick();
			if (this.parent.Map != null && this.parent.IsHashIntervalTick(Props.tickRate))
            {
				var plants = GenRadial.RadialDistinctThingsAround(this.parent.Position, this.parent.Map, Props.effectiveRadius, true).OfType<Plant>().Where(x => x.Blighted);
				if (plants.Any())
                {
					var blighted = plants.First();
					var blight = blighted.Position.GetFirstBlight(blighted.Map);
					blight.Destroy();
				}
            }
        }
    }
}
