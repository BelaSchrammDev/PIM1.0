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
        //O2Gen
        public class dyGasgenerator : dyBlockPowerConsumer
        {
            static MyItemType ice = new MyItemType(IG_ + "Ore", "Ice"), hb = new MyItemType(IG_ + IG_HBottles, "HydrogenBottle"), ob = new MyItemType(IG_ + IG_OBottles, "OxygenBottle"); float icea = 0; IMyGasGenerator gg; ST wz = new ST(5);
            public dyGasgenerator(IMyTerminalBlock igg) { addChangeType = ChangeType.Gasgenerator_add; nameChangeType = ChangeType.Gasgenerator_name; tb = igg; gg = (IMyGasGenerator)igg; getBlockDefinition(); setBlockTypeFields(); addChange(); sT("@fRf", "", 20); sM("@fIa", 0, 24); sD("@fHB", 0, 6, true); sD("@fOB", 0, 6, true); }
            public override void Display() { sT("@fRf", "AutoRefill " + (gg.AutoRefill ? "on" : "off"), 20); var ggi = getInventarList(0); sM("@fIa", ggi.gA(ice), 24); sD("@fHB", ggi.gA(hb), 6, true); sD("@fOB", ggi.gA(ob), 6, true); base.Display(); }
            public override void getPCvar(dyDisplayableVariable pv) { var gi = readInventarList(pv); if (wz.ifS()) { var ia = gi.gA(ice); IsW = icea <= ia ? false : true; icea = ia; } sI(pv); }
        }
        //AirVent
        public class Luefter : dyBlockPowerConsumer
        {
            float ox = 0; public IMyAirVent av;
            public Luefter(IMyTerminalBlock b) { addChangeType = ChangeType.Airvent_add; nameChangeType = ChangeType.Airvent_name; tb = b; av = (IMyAirVent)b; getBlockDefinition(); setBlockTypeFields(); addChange(); sT("@fAt", ""); sT("@fPz", "", 35); }
            public override void Display() { sT("@fAt", av.CanPressurize ? "AirTight" : "Leaking"); sT("@fPz", av.Depressurize ? "Depressurize" : "Pressurize", 35); base.Display(); }
            public override void getPCvar(dyDisplayableVariable pv) { float olev = av.GetOxygenLevel(); IsW = (olev == ox) ? false : true; ox = olev; sV('V', 1f, olev); sI(pv); }
        }
        //Tanks
        public class GasTank : dyBlockPowerConsumer
        {
            public IMyGasTank tank; public string medium = "#";
            public GasTank(IMyTerminalBlock b)
            {
                tb = b;
                tank = (IMyGasTank)b;
                getBlockDefinition();
                if (bs == "" || bs.Contains("Oxygen"))
                {
                    medium = "OXYGEN";
                    addChangeType = ChangeType.TankOxygen_add;
                    nameChangeType = ChangeType.TankOxygen_name;
                }
                else
                {
                    bt = "HydrogenTank";
                    medium = "HYDROGEN";
                    addChangeType = ChangeType.TankHydrogen_add;
                    nameChangeType = ChangeType.TankHydrogen_name;
                }
                addChange();
                setBlockTypeFields();
                getVC('V');
                sT("@fSP", "", 7);
            }
            public override void Display()
            {
                sT("@fSP", tank.Stockpile ? "[S]" : ("[" + SoftHyphen + SoftHyphen + "]"), 7);
                base.Display();
            }
            public override void getPCvar(dyDisplayableVariable pv)
            {
                readInventarList(pv);
                sI(pv);
                var tc = tank.Capacity;
                sV('V', tc, tc * (float)tank.FilledRatio);
                pv.aV(medium[0], tc, tc * (float)tank.FilledRatio);
            }
        }
    }
}
