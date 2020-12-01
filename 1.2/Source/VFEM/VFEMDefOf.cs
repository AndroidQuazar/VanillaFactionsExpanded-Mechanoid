using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFEMech
{
    [DefOf]
    public static class VFEMDefOf
    {
        public static FactionDef     VFE_Mechanoid;
        public static HediffDef      VFE_MechanoidUplink;
        public static ThingDef       VFE_TrooperStorage;
        public static JobDef         VFEM_Disassemble;
        public static TerrainDef     VFE_FactoryPath;
        public static HediffDef      VFE_FasterMovement;
        public static TerrainDef     VFE_FactoryFloor;
        public static ThingDef       VFE_ConduitPylon;
        public static ThingDef       VFE_ComponentMechanoid;
        public static ThingDef       VFEM_LongMote_DustPuff;
        public static ThingDef       VFE_LongRangeMissileLauncher;
        public static JobDef         VFEM_RefuelSilo;
        public static ThingDef       VFEM_MissileLeaving;
        public static ThingDef       VFEM_MissileIncoming;
        public static WorldObjectDef VFEM_TravelingMissile;
    }
}
