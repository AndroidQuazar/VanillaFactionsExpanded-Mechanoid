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
	public class CompProperties_ActiveGameCondition_PsychicEmanation : CompProperties
	{
		public CompProperties_ActiveGameCondition_PsychicEmanation()
		{
			compClass = typeof(CompActiveGameCondition_PsychicEmanation);
		}

		public PsychicDroneLevel droneLevel = PsychicDroneLevel.BadMedium;

		public int droneLevelIncreaseInterval = int.MinValue;
	}
	public class CompActiveGameCondition_PsychicEmanation : ThingComp
	{
		public CompProperties_ActiveGameCondition_PsychicEmanation Props => (CompProperties_ActiveGameCondition_PsychicEmanation)props;

		public GameCondition gameCondition;

		public Gender gender;

		private int ticksToIncreaseDroneLevel;

		private PsychicDroneLevel droneLevel = PsychicDroneLevel.BadHigh;
		public PsychicDroneLevel Level => droneLevel;

		private bool DroneLevelIncreases => Props.droneLevelIncreaseInterval != int.MinValue;

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			gender = (Rand.Bool ? Gender.Male : Gender.Female);
			droneLevel = PsychicDroneLevel.BadMedium;
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			if (!respawningAfterLoad && DroneLevelIncreases)
			{
				ticksToIncreaseDroneLevel = Props.droneLevelIncreaseInterval;
				SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera(parent.Map);
			}
		}

		public override void CompTick()
		{
			base.CompTick();
			if (this.gameCondition == null)
			{
				this.gameCondition = CreateConditionOn(this.parent.Map);
			}
			if (parent.Spawned && DroneLevelIncreases)
			{
				ticksToIncreaseDroneLevel--;
				if (ticksToIncreaseDroneLevel <= 0)
				{
					IncreaseDroneLevel();
					ticksToIncreaseDroneLevel = Props.droneLevelIncreaseInterval;
				}
			}
		}

		private void IncreaseDroneLevel()
		{
			if (droneLevel != PsychicDroneLevel.BadExtreme)
			{
				droneLevel++;
				TaggedString taggedString = "LetterPsychicDroneLevelIncreased".Translate(gender.GetLabel());
				Find.LetterStack.ReceiveLetter("LetterLabelPsychicDroneLevelIncreased".Translate(), taggedString, LetterDefOf.NegativeEvent);
				SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera(parent.Map);
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref gender, "gender", Gender.None);
			Scribe_Values.Look(ref ticksToIncreaseDroneLevel, "ticksToIncreaseDroneLevel", 0);
			Scribe_Values.Look(ref droneLevel, "droneLevel", PsychicDroneLevel.None);
			Scribe_References.Look(ref gameCondition, "gameCondition");
		}

		public override string CompInspectStringExtra()
		{
			string text = base.CompInspectStringExtra();
			if (!text.NullOrEmpty())
			{
				text += "\n";
			}
			return text + ("AffectedGender".Translate() + ": " + gender.GetLabel().CapitalizeFirst() + "\n" + "PsychicDroneLevel".Translate(droneLevel.GetLabelCap()));
		}

		protected GameCondition CreateConditionOn(Map map)
		{
			GameCondition gameCondition = GameConditionMaker.MakeCondition(GameConditionDefOf.PsychicDrone);
			gameCondition.Permanent = true;
			gameCondition.conditionCauser = parent;
			map.gameConditionManager.RegisterCondition(gameCondition);
			SetupCondition(gameCondition, map);
			return gameCondition;
		}

		protected void SetupCondition(GameCondition condition, Map map)
		{
			GameCondition_PsychicEmanation obj = (GameCondition_PsychicEmanation)condition;
			obj.gender = gender;
			obj.level = Level;
			condition.suppressEndMessage = true;
		}

		public override void PostDestroy(DestroyMode mode, Map previousMap)
		{
			Messages.Message("MessageConditionCauserDespawned".Translate(parent.def.LabelCap), new TargetInfo(parent.Position, previousMap), MessageTypeDefOf.NeutralEvent);
			this.gameCondition.End();
		}
    }
}
