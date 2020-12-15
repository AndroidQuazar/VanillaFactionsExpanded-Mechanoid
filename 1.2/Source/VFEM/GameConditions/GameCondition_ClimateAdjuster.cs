using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFEMech
{
	public class GameCondition_ClimateAdjuster : GameCondition
	{
		public float currentOffset = -20f;
		public override float TemperatureOffset()
		{
			return GameConditionUtility.LerpInOutValue(this, TransitionTicks, currentOffset);
		}
        public override int TransitionTicks => 6000;
        public override void GameConditionTick()
        {
            base.GameConditionTick();
            if (Find.TickManager.TicksAbs % GenDate.TicksPerDay == 0)
            {
                currentOffset -= 20f;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
			Scribe_Values.Look(ref currentOffset, "currentOffset", -20f);
        }
    }
}
