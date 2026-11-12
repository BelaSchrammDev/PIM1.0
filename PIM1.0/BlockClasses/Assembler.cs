using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRage;
using VRage.Game;

namespace IngameScript
{
    partial class Program
    {
        public class Assembler : ProductionBlockWithInventorys
        {
            public static Dictionary<string, List<AssemblerBluePrint>> AssemblerTypesAcceptedBluePrints = new Dictionary<string, List<AssemblerBluePrint>>();

            public List<AssemblerBluePrint> OwnBlueprintList;
            public IMyAssembler AssemblerBlock;
            public string SubTypeName;
            bool outputInventoryNotEmpty = false;
            bool IsSurvivalKit = false;
            // ManageQueue alternates: true = move the most important item to the front, false = remove the items at their maximum
            bool moveToFrontNext = true;

            public Assembler(IMyAssembler a)
            {
                AssemblerBlock = a;
                SubTypeName = a.BlockDefinition.SubtypeName;
                IsSurvivalKit = a.BlockDefinition.TypeIdString == "SurvivalKit";
            }

            public override IMyFunctionalBlock GetFunctionalBlock()
            {
                return AssemblerBlock;
            }

            public bool AddQueueItemSave(AssemblerBluePrint bluePrint, MyFixedPoint amountPerAssembler)
            {
                try
                {
                    if (bluePrint.valid)
                    {
                        AssemblerBlock.AddQueueItem(bluePrint.definition_id, (MyFixedPoint)amountPerAssembler);
                        return true;
                    }
                }
                catch
                {
                    bluePrint.valid = false;
                }
                return false;
            }

            public void GetErrorInfo(StringBuilderExtended errString)
            {
                if (IsClosed)
                {
                    return;
                }

                if (outputInventoryNotEmpty)
                {
                    errString.Append(Parameter.Name);
                    errString.Append(" cannot unload output items.\n");
                }

                // only for tagged assemblers, like the refinerys: a damaged block PIM does not use is no PIM problem
                if (IsPimControlled && !IsFunctional)
                {
                    errString.Append(Parameter.Name);
                    errString.Append(" is damaged.\n");
                }
            }

            public void Refresh()
            {
                if (IsClosed)
                {
                    return;
                }

                var wasPimControlled = IsPimControlled;
                var pimControlled = Parameter.ParseArgs(AssemblerBlock.CustomName, true);
                if (wasPimControlled && !pimControlled) Release();

                var queue = new List<MyProductionItem>();
                var queueBluePrints = new List<AssemblerBluePrint>();

                RefreshState(queue, queueBluePrints);

                if (!pimControlled || !AssemblerBlock.IsFunctional)
                {
                    return;
                }

                if (AssemblerBlock.IsQueueEmpty)
                {
                    SetIdle();
                }
                else
                {
                    AssemblerBlock.Enabled = true;

                    if (AssemblerBlock.Mode == MyAssemblerMode.Assembly)
                    {
                        ManageQueue(queue, queueBluePrints);
                    }
                }
            }

            /// <summary>
            /// The tag was removed: give the block back to the player switched on, PIM switches it off while
            /// idle (assemblers_off). Survival kits are never switched off by PIM.
            /// </summary>
            void Release()
            {
                if (!IsSurvivalKit) AssemblerBlock.Enabled = true;
            }

            /// <summary>
            /// For every assembler, also the ones without tag: count the queue as "in production" and unload.
            /// </summary>
            void RefreshState(List<MyProductionItem> queue, List<AssemblerBluePrint> queueBluePrints)
            {
                var isAssembly = AssemblerBlock.Mode == MyAssemblerMode.Assembly;

                // disassembling puts the components into the input inventory (0)
                var outInventory = AssemblerBlock.GetInventory(isAssembly ? 1 : 0);
                ClearInventory(outInventory);
                outputInventoryNotEmpty = outInventory.CurrentVolume > 0;

                if (!isAssembly) return;

                AssemblerBlock.GetQueue(queue);
                for (int i = queue.Count - 1; i >= 0; i--)
                {
                    var bprint = Program.MyProgram.AddProductionAmount(queue[i]);
                    if (bprint != null)
                    {
                        queueBluePrints.Add(bprint);
                    }
                }
            }

            /// <summary>Empty queue: switch off (config) and empty the inventory that is not unloaded while working.</summary>
            void SetIdle()
            {
                if (!IsSurvivalKit)
                {
                    AssemblerBlock.Enabled = !Config.Instance.assemblers_off || Parameter.IsParameter("Nooff");
                }

                ClearInventory(AssemblerBlock.GetInventory(AssemblerBlock.Mode == MyAssemblerMode.Disassembly ? 1 : 0));
            }

            /// <summary>Alternates between removing items that reached their maximum and moving the most important one to the front.</summary>
            void ManageQueue(List<MyProductionItem> queue, List<AssemblerBluePrint> queueBluePrints)
            {
                if (moveToFrontNext)
                {
                    MoveMostImportantToFront(queue);
                }
                else if (Config.Instance.delete_queueItem_if_max)
                {
                    RemoveItemsAtMaximum(queue, queueBluePrints);
                }
                moveToFrontNext = !moveToFrontNext;
            }

            void RemoveItemsAtMaximum(List<MyProductionItem> queue, List<AssemblerBluePrint> queueBluePrints)
            {
                for (int i = queue.Count - 1; i >= 0; i--)
                {
                    var productionItem = queue[i];
                    foreach (var bprint in queueBluePrints)
                    {
                        if (bprint.definition_id.SubtypeName == productionItem.BlueprintId.SubtypeName
                            && bprint.MaximumItemAmount != 0
                            && bprint.MaximumItemAmount <= bprint.CurrentItemAmount)
                        {
                            AssemblerBlock.RemoveQueueItem(i, productionItem.Amount);
                            queue.RemoveAt(i);
                            // the item is gone: without the break a blueprint that is in the queue
                            // twice (and so twice in queueBluePrints) would remove index i again
                            break;
                        }
                    }
                }
            }

            void MoveMostImportantToFront(List<MyProductionItem> queue)
            {
                var firstBlueprint = queue.Count > 1 ? Program.MyProgram.GetBluePrintByProductionItem(queue[0]) : null;
                if (firstBlueprint == null) return;

                for (int i = queue.Count - 1; i > 0; i--)
                {
                    var productionItemBlueprint = Program.MyProgram.GetBluePrintByProductionItem(queue[i]);
                    if (productionItemBlueprint != null
                        && productionItemBlueprint.MaximumItemAmount != 0
                        && productionItemBlueprint.ItemPriority > firstBlueprint.ItemPriority)
                    {
                        AssemblerBlock.MoveQueueItemRequest(queue[i].ItemId, 0);
                        break;
                    }
                }
            }
        }
    }
}
