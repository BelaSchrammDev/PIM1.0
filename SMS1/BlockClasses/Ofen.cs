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
        //Ofen
        public class Ofen : dyBlockPowerConsumer
        {
            static ST lsch = new ST(3); Dictionary<string, bool> ores = new Dictionary<string, bool>(); ST schc = new ST(5); public IMyRefinery raff; public int fertig;
            public Ofen(IMyTerminalBlock r) { addChangeType = ChangeType.Refinery_add; nameChangeType = ChangeType.Refinery_name; tb = r; raff = (IMyRefinery)r; getBlockDefinition(); setBlockTypeFields(); addChange(); }
            public override void getPCvar(dyDisplayableVariable pv)
            {
                var gi = readInventarList(pv, 0, '0');
                readInventarList(pv, 1, '1');
                string ws;
                string nws = "";
                float waf;
                float nwaf = 0;
                if (bs.Contains("Hydroponics"))
                {
                    waf = (float)((gi.volume.C / 1000) / 56.9f) * 12.5f;
                    ws = waf == 0 ? "" : "AlgaeSoy";
                }
                else
                {
                    ws = gi.gISt(0);
                    nws = gi.gISt(1);
                    waf = gi.gA(0);
                    nwaf = gi.gA(1);
                }
                if (raff.IsProducing)
                {
                    fertig = 100 - (int)((gi.volume.C * 100) / gi.volume.M);
                    sT("@fWM", "Producing");
                    if (oiil.Count > 0 && oiil.ContainsKey(ws)) oiil[ws].of++;
                }
                else
                {
                    sT("@fWM", "Idle");
                    fertig = 100;
                }
                sT("@fW", ws == "" ? "----" : ws);
                sM("@fWa", waf, 25);
                sT("@fNW", nws == "" ? "----" : nws);
                sM("@fNWa", nwaf, 25);
                sV('V', 100, fertig);
                base.getPCvar(pv);
            }
        }
    }
}
