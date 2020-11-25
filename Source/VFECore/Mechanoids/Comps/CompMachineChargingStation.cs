using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using VFE.Mechanoids.Buildings;
using VFECore;

namespace VFE.Mechanoids
{
    public class CompMachineChargingStation : CompPawnDependsOn
    {
        public bool wantsRespawn=false; //Used to determine whether a rebuild job is desired
        public bool wantsRest = false; //Used to force a machine to return to base, for healing or upgrading
        public ThingDef turretToInstall = null; //Used to specify a turret to put on the mobile turret

        public new CompProperties_MachineChargingStation Props
        {
            get
            {
                return (CompProperties_MachineChargingStation)this.props;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
                SpawnMyPawn();
            else
                CheckWantsRespawn();
        }

        public override void SpawnMyPawn()
        {
            base.SpawnMyPawn();
            myPawn.story = new Pawn_StoryTracker(myPawn);
            myPawn.skills = new Pawn_SkillTracker(myPawn);
            myPawn.workSettings = new Pawn_WorkSettings(myPawn);
            myPawn.relations = new Pawn_RelationsTracker(myPawn);
            DefMap<WorkTypeDef,int> priorities = new DefMap<WorkTypeDef, int>();
            priorities.SetAll(0);
            typeof(Pawn_WorkSettings).GetField("priorities", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(myPawn.workSettings,priorities);
            foreach (WorkTypeDef workType in Props.allowedWorkTypes)
            {
                foreach (SkillDef skill in workType.relevantSkills)
                {
                    SkillRecord record = myPawn.skills.skills.Find(rec => rec.def == skill);
                    record.levelInt = Props.skillLevel;
                }
                myPawn.workSettings.SetPriority(workType, 1);
            }
            if(myPawn.TryGetComp<CompMachine>().Props.violent)
            {
                myPawn.drafter = new Pawn_DraftController(myPawn);
                if(Props.spawnWithWeapon!=null)
                {
                    ThingWithComps thing = (ThingWithComps)ThingMaker.MakeThing(Props.spawnWithWeapon);
                    myPawn.equipment.AddEquipment(thing);
                }
            }
            wantsRespawn = false;
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            Building_BedMachine bed = (Building_BedMachine)parent;
            if(bed.GetCurOccupant(0)!=null)
            {
                parent.TryGetComp<CompPowerTrader>().powerOutputInt = 0 - parent.TryGetComp<CompPowerTrader>().Props.basePowerConsumption - Props.extraChargingPower;
                if (myPawn.health.hediffSet.HasNaturallyHealingInjury())
                {
                    float num3 = 12f;
                (from x in myPawn.health.hediffSet.GetHediffs<Hediff_Injury>()
                 where x.CanHealNaturally()
                 select x).RandomElement().Heal(num3 * myPawn.HealthScale * 0.01f);
                }
            }
            else
            {
                parent.TryGetComp<CompPowerTrader>().powerOutputInt = 0 - parent.TryGetComp<CompPowerTrader>().Props.basePowerConsumption;
            }
            CheckWantsRespawn();
        }

        void CheckWantsRespawn()
        {
            if (myPawn == null || !myPawn.Spawned || myPawn.Dead)
                wantsRespawn = true;
            else
                wantsRespawn = false;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref wantsRest, "wantsRest");
            Scribe_Defs.Look<ThingDef>(ref turretToInstall, "turretToInstall");
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            List<Gizmo> gizmos = new List<Gizmo>();
            gizmos.AddRange(base.CompGetGizmosExtra());

            Command_Toggle forceRest = new Command_Toggle
            {
                defaultLabel = "VFEMechForceRecharge".Translate(),
                defaultDesc = "VFEMechForceRechargeDesc".Translate(),
                toggleAction = delegate { wantsRest = !wantsRest; turretToInstall = null; }
            };
            forceRest.isActive = delegate { return wantsRest; };
            gizmos.Add(forceRest);

            if (Props.turret)
            {
                Command_Action attachTurret = new Command_Action
                {
                    defaultLabel = "VFEMechAttachTurret".Translate(),
                    defaultDesc = "VFEMechAttachTurretDesc".Translate(),
                    action = delegate {
                        List<FloatMenuOption> options = new List<FloatMenuOption>();
                        foreach(ThingDef thing in DefDatabase<ThingDef>.AllDefs.Where(t=>t.building!=null&&t.building.turretGunDef!=null&&t.costList!=null))
                        {
                            FloatMenuOption opt = new FloatMenuOption(thing.label, delegate
                            {
                                turretToInstall = thing;
                                wantsRest = true;
                            });
                            options.Add(opt);
                        }
                        Find.WindowStack.Add(new FloatMenu(options));
                    }
                };
                gizmos.Add(attachTurret);
            }

            return gizmos;
        }
    }
}
