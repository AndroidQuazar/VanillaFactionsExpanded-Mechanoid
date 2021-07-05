using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFEMech
{
    public class Mech_Inquisitor : VFEMechanoid
    {
        public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            base.PreApplyDamage(ref dinfo, out absorbed);
            if (dinfo.Weapon?.IsRangedWeapon ?? false && Rand.Chance(0.02f))
            {
                GenExplosion.DoExplosion(this.Position, this.Map, 5f, DamageDefOf.Bomb, dinfo.Instigator, 100);
                this.Kill(dinfo);
            }
        }
    }
}
