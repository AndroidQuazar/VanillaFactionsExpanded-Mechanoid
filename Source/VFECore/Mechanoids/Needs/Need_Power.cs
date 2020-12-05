using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFE.Mechanoids.Needs
{
    class Need_Power : Need
    {
		private int lastRestTick = -999;

		private float RestFallFactor => pawn.health.hediffSet.RestFallFactor;

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
				switch (CurCategory)
				{
					case RestCategory.Rested:
						return 1.58333332E-05f * RestFallFactor;
					case RestCategory.Tired:
						return 1.58333332E-05f * RestFallFactor * 0.7f;
					case RestCategory.VeryTired:
						return 1.58333332E-05f * RestFallFactor * 0.3f;
					case RestCategory.Exhausted:
						return 1.58333332E-05f * RestFallFactor * 0.6f;
					default:
						return 999f;
				}
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
			if (!IsFrozen)
			{
				if (Resting)
				{
					float num = 1f;
					num *= pawn.GetStatValue(StatDefOf.RestRateMultiplier);
					if (num > 0f)
					{
						CurLevel += 0.005714286f * num;
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
			if (!(restEffectiveness <= 0f))
			{
				lastRestTick = Find.TickManager.TicksGame;
			}
		}
	}
}
