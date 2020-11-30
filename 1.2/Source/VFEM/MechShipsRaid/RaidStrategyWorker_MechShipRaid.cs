using KCSG;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;
using VFEMech;

namespace VFEM
{
    class RaidStrategyWorker_MechShipRaid : RaidStrategyWorker_Siege
    {
        public struct TTIR
        {
            public ThingDef faller;
            public Thing toSpawn;
            public IntVec3 cell;
            public Rot4 rot;
        }

        public override bool CanUseWith(IncidentParms parms, PawnGroupKindDef groupKind)
        {
            return parms.points >= this.MinimumPoints(parms.faction, groupKind) && parms.faction.def.defName == "VFE_Mechanoid" && MechUtils.MechPresence() > 0 && this.FindRect((Map)parms.target) != IntVec3.Invalid;
        }

        public override List<Pawn> SpawnThreats(IncidentParms parms)
        {
            StructureLayoutDef structureLayoutDef;
            if (ModLister.RoyaltyInstalled) structureLayoutDef = this.def.GetModExtension<DefModExtension_ShipsSpawnable>().structureLayouts.RandomElement();
            else structureLayoutDef = this.def.GetModExtension<DefModExtension_ShipsSpawnable>().structureLayouts.FindAll(s => !s.defName.Contains("DLC")).RandomElement();

            KCSG_Utilities.HeightWidthFromLayout(structureLayoutDef, out int h, out int w);
            CellRect cellRect = CellRect.CenteredOn(parms.spawnCenter, w, h);

            // SHIP GENERATION
            List<string> allSymbList = new List<string>();
            Map map = parms.target as Map;

            foreach (string str in structureLayoutDef.layouts[0])
            {
                List<string> symbSplitFromLine = str.Split(',').ToList();
                symbSplitFromLine.ForEach((s) => allSymbList.Add(s));
            }

            Dictionary<string, SymbolDef> pairsSymbolLabel = KCSG_Utilities.FillpairsSymbolLabel();
            List<TTIR> fallers = new List<TTIR>();
            Dictionary<ActiveDropPodInfo, IntVec3> pods = new Dictionary<ActiveDropPodInfo, IntVec3>();

            int l = 0;
            foreach (IntVec3 cell in cellRect.Cells)
            {
                if (l < allSymbList.Count && allSymbList[l] != ".")
                {
                    SymbolDef temp;
                    pairsSymbolLabel.TryGetValue(allSymbList[l], out temp);
                    Thing thing;

                    if (temp.thingDef != null && temp.defName.Contains("VFE"))
                    {
                        TTIR ttir = new TTIR();

                        thing = ThingMaker.MakeThing(temp.thingDef, temp.stuffDef);
                        thing.SetFactionDirect(parms.faction);

                        if (thing.def.rotatable && thing.def.category == ThingCategory.Building)
                        {
                            ttir.rot = new Rot4(temp.rotation.AsInt);
                        }

                        ThingDef faller = new ThingDef();
                        faller.category = ThingCategory.Ethereal;
                        faller.useHitPoints = false;

                        faller.thingClass = typeof(VFEM_Skyfaller);
                        faller.useHitPoints = false;
                        faller.drawOffscreen = true;
                        faller.tickerType = TickerType.Normal;
                        faller.altitudeLayer = AltitudeLayer.Skyfaller;
                        faller.drawerType = DrawerType.RealtimeOnly;
                        faller.skyfaller = new SkyfallerProperties();

                        faller.defName = temp.thingDef.defName;
                        string label = structureLayoutDef.defName.Remove(0, 5).Replace("DLC", "");
                        label.Remove(label.Length-1, 1);
                        faller.label = label + " (incoming)";
                        faller.size = new IntVec2(thing.def.size.x, thing.def.size.z);
                        faller.skyfaller.shadowSize = new UnityEngine.Vector2((float)(thing.def.size.x + 1), (float)(thing.def.size.z + 1));
                        faller.skyfaller.ticksToImpactRange = new IntRange(250, 250);

                        ttir.faller = faller;
                        ttir.toSpawn = thing;
                        ttir.cell = cell;

                        fallers.Add(ttir);
                    }
                    else if (temp.thingDef != null)
                    {
                        thing = ThingMaker.MakeThing(temp.thingDef, temp.stuffDef);
                        thing.SetFactionDirect(parms.faction);

                        ActiveDropPodInfo activeDropPodInfo = new ActiveDropPodInfo();
                        activeDropPodInfo.innerContainer.TryAdd(thing);
                        activeDropPodInfo.openDelay = 40;
                        activeDropPodInfo.leaveSlag = false;
                        pods.Add(activeDropPodInfo, cell);
                    }
                }
                l++;
            }
            // ARRIVAL
            fallers.ForEach(ttir => this.VFEM_SpawnSkyfaller(ttir.faller, ttir.toSpawn, ttir.cell, map, ttir.rot));
            for (int i = 0; i < pods.Count; i++)
            {
                DropPodUtility.MakeDropPodAt(pods.ElementAt(i).Value, map, pods.ElementAt(i).Key);
            }
            base.SpawnThreats(parms);
            return null;
        }

        private VFEM_Skyfaller VFEM_SpawnSkyfaller(ThingDef skyfaller, Thing innerThing, IntVec3 pos, Map map, Rot4 rot)
        {
            VFEM_Skyfaller skyfaller2 = (VFEM_Skyfaller)SkyfallerMaker.MakeSkyfaller(skyfaller);
            if (innerThing != null && !skyfaller2.innerContainer.TryAdd(innerThing, true))
            {
                Log.Error("Could not add " + innerThing.ToStringSafe<Thing>() + " to a skyfaller.", false);
                innerThing.Destroy(DestroyMode.Vanish);
            }
            skyfaller2.rot = rot;

            return (VFEM_Skyfaller)GenSpawn.Spawn(skyfaller2, pos, map, rot, WipeMode.Vanish);
        }

        protected override LordJob MakeLordJob(IncidentParms parms, Map map, List<Pawn> pawns, int raidSeed)
        {
            IntVec3 originCell = parms.spawnCenter.IsValid ? parms.spawnCenter : pawns[0].PositionHeld;
            if (parms.faction.HostileTo(Faction.OfPlayer))
            {
                return new LordJob_AssaultColony(parms.faction, true, true, false, false, true);
            }
            IntVec3 fallbackLocation;
            RCellFinder.TryFindRandomSpotJustOutsideColony(originCell, map, out fallbackLocation);
            return new LordJob_AssistColony(parms.faction, fallbackLocation);
        }

        public IntVec3 FindRect(Map map)
        {
            CellRect rect;
            bool shre = true;
            while (shre)
            {
                rect = CellRect.CenteredOn(CellFinder.RandomNotEdgeCell(33, map), 33, 33);
                if (rect.Cells.ToList().Any(i => !i.Walkable(map))) { }
                else return rect.CenterCell;
            }
            return IntVec3.Invalid;
        }
    }
}
