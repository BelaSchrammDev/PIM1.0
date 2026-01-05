using System.Collections.Generic;
using VRage;
using VRage.Game.ModAPI.Ingame;
using static IngameScript.Program;
using static IngameScript.Program.Refinery;

namespace IngameScript
{
    partial class Program
    {
        public class VanillaRefineryManager : RefineryBlockManagerer
        {
            enum ManagerState
            {
                CLEAR_INPUT,
                SWAP_ORES,
            };

            ManagerState CurrentRefineryState = ManagerState.CLEAR_INPUT;

            public VanillaRefineryManager(ManageableBlock block) : base(block)
            {
            }

            public override void DoManage()
            {
                Program.LCD_DebugString += "VanillaRefineryManager_DoManage\n";

                if (!ingotprio.ContainsKey(Refinery.BlockSubType))
                {
                    return;
                }

                if (Refinery.Success > 80 || IfForceManagerExecuting())
                {
                    RefineryFilling();
                }

                if (CurrentRefineryState == ManagerState.CLEAR_INPUT)
                {
                    CurrentRefineryState = ManagerState.SWAP_ORES;
                    ClearInput();
                }
                else if (CurrentRefineryState == ManagerState.SWAP_ORES)
                {
                    CurrentRefineryState = ManagerState.CLEAR_INPUT;
                    OreSwap();
                }
            }

            public void ClearInput()
            {
                var inventoryItems = new List<MyInventoryItem>();
                Refinery.InputInventory.GetItems(inventoryItems);
                if (inventoryItems.Count == 0) return;
                for (int i = inventoryItems.Count - 1; i >= 0; i--)
                {
                    var inventoryItem = inventoryItems[i];
                    var itemType = GetPIMItemID(inventoryItem.Type);
                    var refBP = Refinery.AcceptedBlueprints.Find(b => b.InputID == itemType);
                    if (refBP == null) continue;
                    var itemIPrio = IPrio.GetBlueprintPrio(ingotprio[Refinery.BlockSubType], refBP);
                    if (itemIPrio != null && itemIPrio.initp == 0)
                    {
                        clearItemByType(Refinery.InputInventory, itemType, inventoryItem);
                    }
                }
            }

            public bool Accept(RefineryBlueprint ore)
            {
                if (Refinery.RefineryBlock.GetInventory(0).IsFull) return false;
                return Refinery.AcceptedBlueprints.Contains(ore);
            }

            public void OreSwap()
            {
                var inventoryItems = new List<MyInventoryItem>();
                var inventory = Refinery.RefineryBlock.GetInventory(0);
                inventory.GetItems(inventoryItems);

                if (inventoryItems.Count > 1)
                {
                    var scrap = -1;
                    for (int index = 0; index < inventoryItems.Count; index++)
                    {
                        if (inventoryItems[index].Type.SubtypeId.ToLower().Contains("scrap")) 
                        {
                            scrap = index; 
                            break; 
                        }
                    }

                    if (scrap == -1)
                    {
                        int p1 = 0, p2 = 0;
                        var ostrID1 = GetPIMItemID(inventoryItems[0].Type);
                        var ostrID2 = GetPIMItemID(inventoryItems[1].Type);
                        foreach (IPrio p in ingotprio[Refinery.BlockSubType])
                        {
                            if (ostrID1 == p.refineryBP.InputID) p1 = p.initp;
                            else if (ostrID2 == p.refineryBP.InputID) p2 = p.initp;
                        }
                        if (p1 < p2)
                        {
                            inventory.TransferItemTo(inventory, 0, 1, true, inventoryItems[0].Amount);
                        }

                        if (inventoryItems.Count == 3)
                        {
                            clearItemByType(inventory, GetPIMItemID(inventoryItems[2].Type), inventoryItems[2]);
                        }
                    }
                    else
                    {
                        inventory.TransferItemTo(inventory, scrap, 0, true, inventoryItems[scrap].Amount);
                    }
                }
            }

