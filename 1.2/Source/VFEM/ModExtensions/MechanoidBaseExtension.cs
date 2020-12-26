using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFEMech
{
    using UnityEngine.Animations;
    using Verse;

    public class MechanoidBaseExtension : DefModExtension
    {
        public int raisesPresence;
        public RulePackDef nameMaker;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string configError in base.ConfigErrors())
                yield return configError;
            
            if (this.raisesPresence <= 0)
                yield return "No presence raised";

            if (this.nameMaker is null)
                yield return "No namemaker configured";
        }
    }
}