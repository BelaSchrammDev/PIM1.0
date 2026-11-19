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
        //Reactors
        public class Reactor : dyBlockPowerStorage
        {
            public IMyReactor m;
            float mq = 0.052f, brennstoff, eff = 1;
            double ms;
            public Reactor(IMyTerminalBlock b)
            {
                addChangeType = ChangeType.Reactor_add;
                nameChangeType = ChangeType.Reactor_name;
                tb = b;
                m = (IMyReactor)b;
                getBlockDefinition();
                if (bs.Contains("Naquadah")) { eff = 10; mq = 10; }
                else if (bs.Contains("BFusionReactorT")) { eff = 50; mq = 2; }
                setBlockTypeFields();
                addChange();
                sM("@fIG", 0, 23);
                sT("@fChT", "", 25);
            }
            public override void getPCvar(dyDisplayableVariable pv)
            {
                var gi = readInventarList(pv);
                ms = gi.volume.M / mq;
                brennstoff = gi.gA(0);
                sV('S', ms * eff, brennstoff * eff, pv);
                aRS(brennstoff * eff);
                base.getPCvar(pv);
            }
            public override void Display()
            {
                sM("@fIG", brennstoff, 23);
                sT("@fChT", m.CurrentOutput > 0 ? fT((brennstoff * eff) / m.CurrentOutput) : "----", 25);
                base.Display();
            }
        }
    }
}
