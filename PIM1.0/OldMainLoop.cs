using EmptyKeys.UserInterface.Generated.WorkshopBrowserView_Bindings;
using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using VRage;
using VRage.Game;
using static IngameScript.Program;

namespace IngameScript
{
    // TODO: this parameter in Job Classes remove, use Tools.ProgramInstance instead

    // TODO: LoopManager.firstrun not used for SendInfosToSMS() anymore
    // TODO: Tools class implementing
    // TODO: refactoring, refactoring, refactoring...
    // TODO: detecting other PIM blocks
    // TODO: container for ammo not needed when armory is defined
    // TODO: Remove DNSK Mod, and other mods not exsisting anymore
    // TODO: Autocrafting job integration

    partial class Program
    {
        public class AutoCraftingJob : CountingJob
        {
            private AssemblerBluePrint[] _bluePrints;
            public AutoCraftingJob(Program program) : base(program)
            {
            }

            protected override void ConfigureCountingBounds(out int startIndex, out int endIndex)
            {
                _bluePrints = Lists.Data.BluePrints_Active.Values.ToArray();
                startIndex = 0;
                endIndex = Lists.Data.BluePrints_Active.Count - 1;
            }

            protected override void ProcessingIndex(int index)
            {
                var bluePrint = _bluePrints[index];
                if (bluePrint.NeedsAssembling())
                {
                    var bluePrintNeededAmount = (bluePrint.MaximumItemAmount - bluePrint.CurrentItemAmount - bluePrint.AssemblyAmount);
                    var validAssemblers = Lists.Data.AssemblerList.FindAll(x => x.OwnBlueprintList.Contains(bluePrint));
                    MyFixedPoint amount = bluePrintNeededAmount / validAssemblers.Count;
                    foreach (var assembler in validAssemblers)
                    {
                        assembler.AddQueueItemSave(bluePrint, amount);
                    }
                }
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
                                m0 = 55;
                            }
                        }
                        break;

                    // AutoCrafting -----------------------------------------------------------------------------------------------------------------
                    //case 53:
                    //    m1 = 0;
                    //    m2 = 0;
                    //    Lists.Data.AssemblerList.Sort();
                    //    m0++;
                    //    break;
                    //case 54:
                    //    for (int i = m1; i < Lists.Data.AssemblerList.Count; i++, m1++)
                    //    {
                    //        if (LoopManager.IsMaxInstructionsArrived()) return;
                    //        var assembler = Lists.Data.AssemblerList[i];
                    //        if (assembler.parameter.ControledByPIM())                       // wird der assembler von PIM gesteuert
                    //        {
                    //            if (assembler.BlueprintList.Count > 0)                      // hat der assembler blueprints in der liste
                    //            {
                    //                assembler.BlueprintList.Sort();                         // sortiere die blueprints nach priorität
                    //                var assemblerBluePrint = assembler.BlueprintList[0];
                    //                m2++;
                    //                if (assembler.AddBlueprintToQueue(assemblerBluePrint))  // wird nur ein assembler benutzt ???
                    //                {
                    //                    foreach (var ass in assemblerBluePrint.ValidAssemblers) // dann entferne die blueprints aus den anderen assemblers
                    //                    {
                    //                        ass.BlueprintList.Remove(assemblerBluePrint);
                    //                    }
                    //                }
                    //                else
                    //                {
                    //                    assembler.BlueprintList.Remove(assemblerBluePrint); // ansonsten entferne das blueprint aus nur diesem assembler
                    //                }
                    //            }
                    //        }
                    //    }
                    //    if (m2 == 0) m0++;
                    //    else m0--;
                    //    break;

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
                            var condition = (inventar.ContainsKey(item.Key) && inventar[item.Key] > 0 && !Lists.Data.InventoryManagerList.ContainsKey(item.Key));
                            SetWarningByCondition(condition, Warning.ID.CARGORECOMMENDED, item.Value);
                        }
                        // -----------------------
                        foreach (var c in Lists.Data.CargoUseList.Keys)
                        {
                            var cargoUseRatio = Lists.Data.CargoUseList[c].GetCarcocapacityUseRatio();
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

            while (!LoopManager.IsMaxInstructionsArrived());
        }
    }
}
