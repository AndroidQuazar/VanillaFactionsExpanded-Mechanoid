using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace VFEMech
{
    [HarmonyPatch(typeof(SignalManager), "SendSignal")]
    internal static class Patch_SendSignal
    {
        private static void Postfix(Signal signal)
        {
            Log.Message("signal: " + signal.tag);
        }
    }

    [HarmonyPatch(typeof(Faction), "TryAffectGoodwillWith")]
    internal static class Patch_TryAffectGoodwillWith
    {
        private static void Postfix(Faction other, int goodwillChange, bool canSendMessage = true, bool canSendHostilityLetter = true, string reason = null, GlobalTargetInfo? lookTarget = null)
        {
            Log.Message("TEST: " + other);
        }
    }

    [HarmonyPatch(typeof(TransportPodsArrivalAction_VisitSite), "Arrived")]
    internal static class Patch_Arrived
    {
        private static bool Prefix(Site ___site, PawnsArrivalModeDef ___arrivalMode, List<ActiveDropPodInfo> pods, int tile)
        {
            if (___site.parts != null)
            {
                foreach (var part in ___site.parts)
                {
                    if (part.def == VFEMDefOf.VFE_MechanoidAttackParty)
                    {
                        Thing lookTarget = TransportPodsArrivalActionUtility.GetLookTarget(pods);
                        bool num = !___site.HasMap;
                        Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(___site.Tile, CaravanArrivalAction_VisitSite.MapSize, null);
                        if (num)
                        {
                            Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
                            PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(orGenerateMap.mapPawns.AllPawns, "LetterRelatedPawnsInMapWherePlayerLanded".Translate(Faction.OfPlayer.def.pawnsPlural), LetterDefOf.NeutralEvent, informEvenIfSeenBefore: true);
                        }
                        Messages.Message("MessageTransportPodsArrived".Translate(), lookTarget, MessageTypeDefOf.TaskCompletion);
                        ___arrivalMode.Worker.TravelingTransportPodsArrived(pods, orGenerateMap);
                        return false;
                    }
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(SettlementUtility), "AffectRelationsOnAttacked_NewTmp")]
    internal static class Patch_AffectRelationsOnAttacked_NewTmp
    {
        private static bool Prefix(MapParent mapParent, ref TaggedString letterText)
        {
            Log.Message("AffectRelationsOnAttacked_NewTmp");
            if (mapParent is Site site && site.parts != null)
            {
                foreach (var part in site.parts)
                {
                    if (part.def == VFEMDefOf.VFE_MechanoidAttackParty)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
