using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFEMech
{
    using RimWorld;
    using VFEMech;

    public class PawnGroupMakerMech : PawnGroupMaker
    {
        public int minimumPresence = 0;
        public int maximumPresence = int.MaxValue;

        public bool CanGenerate(PawnGroupMakerParms parms)
        {
            int mechPresence = MechUtils.MechPresence();
            return mechPresence > this.minimumPresence && mechPresence < this.maximumPresence;
        }
    }
}
