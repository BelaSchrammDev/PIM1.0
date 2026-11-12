using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using VRage;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        // Array of jobs to execute in sequence
        private readonly Job[] _jobs;

        // RunningSign instance
        private readonly RunningSign RunSign = new RunningSign();

        // singleton Program Instance
        public static Program MyProgram;

        public Program()
        {
            MyProgram = this;
            
            viewList.Add(new AmmoManagerInfo());
            viewList.Add(new StorageManagerInfo());
            viewList.Add(new RefineryManagerInfo());
            viewList.Add(new AssemblerManagerInfo());

            LoopManager.Init();
            LoopManager.StartupJob = new BluePrintLoadJob(new BluePrintSet(Config.Instance.usedMods, Lists.Data.BluePrints_Inactive, RefineryBlueprints));

            if (Config.Instance.collect_all_Ore) Lists.Data.collectAll_List.Add(IG_ + "Ore");
            if (Config.Instance.collect_all_Ingot) Lists.Data.collectAll_List.Add(IG_ + IG_I);
            if (Config.Instance.collect_all_Component) Lists.Data.collectAll_List.Add(IG_ + IG_Component);

            _jobs = new Job[]
            {
                LoopManager.StartupJob,   // must be first: all other jobs need the blueprints
                new LoopManager(),
                new ChangeAutoCraftingSettingsJob(),
                new ClearJob(),
                new GridScanningJob(),
                new FindControllingGunJob(),
                new RefreshControllingGunsJob(),
                new FindStorageContainersJob(),
                new StackingJob(
                    Config.Instance.stacking_cycle,
                    new StackingMergeInContainerJob(),
                    new StackingGatherJob(),
                    new StackingGatherInEmptiestJob(),
                    new StackingMergePairsJob(),
                    new StackingSwapJob()),
                new RefreshBlockListJob<IMyRefinery, Refinery>(Lists.Data.Refinerys, Lists.Data.RefineryList, b => new Refinery(b)),
                new RefreshBlockListJob<IMyAssembler, Assembler>(Lists.Data.Assemblers, Lists.Data.AssemblerList, b => new Assembler(b)),
                new InventoryClearingJob(Lists.Data.SmsFlagedInventoryList),
                new InventoryClearingJob(Lists.Data.NoneSmsFlagedInventoryList, Lists.Data.collectAll_List),
                new StorageInventoryRefreshJob(),
                new FindingRefinerysJob(),
                new RefineryManagerJob(),
                new FindingAssemblersJob(),
                new BluePrintAmountJob(Lists.Data.BluePrints_Active, true),
                new BluePrintAmountJob(Lists.Data.BluePrints_Inactive, false),
                // LCD pages: after the amounts are counted, before autocrafting uses the max amounts
                new SequentialJob(new AutocraftingPage(), new OrePrioPage(), new RefiningPage()),
                new AutoCraftingJob(),
                // after every job that reports hints (SetWarning): it removes the hints not reported in this cycle
                new ViewManagerJob(),
                new SendInfosToSmsJob()
            };
        }

        void Main(string argument, UpdateType updateSource)
        {
            if (argument != "")
            {
                if (!CommandDispatcher.Dispatch(argument))
                {
                    SetInfo("unknown command: \"" + argument + "\"");
                }

                return;
            }

            LoopManager.CountRun();

            do
            {
                var result = _jobs[Loop.Data.CurrentJobIndex].Schedule();

                if (result == Job.ScheduleResult.Done)
                {
                    // Move to the next job, wrapping around if necessary
                    Loop.Data.CurrentJobIndex++;

                    if (Loop.Data.CurrentJobIndex >= _jobs.Length)
                    {
                        Loop.Data.CurrentJobIndex = 0;
                    }
                }
            }
            while (!LoopManager.IsMaxInstructionsArrived());

            WritePimRunInfo();
        }

        // every run: only the running sign changes, the status (PimStatus) is built once per cycle by CalculateInfos
        void WritePimRunInfo()
        {
            var s = Strings.PimVersion + Strings.PimCopyright + RunSign.Next() + PimStatus;

            Echo(s);

            if (Config.Instance.ShowInfoPBLcd)
            {
                var tp = Me.GetSurface(0);
                if (tp.ContentType != ContentType.TEXT_AND_IMAGE)
                {
                    tp.Alignment = TextAlignment.LEFT;
                    tp.ContentType = ContentType.TEXT_AND_IMAGE;
                }
                tp.WriteText(s);
            }
        }
    }
}
