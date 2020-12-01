using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Sound;

namespace VFEMech
{
	public class CompProperties_ActiveGameCondition_PsychicSuppression : CompProperties
	{
		public CompProperties_ActiveGameCondition_PsychicSuppression()
		{
			compClass = typeof(CompActiveGameCondition_PsychicSuppression);
		}

	}
	public class CompActiveGameCondition_PsychicSuppression : ThingComp
	{
		public GameCondition gameCondition;

		public Gender gender;
		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			gender = (Rand.Bool ? Gender.Male : Gender.Female);
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref gender, "gender", Gender.None);
			Scribe_References.Look(ref gameCondition, "gameCondition");
		}


		public override void CompTick()
		{
			base.CompTick();
			if (this.gameCondition == null && this.parent?.Map != null)
			{
				this.gameCondition = CreateConditionOn(this.parent.Map);
			}
		}
		public override string CompInspectStringExtra()
		{
			string text = base.CompInspectStringExtra();
			if (!text.NullOrEmpty())
			{
				text += "\n";
			}
			return text + ("AffectedGender".Translate() + ": " + gender.GetLabel().CapitalizeFirst());
		}
		protected GameCondition CreateConditionOn(Map map)
		{
			GameCondition gameCondition = GameConditionMaker.MakeCondition(GameConditionDefOf.PsychicSuppression);
			gameCondition.Permanent = true;
			gameCondition.conditionCauser = parent;
			map.gameConditionManager.RegisterCondition(gameCondition);
			SetupCondition(gameCondition, map);
			return gameCondition;
		}

		protected void SetupCondition(GameCondition condition, Map map)
		{
			((GameCondition_PsychicSuppression)condition).gender = gender;
		}

		public override void PostDestroy(DestroyMode mode, Map previousMap)
		{
			Messages.Message("MessageConditionCauserDespawned".Translate(parent.def.LabelCap), new TargetInfo(parent.Position, previousMap), MessageTypeDefOf.NeutralEvent);
			this.gameCondition.End();
		}
    }
}
