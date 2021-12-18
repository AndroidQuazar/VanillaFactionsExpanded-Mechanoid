using RimWorld;
using Verse;

namespace VFEMech
{
    [DefOf]
    public static class VFEMDefOf
    {
        public static FactionDef       VFE_Mechanoid;
        public static HediffDef        VFE_MechanoidUplink;
        public static ThingDef         VFE_TrooperStorage;
        public static JobDef           VFEM_Disassemble;
        public static TerrainDef       VFE_FactoryPath;
        public static HediffDef        VFE_FasterMovement;
        public static TerrainDef       VFE_FactoryFloor;
        public static ThingDef         VFE_ConduitPylon;
        public static ThingDef         VFE_ComponentMechanoid;
        public static ThingDef         VFE_LongRangeMissileLauncher;
        public static JobDef           VFEM_RefuelSilo;
        public static ThingDef         VFEM_MissileLeaving;
        public static ThingDef         VFEM_MissileIncoming;
        public static WorldObjectDef   VFEM_TravelingMissile;
        public static ThingSetMakerDef VFEMech_MechanoidStorageContent;
        public static LetterDef        VFEMech_AcceptVisitors;
        public static ThingDef         VFE_MechLandingBeacon;
        public static ThingDef         VFEM_HeavyHopper;

        public static SoundDef VFE_LongRangeMissile_ExplosionFar;
        public static SoundDef VFE_LongRangeMissile_LaunchSiren;
        public static SoundDef VFE_LongRangeMissile_Launch;
        public static SoundDef VFE_LongRangeMissile_Incoming;
        public static SoundDef VFE_LongRangeMissile_ExplosionOnMap;

        public static SitePartDef VFE_MechanoidAttackParty;
        public static SitePartDef VFE_MechanoidShipLanding;
        public static SitePartDef VFE_MechanoidStorage;

        public static JobDef VFE_Mechanoids_Recharge;
        public static NeedDef VFE_Mechanoids_Power;

        public static HediffDef VFE_BrainWashedNotFully;
        public static HediffDef VFE_BrainWashedFully;

        public static ThingDef VFE_Turret_AutoTesla;
        public static JobDef VFEM_EnterIndoctrinationPod;
    }
}
