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
        public enum MDItype { Line, Space, String, Bar };
        //Box
        public class MDIbox
        {
            public bool fuell;
            public MDI mdio;
            public string btext = "";
            public float mdi_w;
            BoxAlign ba = BoxAlign.Right;
            enum BoxAlign { Left, Right };
            public MDIbox(float gw, string fk, Dictionary<string, MDI> ml)
            {
                var ksl = fk.Trim().Split('(', ')');
                var rk = ksl[0].Trim();
                var RK = rk.ToUpper();
                if (rk == "") mdio = new MDIspace();
                else if (rk[0] == '@')
                {
                    if (rk.Length == 2) mdio = new MDIline(fk[1]);
                    else if (ml.ContainsKey(rk)) mdio = ml[rk];
                    else if (ml.ContainsKey(RK)) mdio = ml[RK];
                    else if (allMDIs.ContainsKey(rk)) mdio = allMDIs[rk];
                    else if (allMDIs.ContainsKey(RK)) mdio = allMDIs[RK];
                    else mdio = new MDIs("?" + ksl[0] + "?");
                }
                else mdio = new MDIs(ksl[0]);
                mdi_w = mdio.defw;
                if (ksl.Length > 1)
                    foreach (var fs in ksl[1].Split(' '))
                        if (fs != "" && fs[0] != '@')
                        {
                            if (fs[fs.Length - 1] == '%')
                            {
                                mdi_w = (int)((gw * getIntegerWithBounds(fs.Split('%')[0], 20, 1, 100)) / 100);
                            }
                            else
                                switch (fs.ToUpper())
                                {
                                    case ("RIGHT"): ba = BoxAlign.Right; break;
                                    case ("LEFT"): ba = BoxAlign.Left; break;
                                    case ("FILLED"): mdi_w = 0; break;
                                    default: float wr; if (float.TryParse(fs, out wr)) mdi_w = wr; break;
                                }
                        }
                if (mdio.type == MDItype.Space || mdio.type == MDItype.Line) mdio.SetWI(mdi_w);
                if (!(mdio.type == MDItype.String) && mdi_w == 0) fuell = true;
                else fuell = false;
            }
            public void calcBox(float wi = 0)
            {
                if (mdio.type == MDItype.String)
                {
                    if (mdi_w != 0)
                    {
                        SP sps = fspm(mdi_w - mdio.with);
                        if (ba == BoxAlign.Left) btext = mdio.text + sps.getFStr();
                        else btext = sps.getFStr() + mdio.text;
                    }
                    else btext = mdio.text;
                }
                else btext = mdio.SetWI(mdi_w == 0 ? wi : mdi_w);
            }
        }
        //MDI
        public abstract class MDI
        {
            public float defw;
            public string text = "";
            public DateTime curdt;
            public MDItype type = MDItype.Space;
            public float with;
            public virtual string SetWI(float wi)
            {
                return "";
            }
        }
        //Line
        public class MDIline : MDI
        {
            char c;
            public MDIline(char C)
            {
                c = C;
                type = MDItype.Line;
            }
            public override string SetWI(float wi)
            {
                with = wi;
                curdt = DateTime.Now;
                var i = (int)(wi / fontCharWidths[c]);
                return new String(c, i > 0 ? i : 1);
            }
        }
        //Space
        public class MDIspace : MDI
        {
            public override string SetWI(float wi)
            {
                with = wi;
                curdt = DateTime.Now;
                return fSm(wi);
            }
        }
        //Bar
        public class MDIbar : MDI
        {
            int we = -1;
            public MDIbar()
            {
                type = MDItype.Bar; defw = 35;
            }
            public override string SetWI(float wi)
            {
                curdt = DateTime.Now;
                return fbarm(wi, we).bstr;
            }
            public void sB(double d)
            {
                int w = (int)d;
                if (w == we) return;
                we = w;
                curdt = DateTime.Now;
            }
        }
        //String
        public class MDIs : MDI
        {
            string kc = ""; double w = -2; bool nn = false;
            public MDIs(float dw = 0, bool n = false, string ikc = "") { nn = n; defw = dw; type = MDItype.String; kc = ikc; }
            public MDIs(string istr) { type = MDItype.String; sT(istr); }
            public void sT(string s) { if (s.Contains("Infinity") || s.Contains("NaN")) s = "---"; var st = kc == "" ? s : kc[0] + s + kc[1]; if (text != st) { w = -2; text = st; with = Sm(st); curdt = DateTime.Now; } }
            bool iNW(double d) { if (d == w) return false; w = d; return true; }
            public void sG(double d) { if (iNW(d)) sT(Fs((d / 9.81), "0.00", " g")); }
            public void sM(double d) { if (iNW(d)) sT((d == 0 && nn) ? " " : F_(d, pM)); }
            public void sD(double d) { if (iNW(d)) sT((d == 0 && nn) ? " " : d.ToString("0.")); }
            public void sZ(double d) { if (iNW(d)) sT(Fs(d, "0.#", " %")); }
        }
        //NMDL
        public class dyMultiDisplayLine { public int live; string Text = ""; public float cfx = 0; public float with; public string mode; public DateTime curdt; List<MDIbox> mdis; public dyMultiDisplayLine(float iwith, string imode, ref List<MDIbox> imdis) { live = 10; with = iwith; mode = imode; mdis = imdis; } public string getText(TextRenderEngine sd) { bool ns = false; bool nxs = false; var fixsize = 0f; int fuelcount = 0; var fuelsize = 0f; foreach (var memdi in mdis) { if (memdi.mdio.curdt > curdt) { ns = true; if (memdi.mdi_w == 0) nxs = true; if (!memdi.fuell) memdi.calcBox(); } if (memdi.fuell) fuelcount++; else { if (memdi.mdi_w == 0) fixsize += memdi.mdio.with; else fixsize += memdi.mdi_w; } } if (Text == "") ns = nxs = true; if (ns) { fixsize += 1.29166f + (1.29166f * mdis.Count); fuelsize = (with - fixsize) / fuelcount; Text = " "; foreach (MDIbox memdi in mdis) { if (memdi.fuell && (nxs || memdi.btext == "")) memdi.calcBox(fuelsize); Text += memdi.btext + " "; } cfx = fuelcount == 0 ? with - fixsize : fuelsize * fuelcount; if (float.IsNaN(cfx)) cfx = 0; curdt = DateTime.Now; } live = 20; return Text; } }
    }
}
