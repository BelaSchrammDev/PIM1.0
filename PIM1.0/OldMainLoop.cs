using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public class GridInventoryScanningJob : CountingJob
        {
            private readonly List<IMyInventoryOwner> _inventoryOwners = new List<IMyInventoryOwner>();

            public GridInventoryScanningJob(Program program, string name, int cooldownSeconds = 0) : base(program, name, cooldownSeconds)
            {
            }

            protected override void ConfigureCountingBounds(out int startIndex, out int endIndex)
            {
                Program.GridTerminalSystem.GetBlocksOfType<IMyInventoryOwner>(_inventoryOwners,b => (b as IMyTerminalBlock).IsSameConstructAs(Program.Me));
                startIndex = 0;
                endIndex = _inventoryOwners.Count - 1;
            }

            protected override void ProcessingIndex(int index)
            {
                var owner = _inventoryOwners[index];
                ProcessingTerminalBlock(owner as IMyTerminalBlock);
            }

            protected void ProcessingTerminalBlock(IMyTerminalBlock inventoryOwner)
            {
                Parameter pm = new Parameter();
                bool isStorage = inventoryOwner.CustomName.Contains(X_StorageTag)
                    , isSmsBlock = false
                    , isNoKeep = false
                    , isNoLcd = false
                    , isContainerOrConnector = false;

                if (pm.ParseArgs(inventoryOwner.CustomName))
                {
                    isSmsBlock = true;
                    isNoKeep = !pm.IsParameter("Keep");
                    isNoLcd = !pm.IsParameter("Infolcd");
                }

                isContainerOrConnector = inventoryOwner.BlockDefinition.SubtypeId.Contains("Container") || inventoryOwner.BlockDefinition.SubtypeId.Contains("Connector");

                for (int i = 0; i < inventoryOwner.InventoryCount; i++)
                {
                    var inv = inventoryOwner.GetInventory(i);
                    AddToInventory(inv);

                    if(isStorage) continue;

                    if (isSmsBlock)
                    {
                        if (isNoKeep) InventoryList_SMSflagged.Add(inv);
                        if (isNoLcd) Program.addToInventoryList(inv, pm.ParameterList);
                    }
                    else if (isContainerOrConnector) InventoryList_SMSflagged.Add(inv);
                    else InventoryList_nonSMSflagged.Add(inv);
                }
            }
        }

        public void OldMainLoop(UpdateType updateSource)
        {
            do
            {
                switch (m0)
                {
                    case -1:
                        if (_jobs[Loop.CurrentJobIndex].Schedule() == Job.ScheduleResult.Done)
                        {
                            // Move to the next job, wrapping around if necessary
                            Loop.CurrentJobIndex++;

                            if (Loop.CurrentJobIndex >= _jobs.Length)
                            {
                                Loop.CurrentJobIndex = 0;
                                m0 = 10;
                            }
                        }
                        break;


                    // Ammomanager -----------------------------------------------------------------------------------------------------------------
                    case 10:
                        var group = GridTerminalSystem.GetBlockGroupWithName(gungroupName);
                        if (group == null)
                        {
                            if (guns.Count > 0)
                            {
                                for (int i = guns.Count - 1; i >= 0; i--) guns[i].Remove();
                                guns.Clear();
                            }
                            m0++;
                            m0++;
                            break;
                        }
                        else group.GetBlocksOfType<IMyUserControllableGun>(ugun, block => block.IsSameConstructAs(Me));
                        for (int i = guns.Count - 1; i >= 0; i--)
                        {
                            if (ugun.Contains(guns[i].gun)) ugun.Remove(guns[i].gun);
                            else
                            {
                                var gun = guns[i];
                                gun.Remove();
                                guns.Remove(gun);
                            }
                        }
                        m1 = ugun.Count - 1;
                        m0++;
                        break;

                    case 11:
                        for (int i = m1; i >= 0; i--, m1--)
                        {
                            if (maxInstructions()) return;
                            guns.Add(new Gun(ugun[i]));
                        }
                        m0++;
                        break;

                    case 12:
                        m1 = 0;
                        m0++;
                        break;

                    case 13:
                        for (int i = m1; i < guns.Count; i++, m1++)
                        {
                            if (maxInstructions()) return;
                            guns[i].Refresh();
                        }
                        m0++;
                        break;

                    // StorageCargo -----------------------------------------------------------------------------------------------------------------
                    case 14:
                        GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(tbl, cargo => (cargo.CustomName.Contains(X_StorageTag)));
                        for (int i = storageCargos.Count - 1; i >= 0; i--)
                        {
                            if (tbl.Contains(storageCargos[i].container)) tbl.Remove(storageCargos[i].container);
                            else
                            {
                                var stor = storageCargos[i];
                                stor.Remove();
                                storageCargos.Remove(stor);
                            }
                        }
                        m1 = tbl.Count - 1;
                        m0++;
                        break;

                    case 15:
                        for (int i = m1; i >= 0; i--, m1--)
                        {
                            if (maxInstructions()) return;
                            storageCargos.Add(new StorageCargo(tbl[i]));
                        }
                        m0++;
                        break;
                    case 16:
                        m0++;
                        break;
                    case 17:
                        m0++;
                        break;
                    case 18: m0++; break;
                    case 19: m0++; break;
                    case 20: m0++; break;
                    case 21: m0++; break;
                    case 22: m0++; break;
                    case 23: m0++; break;


                    // Counting WelderBlocks, if working do nothing ------------------------------------------------------------------------------------------------
                    case 24:
                        GridTerminalSystem.GetBlocksOfType<IMyShipWelder>(tbl, block => block.IsSameConstructAs(Me));
                        m1 = 0;
                        m0++;
                        break;
                    case 25:
                        for (int i = m1; i < tbl.Count; i++, m1++)
                        {
                            if (maxInstructions()) return;
                            var bd = tbl[i].BlockDefinition.SubtypeId.ToString();
                            if (bd.Contains("ShipLaserMultitool"))
                            {
                                bool weld = true;
                                var pp = tbl[i].GetProperty("ToolMode");
                                if (pp != null && pp.TypeName == "Boolean") weld = tbl[i].GetValue<bool>("ToolMode");
                                if (weld) { if (!(tbl[i] as IMyFunctionalBlock).Enabled) pushTerminalBlock(tbl[i]); }
                                else pushTerminalBlock(tbl[i]);
                            }
                            else if (!(tbl[i] as IMyFunctionalBlock).Enabled) pushTerminalBlock(tbl[i]);
                        }
                        m0++;
                        break;

                    case 26:
                        m0++;
                        break;
                    case 27:
                        m0++;
                        break;
                    case 28:
                        m0++;
                        break;
                    case 29:
                        m0++;
                        break;

                    // Stacking --------------------------------------------------------------------------------------------------------------------------------
                    case 30:
                        m1 = 0;
                        if (stacking_cycle == 0 || (DateTime.Now - StackingCounter).TotalSeconds < stacking_cycle) m0 += 2;
                        else
                        {
                            StackItemList.Clear();
                            StackItem.ClearStackInventory();
                            if (InventoryManagerList.ContainsKey(stack_type))
                            {
                                foreach (var i in InventoryManagerList[stack_type])
                                {
                                    new_stackcount(i, stack_type);
                                    StackItem.CalculateFreeInventory(i);
                                }
                            }
                            if (stack_mode == 0)
                            {
                                int max_stack = 0;
                                foreach (var s in StackItemList) if (max_stack < s.Stackcount) max_stack = s.Stackcount;
                                if (max_stack > 1)
                                {
                                    StackItem.CurrentStackingType = StackItem.StackingType.Stack;
                                    StackItemList.Sort();
                                }
                                else
                                {
                                    stack_mode = -1;
                                }
                            }
                            else if (stack_mode == 1)
                            {
                                int max_stack = 0;
                                foreach (var s in StackItemList) if (max_stack < s.Stackcount) max_stack = s.Stackcount;
                                if (max_stack > 1)
                                {
                                    StackItem.CurrentStackingType = StackItem.StackingType.Stack;
                                    StackItemList.Sort();
                                }
                                else
                                {
                                    stack_mode = -1;
                                }
                            }
                            else if (stack_mode == 2)
                            {
                                StackItem.CurrentStackingType = StackItem.StackingType.VolumeBack;
                                StackItemList.Sort();
                            }
                            else if (stack_mode == 3)
                            {
                                StackItem.CurrentStackingType = StackItem.StackingType.Volume;
                                StackItemList.Sort();
                            }
                            else if (stack_mode == 4)
                            {
                                StackItem.CurrentStackingType = StackItem.StackingType.Stack;
                                StackItemList.Sort();
                                while (StackItemList.Count > 0)
                                {
                                    var s = StackItemList[0];
                                    if (s.check_stacking_gamma()) break;
                                    StackItemList.Remove(s);
                                }
                            }
                            StackingCounter = DateTime.Now;
                            m1 = 0;
                        }
                        m0++;
                        break;
                    case 31:
                        switch (stack_mode)
                        {
                            case 0:
                                for (int i = m1; i < StackItemList.Count; i++, m1++)
                                {
                                    if (maxInstructions()) return;
                                    StackItemList[i].stacking_single();
                                }
                                m0++;
                                stack_mode++;
                                break;
                            case 1:
                                for (int i = m1; i < StackItemList.Count; i++, m1++)
                                {
                                    if (maxInstructions()) return;
                                    StackItemList[i].stacking_alpha();
                                }
                                m0++;
                                stack_mode++;
                                break;
                            case 2:
                                for (int i = m1; i < StackItemList.Count; i++, m1++)
                                {
                                    if (maxInstructions()) return;
                                    if (!StackItemList[i].stacking_beta()) break;
                                }
                                m0++;
                                stack_mode++;
                                break;
                            case 3:
                                for (int i = m1; i < StackItemList.Count; i++, m1++)
                                {
                                    if (maxInstructions()) return;
                                    StackItemList[i].stacking_delta();
                                }
                                m0++;
                                stack_mode++;
                                break;
                            case 4:
                                if (StackItemList.Count > 0) for (int i = m1; i < 300; i++, m1++)
                                    {
                                        if (maxInstructions()) return;
                                        if (StackItemList[0].stacking_gamma()) break;
                                    }
                                m0++;
                                stack_mode++;
                                break;
                            default:
                                stack_mode = 0;
                                cur_stack_type++;
                                if (cur_stack_type >= stack_types.Length) cur_stack_type = 0;
                                stack_type = stack_types[cur_stack_type];
                                m0++;
                                break;
                        }
                        break;
                    case 32:
                        m0++;
                        break;


                    case 33:
                        foreach (var b in Lists.BluePrints_Active.Values) b.AssemblyAmount = 0;
                        foreach (var b in Lists.BluePrints_Inactive.Values) b.AssemblyAmount = 0;
                        GridTerminalSystem.GetBlocksOfType<IMyRefinery>(Lists.Refinerys, block => block.CubeGrid == Me.CubeGrid);
                        for (int i = RefineryList.Count - 1; i >= 0; i--)
                        {
                            if (Lists.Refinerys.Contains(RefineryList[i].RefineryBlock)) Lists.Refinerys.Remove(RefineryList[i].RefineryBlock);
                            else
                            {
                                Propertys.changeAutoCraftingSettings = true;
                                RefineryList.Remove(RefineryList[i]);
                            }
                        }
                        Refinery.priobt = "";
                        m1 = Lists.Refinerys.Count - 1;
                        m0++;
                        break;
                    case 34:
                        for (int i = m1; i >= 0; i--, m1--)
                        {
                            if (maxInstructions()) return;
                            RefineryList.Add(new Refinery(Lists.Refinerys[i]));
                        }
                        m0++;
                        break;
                    case 35:
                        GridTerminalSystem.GetBlocksOfType<IMyAssembler>(Lists.Assemblers, block => block.CubeGrid == Me.CubeGrid);
                        for (int i = AssemblerList.Count - 1; i >= 0; i--)
                        {
                            if (Lists.Assemblers.Contains(AssemblerList[i].AssemblerBlock)) Lists.Assemblers.Remove(AssemblerList[i].AssemblerBlock);
                            else
                            {
                                Propertys.changeAutoCraftingSettings = true;
                                AssemblerList.Remove(AssemblerList[i]);
                            }
                        }
                        m1 = Lists.Assemblers.Count - 1;
                        m0++;
                        break;
                    case 36:
                        for (int i = m1; i >= 0; i--, m1--)
                        {
                            if (maxInstructions()) return;
                            AssemblerList.Add(new Assembler(Lists.Assemblers[i]));
                        }
                        m0++;
                        break;
                    case 37:
                        m1 = 0;
                        m0++;
                        break;
                    case 38:
                        for (int i = m1; i < InventoryList_SMSflagged.Count; i++, m1++)
                        {
                            if (maxInstructions()) return;
                            ClearInventory(InventoryList_SMSflagged[i]);
                        }
                        m0++;
                        break;
                    case 39:
                        m1 = 0;
                        m0++;
                        break;
                    case 40:
                        for (int i = m1; i < InventoryList_nonSMSflagged.Count; i++, m1++)
                        {
                            if (maxInstructions()) return;
                            ClearInventory(InventoryList_nonSMSflagged[i], collectAll_List);
                        }
                        m0++;
                        break;
                    case 41:
                        m1 = 0;
                        foreach (var a in ammoDefs.Values) a.CalcAmmoInventoryRatio();
                        m0++;
                        break;
                    case 42: // ItemTransfer
                        for (int i = m1; i < storageinvs.Count; i++, m1++)
                        {
                            if (maxInstructions()) return;
                            storageinvs[i].reloadItems();
                        }
                        m0++;
                        break;
                    case 43:
                        Refinery.cn = 0;
                        foreach (var b in RefineryBlueprints) b.RefineryCount = 0;
                        m1 = 0;
                        m0++;
                        break;
                    case 44:
                        for (int i = m1; i < RefineryList.Count; i++, m1++)
                        {
                            if (maxInstructions()) return;
                            RefineryList[i].Refresh();
                        }
                        m0++;
                        break;
                    case 45:
                        CalcIngotPrio();
                        RenderAmmoPrioLCDS();
                        RenderResourceProccesingLCD();
                        m1 = 0;
                        m0++;
                        break;
                    case 46:
                        for (int i = m1; i < RefineryList.Count; i++, m1++)
                        {
                            if (maxInstructions()) return;
                            RefineryList[i].RefineryManager();
                        }
                        m0++;
                        break;
                    case 47:
                        m1 = 0;
                        m0++;
                        break;
                    case 48:
                        for (int i = m1; i < AssemblerList.Count; i++, m1++)
                        {
                            if (maxInstructions()) return;
                            AssemblerList[i].Refresh();
                        }
                        m0++;
                        break;
                    case 49:
                        s0 = new List<string>(Lists.BluePrints_Active.Keys);
                        m1 = 0;
                        m0++;
                        break;
                    case 50:
                        for (int i = m1; i < s0.Count; i++, m1++)
                        {
                            if (maxInstructions()) return;
                            var b = Lists.BluePrints_Active[s0[i]];
                            b.SetCurrentAmount((int)inventar.GetValueOrDefault(b.ItemName, 0));
                            b.CalcPriority();
                            if (b.NeedsAssembling())
                            {
                                if (s0[i] != Ingot.SubFresh)
                                {
                                    foreach (var o in AssemblerList) o.AddValidBlueprint(b);
                                }
                            }
                        }
                        m0++;
                        break;
                    case 51:
                        s0 = new List<string>(Lists.BluePrints_Inactive.Keys);
                        m1 = 0;
                        m0++;
                        break;
                    case 52:
                        for (int i = m1; i < s0.Count; i++, m1++)
                        {
                            if (maxInstructions()) return;
                            var b = Lists.BluePrints_Inactive[s0[i]];
                            if (inventar.ContainsKey(b.ItemName)) b.SetCurrentAmount((int)inventar[b.ItemName]);
                        }
                        m0++;
                        break;
                    case 53:
                        m1 = 0;
                        m2 = 0;
                        AssemblerList.Sort();
                        m0++;
                        break;
                    case 54:
                        for (int i = m1; i < AssemblerList.Count; i++, m1++)
                        {
                            if (maxInstructions()) return;
                            var o = AssemblerList[i];
                            if (o.AssemblerBlock.CubeGrid == Me.CubeGrid && o.parameter.ControledByPIM())
                            {
                                if (o.BlueprintList.Count > 0)
                                {
                                    o.BlueprintList.Sort();
                                    var b = o.BlueprintList[0];
                                    m2++;
                                    if (o.AddBlueprintToQueue(b))
                                    {
                                        foreach (var oo in b.o) oo.BlueprintList.Remove(b);
                                    }
                                    else o.BlueprintList.Remove(b);
                                }
                            }
                        }
                        if (m2 == 0) m0++;
                        else m0--;
                        break;
                    default:
                        for (int i = viewList.Count - 1; i >= 0; i--) { if (viewList[i].IsOver()) viewList.Remove(viewList[i]); }
                        // -----------------------
                        Dictionary<string, string> recommendedItems = new Dictionary<string, string>
                        {
                            { Ore.Stone, Resources.RStone },
                            { Ingot.Stone, "Gravel" },
                            { Ore.Ice, "Ice" },
                            { Ingot.WaterFood, "Water" },
                            { Ingot.GreyWater, "Greywater" },
                            { Ingot.DeuteriumContainer, "Deuterium" },
                            { Ore.Organic, "Organic" },
                        };
                        foreach (var item in recommendedItems)
                        {
                            var condition = (inventar.ContainsKey(item.Key) && inventar[item.Key] > 0 && !InventoryManagerList.ContainsKey(item.Key));
                            SetWarningByCondition(condition, Warning.ID.CARGORECOMMENDED, item.Value);
                        }
                        // -----------------------
                        foreach (var c in CargoUseList.Keys)
                        {
                            var cargoUseRatio = CargoUseList[c].GetCarcocapacityUseRatio();
                            if (cargoUseRatio >= 90)
                            {
                                SetWarning(Warning.ID.CARGOUSEHEAVY, c);
                                ClearWarning(Warning.ID.CARGOUSEFULL, c);
                            }
                            else if (cargoUseRatio >= 99)
                            {
                                SetWarning(Warning.ID.CARGOUSEFULL, c);
                                ClearWarning(Warning.ID.CARGOUSEHEAVY, c);
                            }
                            else
                            {
                                ClearWarning(Warning.ID.CARGOUSEHEAVY, c);
                                ClearWarning(Warning.ID.CARGOUSEFULL, c);
                            }
                        }
                        CalcutateInfos();
                        LoopManager.firstRun = false;
                        SendInfosToSMS();
                        m0 = -1;
                        break;
                }
            }
            while (!maxInstructions());


        }
    }
}
