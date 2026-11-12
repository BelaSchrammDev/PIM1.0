using Sandbox.ModAPI.Ingame;
using System;
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

            public static int GetBlockList<T>(List<T> blockList)
                where T : class, IMyTerminalBlock
            {
                Program.MyProgram.GridTerminalSystem.GetBlocksOfType(blockList, block => block.IsMainConstruct());
                return blockList.Count;
            }

            /// <summary>
            /// Compares the PIM objects with the blocks found on the grid: the objects of blocks that are gone
            /// are removed (after <paramref name="removed"/> was called for them). Afterwards only the new blocks
            /// are left in <paramref name="found"/>.
            /// </summary>
            public static void SyncBlockList<TBlock, TObj>(List<TBlock> found, List<TObj> objects, Func<TObj, TBlock> blockOf, Action<TObj> removed)
            {
                for (int i = objects.Count - 1; i >= 0; i--)
                {
                    if (found.Remove(blockOf(objects[i]))) continue;
                    removed(objects[i]);
                    objects.RemoveAt(i);
                }
            }
        }
    }
}
