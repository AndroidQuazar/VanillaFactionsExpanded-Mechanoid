using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFE.Mechanoids.Needs
{
    public class Need_Power : Need
    {
		private int lastRestTick = -999;
		private float RestFallFactor => pawn.health.hediffSet.RestFallFactor;
		bool Enabled => CompMachine.cachedMachinesPawns[pawn] != null;

        public override bool ShowOnNeedList => Enabled;

        public RestCategory CurCategory
		{
			get
			{
				if (CurLevel < 0.01f)
				{
					return RestCategory.Exhausted;
				}
				if (CurLevel < 0.14f)
				{
					return RestCategory.VeryTired;
				}
				if (CurLevel < 0.28f)
				{
					return RestCategory.Tired;
				}
				return RestCategory.Rested;
			}
		}

		public float RestFallPerTick
		{
			get
			{
				return 1 / CompMachine.cachedMachinesPawns[pawn].Props.hoursActive / 2500;
			}
		}

		public override int GUIChangeArrow
		{
			get
			{
				if (Resting)
				{
					return 1;
				}
				return -1;
			}
		}

		private bool Resting => Find.TickManager.TicksGame < lastRestTick + 2;

		public Need_Power(Pawn pawn)
			: base(pawn)
		{
			threshPercents = new List<float>();
			threshPercents.Add(0.28f);
			threshPercents.Add(0.14f);
		}

		public override void SetInitialLevel()
		{
			CurLevel = 1f;
		}

		public override void NeedInterval()
		{
			if (Enabled && !IsFrozen)
			{
				if (Resting)
				{
					float num = 1f;
					num *= pawn.GetStatValue(StatDefOf.RestRateMultiplier);
					if (num > 0f)
					{
						CurLevel += 1 / CompMachine.cachedMachinesPawns[pawn].myBuilding.TryGetComp<CompMachineChargingStation>().Props.hoursToRecharge / 2500 * num * 150f;
					}
				}
				else
				{
					CurLevel -= RestFallPerTick * 150f;
				}
			}
		}

		public void TickResting(float restEffectiveness)
		{
			if (Enabled && !(restEffectiveness <= 0f))
			{
				lastRestTick = Find.TickManager.TicksGame;
			}
		}
	}
}
