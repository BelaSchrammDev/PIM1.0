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
        void PT() { double slc = 0.0, soc = 0.0, cpt = 0.0, cct = 0.0, sic = 0.0; foreach (var p in pvl) { if (p.isID("SE", 's')) { if (p.Name == "Total") continue; if (p.isID("I", 's')) { sic += p.getVC('I').C; var b = p.getVC('S'); slc += b.M - b.C; } var o = p.getVC('E'); soc += o == null ? 0 : o.C; } } if (soc < sic) cct = slc / (sic - soc); else cpt = rsp / (soc - sic); if (cct == 0 && cpt == 0) { if (rsp == 0) pt.sT("no stored energy!"); else pt.sT("PowerTime " + fT(rsp / soc)); } else pt.sT(cct > 0 ? "Charging -> " + fT(cct) : "Powertime " + fT(rsp / (soc - sic))); }
        void gFSD() { F("\n", 0f); F("'|ÎÏ", 1f); F(" !`Iiîïjl", 1.29166f); F("(),.:;[]{}1ft", 1.43076f); F("\"-r", 1.57627f); F("*", 1.72222f); F("\\", 1.86f); F("/", 2.16279f); F("«»Lvx_ƒ", 2.325f); F("?7Jcçz", 2.44736f); F("3FKTaäàâbdeèéêëghknoöôpqsuüùûßyÿ", 2.58333f); F("+<>=^~EÈÉÊË", 2.73529f); F("#0245689CÇXZ", 2.90625f); F("$&GHPUÜÙÛVYŸ", 3f); F("AÄÀÂBDNOÖÔQRS", 3.20689f); F("%", 3.57692f); F("@", 3.72f); F("M", 3.875f); F("æœmw", 4.04347f); F("WÆŒ", 4.65f); F(SoftHyphen, 1.578695f); }
        void F(string s, float z) { foreach (var c in s) fontCharWidths.Add(c, z); }
        static float Sm(string strData) { float fltTotal = 0; foreach (var c in strData) { fltTotal += fontCharWidths.ContainsKey(c) ? fontCharWidths[c] : 2f; } return fltTotal; }
        static string fmstr(float with, string arg) { return fSm(with - 1.29166f - Sm(arg)) + arg; }
    }
}
