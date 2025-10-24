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
    // TODO: Tools class implementing

    partial class Program
    {

        public class InventoryClearingJob : CountingJob
        {
            private List<IMyInventory> _invList;
            private List<string> _collectList;
            public InventoryClearingJob(Program program, string name, List<IMyInventory> invList, List<string> collect = null) : base(program, name)
            {
                _invList = invList;
                _collectList = collect;
            }

            protected override void ConfigureCountingBounds(out int startIndex, out int endIndex)
            {
                startIndex = 0;
                endIndex = _invList.Count - 1;
            }

            protected override void ProcessingIndex(int index)
            {
                ClearInventory(_invList[index], _collectList);
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
                                m0 = 41;
                            }
                        }
                        break;


                    // Inventory Clearing -----------------------------------------------------------------------------------------------------------------
                    //case 37:
                    //    m1 = 0;
                    //    m0++;
                    //    break;
                    //case 38:
                    //    for (int i = m1; i < NonSmsFlagedInventoryList.Count; i++, m1++)
                    //    {
                    //        if (maxInstructions()) return;
                    //        ClearInventory(NonSmsFlagedInventoryList[i]);
                    //    }
                    //    m0++;
                    //    break;
                    //case 39:
                    //    m1 = 0;
                    //    m0++;
                    //    break;
                    //case 40:
                    //    for (int i = m1; i < SmsFlagedInventoryList.Count; i++, m1++)
                    //    {
                    //        if (maxInstructions()) return;
                    //        ClearInventory(SmsFlagedInventoryList[i], collectAll_List);
                    //    }
                    //    m0++;
                    //    break;

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
                        for (int i = m1; i < Lists.Data.RefineryList.Count; i++, m1++)
                        {
                            if (maxInstructions()) return;
                            Lists.Data.RefineryList[i].Refresh();
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
                        for (int i = m1; i < Lists.Data.RefineryList.Count; i++, m1++)
                        {
                            if (maxInstructions()) return;
                            Lists.Data.RefineryList[i].RefineryManager();
                        }
                        m0++;
                        break;

                    // Assembler Refresh -----------------------------------------------------------------------------------------------------------------
                    case 47:
                        m1 = 0;
                        m0++;
                        break;
                    case 48:
                        for (int i = m1; i < Lists.Data.AssemblerList.Count; i++, m1++)
                        {
                            if (maxInstructions()) return;
                            Lists.Data.AssemblerList[i].Refresh();
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
                                    foreach (var o in Lists.Data.AssemblerList) o.AddValidBlueprint(b);
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
                        Lists.Data.AssemblerList.Sort();
                        m0++;
                        break;
                    case 54:
                        for (int i = m1; i < Lists.Data.AssemblerList.Count; i++, m1++)
                        {
                            if (maxInstructions()) return;
                            var o = Lists.Data.AssemblerList[i];
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
