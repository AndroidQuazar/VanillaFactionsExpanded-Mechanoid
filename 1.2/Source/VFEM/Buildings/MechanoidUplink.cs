using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFEMech
{
    public class MapPawns
    {
        public MapPawns(List<Pawn> pawns)
        {
            this.pawns = pawns;
            this.lastTickCheck = Find.TickManager.TicksAbs;
        }
        public List<Pawn> pawns;
        public int lastTickCheck;
    }
    public class MechanoidUplink : MechShipPart
    {
        public float communicationRadius = 50f;
        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            GenDraw.DrawRadiusRing(Position, communicationRadius);
        }

        public static Dictionary<Map, MapPawns> mapPawns = new Dictionary<Map, MapPawns>();
        public static List<Pawn> GetAllPawns(Map map, Faction faction)
        {
            if (mapPawns.TryGetValue(map, out MapPawns mapPawns2)) 
            {
                if (Find.TickManager.TicksAbs > mapPawns2.lastTickCheck + 60)
                {
                    mapPawns2.pawns = map.mapPawns.AllPawns.Where(x => x.Faction == faction && x.kindDef.HasModExtension<UplinkCompatible>()).ToList();
                    mapPawns2.lastTickCheck = Find.TickManager.TicksAbs;
                }
                return mapPawns2.pawns;
            }
            else
            {
                var pawns = map.mapPawns.AllPawns.Where(x => x.Faction == faction && x.kindDef.HasModExtension<UplinkCompatible>()).ToList();
                mapPawns[map] = new MapPawns(pawns);
                return pawns;
            }
        }
        public override void Tick()
        {
            base.Tick();
            if (this.Spawned)
            {
                foreach (var pawn in GetAllPawns(this.Map, this.Faction))
                {
                    if (!pawn.Dead && pawn.Spawned && pawn.Position.DistanceTo(this.Position) <= communicationRadius)
                    {
                        var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(VFEMDefOf.VFE_MechanoidUplink);
                        if (hediff == null)
                        {
                            var mechUplinkHediff = HediffMaker.MakeHediff(VFEMDefOf.VFE_MechanoidUplink, pawn) as Hediff_MechanoidUplink;
                            mechUplinkHediff.mechanoidUplink = this;
                            pawn.health.AddHediff(mechUplinkHediff);
                        }
                    }
                }
            }
        }
    }
}
