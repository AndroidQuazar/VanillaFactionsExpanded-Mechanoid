using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using VFECore;

namespace VFE.Mechanoids
{
    class CompMachine : CompDependsOnBuilding
    {
        public ThingDef turretAttached = null;
        public float turretAngle = 0f; //Purely cosmetic, don't need to save it
        public float turretAnglePerFrame = 0.1f;

        public override void OnBuildingDestroyed()
        {
            base.OnBuildingDestroyed();
            parent.Kill();
        }

        public new CompProperties_Machine Props
        {
            get
            {
                return (CompProperties_Machine)this.props;
            }
        }

        public void AttachTurret(ThingDef turret)
        {
            if(turretAttached!=null)
            {
                foreach(ThingDefCountClass stack in turretAttached.costList)
                {
                    Thing thing = ThingMaker.MakeThing(stack.thingDef);
                    thing.stackCount = stack.count;
                    GenPlace.TryPlaceThing(thing, parent.Position, parent.Map, ThingPlaceMode.Near);
                }
                ((Pawn)parent).equipment.DestroyAllEquipment();
            }
            turretAttached = turret;
            Thing turretThing = ThingMaker.MakeThing(turret.building.turretGunDef);
            ((Pawn)parent).equipment.AddEquipment((ThingWithComps)turretThing);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look<ThingDef>(ref turretAttached, "turretAttached");
        }

        public override void CompTick()
        {
            base.CompTick();
            if(turretAttached!=null)
            {
                turretAngle += turretAnglePerFrame;
            }
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            turretAnglePerFrame = Rand.Range(-0.5f, 0.5f);
        }
    }
}
