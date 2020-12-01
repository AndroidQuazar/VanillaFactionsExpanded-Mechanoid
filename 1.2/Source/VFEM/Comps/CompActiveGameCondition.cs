using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFEMech
{
	public class CompProperties_ActiveGameCondition : CompProperties
	{
		public GameConditionDef conditionDef;
		public CompProperties_ActiveGameCondition()
		{
			compClass = typeof(CompActiveGameCondition);
		}
	}
	public class CompActiveGameCondition : ThingComp
	{
		public CompProperties_ActiveGameCondition Props => (CompProperties_ActiveGameCondition)props;
		public GameConditionDef ConditionDef => Props.conditionDef;

		public GameCondition gameCondition;
		public override void CompTick()
		{
			if (this.gameCondition == null && this.parent?.Map != null)
            {

				this.gameCondition = CreateConditionOn(this.parent.Map);
            }
		}

		protected GameCondition CreateConditionOn(Map map)
		{
			Log.Message("Creating " + ConditionDef);
			GameCondition gameCondition = GameConditionMaker.MakeCondition(ConditionDef);
			gameCondition.Permanent = true;
			gameCondition.conditionCauser = parent;
			map.gameConditionManager.RegisterCondition(gameCondition);
			SetupCondition(gameCondition, map);
			return gameCondition;
		}

		protected virtual void SetupCondition(GameCondition condition, Map map)
		{
			condition.suppressEndMessage = true;
		}

		public override void PostDestroy(DestroyMode mode, Map previousMap)
		{
			Messages.Message("MessageConditionCauserDespawned".Translate(parent.def.LabelCap), new TargetInfo(parent.Position, previousMap), MessageTypeDefOf.NeutralEvent);
			this.gameCondition.End();
		}

		public override string CompInspectStringExtra()
		{
			return base.CompInspectStringExtra();
		}

        public override void PostExposeData()
        {
            base.PostExposeData();
			Scribe_References.Look(ref gameCondition, "gameCondition");
        }
    }
}
