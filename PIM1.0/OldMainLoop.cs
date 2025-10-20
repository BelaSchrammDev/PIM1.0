using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using VRage;
using VRage.Game;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        // TODO: Stacking cooldown
        public class StackingJob : SequentialJob
        {
            static string[] stack_types = new string[] { "Component", "Ore", "Ingot" };
            
            private int cur_stack_type;
            public static string StackType { get; private set; }

            public StackingJob(Program program, string name, params Job[] jobs) : base(program, name, jobs)
            {
                cur_stack_type = -1;
            }

            public override void InitJob()
            {
                cur_stack_type++;
                if (cur_stack_type >= stack_types.Length)
                {
                    cur_stack_type = 0;
                    NextJob();
                }
                StackType = stack_types[cur_stack_type];
            }
        }

        public abstract class StackingJobBase : CountingJob
        {
            protected List<StackItem> StackItemList = new List<StackItem>();

            public enum StackingMode
            {
                Single,
                Alpha,
                Beta,
                Delta,
                Gamma
            }

            protected StackingJobBase(Program program, string name, StackingMode mode) : base(program, name)
            {
            }

            protected override void ConfigureCountingBounds(out int startIndex, out int endIndex)
            {
                StackItemList.Clear();
                StackItem.ClearStackInventory();
                if (InventoryManagerList.ContainsKey(StackingJob.StackType))
                {
                    foreach (var i in InventoryManagerList[StackingJob.StackType])
                    {
                        NewStackCount(i, StackingJob.StackType);
                        StackItem.CalculateFreeInventory(i);
                    }
                }
                Program.debugString += Name + "_Init " + StackItemList.Count + "\n";

                startIndex = 0;
                endIndex = InitStacking() ? StackItemList.Count - 1 : 0;
            }

            public abstract bool InitStacking();

            void NewStackCount(IMyInventory quelle, string ti)
            {
                var von = new List<MyInventoryItem>();
                quelle.GetItems(von);
                foreach (var i in von)
                {
                    if (i.Type.TypeId.Contains(ti) && !InventoryManagerList.ContainsKey(GetPIMItemID(i.Type)))
                    {
                        GetStackItem(i.Type).AddStack(quelle, i.Amount);
                    }
                }
            }

            StackItem GetStackItem(MyItemType t) { foreach (var s in StackItemList) if (s.type == t) return s; var nt = new StackItem(t); StackItemList.Add(nt); return nt; }

        }

        public class StackingSingleJob : StackingJobBase
        {
            public StackingSingleJob(Program program) : base(program, "Stacking Single", StackingMode.Single)
            {
            }

            public override bool InitStacking()
            {
                int max_stack = 0;
                foreach (var s in StackItemList) if (max_stack < s.Stackcount) max_stack = s.Stackcount;
                if (max_stack > 1)
                {
                    StackItem.CurrentStackingType = StackItem.StackingType.Stack;
                    StackItemList.Sort();
                    return true;
                }
                else
                {
                    return false;
                }
            }

            protected override void ProcessingIndex(int index)
            {
                StackItemList[index].stacking_single();
            }
        }

        public class StackingAlphaJob : StackingJobBase
        {
            public StackingAlphaJob(Program program) : base(program, "Stacking Alpha", StackingMode.Alpha)
            {
            }

            public override bool InitStacking()
            {
                int max_stack = 0;
                foreach (var s in StackItemList) if (max_stack < s.Stackcount) max_stack = s.Stackcount;
                if (max_stack > 1)
                {
                    StackItem.CurrentStackingType = StackItem.StackingType.Stack;
                    StackItemList.Sort();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            protected override void ProcessingIndex(int index)
            {
                StackItemList[index].stacking_alpha();
            }
        }

        public class StackingBetaJob : StackingJobBase
        {
            public StackingBetaJob(Program program) : base(program, "Stacking Beta", StackingMode.Beta)
            {
            }

            public override bool InitStacking()
            {
                StackItem.CurrentStackingType = StackItem.StackingType.VolumeBack;
                StackItemList.Sort();
                return true;
            }

            protected override void ProcessingIndex(int index)
            {
                StackItemList[index].stacking_beta();
            }
        }

        public class StackingDeltaJob : StackingJobBase
        {
            public StackingDeltaJob(Program program) : base(program, "Stacking Delta", StackingMode.Delta)
            {
            }
            public override bool InitStacking()
            {
                StackItem.CurrentStackingType = StackItem.StackingType.Volume;
                StackItemList.Sort();
                return true;
            }
            protected override void ProcessingIndex(int index)
            {
                StackItemList[index].stacking_delta();
            }
        }

        public class StackingGammaJob : StackingJobBase
        {
            public StackingGammaJob(Program program) : base(program, "Stacking Gamma", StackingMode.Gamma)
            {
            }
            public override bool InitStacking()
            {
                StackItem.CurrentStackingType = StackItem.StackingType.Stack;
                StackItemList.Sort();
                while (StackItemList.Count > 0)
                {
                    var s = StackItemList[0];
                    if (s.check_stacking_gamma()) break;
                    StackItemList.Remove(s);
                }
                return StackItemList.Count > 0;
            }
            protected override void ProcessingIndex(int index)
            {
                StackItemList[0].stacking_gamma();
            }
        }

        void OldMainLoop(UpdateType updateSource)
        {
            do
            {
                switch (m0)
                {
                    case -1:
                        if (_jobs[Loop.Data.CurrentJobIndex].Schedule() == Job.ScheduleResult.Done)
                        {
                            // Move to the next job, wrapping around if necessary
                            Loop.Data.CurrentJobIndex++;

                            if (Loop.Data.CurrentJobIndex >= _jobs.Length)
                            {
                                Loop.Data.CurrentJobIndex = 0;
                                m0 = 33;
                            }
                        }
                        break;

                    // Stacking --------------------------------------------------------------------------------------------------------------------------------
                    //case 30:
                    //    m1 = 0;
                    //    if (stacking_cycle == 0 || (DateTime.Now - StackingCounter).TotalSeconds < stacking_cycle) m0 += 2;
                    //    else
                    //    {
                    //        StackItemList.Clear();
                    //        StackItem.ClearStackInventory();
                    //        if (InventoryManagerList.ContainsKey(stack_type))
                    //        {
                    //            foreach (var i in InventoryManagerList[stack_type])
                    //            {
                    //                NewStackCount(i, stack_type);
                    //                StackItem.CalculateFreeInventory(i);
                    //            }
                    //        }
                    //        if (stack_mode == 0)
                    //        {
                    //            int max_stack = 0;
                    //            foreach (var s in StackItemList) if (max_stack < s.Stackcount) max_stack = s.Stackcount;
                    //            if (max_stack > 1)
                    //            {
                    //                StackItem.CurrentStackingType = StackItem.StackingType.Stack;
                    //                StackItemList.Sort();
                    //            }
                    //            else
                    //            {
                    //                stack_mode = -1;
                    //            }
                    //        }
                    //        else if (stack_mode == 1)
                    //        {
                    //            int max_stack = 0;
                    //            foreach (var s in StackItemList) if (max_stack < s.Stackcount) max_stack = s.Stackcount;
                    //            if (max_stack > 1)
                    //            {
                    //                StackItem.CurrentStackingType = StackItem.StackingType.Stack;
                    //                StackItemList.Sort();
                    //            }
                    //            else
                    //            {
                    //                stack_mode = -1;
                    //            }
                    //        }
                    //        else if (stack_mode == 2)
                    //        {
                    //            StackItem.CurrentStackingType = StackItem.StackingType.VolumeBack;
                    //            StackItemList.Sort();
                    //        }
                    //        else if (stack_mode == 3)
                    //        {
                    //            StackItem.CurrentStackingType = StackItem.StackingType.Volume;
                    //            StackItemList.Sort();
                    //        }
                    //        else if (stack_mode == 4)
                    //        {
                    //            StackItem.CurrentStackingType = StackItem.StackingType.Stack;
                    //            StackItemList.Sort();
                    //            while (StackItemList.Count > 0)
                    //            {
                    //                var s = StackItemList[0];
                    //                if (s.check_stacking_gamma()) break;
                    //                StackItemList.Remove(s);
                    //            }
                    //        }
                    //        StackingCounter = DateTime.Now;
                    //        m1 = 0;
                    //    }
                    //    m0++;
                    //    break;
                    //case 31:
                    //    switch (stack_mode)
                    //    {
                    //        case 0:
                    //            for (int i = m1; i < StackItemList.Count; i++, m1++)
                    //            {
                    //                if (maxInstructions()) return;
                    //                StackItemList[i].stacking_single();
                    //            }
                    //            m0++;
                    //            stack_mode++;
                    //            break;
                    //        case 1:
                    //            for (int i = m1; i < StackItemList.Count; i++, m1++)
                    //            {
                    //                if (maxInstructions()) return;
                    //                StackItemList[i].stacking_alpha();
                    //            }
                    //            m0++;
                    //            stack_mode++;
                    //            break;
                    //        case 2:
                    //            for (int i = m1; i < StackItemList.Count; i++, m1++)
                    //            {
                    //                if (maxInstructions()) return;
                    //                if (!StackItemList[i].stacking_beta()) break;
                    //            }
                    //            m0++;
                    //            stack_mode++;
                    //            break;
                    //        case 3:
                    //            for (int i = m1; i < StackItemList.Count; i++, m1++)
                    //            {
                    //                if (maxInstructions()) return;
                    //                StackItemList[i].stacking_delta();
                    //            }
                    //            m0++;
                    //            stack_mode++;
                    //            break;
                    //        case 4:
                    //            if (StackItemList.Count > 0) for (int i = m1; i < 300; i++, m1++)
                    //                {
                    //                    if (maxInstructions()) return;
                    //                    if (StackItemList[0].stacking_gamma()) break;
                    //                }
                    //            m0++;
                    //            stack_mode++;
                    //            break;
                    //        default:
                    //            stack_mode = 0;
                    //            cur_stack_type++;
                    //            if (cur_stack_type >= stack_types.Length) cur_stack_type = 0;
                    //            stack_type = stack_types[cur_stack_type];
                    //            m0++;
                    //            break;
                    //    }
                    //    break;
                    //case 32:
                    //    m0++;
                    //    break;

                    // Find Refinery Blocks -----------------------------------------------------------------------------------------------------------------
                    case 33:
                        foreach (var b in Lists.Data.BluePrints_Active.Values) b.AssemblyAmount = 0;
                        foreach (var b in Lists.Data.BluePrints_Inactive.Values) b.AssemblyAmount = 0;
                        GridTerminalSystem.GetBlocksOfType<IMyRefinery>(Lists.Data.Refinerys, block => block.CubeGrid == Me.CubeGrid);
                        for (int i = RefineryList.Count - 1; i >= 0; i--)
                        {
                            if (Lists.Data.Refinerys.Contains(RefineryList[i].RefineryBlock)) Lists.Data.Refinerys.Remove(RefineryList[i].RefineryBlock);
                            else
                            {
                                Propertys.Data.changeAutoCraftingSettings = true;
                                RefineryList.Remove(RefineryList[i]);
                            }
                        }
                        Refinery.priobt = "";
                        m1 = Lists.Data.Refinerys.Count - 1;
                        m0++;
                        break;
                    case 34:
                        for (int i = m1; i >= 0; i--, m1--)
                        {
                            if (maxInstructions()) return;
                            RefineryList.Add(new Refinery(Lists.Data.Refinerys[i]));
                        }
                        m0++;
                        break;

                    // Find Assembler Blocks -----------------------------------------------------------------------------------------------------------------
                    case 35:
                        GridTerminalSystem.GetBlocksOfType<IMyAssembler>(Lists.Data.Assemblers, block => block.CubeGrid == Me.CubeGrid);
                        for (int i = AssemblerList.Count - 1; i >= 0; i--)
                        {
                            if (Lists.Data.Assemblers.Contains(AssemblerList[i].AssemblerBlock)) Lists.Data.Assemblers.Remove(AssemblerList[i].AssemblerBlock);
                            else
                            {
                                Propertys.Data.changeAutoCraftingSettings = true;
                                AssemblerList.Remove(AssemblerList[i]);
                            }
                        }
                        m1 = Lists.Data.Assemblers.Count - 1;
                        m0++;
                        break;
                    case 36:
                        for (int i = m1; i >= 0; i--, m1--)
                        {
                            if (maxInstructions()) return;
                            AssemblerList.Add(new Assembler(Lists.Data.Assemblers[i]));
                        }
                        m0++;
                        break;

                    // Inventory Clearing -----------------------------------------------------------------------------------------------------------------
                    case 37:
                        m1 = 0;
                        m0++;
                        break;
                    case 38:
                        for (int i = m1; i < NonSmsFlagedInventoryList.Count; i++, m1++)
                        {
                            if (maxInstructions()) return;
                            ClearInventory(NonSmsFlagedInventoryList[i]);
                        }
                        m0++;
                        break;
                    case 39:
                        m1 = 0;
                        m0++;
                        break;
                    case 40:
                        for (int i = m1; i < SmsFlagedInventoryList.Count; i++, m1++)
                        {
                            if (maxInstructions()) return;
                            ClearInventory(SmsFlagedInventoryList[i], collectAll_List);
                        }
                        m0++;
                        break;

                    // Storage Inventory Refresh -----------------------------------------------------------------------------------------------------------------
                    case 41:
                        m1 = 0;
                        foreach (var a in ammoDefs.Values) a.CalcAmmoInventoryRatio();
                        m0++;
                        break;

                    case 42:
                        for (int i = m1; i < storageinvs.Count; i++, m1++)
                        {
                            if (maxInstructions()) return;
                            storageinvs[i].ReloadItems();
                        }
                        m0++;
                        break;


                    // Refinery Refresh ----------------------------------------------------------------------------------------------------------------- 
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

                    // RefineryManager -----------------------------------------------------------------------------------------------------------------
                    case 45:
                        CalcIngotPrio();
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

                    // Assembler Refresh -----------------------------------------------------------------------------------------------------------------
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

                    // Assembler Manager -----------------------------------------------------------------------------------------------------------------
                    case 49:
                        s0 = new List<string>(Lists.Data.BluePrints_Active.Keys);
                        m1 = 0;
                        m0++;
                        break;
                    case 50:
                        for (int i = m1; i < s0.Count; i++, m1++)
                        {
                            if (maxInstructions()) return;
                            var b = Lists.Data.BluePrints_Active[s0[i]];
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

                    // Set Amount of Inavtive BluePrints -----------------------------------------------------------------------------------------------------------------
                    case 51:
                        s0 = new List<string>(Lists.Data.BluePrints_Inactive.Keys);
                        m1 = 0;
                        m0++;
                        break;
                    case 52:
                        for (int i = m1; i < s0.Count; i++, m1++)
                        {
                            if (maxInstructions()) return;
                            var b = Lists.Data.BluePrints_Inactive[s0[i]];
                            if (inventar.ContainsKey(b.ItemName)) b.SetCurrentAmount((int)inventar[b.ItemName]);
                        }
                        m0++;
                        break;

                    // AutoCrafting -----------------------------------------------------------------------------------------------------------------
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
