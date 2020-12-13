using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using VFE.Mechanoids.Buildings;
using VFE.Mechanoids.Needs;
using VFECore;

namespace VFE.Mechanoids
{
    public class CompMachineChargingStation : CompPawnDependsOn
    {
        public bool wantsRespawn=false; //Used to determine whether a rebuild job is desired
        public bool wantsRest = false; //Used to force a machine to return to base, for healing or recharging
        public ThingDef turretToInstall = null; //Used to specify a turret to put on the mobile turret
        public Area allowedArea = null;

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
            if(myPawn.story==null)
                myPawn.story = new Pawn_StoryTracker(myPawn);
            if(myPawn.skills==null)
                myPawn.skills = new Pawn_SkillTracker(myPawn);
            if(myPawn.workSettings==null)
                myPawn.workSettings = new Pawn_WorkSettings(myPawn);
            if(myPawn.relations==null)
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
                if(myPawn.drafter==null)
                    myPawn.drafter = new Pawn_DraftController(myPawn);
                if(Props.spawnWithWeapon!=null)
                {
                    ThingWithComps thing = (ThingWithComps)ThingMaker.MakeThing(Props.spawnWithWeapon);
                    myPawn.equipment.AddEquipment(thing);
                }
            }
            if(myPawn.needs.TryGetNeed<Need_Power>()==null)
                typeof(Pawn_NeedsTracker).GetMethod("AddNeed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(myPawn.needs, new object[] { DefDatabase<NeedDef>.GetNamed("VFE_Mechanoids_Power") });
            myPawn.playerSettings.AreaRestriction = allowedArea;
            wantsRespawn = false;
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            Building_BedMachine bed = (Building_BedMachine)parent;
            if(bed.occupant!=null)
            {
                parent.TryGetComp<CompPowerTrader>().powerOutputInt = 0 - parent.TryGetComp<CompPowerTrader>().Props.basePowerConsumption - Props.extraChargingPower;
                if (myPawn.health.hediffSet.HasNaturallyHealingInjury() && bed.TryGetComp<CompPowerTrader>().PowerOn)
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
            Scribe_References.Look<Area>(ref allowedArea, "allowedArea");
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
                    icon = ContentFinder<Texture2D>.Get("UI/AttachTurret"),
                    action = delegate {
                        List<FloatMenuOption> options = new List<FloatMenuOption>();
                        foreach (var b in Props.blackListTurretGuns)
                        {
                            Log.Message("Blacklist: " + b);
                        }
                        foreach(ThingDef thing in DefDatabase<ThingDef>.AllDefs.Where(t=>
                        !Props.blackListTurretGuns.Contains(t.defName)
                        &&t.building!=null
                        &&t.building.turretGunDef!=null
                        &&t.costList!=null
                        &&t.GetCompProperties<CompProperties_Mannable>()==null
                        &&t.size.x<=3
                        &&t.size.z<=3
                        
                        ))
                        {
                            FloatMenuOption opt = new FloatMenuOption(thing.label, delegate
                            {
                                turretToInstall = thing;
                                wantsRest = true;
                            },thing.building.turretGunDef);
                            options.Add(opt);
                        }
                        Find.WindowStack.Add(new FloatMenu(options));
                    }
                };
                gizmos.Add(attachTurret);
            }

            Command_Action setArea = new Command_Action
            {
                defaultLabel = "VFEMechSetArea".Translate(),
                defaultDesc = "VFEMechSetAreaDesc".Translate(),
                action = delegate
                {
                    AreaUtility.MakeAllowedAreaListFloatMenu(delegate (Area area)
                    {
                        this.allowedArea = area;
                        if (myPawn != null && myPawn.Spawned && !myPawn.Dead)
                            myPawn.playerSettings.AreaRestriction = area;
                    }, true, true, parent.Map);
                },
                icon = ContentFinder<Texture2D>.Get("UI/SelectZone")
            };
            gizmos.Add(setArea);

            return gizmos;
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder builder = new StringBuilder(base.CompInspectStringExtra());
            if(myPawn==null || myPawn.Dead || !myPawn.Spawned)
            {
                bool comma = false;
                string resources = "VFEMechReconstruct".Translate()+" ";
                foreach(ThingDefCountClass resource in Props.pawnToSpawn.race.butcherProducts)
                {
                    if (comma)
                        resources += ", ";
                    comma = true;
                    resources += resource.thingDef.label + " x" + resource.count;
                }
                builder.AppendLine(resources);
            }
            if(turretToInstall!=null)
            {
                bool comma = false;
                string resources = "VFEMechTurretResources".Translate()+" ";
                foreach (ThingDefCountClass resource in turretToInstall.costList)
                {
                    if (comma)
                        resources += ", ";
                    comma = true;
                    resources += resource.thingDef.label + " x" + resource.count;
                }
                builder.AppendLine(resources);
            }
            return builder.ToString().Trim();
        }
    }
}
