using System.Collections.Generic;
using VRage;
using VRage.Game.ModAPI.Ingame;
using static IngameScript.Program;
using static IngameScript.Program.Refinery;

namespace IngameScript
{
    partial class Program
    {
        public class VanillaRefineryManager : BlockManager
        {
            enum ManagerState
            {
                CLEAR_INPUT,
                SWAP_ORES,
            };

            ManagerState CurrentRefineryState = ManagerState.CLEAR_INPUT;

            private Refinery _Refinery => (Refinery)Block;

            public VanillaRefineryManager(ManageableBlock block) : base(block)
            {
            }

            public override void DoManage()
            {
                Program.LCD_DebugString += "VanillaRefineryManager_DoManage\n";

                if (!ingotprio.ContainsKey(_Refinery.BlockSubType))
                {
                    return;
                }

                if (_Refinery.fertig > 80 || IfForceManagerExecuting())
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
                _Refinery.InputInventory.GetItems(inventoryItems);
                if (inventoryItems.Count == 0) return;
                for (int i = inventoryItems.Count - 1; i >= 0; i--)
                {
                    var inventoryItem = inventoryItems[i];
                    var itemType = GetPIMItemID(inventoryItem.Type);
                    var refBP = _Refinery.AcceptedBlueprints.Find(b => b.InputID == itemType);
                    if (refBP == null) continue;
                    var itemIPrio = IPrio.GetBlueprintPrio(ingotprio[_Refinery.BlockSubType], refBP);
                    if (itemIPrio != null && itemIPrio.initp == 0)
                    {
                        clearItemByType(_Refinery.InputInventory, itemType, inventoryItem);
                    }
                }
            }

            public void OreSwap()
            {
                var inventoryItems = new List<MyInventoryItem>();
                var inventory = _Refinery.RefineryBlock.GetInventory(0);
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
                        foreach (IPrio p in ingotprio[_Refinery.BlockSubType])
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
                List<IPrio> ingotPrioList = ingotprio[_Refinery.BlockSubType];

                for (int index = 0; index < ingotPrioList.Count; index++)
                {
                    IPrio ingotPrio = ingotPrioList[index];

                    if (!_Refinery.Accept(ingotPrio.refineryBP) || ingotPrio.prio == 0 || !Lists.Data.inventar.ContainsKey(ingotPrio.refineryBP.InputID))
                    {
                        continue;
                    }

                    newworkBP = ingotPrio.refineryBP;

                    if (_Refinery.fertig < 50)
                    {
                        _Refinery.ClearInputInventoryIfControledByPIM();
                    }

                    var types = newworkBP.InputID.Split(' ');

                    if (Lists.Data.inventar.ContainsKey(newworkBP.InputID))
                    {
                        refineryFilled = SendItemByTypeAndSubtype("MyObjectBuilder_" + types[0], types[1], Lists.Data.inventar[newworkBP.InputID], _Refinery.RefineryBlock.GetInventory(0));
                    }

                    if (!refineryFilled)
                    {
                        refineryFilled = OreStealing(newworkBP);
                    }

                    _Refinery.SetErrorByCondition(Refinery.RefError.NotFilled, !refineryFilled && _Refinery.InputInventory.CurrentVolume == 0);
                }

                if (refineryFilled)
                {
                    SetIngotPrio(ingotPrioList, newworkBP);
                }
            }


            bool OreStealing(RefineryBlueprint blueprint)
            {
                var oamount = 0f;
                if (blueprint == _Refinery.CurrentWorkBluePrint) oamount = _Refinery.CurrentWorkOreAmount;
                else if (blueprint == _Refinery.NextWorkBluePrint) oamount = _Refinery.NexWorkOreAmount;
                if (_Refinery.InputInventory.CurrentVolume.RawValue < 100)
                {
                    foreach (Refinery refinery in Lists.Data.RefineryList)
                    {
                        if (refinery.BlockRemoved()) continue;
                        int inum = 0;
                        var inventoryList = new List<MyInventoryItem>();
                        refinery.InputInventory.GetItems(inventoryList);
                        foreach (var inventoryItem in inventoryList)
                        {
                            var ostrID = _Refinery.AcceptedBlueprints.Find(b => b.InputID == GetPIMItemID(inventoryItem.Type));
                            if (blueprint == ostrID && (float)inventoryItem.Amount > 100 && (float)inventoryItem.Amount > oamount && _Refinery.Accept(ostrID))
                            {
                                var amount = MyFixedPoint.MultiplySafe(inventoryItem.Amount, (inum == 0 ? 0.5f : 1f));
                                var xx = refinery.InputInventory.TransferItemTo(_Refinery.InputInventory, inum, null, true, amount);
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
                foreach (IPrio p in ingotprio[_Refinery.BlockSubType])
                {
                    if (p.refineryBP == _Refinery.CurrentWorkBluePrint || p.refineryBP == _Refinery.NextWorkBluePrint) op = op < p.prio ? (int)(p.prio * 1.5) : op;
                    else if (wp100 == 0 && _Refinery.Accept(p.refineryBP)) wp100 = p.prio;
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
