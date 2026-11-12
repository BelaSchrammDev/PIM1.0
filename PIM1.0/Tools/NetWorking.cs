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
            GridTerminalSystem.GetBlocksOfType(comp, block => block.IsMainConstruct());
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

                    foreach (var sx in ingotprio.Keys)
                    {
                        s += "@INGOTPRIOLIST;" + sx + "\n";
                        foreach (var i in ingotprio[sx]) if (i.initp > 0) s += "@INGOTPRIO;" + i.refineryBP.OutputIDName + ";" + i.initp + "\n";
                    }

                    foreach (var c in Lists.Data.CargoUseList.Values) s += "@CARGOUSE;" + c.Type + ";" + c.CurrentCapacity + ";" + c.MaximumCapacity + "\n";

                    s += BluePrintInfos(Lists.Data.BluePrints_Active.Values) + BluePrintInfos(Lists.Data.BluePrints_Inactive.Values);

                    p.CustomData = s;
                }
            }
        }

        /// <summary>Max amount and amount in production of the blueprints, one line each (for SMS).</summary>
        static string BluePrintInfos(IEnumerable<AssemblerBluePrint> bluePrints)
        {
            var s = "";
            foreach (var b in bluePrints)
            {
                if (b.MaximumItemAmount > 0) s += Strings.ItemMaxNameSemicolon + b.ItemName + ";" + b.MaximumItemAmount + ";" + b.subtype + "\n";
                if (b.AssemblyAmount > 0) s += Strings.AssemblerQueueNameSemicolon + b.ItemName + ";" + b.AssemblyAmount + ";" + b.subtype + "\n";
                else if (b.RefineryAmount > 0) s += Strings.AssemblerQueueNameSemicolon + b.ItemName + ";" + b.RefineryAmount + ";" + b.subtype + "\n";
            }
            return s;
        }
    }
}
