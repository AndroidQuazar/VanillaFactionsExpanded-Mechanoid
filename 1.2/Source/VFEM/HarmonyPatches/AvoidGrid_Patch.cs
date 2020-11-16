using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace VFEMech
{
	[HarmonyPatch(typeof(CastPositionFinder), "CastPositionPreference")]
	internal static class AvoidGrid_Patch
	{
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            FieldInfo avoidCoverInfo = AccessTools.Field(typeof(PawnKindDef), nameof(PawnKindDef.aiAvoidCover));

            bool done = false;

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];
                yield return instruction;

                if (!done && instructionList[i + 2].OperandIs(avoidCoverInfo))
                {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AvoidGrid_Patch), nameof(AvoidCover)));
                    i += 2;
                    done = true;
                }
            }
        }

        public static bool AvoidCover(Pawn pawn)
        {
            if (pawn.Faction?.def == VFEMDefOf.VFE_Mechanoid && (pawn.health?.hediffSet?.HasHediff(VFEMDefOf.VFE_MechanoidUplink) ?? false))
            {
                Log.Message(pawn + " should use covers");
                return false;
            }
            return pawn.kindDef.aiAvoidCover;
        }
    }
}
