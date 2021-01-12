using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFEMech
{
    using RimWorld;
    using RimWorld.Planet;
    using Verse;
    using VFEM;

    public class VFEMWorldComponent : WorldComponent
    {
        private int lastMechShipSpawn;
        private int nextMechShipSpawn;

        public VFEMWorldComponent(World world) : base(world)
        {
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();

            if (!MechShipsMod.settings.totalWarIsDisabled && Find.TickManager.TicksGame >= this.nextMechShipSpawn)
            {
                this.lastMechShipSpawn = Find.TickManager.TicksGame;

                IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, this.world);

                if (MechShipsMod.settings.mechShipIncidentChances.TryRandomElementByWeight(kvp => kvp.Value, out KeyValuePair<string, float> incident))
                {
                    if (IncidentDef.Named(incident.Key).Worker.CanFireNow(parms))
                    {
                        IncidentDef.Named(incident.Key).Worker.TryExecute(parms);
                    }
                }
                this.nextMechShipSpawn = Find.TickManager.TicksGame + MechShipsMod.settings.mechShipTimeInterval.RandomInRange;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.lastMechShipSpawn,    "VFEM_" + nameof(this.lastMechShipSpawn));
            Scribe_Values.Look(ref this.nextMechShipSpawn, "VFEM_" + nameof(this.nextMechShipSpawn), 0);
        }
    }
}
