using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace IngameScript
{
    partial class Program
    {
        public class StateMachine
        {
            private readonly Program prg;

            // State machine variables
            private int currentState = 1;
            private int iterationIndex = 0;
            private int secondaryIndex = 0;
            private List<string> stringBuffer;
            private DateTime lastExecutionStart = DateTime.Now;
            private DateTime stackingCounterTime = DateTime.Now;
            private double currentCycleTimeSeconds = 0f;

            // State execution control
            private bool shouldExitExecution = false;
            private bool hasReachedInstructionLimit = false;

            public StateMachine(Program parentProgram)
            {
                prg = parentProgram;
            }

            public void ProcessCommand(string argument)
            {
                if (string.IsNullOrEmpty(argument)) return;

                switch (argument.ToLower())
                {
                    case "flushrefinerys_all":
                        foreach (var refinery in Program.RefineryList)
                            refinery.FlushAllInventorys();
                        Program.SetInfo($"all ({Program.RefineryList.Count}) refinerys flushed.");
                        break;
                    default:
                        Program.SetInfo($"unknown command: \"{argument}\"");
                        break;
                }
            }

            public void Execute()
            {
                shouldExitExecution = false;
                hasReachedInstructionLimit = false;

                do
                {
                    ExecuteCurrentState();
                    if (shouldExitExecution || hasReachedInstructionLimit)
                        break;
                }
                while (!CheckInstructionLimit());

                prg.writeInfo();
            }

            private void ExecuteCurrentState()
            {
                switch (currentState)
                {
                    case 0: ExecuteState_TimingControl(); break;
                    case 1: ExecuteState_AutoCraftingSetup(); break;
                    case 2: ExecuteState_BlueprintAssignment(); break;
                    case 3: ExecuteState_SystemInitialization(); break;
                    case 4: ExecuteState_CargoDiscovery(); break;
                    case 5: ExecuteState_CargoProcessing(); break;
                    case 6: ExecuteState_ConnectorDiscovery(); break;
                    case 7: ExecuteState_ConnectorProcessing(); break;
                    case 8: ExecuteState_ControllerDiscovery(); break;
                    case 9: ExecuteState_ControllerProcessing(); break;
                    case 10: ExecuteState_GunGroupManagement(); break;
                    case 11: ExecuteState_GunAddition(); break;
                    case 12: ExecuteState_GunRefreshPreparation(); break;
                    case 13: ExecuteState_GunRefresh(); break;
                    case 14: ExecuteState_StorageCargoManagement(); break;
                    case 15: ExecuteState_StorageCargoAddition(); break;
                    case 16:
                    case 17:
                    case 18:
                    case 19:
                    case 20:
                    case 21:
                    case 22:
                    case 23:
                        AdvanceState(); break;
                    case 24: ExecuteState_WelderDiscovery(); break;
                    case 25: ExecuteState_WelderProcessing(); break;
                    case 26: ExecuteState_GrinderDiscovery(); break;
                    case 27: ExecuteState_GrinderProcessing(); break;
                    case 28: ExecuteState_DrillDiscovery(); break;
                    case 29: ExecuteState_DrillProcessing(); break;
                    case 30: ExecuteState_StackingInitialization(); break;
                    case 31: ExecuteState_StackingExecution(); break;
                    case 32: AdvanceState(); break;
                    case 33: ExecuteState_RefinerySetup(); break;
                    case 34: ExecuteState_RefineryAddition(); break;
                    case 35: ExecuteState_AssemblerSetup(); break;
                    case 36: ExecuteState_AssemblerAddition(); break;
                    case 37: ExecuteState_InventoryPreparation(); break;
                    case 38: ExecuteState_SMSInventoryClearing(); break;
                    case 39: ExecuteState_NonSMSInventoryPreparation(); break;
                    case 40: ExecuteState_NonSMSInventoryClearing(); break;
                    case 41: ExecuteState_AmmunitionCalculation(); break;
                    case 42: ExecuteState_ItemTransfer(); break;
                    case 43: ExecuteState_RefineryBlueprintSetup(); break;
                    case 44: ExecuteState_RefineryRefresh(); break;
                    case 45: ExecuteState_PriorityCalculations(); break;
                    case 46: ExecuteState_RefineryManagement(); break;
                    case 47: ExecuteState_AssemblerPreparation(); break;
                    case 48: ExecuteState_AssemblerRefresh(); break;
                    case 49: ExecuteState_BlueprintProcessingSetup(); break;
                    case 50: ExecuteState_BlueprintProcessing(); break;
                    case 51: ExecuteState_BlueprintPoolSetup(); break;
                    case 52: ExecuteState_BlueprintPoolUpdate(); break;
                    case 53: ExecuteState_AssemblerSorting(); break;
                    case 54: ExecuteState_AssemblerQueueManagement(); break;
                    default: ExecuteState_SystemCompletion(); break;
                }
            }

            private bool CheckInstructionLimit()
            {
                hasReachedInstructionLimit = Program.rti.CurrentInstructionCount > prg.MAXIC;
                return hasReachedInstructionLimit;
            }

            private void ExitIfInstructionLimitReached()
            {
                if (CheckInstructionLimit())
                {
                    shouldExitExecution = true;
                    prg.writeInfo();
                }
            }

            private void AdvanceState()
            {
                currentState++;
            }

            private void ResetIterationIndex()
            {
                iterationIndex = 0;
            }

            private void ResetSecondaryIndex()
            {
                secondaryIndex = 0;
            }

            // State execution methods
            private void ExecuteState_TimingControl()
            {
                if (prg.MainLoopTimeSpan.IfTimeSpanReady())
                {
                    AdvanceState();
                    currentCycleTimeSeconds = (DateTime.Now - lastExecutionStart).TotalSeconds;
                    lastExecutionStart = DateTime.Now;
                }
                else
                {
                    shouldExitExecution = true;
                    prg.writeInfo();
                }
            }

            private void ExecuteState_AutoCraftingSetup()
            {
                if (!Program.changeAutoCraftingSettings)
                {
                    Program.debugString += "kein calc_ACDef\n";
                    currentState += 2;
                    return;
                }

                Program.debugString += "calc_ACDef\n";
                prg.GridTerminalSystem.GetBlocksOfType<IMyAssembler>(prg.ass, block => block.CubeGrid == prg.Me.CubeGrid);
                prg.GridTerminalSystem.GetBlocksOfType<IMyRefinery>(prg.raff, block => block.CubeGrid == prg.Me.CubeGrid);

                stringBuffer = new List<string>(Program.bprints.Keys);
                for (int i = stringBuffer.Count - 1; i >= 0; i--)
                {
                    var blueprint = Program.bprints[stringBuffer[i]];
                    Program.bprints_pool.Add(stringBuffer[i], blueprint);
                    Program.bprints.Remove(stringBuffer[i]);
                }

                stringBuffer = new List<string>(Program.bprints_pool.Keys);
                iterationIndex = stringBuffer.Count - 1;
                Program.changeAutoCraftingSettings = false;
                AdvanceState();
            }

            private void ExecuteState_BlueprintAssignment()
            {
                for (int i = iterationIndex; i >= 0; i--, iterationIndex--)
                {
                    ExitIfInstructionLimitReached();
                    if (shouldExitExecution) return;

                    var blueprint = Program.bprints_pool[stringBuffer[i]];
                    if (stringBuffer[i] == Program.Ingot.SubFresh || stringBuffer[i] == Program.Refinery.BluePrintID_SpentFuelReprocessing)
                    {
                        foreach (var refinery in prg.raff)
                        {
                            var subTypeName = refinery.BlockDefinition.SubtypeId;
                            if (refinery.CustomName.Contains("(sms") &&
                                (subTypeName.Contains("Hydroponics") || subTypeName.Contains("Reprocessor")))
                            {
                                Program.bprints.Add(stringBuffer[i], blueprint);
                                Program.bprints_pool.Remove(stringBuffer[i]);
                                break;
                            }
                        }
                    }
                    else
                    {
                        foreach (var assembler in prg.ass)
                        {
                            if (assembler.CustomName.Contains("(sms") && assembler.CanUseBlueprint(blueprint.definition_id))
                            {
                                Program.bprints.Add(stringBuffer[i], blueprint);
                                Program.bprints_pool.Remove(stringBuffer[i]);
                                break;
                            }
                        }
                    }
                }
                prg.InitAutoCraftingTypes();
                AdvanceState();
            }

            private void ExecuteState_SystemInitialization()
            {
                if (!prg.Slave())
                {
                    shouldExitExecution = true;
                    prg.writeInfo();
                    return;
                }

                prg.loadAutocratingDefinitions();
                prg.debug();
                Program.ClearInventoryList(Program.inventar);
                Program.InventoryList_SMSflagged.Clear();
                Program.InventoryList_nonSMSflagged.Clear();
                prg.CargoUseList.Clear();

                foreach (var inventoryList in Program.InventoryManagerList.Values)
                    inventoryList.Clear();
                Program.InventoryManagerList.Clear();

                AdvanceState();
            }

            private void ExecuteState_CargoDiscovery()
            {
                prg.GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(prg.tbl, block => block.IsSameConstructAs(prg.Me));
                ResetIterationIndex();
                AdvanceState();
            }

            private void ExecuteState_CargoProcessing()
            {
                for (int i = iterationIndex; i < prg.tbl.Count; i++, iterationIndex++)
                {
                    ExitIfInstructionLimitReached();
                    if (shouldExitExecution) return;
                    prg.pushTerminalBlock(prg.tbl[i]);
                }
                AdvanceState();
            }

            private void ExecuteState_ConnectorDiscovery()
            {
                prg.GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(prg.tbl, block => block.IsSameConstructAs(prg.Me));
                ResetIterationIndex();
                AdvanceState();
            }

            private void ExecuteState_ConnectorProcessing()
            {
                for (int i = iterationIndex; i < prg.tbl.Count; i++, iterationIndex++)
                {
                    ExitIfInstructionLimitReached();
                    if (shouldExitExecution) return;
                    prg.pushTerminalBlock(prg.tbl[i]);
                }
                AdvanceState();
            }

            private void ExecuteState_ControllerDiscovery()
            {
                prg.GridTerminalSystem.GetBlocksOfType<IMyShipController>(prg.tbl, block => block.IsSameConstructAs(prg.Me));
                ResetIterationIndex();
                AdvanceState();
            }

            private void ExecuteState_ControllerProcessing()
            {
                for (int i = iterationIndex; i < prg.tbl.Count; i++, iterationIndex++)
                {
                    ExitIfInstructionLimitReached();
                    if (shouldExitExecution) return;
                    prg.pushTerminalBlock(prg.tbl[i]);
                }
                AdvanceState();
            }

            private void ExecuteState_GunGroupManagement()
            {
                var group = prg.GridTerminalSystem.GetBlockGroupWithName(prg.gungroupName);
                if (group == null)
                {
                    if (Program.guns.Count > 0)
                    {
                        for (int i = Program.guns.Count - 1; i >= 0; i--)
                            Program.guns[i].Remove();
                        Program.guns.Clear();
                    }
                    currentState += 2; // Skip next state
                    return;
                }

                group.GetBlocksOfType<IMyUserControllableGun>(prg.ugun, block => block.IsSameConstructAs(prg.Me));

                for (int i = Program.guns.Count - 1; i >= 0; i--)
                {
                    if (prg.ugun.Contains(Program.guns[i].gun))
                        prg.ugun.Remove(Program.guns[i].gun);
                    else
                    {
                        var gun = Program.guns[i];
                        gun.Remove();
                        Program.guns.Remove(gun);
                    }
                }

                iterationIndex = prg.ugun.Count - 1;
                AdvanceState();
            }

            private void ExecuteState_GunAddition()
            {
                for (int i = iterationIndex; i >= 0; i--, iterationIndex--)
                {
                    ExitIfInstructionLimitReached();
                    if (shouldExitExecution) return;
                    guns.Add(new Gun(prg.ugun[i]));
                }
                AdvanceState();
            }

            private void ExecuteState_GunRefreshPreparation()
            {
                ResetIterationIndex();
                AdvanceState();
            }

            private void ExecuteState_GunRefresh()
            {
                for (int i = iterationIndex; i < guns.Count; i++, iterationIndex++)
                {
                    ExitIfInstructionLimitReached();
                    if (shouldExitExecution) return;
                    guns[i].Refresh();
                }
                AdvanceState();
            }

            private void ExecuteState_StorageCargoManagement()
            {
                prg.GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(prg.tbl,
                    cargo => cargo.CustomName.Contains(X_StorageTag));

                for (int i = storageCargos.Count - 1; i >= 0; i--)
                {
                    if (prg.tbl.Contains(storageCargos[i].container))
                        prg.tbl.Remove(storageCargos[i].container);
                    else
                    {
                        var storage = storageCargos[i];
                        storage.Remove();
                        storageCargos.Remove(storage);
                    }
                }

                iterationIndex = prg.tbl.Count - 1;
                AdvanceState();
            }

            private void ExecuteState_StorageCargoAddition()
            {
                for (int i = iterationIndex; i >= 0; i--, iterationIndex--)
                {
                    ExitIfInstructionLimitReached();
                    if (shouldExitExecution) return;
                    storageCargos.Add(new StorageCargo(prg.tbl[i]));
                }
                AdvanceState();
            }

            private void ExecuteState_WelderDiscovery()
            {
                prg.GridTerminalSystem.GetBlocksOfType<IMyShipWelder>(prg.tbl, block => block.IsSameConstructAs(prg.Me));
                ResetIterationIndex();
                AdvanceState();
            }

            private void ExecuteState_WelderProcessing()
            {
                for (int i = iterationIndex; i < prg.tbl.Count; i++, iterationIndex++)
                {
                    ExitIfInstructionLimitReached();
                    if (shouldExitExecution) return;

                    var blockDefinition = prg.tbl[i].BlockDefinition.SubtypeId.ToString();
                    if (blockDefinition.Contains("ShipLaserMultitool"))
                    {
                        bool weld = true;
                        var property = prg.tbl[i].GetProperty("ToolMode");
                        if (property != null && property.TypeName == "Boolean")
                            weld = prg.tbl[i].GetValue<bool>("ToolMode");

                        if (weld)
                        {
                            if (!(prg.tbl[i] as IMyFunctionalBlock).Enabled)
                                prg.pushTerminalBlock(prg.tbl[i]);
                        }
                        else prg.pushTerminalBlock(prg.tbl[i]);
                    }
                    else if (!(prg.tbl[i] as IMyFunctionalBlock).Enabled)
                        prg.pushTerminalBlock(prg.tbl[i]);
                }
                AdvanceState();
            }

            private void ExecuteState_GrinderDiscovery()
            {
                prg.GridTerminalSystem.GetBlocksOfType<IMyShipGrinder>(prg.tbl, block => block.IsSameConstructAs(prg.Me));
                ResetIterationIndex();
                AdvanceState();
            }

            private void ExecuteState_GrinderProcessing()
            {
                for (int i = iterationIndex; i < prg.tbl.Count; i++, iterationIndex++)
                {
                    ExitIfInstructionLimitReached();
                    if (shouldExitExecution) return;
                    prg.pushTerminalBlock(prg.tbl[i]);
                }
                AdvanceState();
            }

            private void ExecuteState_DrillDiscovery()
            {
                prg.GridTerminalSystem.GetBlocksOfType<IMyShipDrill>(prg.tbl, block => block.IsSameConstructAs(prg.Me));
                ResetIterationIndex();
                AdvanceState();
            }

            private void ExecuteState_DrillProcessing()
            {
                for (int i = iterationIndex; i < prg.tbl.Count; i++, iterationIndex++)
                {
                    ExitIfInstructionLimitReached();
                    if (shouldExitExecution) return;
                    prg.pushTerminalBlock(prg.tbl[i]);
                }
                AdvanceState();
            }

            private void ExecuteState_StackingInitialization()
            {
                ResetIterationIndex();
                if (prg.stacking_cycle == 0 ||
                    (DateTime.Now - stackingCounterTime).TotalSeconds < prg.stacking_cycle)
                {
                    currentState += 2; // Skip stacking
                }
                else
                {
                    StackItemList.Clear();
                    StackItem.ClearStackInventory();

                    if (InventoryManagerList.ContainsKey(prg.stack_type))
                    {
                        foreach (var inventory in InventoryManagerList[prg.stack_type])
                        {
                            prg.new_stackcount(inventory, prg.stack_type);
                            StackItem.CalculateFreeInventory(inventory);
                        }
                    }

                    // Initialize stacking mode logic
                    InitializeStackingMode();
                    stackingCounterTime = DateTime.Now;
                    ResetIterationIndex();
                }
                AdvanceState();
            }

            private void InitializeStackingMode()
            {
                if (prg.stack_mode == 0 || prg.stack_mode == 1)
                {
                    int maxStack = 0;
                    foreach (var stackItem in StackItemList)
                        if (maxStack < stackItem.Stackcount) maxStack = stackItem.Stackcount;

                    if (maxStack > 1)
                    {
                        StackItem.CurrentStackingType = StackItem.StackingType.Stack;
                        StackItemList.Sort();
                    }
                    else
                    {
                        prg.stack_mode = -1;
                    }
                }
                else if (prg.stack_mode == 2)
                {
                    StackItem.CurrentStackingType = StackItem.StackingType.VolumeBack;
                    StackItemList.Sort();
                }
                else if (prg.stack_mode == 3)
                {
                    StackItem.CurrentStackingType = StackItem.StackingType.Volume;
                    StackItemList.Sort();
                }
                else if (prg.stack_mode == 4)
                {
                    StackItem.CurrentStackingType = StackItem.StackingType.Stack;
                    StackItemList.Sort();
                    while (StackItemList.Count > 0)
                    {
                        var stackItem = StackItemList[0];
                        if (stackItem.check_stacking_gamma()) break;
                        StackItemList.Remove(stackItem);
                    }
                }
            }

            private void ExecuteState_StackingExecution()
            {
                switch (prg.stack_mode)
                {
                    case 0:
                        for (int i = iterationIndex; i < StackItemList.Count; i++, iterationIndex++)
                        {
                            ExitIfInstructionLimitReached();
                            if (shouldExitExecution) return;
                            StackItemList[i].stacking_single();
                        }
                        AdvanceState();
                        prg.stack_mode++;
                        break;
                    case 1:
                        for (int i = iterationIndex; i < StackItemList.Count; i++, iterationIndex++)
                        {
                            ExitIfInstructionLimitReached();
                            if (shouldExitExecution) return;
                            StackItemList[i].stacking_alpha();
                        }
                        AdvanceState();
                        prg.stack_mode++;
                        break;
                    case 2:
                        for (int i = iterationIndex; i < StackItemList.Count; i++, iterationIndex++)
                        {
                            ExitIfInstructionLimitReached();
                            if (shouldExitExecution) return;
                            if (!StackItemList[i].stacking_beta()) break;
                        }
                        AdvanceState();
                        prg.stack_mode++;
                        break;
                    case 3:
                        for (int i = iterationIndex; i < StackItemList.Count; i++, iterationIndex++)
                        {
                            ExitIfInstructionLimitReached();
                            if (shouldExitExecution) return;
                            StackItemList[i].stacking_delta();
                        }
                        AdvanceState();
                        prg.stack_mode++;
                        break;
                    case 4:
                        if (StackItemList.Count > 0)
                        {
                            for (int i = iterationIndex; i < 300; i++, iterationIndex++)
                            {
                                ExitIfInstructionLimitReached();
                                if (shouldExitExecution) return;
                                if (StackItemList[0].stacking_gamma()) break;
                            }
                        }
                        AdvanceState();
                        prg.stack_mode++;
                        break;
                    default:
                        prg.stack_mode = 0;
                        prg.cur_stack_type++;
                        if (prg.cur_stack_type >= stack_types.Length)
                            prg.cur_stack_type = 0;
                        prg.stack_type = stack_types[prg.cur_stack_type];
                        AdvanceState();
                        break;
                }
            }

            private void ExecuteState_RefinerySetup()
            {
                foreach (var blueprint in bprints.Values) blueprint.AssemblyAmount = 0;
                foreach (var blueprint in bprints_pool.Values) blueprint.AssemblyAmount = 0;

                prg.GridTerminalSystem.GetBlocksOfType<IMyRefinery>(prg.raff, block => block.CubeGrid == prg.Me.CubeGrid);

                for (int i = RefineryList.Count - 1; i >= 0; i--)
                {
                    if (prg.raff.Contains(RefineryList[i].RefineryBlock))
                        prg.raff.Remove(RefineryList[i].RefineryBlock);
                    else
                    {
                        changeAutoCraftingSettings = true;
                        RefineryList.Remove(RefineryList[i]);
                    }
                }

                Refinery.priobt = "";
                iterationIndex = prg.raff.Count - 1;
                AdvanceState();
            }

            private void ExecuteState_RefineryAddition()
            {
                for (int i = iterationIndex; i >= 0; i--, iterationIndex--)
                {
                    ExitIfInstructionLimitReached();
                    if (shouldExitExecution) return;
                    RefineryList.Add(new Program.Refinery(prg.raff[i]));
                }
                AdvanceState();
            }

            private void ExecuteState_AssemblerSetup()
            {
                prg.GridTerminalSystem.GetBlocksOfType<IMyAssembler>(prg.ass, block => block.CubeGrid == prg.Me.CubeGrid);

                for (int i = AssemblerList.Count - 1; i >= 0; i--)
                {
                    if (prg.ass.Contains(AssemblerList[i].AssemblerBlock))
                        prg.ass.Remove(AssemblerList[i].AssemblerBlock);
                    else
                    {
                        changeAutoCraftingSettings = true;
                        AssemblerList.Remove(AssemblerList[i]);
                    }
                }

                iterationIndex = prg.ass.Count - 1;
                AdvanceState();
            }

            private void ExecuteState_AssemblerAddition()
            {
                for (int i = iterationIndex; i >= 0; i--, iterationIndex--)
                {
                    ExitIfInstructionLimitReached();
                    if (shouldExitExecution) return;
                    AssemblerList.Add(new Assembler(prg.ass[i]));
                }
                AdvanceState();
            }

            private void ExecuteState_InventoryPreparation()
            {
                ResetIterationIndex();
                AdvanceState();
            }

            private void ExecuteState_SMSInventoryClearing()
            {
                for (int i = iterationIndex; i < InventoryList_SMSflagged.Count; i++, iterationIndex++)
                {
                    ExitIfInstructionLimitReached();
                    if (shouldExitExecution) return;
                    ClearInventory(InventoryList_SMSflagged[i]);
                }
                AdvanceState();
            }

            private void ExecuteState_NonSMSInventoryPreparation()
            {
                ResetIterationIndex();
                AdvanceState();
            }

            private void ExecuteState_NonSMSInventoryClearing()
            {
                for (int i = iterationIndex; i < InventoryList_nonSMSflagged.Count; i++, iterationIndex++)
                {
                    ExitIfInstructionLimitReached();
                    if (shouldExitExecution) return;
                    ClearInventory(InventoryList_nonSMSflagged[i], prg.collectAll_List);
                }
                AdvanceState();
            }

            private void ExecuteState_AmmunitionCalculation()
            {
                ResetIterationIndex();
                foreach (var ammoDef in ammoDefs.Values)
                    ammoDef.CalcAmmoInventoryRatio();
                AdvanceState();
            }

            private void ExecuteState_ItemTransfer()
            {
                for (int i = iterationIndex; i < storageinvs.Count; i++, iterationIndex++)
                {
                    ExitIfInstructionLimitReached();
                    if (shouldExitExecution) return;
                    storageinvs[i].reloadItems();
                }
                AdvanceState();
            }

            private void ExecuteState_RefineryBlueprintSetup()
            {
                Refinery.cn = 0;
                foreach (var blueprint in RefineryBlueprints)
                    blueprint.RefineryCount = 0;
                ResetIterationIndex();
                AdvanceState();
            }

            private void ExecuteState_RefineryRefresh()
            {
                for (int i = iterationIndex; i < RefineryList.Count; i++, iterationIndex++)
                {
                    ExitIfInstructionLimitReached();
                    if (shouldExitExecution) return;
                    RefineryList[i].Refresh();
                }
                AdvanceState();
            }

            private void ExecuteState_PriorityCalculations()
            {
                prg.CalcIngotPrio();
                prg.RenderAmmoPrioLCDS();
                prg.RenderResourceProccesingLCD();
                ResetIterationIndex();
                AdvanceState();
            }

            private void ExecuteState_RefineryManagement()
            {
                for (int i = iterationIndex; i < RefineryList.Count; i++, iterationIndex++)
                {
                    ExitIfInstructionLimitReached();
                    if (shouldExitExecution) return;
                    RefineryList[i].RefineryManager();
                }
                AdvanceState();
            }

            private void ExecuteState_AssemblerPreparation()
            {
                ResetIterationIndex();
                AdvanceState();
            }

            private void ExecuteState_AssemblerRefresh()
            {
                for (int i = iterationIndex; i < AssemblerList.Count; i++, iterationIndex++)
                {
                    ExitIfInstructionLimitReached();
                    if (shouldExitExecution) return;
                    AssemblerList[i].Refresh();
                }
                AdvanceState();
            }

            private void ExecuteState_BlueprintProcessingSetup()
            {
                stringBuffer = new List<string>(bprints.Keys);
                ResetIterationIndex();
                AdvanceState();
            }

            private void ExecuteState_BlueprintProcessing()
            {
                for (int i = iterationIndex; i < stringBuffer.Count; i++, iterationIndex++)
                {
                    ExitIfInstructionLimitReached();
                    if (shouldExitExecution) return;

                    var blueprint = bprints[stringBuffer[i]];
                    blueprint.SetCurrentAmount((int)inventar.GetValueOrDefault(blueprint.ItemName, 0));
                    blueprint.CalcPriority();

                    if (blueprint.NeedsAssembling())
                    {
                        if (stringBuffer[i] != Ingot.SubFresh)
                        {
                            foreach (var assembler in AssemblerList)
                                assembler.AddValidBlueprint(blueprint);
                        }
                    }
                }
                AdvanceState();
            }

            private void ExecuteState_BlueprintPoolSetup()
            {
                stringBuffer = new List<string>(bprints_pool.Keys);
                ResetIterationIndex();
                AdvanceState();
            }

            private void ExecuteState_BlueprintPoolUpdate()
            {
                for (int i = iterationIndex; i < stringBuffer.Count; i++, iterationIndex++)
                {
                    ExitIfInstructionLimitReached();
                    if (shouldExitExecution) return;

                    var blueprint = bprints_pool[stringBuffer[i]];
                    if (inventar.ContainsKey(blueprint.ItemName))
                        blueprint.SetCurrentAmount((int)inventar[blueprint.ItemName]);
                }
                AdvanceState();
            }

            private void ExecuteState_AssemblerSorting()
            {
                ResetIterationIndex();
                ResetSecondaryIndex();
                AssemblerList.Sort();
                AdvanceState();
            }

            private void ExecuteState_AssemblerQueueManagement()
            {
                for (int i = iterationIndex; i < AssemblerList.Count; i++, iterationIndex++)
                {
                    ExitIfInstructionLimitReached();
                    if (shouldExitExecution) return;

                    var assembler = AssemblerList[i];
                    // Additional assembler queue management logic would continue here
                    // This is a simplified version - the full implementation would be more complex
                }
                AdvanceState();
            }

            private void ExecuteState_SystemCompletion()
            {
                prg.CalcutateInfos();
                prg.firstRun = false;
                currentState = 0; // Reset to beginning
            }

            // Public properties for monitoring
            public int CurrentState => currentState;
            public bool IsExecuting => !shouldExitExecution && !hasReachedInstructionLimit;
            public double CurrentCycleTime => currentCycleTimeSeconds;
        }
    }
}
