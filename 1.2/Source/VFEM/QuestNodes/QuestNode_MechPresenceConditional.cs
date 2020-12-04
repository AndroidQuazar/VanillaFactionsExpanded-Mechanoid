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
	public class QuestNode_MechPresenceConditional : QuestNode
	{
		public SlateRef<float?> minMechPresence;

		public SlateRef<float?> maxMechPresence;

		public SlateRef<bool?> anyShips;
		protected override bool TestRunInt(Slate slate)
		{
			var mechPresenceValue = MechUtils.MechPresence();
			if (minMechPresence.GetValue(slate).HasValue && minMechPresence.GetValue(slate).Value < mechPresenceValue)
            {
				return false;
            }
			if (maxMechPresence.GetValue(slate).HasValue && maxMechPresence.GetValue(slate).Value > mechPresenceValue)
			{
				return false;
			}
			if (anyShips.GetValue(slate).HasValue)
            {
				if (anyShips.GetValue(slate).Value)
                {
					return mechPresenceValue > 0;
                }
				else
                {
					return !(mechPresenceValue > 0);
				}
			}
			return true;
		}

		protected override void RunInt()
		{
		}
	}
}