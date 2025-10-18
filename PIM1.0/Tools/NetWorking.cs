using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript
{
    partial class Program
    {
        void SendInfosToSMS()
        {
            var comp = new List<IMyProgrammableBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyProgrammableBlock>(comp, block => block.IsSameConstructAs(Me));
            foreach (var p in comp)
            {
                if (p.Enabled && p.DetailedInfo.StartsWith(SMS) && !LoopManager.firstRun)
                {
                    var s = "";
                    bool configteil = false;
                    foreach (var cstr in p.CustomData.Split('\n'))
                    {
                        if (cstr.Contains(X_Config)) configteil = true;
                        if (configteil) s += cstr + '\n';
                        if (cstr.Contains(X_Config_end)) configteil = false;
                    }
                    if (s != "") s += "\n\n";
                    foreach (var a in ingotprio.Keys.ToArray()) if (ingotprio.ContainsKey(a) && !Refinery.priobt.Contains("@" + a)) ingotprio.Remove(a);
                    foreach (var sx in ingotprio.Keys)
                    {
                        s += "@INGOTPRIOLIST;" + sx + "\n";
                        foreach (var i in ingotprio[sx]) if (i.initp > 0) s += "@INGOTPRIO;" + i.refineryBP.OutputIDName + ";" + i.initp + "\n";
                    }
                    foreach (var c in CargoUseList.Values) s += "@CARGOUSE;" + c.type + ";" + c.Current + ";" + c.Maximum + "\n";
                    foreach (var b in Lists.Data.BluePrints_Active.Values)
                    {
                        if (b.MaximumItemAmount > 0) s += Constants.ItemMaxNameSemikolon + b.ItemName + ";" + b.MaximumItemAmount + ";" + b.subtype + "\n";
                        if (b.AssemblyAmount > 0) s += Constants.AssemblerQueueNameSemikolon + b.ItemName + ";" + b.AssemblyAmount + ";" + b.subtype + "\n";
                        else if (b.RefineryAmount > 0) s += Constants.AssemblerQueueNameSemikolon + b.ItemName + ";" + b.RefineryAmount + ";" + b.subtype + "\n";
                    }
                    foreach (var b in Lists.Data.BluePrints_Inactive.Values)
                    {
                        if (b.MaximumItemAmount > 0) s += Constants.ItemMaxNameSemikolon + b.ItemName + ";" + b.MaximumItemAmount + ";" + b.subtype + "\n";
                        if (b.AssemblyAmount > 0) s += Constants.AssemblerQueueNameSemikolon + b.ItemName + ";" + b.AssemblyAmount + ";" + b.subtype + "\n";
                        else if (b.RefineryAmount > 0) s += Constants.AssemblerQueueNameSemikolon + b.ItemName + ";" + b.RefineryAmount + ";" + b.subtype + "\n";
                    }
                    p.CustomData = s;
                }
            }
        }
    }
}
