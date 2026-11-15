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
        //HEngine
        public class HydrogenEngine : dyBlockPowerStorage
        {
            public IMyPowerProducer he; float capa = 0;
            public HydrogenEngine(IMyTerminalBlock b) { addChangeType = ChangeType.Hydrogenengine_add; nameChangeType = ChangeType.Hydrogenengine_name; tb = b; he = (IMyPowerProducer)b; getBlockDefinition(); setBlockTypeFields(); InitRefresh(); addChange(); }
            public override void getPCvar(dyDisplayableVariable pv)
            {
                float frt = 0, mwh = 0, fill = 0;
                var di = getDetailedInfo();
                if (di.Length > 3)
                {
                    var dix = di[3].Split(':', '(', '/', ')');
                    if (dix.Length > 3)
                    {
                        float.TryParse(dix[1].Split('%')[0], out frt);
                        float.TryParse(dix[2].Split('L')[0], out fill);
                        if (capa == 0) float.TryParse(dix[3].Split('L')[0], out capa);
                        mwh = (fill * 0.005f) / 3600;
                    }
                }
                sV('H', capa, fill, pv);
                sV('S', (capa * 0.005f) / 3600, mwh, pv);
                aRS(mwh);
                sT("@fChT", (he.Enabled && he.CurrentOutput > 0 && frt < 100) ? "empty in " + fT(mwh / he.CurrentOutput) : "---", 35);
                base.getPCvar(pv);
            }
        }
    }
}
