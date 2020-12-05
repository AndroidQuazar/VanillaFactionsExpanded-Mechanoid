using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace VFE.Mechanoids.HarmonyPatches
{
    [HarmonyPatch(typeof(PawnRenderer), "DrawEquipmentAiming")]
    public static class RenderTurretCentered
    {
        static bool replaced = false;
        static FieldInfo pawnField = typeof(PawnRenderer).GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);
        static Pawn pawn;
        static Mesh plane20Flip = MeshMakerPlanes.NewPlaneMesh(2f, flipped: true);

        public static bool Prefix(PawnRenderer __instance)
        {
            pawn = (Pawn)pawnField.GetValue(__instance);
            if (pawn.TryGetComp<CompMachine>()?.turretAttached != null)
                replaced = true;
            else
                replaced = false;
            return !replaced;
        }

        public static void Postfix(PawnRenderer __instance, Thing eq, Vector3 drawLoc, float aimAngle)
        {
            if(replaced)
            {
                if (pawn.Rotation == Rot4.South)
                    drawLoc -= new Vector3(0, 0, -0.33f);
                else if (pawn.Rotation == Rot4.North)
                    drawLoc -= new Vector3(0, -1, -0.22f);
                else if (pawn.Rotation == Rot4.East)
                    drawLoc -= new Vector3(0.2f, 0f, -0.22f);
                else if (pawn.Rotation == Rot4.West)
                    drawLoc -= new Vector3(-0.2f, 0, -0.22f);

                Mesh mesh = null;
                float num = aimAngle - 90f;
                if (aimAngle > 20f && aimAngle < 160f)
                {
                    mesh = MeshPool.plane20;
                    num += eq.def.equippedAngleOffset;
                }
                else if (aimAngle > 200f && aimAngle < 340f)
                {
                    mesh = plane20Flip;
                    num -= 180f;
                    num -= eq.def.equippedAngleOffset;
                }
                else
                {
                    mesh = MeshPool.plane20;
                    num += eq.def.equippedAngleOffset;
                }
                num %= 360f;
                Graphic_StackCount graphic_StackCount = eq.Graphic as Graphic_StackCount;
                Graphics.DrawMesh(material: (graphic_StackCount == null) ? eq.Graphic.MatSingle : graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingle, mesh: mesh, position: drawLoc, rotation: Quaternion.AngleAxis(num, Vector3.up), layer: 0);
            }
            replaced = false;
        }
    }
}
