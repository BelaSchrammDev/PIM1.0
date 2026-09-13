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

                var proditem_list = new List<MyProductionItem>();
                var bprint_list = new List<AssemblerBluePrint>();

                if (AssemblerBlock.Mode == MyAssemblerMode.Assembly)
                {
                    var outInventory = AssemblerBlock.GetInventory(1);
                    ClearInventory(outInventory);
                    outputInventoryNotEmpty = outInventory.CurrentVolume > 0;
                    AssemblerBlock.GetQueue(proditem_list);
                    for (int i = proditem_list.Count - 1; i >= 0; i--)
                    {
                        var bprint = Program.Instance.AddProductionAmount(proditem_list[i]);
                        if (bprint != null)
                        {
                            bprint_list.Add(bprint);
                        }
                    }
                }
                else
                {
                    ClearInventory(AssemblerBlock.GetInventory(0));
                }

                if (!Parameter.ParseArgs(AssemblerBlock.CustomName, true))
                {
                    return;
                }

                if (AssemblerBlock.IsFunctional)
                {
                    if (AssemblerBlock.IsQueueEmpty)
                    {
                        if (!IsSurvivalKit)
                        {
                            AssemblerBlock.Enabled = !Config.Instance.assemblers_off || Parameter.IsParameter("Nooff");
                        }

                        if (AssemblerBlock.Mode == MyAssemblerMode.Disassembly)
                        {
                            ClearInventory(AssemblerBlock.GetInventory(1));
                        }
                        else
                        {
                            ClearInventory(AssemblerBlock.GetInventory(0));
                        }
                    }
                    else
                    {
                        AssemblerBlock.Enabled = true;

                        if (AssemblerBlock.Mode == MyAssemblerMode.Assembly)
                        {
                            if (RemoveItemMode)
                            {
                                RemoveItemMode = false;
                                if (Config.Instance.delete_queueItem_if_max)
                                {
                                    for (int i = proditem_list.Count - 1; i >= 0; i--)
                                    {
                                        var productionItem = proditem_list[i];
                                        foreach (var bprint in bprint_list)
                                        {
                                            if (bprint.definition_id.SubtypeName == productionItem.BlueprintId.SubtypeName
                                                && bprint.MaximumItemAmount != 0
                                                && bprint.MaximumItemAmount <= bprint.CurrentItemAmount)
                                            {
                                                AssemblerBlock.RemoveQueueItem(i, productionItem.Amount);
                                                proditem_list.RemoveAt(i);
                                            }
                                        }

                                    }
                                }
                            }
                            else
                            {
                                RemoveItemMode = true;
                                AssemblerBluePrint firstBlueprint = proditem_list.Count > 1 ? Program.Instance.GetBluePrintByProductionItem(proditem_list[0]) : null;
                                if (firstBlueprint != null)
                                {
                                    for (int i = proditem_list.Count - 1; i > 0; i--)
                                    {
                                        var productionItemBlueprint = Program.Instance.GetBluePrintByProductionItem(proditem_list[i]);
                                        if (productionItemBlueprint != null
                                            && productionItemBlueprint.MaximumItemAmount != 0
                                            && productionItemBlueprint.ItemPriority > firstBlueprint.ItemPriority)
                                        {
                                            // Move
                                            AssemblerBlock.MoveQueueItemRequest(proditem_list[i].ItemId, 0);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
