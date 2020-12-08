using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFE.Mechanoids.Buildings
{
    public class Building_BedMachine : Building
    {
        public Pawn occupant
        {
            get
            {
                if(this.TryGetComp<CompMachineChargingStation>()?.myPawn?.Position==this.Position)
                {
                    return this.TryGetComp<CompMachineChargingStation>().myPawn;
                }
                return null;
            }
        }
    }
}
