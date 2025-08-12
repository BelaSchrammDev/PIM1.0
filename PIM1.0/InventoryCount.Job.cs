// InventoryCount.cs
using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public class InventoryCount : Job
        {
            // List of all inventory owners on this grid
            private readonly List<IMyInventoryOwner> _inventoryOwners = new List<IMyInventoryOwner>();

            // Cursor indices for owner and slot
            private int _ownerIndex;
            private int _slotIndex;

            // Reusable cache to avoid allocations each tick
            private readonly List<MyInventoryItem> _itemsCache = new List<MyInventoryItem>();

            /// <summary>
            /// Constructs the InventoryCount job, injecting the Program and optional cooldown.
            /// </summary>
            public InventoryCount(Program program, int cooldownSeconds = 0)
                : base(program, "InventoryCount", cooldownSeconds)
            {
            }

            /// <summary>
            /// Initializes the job: finds all inventory owners and resets cursors and counts.
            /// </summary>
            public override void InitJob()
            {
                // Gather all inventory-owner blocks belonging to this grid
                _inventoryOwners.Clear();
                Program.GridTerminalSystem.GetBlocksOfType<IMyInventoryOwner>(
                    _inventoryOwners,
                    b => (b as IMyTerminalBlock).IsSameConstructAs(Program.Me)
                );

                // Reset cursors to start of list
                _ownerIndex = 0;
                _slotIndex = 0;

                // Reset all tracked inventory counts
                foreach (var key in Program.Inventory.Keys.ToList())
                {
                    Program.Inventory[key] = 0f;
                }
            }

            /// <summary>
            /// Processes one inventory slot per invocation, counting items.
            /// Returns Continue if more work remains, Finished when complete.
            /// </summary>
            public override RunJobResult RunJob()
            {
                // If all owners have been processed, finish
                if (_ownerIndex >= _inventoryOwners.Count)
                    return RunJobResult.Finished;

                var owner = _inventoryOwners[_ownerIndex];
                var slotCount = owner.InventoryCount;

                // Process exactly one slot of the current owner
                if (_slotIndex < slotCount)
                {
                    var inv = owner.GetInventory(_slotIndex);
                    _itemsCache.Clear();
                    inv.GetItems(_itemsCache);

                    // Tally up each item in this slot
                    foreach (var item in _itemsCache)
                    {
                        if (Program.Inventory.ContainsKey(item.Type))
                            Program.Inventory[item.Type] += (float)item.Amount;
                        else
                            Program.Inventory[item.Type] = (float)item.Amount;
                    }
                }

                // Advance the slot cursor
                _slotIndex++;

                // If we've processed all slots, move to next owner
                if (_slotIndex >= slotCount)
                {
                    _slotIndex = 0;
                    _ownerIndex++;
                }

                // Determine whether there is more work to do
                return _ownerIndex >= _inventoryOwners.Count
                    ? RunJobResult.Finished
                    : RunJobResult.Continue;
            }
        }
    }
}