            public void RefineryFilling()
            {
                bool refineryFilled = false;
                RefineryBlueprint newworkBP = null;
                List<IPrio> ingotPrioList = ingotprio[Refinery.BlockSubType];

                for (int index = 0; index < ingotPrioList.Count; index++)
                {
                    IPrio ingotPrio = ingotPrioList[index];

                    if (!Accept(ingotPrio.refineryBP) || ingotPrio.prio == 0 || !Lists.Data.Inventory.ContainsKey(ingotPrio.refineryBP.InputID))
                    {
                        continue;
                    }

                    newworkBP = ingotPrio.refineryBP;

                    if (Refinery.Success < 50)
                    {
                        Refinery.ClearInputInventoryIfControledByPIM();
                    }

                    var types = newworkBP.InputID.Split(' ');

                    if (Lists.Data.Inventory.ContainsKey(newworkBP.InputID))
                    {
                        refineryFilled = SendItemByTypeAndSubtype("MyObjectBuilder_" + types[0], types[1], Lists.Data.Inventory[newworkBP.InputID], Refinery.RefineryBlock.GetInventory(0));
                    }

                    if (!refineryFilled)
                    {
                        refineryFilled = OreStealing(newworkBP);
                    }

                    Refinery.SetErrorByCondition(Refinery.RefError.NotFilled, !refineryFilled && Refinery.InputInventory.CurrentVolume == 0);
                }

                if (refineryFilled)
                {
                    SetIngotPrio(ingotPrioList, newworkBP);
                }
            }


            bool OreStealing(RefineryBlueprint blueprint)
            {
                var oamount = 0f;
                if (blueprint == Refinery.CurrentWorkBluePrint) oamount = Refinery.CurrentWorkOreAmount;
                else if (blueprint == Refinery.NextWorkBluePrint) oamount = Refinery.NexWorkOreAmount;
                if (Refinery.InputInventory.CurrentVolume.RawValue < 100)
                {
                    foreach (Refinery refinery in Lists.Data.RefineryList)
                    {
                        if (refinery.BlockRemoved()) continue;
                        int inum = 0;
                        var inventoryList = new List<MyInventoryItem>();
                        refinery.InputInventory.GetItems(inventoryList);
                        foreach (var inventoryItem in inventoryList)
                        {
                            var ostrID = Refinery.AcceptedBlueprints.Find(b => b.InputID == GetPIMItemID(inventoryItem.Type));
                            if (blueprint == ostrID && (float)inventoryItem.Amount > 100 && (float)inventoryItem.Amount > oamount && Accept(ostrID))
                            {
                                var amount = MyFixedPoint.MultiplySafe(inventoryItem.Amount, (inum == 0 ? 0.5f : 1f));
                                var xx = refinery.InputInventory.TransferItemTo(Refinery.InputInventory, inum, null, true, amount);
                                if (xx) return true;
                            }
                            inum++;
                        }
                    }
                }
                return false;
            }

            public bool IfForceManagerExecuting()
            {
                int wp100 = 0;
                int op = 0;
                foreach (IPrio p in ingotprio[Refinery.BlockSubType])
                {
                    if (p.refineryBP == Refinery.CurrentWorkBluePrint || p.refineryBP == Refinery.NextWorkBluePrint) op = op < p.prio ? (int)(p.prio * 1.5) : op;
                    else if (wp100 == 0 && Accept(p.refineryBP)) wp100 = p.prio;
                }
                return op < wp100;
            }

            static void SetIngotPrio(List<IPrio> ingotPrioList, RefineryBlueprint bluePrint)
            {
                IPrio ingotPriot = null;
                if (ingotPrioList.Count > 0 && null != (ingotPriot = IPrio.GetBlueprintPrio(ingotPrioList, bluePrint)))
                {
                    if (ingotPriot.prio < 100)
                    {
                        ingotPriot.prio -= ingotPriot.prio / bluePrint.RefineryCount;
                        if (ingotPriot.prio < 0) ingotPriot.prio = 1;
                        ingotPrioList.Sort();
                    }
                }
            }
        }
    }
}
