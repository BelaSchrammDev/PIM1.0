using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public abstract class ProcessingBlockListBase<T> : CountingJob
            where T : class
        {
            protected List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();

            public ProcessingBlockListBase(Program program, string name) : base(program, name)
            {
            }

            protected void FillBlockList()
            {
                Program.GridTerminalSystem.GetBlocksOfType(Blocks, block => IsValidBlock(block));
            }

            protected override void ConfigureCountingBounds(out int startIndex, out int endIndex)
            {
                FillBlockList();
                startIndex = Blocks.Count - 1;
                endIndex = 0;
            }

            protected override void ProcessingIndex(int index)
            {
                if (IsValidBlock(Blocks[index])) 
                {
                    ProcessingTerminalBlock(Blocks[index]);
                }
            }

            protected abstract void ProcessingTerminalBlock(IMyTerminalBlock t);

            private bool IsValidBlock(IMyTerminalBlock t) 
            {
                return t != null && t as T != null && t.IsSameConstructAs(Program.Me);
            }
        }
    }
}
