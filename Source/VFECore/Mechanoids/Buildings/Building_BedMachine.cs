using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFE.Mechanoids.Buildings
{
    public class Building_BedMachine : Building_Bed
    {
        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (def.Minifiable && base.Faction == Faction.OfPlayer)
            {
                yield return InstallationDesignatorDatabase.DesignatorFor(def);
            }
            if (AllComps.Count()>0)
            {
                for (int i = 0; i < AllComps.Count; i++)
                {
                    foreach (Gizmo item in AllComps[i].CompGetGizmosExtra())
                    {
                        yield return item;
                    }
                }
            }
        }
    }
}
