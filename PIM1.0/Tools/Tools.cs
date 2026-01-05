using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        public abstract class Tools 
        {
            public static bool TryParseDouble(string str, out double result, double defaultValue = 0)
            {
                if (double.TryParse(str, out result))
                {
                    return true;
                }

                result = defaultValue;
                return false;
            }

            public static bool TryParseInt(string str, out int result, int defaultValue = 0)
            {
                if (int.TryParse(str, out result))
                {
                    return true;
                }
                result = defaultValue;
                return false;
            }

            public static bool TryParseLong(string str, out long result, long defaultValue = 0)
            {
                if (long.TryParse(str, out result))
                {
                    return true;
                }
                result = defaultValue;
                return false;
            }

            public static Program ProgramInstance => Program.Instance;
            public static IMyProgrammableBlock MySelf => ProgramInstance.Me;
            public static IMyGridProgramRuntimeInfo RunTime => ProgramInstance.Runtime;
            public static IMyGridTerminalSystem GridTerminalSystem => ProgramInstance.GridTerminalSystem;

            public static bool BlockConstructMember(IMyTerminalBlock block) 
            {
                return block.IsSameConstructAs(MySelf);
            }

            public static bool IsAssemblerBluePrintActive(string itemType)
            {
                return Lists.Data.BluePrints_Active.ContainsKey(itemType);
            }

            public static bool TryGetActiveAssemblerBluePrint(string itemType, out AssemblerBluePrint bp)
            {
                bp = null;

                if (IsAssemblerBluePrintActive(itemType))
                {
                    bp = Lists.Data.BluePrints_Active[itemType];
                }

                return bp != null;
            }

            public static bool IsInventoryItemExists(string itemType, Dictionary<string, float> inv = null)
            {
                if (inv == null)
                {
                    return Lists.Data.Inventory.ContainsKey(itemType) && Lists.Data.Inventory[itemType] > 0;
                }
                else
                {
                    return inv.ContainsKey(itemType) && inv[itemType] > 0;
                }
            }

            public static bool IsInventoryItemEmpty(string itemType, Dictionary<string, float> inv = null)
            {
                return !IsInventoryItemExists(itemType, inv);
            }

            public static float GetInventoryItemAmount(string itemType, Dictionary<string, float> inv = null)
            {
                if (inv != null)
                {
                    return inv.GetValueOrDefault(itemType, 0);
                }

                return Lists.Data.Inventory.GetValueOrDefault(itemType, 0);
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
