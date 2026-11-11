using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        public abstract class Tools
        {
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

                return inv.ContainsKey(itemType) && inv[itemType] > 0;
            }

            public static bool IsInventoryItemEmpty(string itemType, Dictionary<string, float> inv = null)
            {
                return !IsInventoryItemExists(itemType, inv);
            }

            public static float GetInventoryItemAmount(string itemType, Dictionary<string, float> inv = null)
            {
                if (inv == null)
                {
                    return Lists.Data.Inventory.GetValueOrDefault(itemType, 0);
                }

                return inv.GetValueOrDefault(itemType, 0);
            }

            public static void AddToDebugString(string str)
            {
                Program.LCD_DebugString.Append(str);
            }

            public static int GetBlockList<T>(List<T> blockList)
                where T : class, IMyTerminalBlock
            {
                Program.MyProgram.GridTerminalSystem.GetBlocksOfType(blockList, block => block.IsMainConstruct());
                return blockList.Count;
            }
        }
    }
}
