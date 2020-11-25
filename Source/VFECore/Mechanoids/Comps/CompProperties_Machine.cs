using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFE.Mechanoids
{
    public class CompProperties_Machine : CompProperties
    {
        public bool violent = false;

        public CompProperties_Machine()
        {
            this.compClass = typeof(CompMachine);
        }
    }
}
