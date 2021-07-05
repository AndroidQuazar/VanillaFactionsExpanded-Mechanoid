using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFEM.HarmonyPatches
{
    using HarmonyLib;
    using Verse;
    using Verse.AI;

    [HarmonyPatch(typeof(GenRadial), "NumCellsInRadius")]
    public class Explosion_Patch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction codeInstruction in instructions)
            {
                if (codeInstruction.OperandIs(10000f))
                {
                    codeInstruction.operand = GenRadial.RadialPattern.Length;
                }

                yield return codeInstruction;
            }
        }
    }
}
