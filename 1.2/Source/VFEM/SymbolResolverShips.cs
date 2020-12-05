using KCSG;
using RimWorld;
using RimWorld.BaseGen;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace VFEM
{
    internal class SymbolResolverShips : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;
            Faction faction = rp.faction ?? Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Undefined);
            SettlementLayoutDef lDef = FactionSettlement.temp;

            List<CellRect> gridRects = KCSG_Utilities.GetRects(rp.rect, lDef, map, out rp.rect);
            FactionSettlement.tempRectList = gridRects;

            foreach (IntVec3 c in rp.rect)
            {
                if (map.fogGrid.IsFogged(c)) map.fogGrid.Unfog(c);
            }

            Lord singlePawnLord = rp.singlePawnLord ?? LordMaker.MakeNewLord(faction, new VFEM.LordJob_VFEMDefendBase(faction, rp.rect.CenterCell), map, null);
            TraverseParms traverseParms = TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false);
            ResolveParams resolveParams = rp;
            resolveParams.rect = rp.rect;
            resolveParams.faction = faction;
            resolveParams.singlePawnLord = singlePawnLord;
            resolveParams.pawnGroupKindDef = (rp.pawnGroupKindDef ?? PawnGroupKindDefOf.Settlement);
            resolveParams.singlePawnSpawnCellExtraPredicate = (rp.singlePawnSpawnCellExtraPredicate ?? ((IntVec3 x) => map.reachability.CanReachMapEdge(x, traverseParms)));
            if (resolveParams.pawnGroupMakerParams == null && faction.def.pawnGroupMakers.Any(pgm => pgm.kindDef == PawnGroupKindDefOf.Settlement))
            {
                resolveParams.pawnGroupMakerParams = new PawnGroupMakerParms();
                resolveParams.pawnGroupMakerParams.tile = map.Tile;
                resolveParams.pawnGroupMakerParams.faction = faction;
                resolveParams.pawnGroupMakerParams.points = (rp.settlementPawnGroupPoints ?? SymbolResolver_Settlement.DefaultPawnsPoints.RandomInRange);
                resolveParams.pawnGroupMakerParams.inhabitants = true;
                resolveParams.pawnGroupMakerParams.seed = rp.settlementPawnGroupSeed;
            }
            if (faction.def.pawnGroupMakers.Any(pgm => pgm.kindDef == PawnGroupKindDefOf.Settlement)) BaseGen.symbolStack.Push("pawnGroup", resolveParams, null);

            ResolveParams rp2 = rp;
            rp2.faction = faction;
            BaseGen.symbolStack.Push("kcsg_roomsgen", rp2, null);

            if (lDef.clearEverything)
            {
                foreach (IntVec3 c in rp.rect)
                {
                    c.GetThingList(map).ToList().FindAll(tt => tt.def.passability == Traversability.Impassable).ForEach((t) => t.DeSpawn());    
                    map.roofGrid.SetRoof(c, null);   
                }
                map.roofGrid.RoofGridUpdate();    
            }
            else
            {
                foreach (IntVec3 c in rp.rect)
                {
                    c.GetThingList(map).ToList().FindAll(t1 => t1.def.category == ThingCategory.Filth || t1.def.category == ThingCategory.Item).ForEach((t) => t.DeSpawn());
                }
            }
        }
    }
}