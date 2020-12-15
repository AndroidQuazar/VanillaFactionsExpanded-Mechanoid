using KCSG;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Grammar;

namespace VFEMech
{
	public class ThingSetMaker_MechanoidStorageContent : ThingSetMaker
	{
		protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
		{
			ThingDef thingDef;
			float num = parms.totalMarketValueRange.Value.RandomInRange;
			do
			{
				thingDef = RandomMechanoidStorageContentsDef();
				Thing thing = ThingMaker.MakeThing(thingDef);
				int num2 = Rand.Range(20, 75);
				if (num2 > thing.def.stackLimit)
				{
					num2 = thing.def.stackLimit;
				}
				if ((float)num2 * thing.def.BaseMarketValue > num)
				{
					num2 = Mathf.FloorToInt(num / thing.def.BaseMarketValue);
				}
				if (num2 == 0)
				{
					num2 = 1;
				}
				thing.stackCount = num2;
				outThings.Add(thing);
				num -= (float)num2 * thingDef.BaseMarketValue;
			}
			while (!(num <= thingDef.BaseMarketValue));
		}

		private static IEnumerable<ThingDef> PossibleMechanoidStorageDefs()
		{
			return DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.IsStuff && d.BaseMarketValue <= 10);
		}

		public static ThingDef RandomMechanoidStorageContentsDef(bool mustBeResource = false)
		{
			IEnumerable<ThingDef> source = PossibleMechanoidStorageDefs();
			if (mustBeResource)
			{
				source = source.Where((ThingDef x) => x.stackLimit > 1);
			}
			int numMeats = source.Where((ThingDef x) => x.IsMeat).Count();
			int numLeathers = source.Where((ThingDef x) => x.IsLeather).Count();
			return source.RandomElementByWeight((ThingDef d) => ThingSetMakerUtility.AdjustedBigCategoriesSelectionWeight(d, numMeats, numLeathers));
		}

        protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
        {
			return PossibleMechanoidStorageDefs();
		}
	}
}