using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;

namespace IngameScript
{
    // Manager infos and the text of the PB display. The infos and hints themselves are in StatusViews.cs.
    partial class Program
    {
        class AmmoManagerInfo : View
        {
            public override StringBuilderExtended GetInfoText()
            {
                if (Lists.Data.guns.Count == 0) return null;
                InfoString.SetText("AmmunitionManager: ", Lists.Data.guns.Count.ToString(), " weapons.");
                return InfoString;
            }
        }


        class StorageManagerInfo : View
        {
            public override StringBuilderExtended GetInfoText()
            {
                if (Lists.Data.StorageCargos.Count == 0) return null;
                InfoString.SetText("StorageManager: ", Lists.Data.StorageCargos.Count.ToString(), " containers.");
                return InfoString;
            }
        }


        class RefineryManagerInfo : View
        {
            public override StringBuilderExtended GetInfoText()
            {
                InfoString.Clear();
                foreach (var refinery in Lists.Data.RefineryList) refinery.GetErrorInfo(InfoString);
                if (InfoString.IsEmpty()) return null;
                InfoString.Insert(0, "--------------- RefineryManager ---------------\n");
                return InfoString;
            }
        }


        class AssemblerManagerInfo : View
        {
            public override StringBuilderExtended GetInfoText()
            {
                InfoString.Clear();
                foreach (var assembler in Lists.Data.AssemblerList) assembler.GetErrorInfo(InfoString);
                if (InfoString.IsEmpty()) return null;
                InfoString.Insert(0, "--------------- AssemblerManager ----------------\n");
                return InfoString;
            }
        }


        // all scripts of the network (sorted by address), with their master; nothing if this PB is alone
        class NetworkInfo : View
        {
            static readonly List<long> addresses = new List<long>();

            public override StringBuilderExtended GetInfoText()
            {
                if (!Config.Instance.ShowNetwork || Net.Peers.Count == 0 || Net.IsDiscovering) return null;

                addresses.Clear();
                addresses.Add(Net.Me);
                addresses.AddRange(Net.Peers.Keys);
                addresses.Sort();

                InfoString.SetText("------------------- Network -------------------\n");
                foreach (var a in addresses)
                {
                    NetWorkingPeer peer;
                    Net.Peers.TryGetValue(a, out peer);
                    var block = MyProgram.GridTerminalSystem.GetBlockWithId(a) as IMyTerminalBlock;
                    InfoString.Append(block != null ? block.CustomName : a.ToString());
                    InfoString.Append(": " + (peer != null ? peer.Version : Strings.PimVersion));
                    if (Net.MasterOf(peer != null ? peer.ScriptType : Net.ScriptType) == a) InfoString.Append(" (master)");
                    if (peer == null) InfoString.Append(" (this PB)");
                    else if (!peer.SameConstruct) InfoString.Append(" (" + (block != null ? block.CubeGrid.CustomName : "other construct") + ")");
                    InfoString.Append('\n');
                }
                return InfoString;
            }
        }

        readonly NetworkInfo networkInfo = new NetworkInfo();

        // scripts with another network protocol: they do not see each other (e.g. two PIMs both work as master)
        class NetworkProtocolHint : View
        {
            public NetworkProtocolHint() : base(ViewType.WARNING) { }

            public override StringBuilderExtended GetInfoText()
            {
                if (Net.Incompatible.Count == 0) return null;
                InfoString.Clear();
                foreach (var p in Net.Incompatible.Values)
                {
                    var block = MyProgram.GridTerminalSystem.GetBlockWithId(p.Address) as IMyTerminalBlock;
                    InfoString.Append("!! PB '" + (block != null ? block.CustomName : p.Address.ToString()) + "' uses network protocol " + p.Protocol
                        + ", this PB " + NetWorkingNode.Protocol + ":\n   update the script on " + (p.Protocol > NetWorkingNode.Protocol ? "this PB" : "that PB") + "\n");
                }
                return InfoString;
            }
        }

        readonly NetworkProtocolHint networkProtocolHint = new NetworkProtocolHint();

        StringBuilderExtended infoString = new StringBuilderExtended(2000);
        StringBuilderExtended warnungstring = new StringBuilderExtended(1000);

        // PB info after the running sign, built once per cycle (WritePimRunInfo runs every tick)
        string PimStatus = " starting...\n";


        void CalculateInfos()
        {
            infoString.SetText("\n");
            warnungstring.Clear();
            foreach (var v in viewList)
            {
                switch (v.type)
                {
                    case View.ViewType.INFO: infoString.AppendLFifNotEmpty(v.GetInfoText()); break;
                    case View.ViewType.WARNING: warnungstring.AppendLF(v.GetInfoText()); break;
                }
            }
            if (!warnungstring.IsEmpty())
            {
                infoString.Append("\n--------------------- Hints ---------------------\n");
                infoString.Append(warnungstring);
            }
            PimStatus = RunStatus.Running(LoopManager.CurrentInstructionAmount, Properties.Data.CurrentCycleInSec) + infoString;
        }

        // in standby the jobs (and CalculateInfos) do not run: the LoopManager sets the status every run
        void SetStandbyStatus()
        {
            var master = GridTerminalSystem.GetBlockWithId(Net.MasterAddress) as IMyTerminalBlock;
            PimStatus = " Standby\nMaster: " + (master != null ? master.CustomName : Net.MasterAddress.ToString()) + "\n\n" + networkInfo.GetInfoText() + networkProtocolHint.GetInfoText();
        }
    }
}
