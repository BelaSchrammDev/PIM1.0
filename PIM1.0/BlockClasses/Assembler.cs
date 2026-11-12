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
            bool RemoveItemMode = false;

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

                if (!IsFunctional)
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

                var queue = new List<MyProductionItem>();
                var queueBluePrints = new List<AssemblerBluePrint>();

                RefreshState(queue, queueBluePrints);

                if (!Parameter.ParseArgs(AssemblerBlock.CustomName, true) || !AssemblerBlock.IsFunctional)
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
            /// For every assembler, also the ones without tag: count the queue as "in production" and unload.
            /// </summary>
            void RefreshState(List<MyProductionItem> queue, List<AssemblerBluePrint> queueBluePrints)
            {
                if (AssemblerBlock.Mode == MyAssemblerMode.Assembly)
                {
                    var outInventory = AssemblerBlock.GetInventory(1);
                    ClearInventory(outInventory);
                    outputInventoryNotEmpty = outInventory.CurrentVolume > 0;
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
                else
                {
                    ClearInventory(AssemblerBlock.GetInventory(0));
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
                RemoveItemMode = !RemoveItemMode;
                if (RemoveItemMode)
                {
                    MoveMostImportantToFront(queue);
                }
                else if (Config.Instance.delete_queueItem_if_max)
                {
                    RemoveItemsAtMaximum(queue, queueBluePrints);
                }
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
