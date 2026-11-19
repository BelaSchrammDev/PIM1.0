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
        //Akku
        public class Akku : dyBlockPowerStorage
        {
            ST ptc = new ST(20);
            public IMyBatteryBlock batt;
            VC vi;
            double mo, mc;
            public Akku(IMyTerminalBlock a) { addChangeType = ChangeType.Battery_add; nameChangeType = ChangeType.Battery_name; tb = a; batt = (IMyBatteryBlock)a; getBlockDefinition(); bt = "Battery"; setBlockTypeFields(); var dilist = getDetailedInfo(); if (dilist.Length > 2) { mc = getPstr(dilist[2]); mo = getPstr(dilist[1]); }; addChange(); vi = getVC('I'); sT("@fChM", F(), 14); sT("@fChT", ""); }
            public override void getPCvar(dyDisplayableVariable pv) { var i = batt.CurrentInput; var o = batt.CurrentOutput; var s = batt.CurrentStoredPower; if (batt.ChargeMode == ChargeMode.Recharge || !iEF()) { sV('I', 0, i, pv); sV('E', 0, o, pv); } else { sV('I', mc, i, pv); sV('E', batt.MaxOutput, o, pv); } sV('M', mo, batt.MaxOutput, pv); sV('S', batt.MaxStoredPower, s, pv); aRS(s); }
            public override void Display()
            {
                sT("@fChM", F(), 14);
                string lst = "Battery full";
                if (iEF())
                {
                    var i = batt.CurrentInput;
                    var o = batt.CurrentOutput;
                    var s = batt.CurrentStoredPower;
                    var chrg = i - o;
                    if (vs.R > 99.9) ptc.UD();
                    else if (chrg > 0) lst = "charged in " + fT((batt.MaxStoredPower - s) / chrg);
                    else if ((chrg < 0) && ptc.ifS(false)) lst = "dischr. in " + fT(s / (o - i));
                }
                else lst = "Battery off";
                sT("@fChT", lst);
                base.Display();
            }
            string F() { switch (batt.ChargeMode) { case (ChargeMode.Auto): return "[Auto]"; case (ChargeMode.Recharge): return "[Chr]"; } return "[DisC]"; }
        }
    }
}
