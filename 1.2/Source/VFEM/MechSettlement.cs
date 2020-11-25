using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFEMech
{
    using RimWorld;
    using RimWorld.Planet;
    using UnityEngine;
    using Verse;

    public class MechSettlement : Settlement
    {
        public MechSettlement()
        {
            this.previouslyGeneratedInhabitants = new List<Pawn>();
            this.trader = null;
        }


        public override Material Material => 
            this.def.Material;

        public override Texture2D ExpandingIcon => 
            this.def.ExpandingIconTexture;

        public override string GetInspectString()
        {
            string text = base.GetInspectString();
            if (!text.NullOrEmpty())
            {
                text += "\n";
            }

            text += this.def.description;

            return text;
        }
    }
}
