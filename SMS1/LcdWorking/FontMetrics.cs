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
        static string fmstr(float with, string arg) { return SpaceFillText(with - SpaceWidth - StringWidth(arg)) + arg; }
    }
}
