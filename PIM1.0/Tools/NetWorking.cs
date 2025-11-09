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
            GridTerminalSystem.GetBlocksOfType(comp, block => Tools.BlockConstructMember(block));
            foreach (var p in comp)
            {
                if (p.Enabled && p.DetailedInfo.StartsWith(Strings.SmsVersion))
                {
                    var s = "";
                    bool configteil = false;

                    foreach (var cstr in p.CustomData.Split('\n'))
                    {
                        if (cstr.Contains(Strings.X_Config)) configteil = true;
                        if (configteil) s += cstr + '\n';
                        if (cstr.Contains(Strings.X_Config_end)) configteil = false;
                    }

                    if (s != "") s += "\n\n";

                    foreach (var a in ingotprio.Keys.ToArray())
                    {
                        if (ingotprio.ContainsKey(a) && !Refinery.PrioBlockTypes.Contains("@" + a)) ingotprio.Remove(a);
                    }

                    foreach (var sx in ingotprio.Keys)
                    {
                        s += "@INGOTPRIOLIST;" + sx + "\n";
                        foreach (var i in ingotprio[sx]) if (i.initp > 0) s += "@INGOTPRIO;" + i.refineryBP.OutputIDName + ";" + i.initp + "\n";
                    }

                    foreach (var c in Lists.Data.CargoUseList.Values) s += "@CARGOUSE;" + c.Type + ";" + c.CurrentCapacity + ";" + c.MaximumCapacity + "\n";

                    foreach (var b in Lists.Data.BluePrints_Active.Values)
                    {
                        if (b.MaximumItemAmount > 0) s += Strings.ItemMaxNameSemikolon + b.ItemName + ";" + b.MaximumItemAmount + ";" + b.subtype + "\n";
                        if (b.AssemblyAmount > 0) s += Strings.AssemblerQueueNameSemikolon + b.ItemName + ";" + b.AssemblyAmount + ";" + b.subtype + "\n";
                        else if (b.RefineryAmount > 0) s += Strings.AssemblerQueueNameSemikolon + b.ItemName + ";" + b.RefineryAmount + ";" + b.subtype + "\n";
                    }

                    foreach (var b in Lists.Data.BluePrints_Inactive.Values)
                    {
                        if (b.MaximumItemAmount > 0) s += Strings.ItemMaxNameSemikolon + b.ItemName + ";" + b.MaximumItemAmount + ";" + b.subtype + "\n";
                        if (b.AssemblyAmount > 0) s += Strings.AssemblerQueueNameSemikolon + b.ItemName + ";" + b.AssemblyAmount + ";" + b.subtype + "\n";
                        else if (b.RefineryAmount > 0) s += Strings.AssemblerQueueNameSemikolon + b.ItemName + ";" + b.RefineryAmount + ";" + b.subtype + "\n";
                    }

                    p.CustomData = s;
                }
            }
        }
    }
}
