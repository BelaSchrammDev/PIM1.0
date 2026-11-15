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
        Dictionary<string, Func<IMyTerminalBlock, bool>> Bf = new Dictionary<string, Func<IMyTerminalBlock, bool>>();
        void addBlockParseFunction()
        {
            Bf.Add("CryoChamber", fW);
            Bf.Add("SolarPanel", fS);
            Bf.Add("WindTurbine", fS);
            Bf.Add("Piston", fW);
            Bf.Add("TextPanel", fW);
            Bf.Add("Welder", fW);
            Bf.Add("Drill", fW);
            Bf.Add("Grinder", fW);
        }
        bool fS(IMyTerminalBlock t)
        {
            var b = t.BlockDefinition.ToString();
            double m = 0;
            if (b.Contains("Solar")) m = b.Contains("LargeBlockSolarPanel") ? 0.16 : 0.04;
            else if (b.Contains("Wind")) m = 0.4;
            var pv = dyDisplayableVariable.getPV(t.CubeGrid, BlockSubtypeFilter + "s");
            pv.aM('M', m);
            double use = 0;
            double max = 0;
            var dilist = t.DetailedInfo.Split('\n');
            if (dilist.Length > 2)
            {
                use = getPstr(dilist[2]);
                max = getPstr(dilist[1]);
            }
            pv.aM('E', max);
            pv.aC('M', max);
            pv.aC('E', use);
            return true;
        }
        bool fW(IMyTerminalBlock t)
        {
            var b = t.BlockDefinition.ToString();
            double m = 0.002;
            if (b.Contains("LaserMultitool")) m = b.Contains("Large") ? 6.19 : 1.44;
            else if (b.Contains("CryoChamber")) m = 0.00003;
            else if (b.Contains("TextPanel"))
            {
                var dilist = t.DetailedInfo.Split('\n');
                if (dilist.Length > 2) m = getPstr(dilist[1]);
            }
            var pv = dyDisplayableVariable.getPV(t.CubeGrid, BlockSubtypeFilter + "s");
            pv.aV('I', m, t.IsWorking ? m : 0);
            if (t.HasInventory)
            {
                var v = t.GetInventory();
                pv.aV('C', v.MaxVolume.RawValue / 1000, v.CurrentVolume.RawValue / 1000);
                AddItemsToInventoryDictionary(t.CubeGrid, gII(v));
            }
            return true;
        }
        // one ForEachJob per block type in Bf; fS/fW read BlockSubtypeFilter
        Job MultiBlockJob()
        {
            var jobs = new List<Job>();
            foreach (var k in Bf.Keys)
            {
                var f = Bf[k];
                var l = new List<IMyTerminalBlock>();
                jobs.Add(new ForEachJob<IMyTerminalBlock>(
                    () => { BlockSubtypeFilter = k; GridTerminalSystem.GetBlocksOfType(l, BlockSutypeCheck); return l; },
                    b => { if (BlockConstructDockingCheck(b)) f(b); }));
            }
            return new SequentialJob(jobs.ToArray());
        }
    }
}
