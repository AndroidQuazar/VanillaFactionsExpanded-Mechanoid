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
	public class CompProperties_PawnProducer : CompProperties
	{
		public List<PawnKindDef> pawnKindToProduceOneOf;
		public IntRange tickSpawnIntervalRange;
		public IntRange spawnCountRange;
		public CompProperties_PawnProducer()
		{
			compClass = typeof(CompPawnProducer);
		}
	}
	public class CompPawnProducer : ThingComp
	{
		public PawnKindDef curPawnKindDef;

		public int nextTick = -1;
		public CompProperties_PawnProducer Props => (CompProperties_PawnProducer)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
			if (!respawningAfterLoad)
            {
				curPawnKindDef = Props.pawnKindToProduceOneOf.RandomElement();
				nextTick = Find.TickManager.TicksAbs + Props.tickSpawnIntervalRange.RandomInRange;
			}
        }

        public override string TransformLabel(string label)
        {
            return base.TransformLabel(label) + " (" + this.curPawnKindDef.LabelCap + ")";
        }
        public override void CompTick()
		{
			if (Find.TickManager.TicksAbs >= nextTick)
            {
				var spawnCount = Props.spawnCountRange.RandomInRange;
				nextTick = Find.TickManager.TicksAbs + Props.tickSpawnIntervalRange.RandomInRange;
				var faction = Find.FactionManager.FirstFactionOfDef(VFEMDefOf.VFE_Mechanoid);
				for (int i = 0; i < spawnCount; i++)
                {
					var mech = PawnGenerator.GeneratePawn(curPawnKindDef, faction);
					GenPlace.TryPlaceThing(mech, this.parent.Position, this.parent.Map, ThingPlaceMode.Near);
					MechUtils.CreateOrAddToAssaultLord(mech);
				}
			}
		}
		public override string CompInspectStringExtra()
		{
			return base.CompInspectStringExtra();
		}

        public override void PostExposeData()
        {
            base.PostExposeData();
			Scribe_Defs.Look(ref curPawnKindDef, "curMechKindDef");
        }
    }
}
