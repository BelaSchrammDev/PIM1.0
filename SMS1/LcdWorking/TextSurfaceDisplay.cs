using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        //SF
        public class TextSurfaceDisplay : TextRenderEngine
        {
            int imaxl = 22, iminl = 2;
            float xp;
            string bt = "";
            public IMyTextSurface ts = null;
            IMyTerminalBlock tb;
            public bool obs;
            public TextSurfaceDisplay(IMyTextSurface nts, IMyTerminalBlock p) { tsg = p.CubeGrid; ts = nts; sMM(); tb = p; bt = p.BlockDefinition.ToString(); sWF(); setLinesBounds(5); }
            public void sMM() { maxl = imaxl; minl = iminl; }
            void sC(float f, float w, int m, float ixp = 0) { fconst = f; wconst = w; imaxl = m; xp = ixp; }
            public void sWF()
            {
                var Y = ts.SurfaceSize.Y; var X = ts.SurfaceSize.X;
                if (bt.Contains("Med") && X == 512f && Y > 307f && Y < 308f) sC(10f, 88.5f, 8, 3);
                else if (bt.Contains("OpenCockpitLarge") && X == 512 && Y == 307.2f) sC(10.2f, 93.4f, 22, 1.5f);
                else if (X == 512 && Y == 307.2f) sC(10.4f, 94.4f, 22);
                else if (X == 512 && Y == 320f) sC(10.4f, 94.4f, 22);
                else if ((Y > 112f) && (Y < 114f)) sC(15.1f, 380.2f, 8);
                else if ((Y > 203f) && (Y < 206f)) sC(13.5f, 189f, 20);
                else if ((Y > 109f) && (Y < 110f)) sC(14.5f, 189f, 6);
                else if (Y == 256 && X == 256) sC(17.5f, 95f, 14);
                else if (Y == 512 && X == 512)
                {
                    if (bt.Contains("Projector")) sC(16f, 87f, 24, 4.5f);
                    else sC(16.5f, 90f, 14, 2.5f);
                }
                else if ((Y > 191f) && (Y < 193f)) sC(13f, 95f, 8);
                else if ((Y > 138f) && (Y < 140f)) sC(9.5f, 95f, 6);
                else if (Y > 170 && Y < 171) sC(11.7f, 94f, 12);
                else if (Y > 175.9 && Y < 178) sC(12f, 94f, 12);
                else if (Y > 182 && Y < 183) sC(12.2f, 94f, 12);
                else if (Y > 146 && Y < 147) sC(10f, 94f, 8);
                else if (Y > 199 && Y < 200) sC(13.5f, 94f, 6);
                else if (Y == 128f && X == 256) sC(17f, 189f, 8);
                else if (Y == 128f) sC(17f, 75f, 8);
                else if (Y == 153.6f) sC(10.5f, 94f, 10);
                else if (Y > 179 && Y < 180) sC(12.2f, 94f, 10);
                else if (Y == 256f && X == 204.8f) sC(17f, 75f, 6);
                else if ((Y > 84f) && (Y < 86f)) sC(11.5f, 96, 5);
            }
            public void Calc(P pm, int nts, string ncds)
            {
                sMM();
                obs = false;
                displayDefinition = ncds;
                if (displayDefinition == "") displayDefinition = "writex(write commands to LCDs 'CustomData' ,)\nwritex(for example 'batterys' or 'energy',)";
                setName((pm.Name.StartsWith("@dis") ? tb.CustomName : pm.Name) + " (" + ts.DisplayName + ")");
                if (pm.isPM("Maxlines")) maxl = getIntegerWithBounds(pm.getPM("Maxlines"), maxlines);
                if (pm.isPM("Minlines")) minl = getIntegerWithBounds(pm.getPM("Minlines"), maxlines);
                if (pm.isPM("Lines"))
                {
                    newlines = getIntegerWithBounds(pm.getPM("Lines"), maxlines, minl, maxl);
                    lfix = true;
                }
                else
                {
                    lfix = false;
                    newlines = maxlines;
                }
            }
            public override void writeToPanel() { w2ts(ts, (pages.Count > 0 ? pages[0] : ""), fontsize, xp); }
        }
    }
}
