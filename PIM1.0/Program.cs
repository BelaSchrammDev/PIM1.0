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

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        // Array of jobs to execute in sequence
        private readonly Job[] _jobs;

        // RunningSign instance
        private readonly RunningSign RunSign = new RunningSign();

        // Inventory dictionary: maps item types to their total quantities
        private readonly Dictionary<MyItemType, float> _inventory = new Dictionary<MyItemType, float>();

        // new inventory dictionary
        public Dictionary<MyItemType, float> Inventory => _inventory;

        // singleton Program Instance
        public static Program Instance;

        public Program()
        {
            Instance = this;
            
            viewList.Add(new AmmoManagerInfo());
            viewList.Add(new StorageManagerInfo());
            viewList.Add(new RefineryManagerInfo());
            viewList.Add(new AssemblerManagerInfo());

            LoopManager.Init();

            if (Config.Instance.collect_all_Ore) Lists.Data.collectAll_List.Add(IG_ + "Ore");
            if (Config.Instance.collect_all_Ingot) Lists.Data.collectAll_List.Add(IG_ + IG_I);
            if (Config.Instance.collect_all_Component) Lists.Data.collectAll_List.Add(IG_ + IG_Component);

            _jobs = new Job[]
            {
                new LoopManager(),
                new ChangeAutoCraftingSettingsJob(),
                new ClearJob(),
                new GridScanningJob(),
                new FindControllingGunJob(),
                new RefreshControllingGunsJob(),
                new FindStorageContainersJob(),
                new StackingJob(
                    new StackingSingleJob(),
                    new StackingAlphaJob(),
                    new StackingBetaJob(),
                    new StackingDeltaJob(),
                    new StackingGammaJob()),
                new RefreshRefineryListJob(),
                new RefreshAssemblerListJob(),
                new InventoryClearingJob("SMSflaggedClearing", Lists.Data.SmsFlagedInventoryList),
                new InventoryClearingJob("NoneSMSflaggedClearing", Lists.Data.NoneSmsFlagedInventoryList, Lists.Data.collectAll_List),
                new StorageInventoryRefreshJob(),
                new FindingRefinerysJob(),
                new RefineryManagerJob(),
                new FindingAssemblersJob(),
                new AssemblerBluePrintManagerJob(),
                new CalculatingAmountOfInactiveBluePrintItemsJob(),
                new AutoCraftingJob(),
                new ViewManagerJob(),
                new SendInfosToSmsJob()
            };
        }

        // Current TODO:
        // ==================================================
        // - refactoring the VanillaRefinerymanager
        // - build proper Managers for all Blocks, also Assemblers
        // - Refinery use storage inv for input
        //
        // long term TODOS:
        // ==================================================
        // TODO: Tools class implementing
        // TODO: refactoring, refactoring, refactoring...
        // TODO: detecting other PIM blocks
        // TODO: container for ammo not needed when armory is defined
        // TODO: DNSK Mod update
        // TODO: create class for runsign

        void Main(string argument, UpdateType updateSource)
        {
            if (argument != "")
            {
                CommandDispatcher.Dispatch(argument);
                return;
            }

            do
            {
                if (_jobs[Loop.Data.CurrentJobIndex].Schedule() == Job.ScheduleResult.Done)
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

        void WritePimRunInfo()
        {
            var s = Strings.PimVersion
                + Strings.PimCopyright
                + RunSign.getRunningSign()
                + (LoopManager.IsMaster
                    ? (" Running / " + LoopManager.CurrentInstructionAmount + " inst. per run\ncurrent cycle: " + Propertys.Data.CurrentCycleInSec.ToString("0.0") + " sec.\n" + infoString)
                    : "Standby\nMaster: " + Tools.MySelf.CustomName);

            Echo(s);

            if (Config.Instance.ShowInfoPBLcd)
            {
                var tp = Me.GetSurface(0);
                tp.Alignment = TextAlignment.LEFT;
                tp.ContentType = ContentType.TEXT_AND_IMAGE;
                tp.WriteText(s);
            }
        }
    }
}
