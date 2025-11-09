using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        public abstract class Tools 
        {
            public static Program ProgramInstance => Program.Instance;
            public static IMyProgrammableBlock MySelf => ProgramInstance.Me;
            public static IMyGridProgramRuntimeInfo RunTime => ProgramInstance.Runtime;
            public static IMyGridTerminalSystem GridTerminalSystem => ProgramInstance.GridTerminalSystem;

            public static bool BlockConstructMember(IMyTerminalBlock block) 
            {
                return block.IsSameConstructAs(MySelf);
            }

            public static void AddToDebugString(string str)
            {
                Program.LCD_DebugString += str;
            }

            public static int GetBlockList<T>(List<T> blockList)
                where T : class, IMyTerminalBlock
            {
                Program.Instance.GridTerminalSystem.GetBlocksOfType(blockList, block => BlockConstructMember(block));
                return blockList.Count;
            }
        }
    }
}
