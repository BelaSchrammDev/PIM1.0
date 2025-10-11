using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        public class InventoryCountingBase<T> : CountingJob
            where T : class, IMyTerminalBlock
        {
            protected List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();

            public InventoryCountingBase(Program program, string name) : base(program, name)
            {
            }

            protected void FillBlockList()
            {
                Program.GridTerminalSystem.GetBlocksOfType<T>(Blocks, block => block.IsSameConstructAs(Program.Me));
            }

            protected override void ConfigureCountingBounds(out int startIndex, out int endIndex)
            {
                FillBlockList();
                startIndex = 0;
                endIndex = Blocks.Count - 1;
            }

            protected override void ProcessingIndex(int index)
            {
                ProcessingTerminalBlock(Blocks[index]);
            }

            protected void ProcessingTerminalBlock(IMyTerminalBlock t)
            {
                if (t.HasInventory)
                {
                    var inv = t.GetInventory(0);
                    AddToInventory(inv);
                    if (t.CustomName.Contains(X_StorageTag)) return;
                    Parameter pm = new Parameter();
                    if (pm.ParseArgs(t.CustomName))
                    {
                        if (!pm.IsParameter("Keep")) InventoryList_SMSflagged.Add(inv);
                        if (!pm.IsParameter("Infolcd")) Program.addToInventoryList(inv, pm.ParameterList);
                    }
                    else if (t.BlockDefinition.SubtypeId.Contains("Container") || t.BlockDefinition.SubtypeId.Contains("Connector"))
                    {
                        InventoryList_SMSflagged.Add(inv);
                    }
                    else InventoryList_nonSMSflagged.Add(inv);
                }
            }
        }
    }
}
