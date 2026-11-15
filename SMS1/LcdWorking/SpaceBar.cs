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
        //Space
        public class SP
        {
            static int SPLrc = 700;
            int live = 5;
            public float with = 0;
            string fuels = "";
            public SP(float iw)
            {
                with = iw;
                if (with < 0.6f) return;
                int spnum = (int)(with / 1.29166f);
                int hsnum = (int)((with - (spnum * 1.29166f)) / 0.287035f);
                if (hsnum >= spnum)
                {
                    spnum += 1;
                    hsnum = 0;
                }
                else spnum -= hsnum;
                fuels += new String(' ', spnum);
                fuels += new String('\u00AD', hsnum);
            }
            public string getFStr()
            {
                live = 5;
                SPLRefresh();
                return fuels;
            }
            static void SPLRefresh()
            {
                if (--SPLrc < 1)
                {
                    for (int i = SPL.Count - 1; i > 0; i--)
                    {
                        SP osp = SPL[i];
                        if (--osp.live < 0) SPL.Remove(osp);
                    }
                    SPLrc = 500;
                }
            }
        }
        static SP fspm(float with) { foreach (SP osp in SPL) { if (osp.with == with) return osp; } SP nsp = new SP(with); SPL.Add(nsp); return nsp; }
        static string fSm(float with) { return fspm(with).getFStr(); }
        //Bar
        public class BarRatio
        {
            public int live = 10;
            int gswert;
            int spaces;
            public string bstr;
            public static int GPLrc = 10;
            public BarRatio(float wi, int w)
            {
                spaces = (int)(wi / 1.43076f) - 2;
                gswert = (spaces * (w > 100 ? 100 : w)) / 100;
                bstr = '[' + (gswert < 0 ? "" : new String(']', gswert)) + ((spaces - gswert) < 0 ? "" : new String('.', spaces - gswert)) + ']';
                live = 20;
                Refresh();
            }
            public bool isWert(int w)
            {
                if (((spaces * (w > 100 ? 100 : w)) / 100) == gswert) return true;
                else return false;
            }
            public static void Refresh()
            {
                if (--GPLrc < 1)
                {
                    foreach (float gw in GPL.Keys)
                    {
                        for (int i = GPL[gw].Count - 1; i > 0; i--)
                        {
                            BarRatio gaus = GPL[gw][i];
                            if (--gaus.live < 0) GPL[gw].Remove(gaus);
                        }
                    }
                    GPLrc = 100;
                }
            }
        }
        static BarRatio fbarm(float with, int wert)
        {
            if (GPL.ContainsKey(with))
            {
                foreach (BarRatio gaus in GPL[with])
                {
                    if (gaus.isWert(wert))
                    {
                        gaus.live = 5;
                        return gaus;
                    }
                }
            }
            else GPL.Add(with, new List<BarRatio>());
            BarRatio gs = new BarRatio(with, wert);
            GPL[with].Add(gs);
            return gs;
        }
    }
}
