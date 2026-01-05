using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public abstract class ProcessingBlockListBase : CountingJob
        {
            protected List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();

            protected void FillBlockList()
            {
                GridTerminalSystem.GetBlocksOfType(Blocks, block => IsValidBlock(block));
            }

            protected override void ConfigureCountingBounds(out int startIndex, out int endIndex)
            {
                FillBlockList();
                startIndex = Blocks.Count - 1;
                endIndex = 0;
            }

            protected override void ProcessingIndex(int index)
            {
                ProcessingTerminalBlock(Blocks[index]);
            }

            protected abstract void ProcessingTerminalBlock(IMyTerminalBlock t);

            protected virtual bool IsValidBlock(IMyTerminalBlock t) 
            {
                return BlockConstructMember(t);
            }
        }
    }
}
