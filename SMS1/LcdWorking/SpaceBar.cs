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
        //Bar
        class BarRatio
        {
            public int live = 10;
            int gswert, spaces;
            public string bstr;
            static int GPLrc = 10;
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
            static void Refresh()
            {
                if (--GPLrc < 1)
                {
                    foreach (var gw in GPL.Keys)
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
