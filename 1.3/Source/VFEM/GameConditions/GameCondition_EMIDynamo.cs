using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFEMech
{
	public class GameCondition_EMIDynamo : GameCondition
	{
        public override bool ElectricityDisabled => true;
    }
}
