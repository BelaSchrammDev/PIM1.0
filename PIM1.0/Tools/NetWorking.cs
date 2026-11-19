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
        /// <summary>
        /// Sends the PIM data to the SMS master on my construct (IGC, NetWorkingScripts.PimData), one line per value.
        /// SMS before the networking (v1.4) read it from its CustomData: such PBs only get a hint.
        /// </summary>
        void SendInfosToSMS()
        {
            var pbs = new List<IMyProgrammableBlock>();
            GridTerminalSystem.GetBlocksOfType(pbs, block => block.IsMainConstruct());
            foreach (var p in pbs) if (p.Enabled && p.DetailedInfo.StartsWith(Strings.OldSmsVersion)) SetWarning(Warning.ID.OLDSMS, p.CustomName);

            var sms = Net.MasterOf(NetWorkingScripts.Sms);
            if (sms == 0) return;

            var s = "";
            foreach (var sx in ingotprio.Keys)
            {
                s += "@INGOTPRIOLIST;" + sx + "\n";
                foreach (var i in ingotprio[sx]) if (i.initp > 0) s += "@INGOTPRIO;" + i.refineryBP.OutputIDName + ";" + i.initp + "\n";
            }

            foreach (var c in Lists.Data.CargoUseList.Values) s += "@CARGOUSE;" + c.Type + ";" + c.CurrentCapacity + ";" + c.MaximumCapacity + "\n";

            s += BluePrintInfos(Lists.Data.BluePrints_Active.Values) + BluePrintInfos(Lists.Data.BluePrints_Inactive.Values);

            Net.Send(sms, NetWorkingScripts.PimData, s);
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
