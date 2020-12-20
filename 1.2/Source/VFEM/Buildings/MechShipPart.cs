using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;

namespace VFEMech
{
    public class MechShipPart : Building
    {
        public override bool ClaimableBy(Faction by)
        {
            return false;
        }
        public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            base.PreApplyDamage(ref dinfo, out absorbed);
            if (this.def != VFEMDefOf.VFE_TrooperStorage && this.Map != null)
            {
                var trooperStorages = this.Map.listerThings.ThingsOfDef(VFEMDefOf.VFE_TrooperStorage).Cast<TrooperStorage>().Where(x => x.CanSpawnTroopers);
                if (trooperStorages.Count() > 0)
                {
                    var trooperStorage = trooperStorages.OrderBy(y => y.Position.DistanceTo(this.Position)).First();
                    trooperStorage.ReleaseTroopers();
                }
            }
        }
    }
}
