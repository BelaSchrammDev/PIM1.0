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

        // Inventory dictionary: maps item types to their total quantities
        private readonly Dictionary<MyItemType, float> _inventory = new Dictionary<MyItemType, float>();

        // new inventory dictionary
        public Dictionary<MyItemType, float> Inventory => _inventory;

        // singleton Program Instance
        public static Program Instance;

        public static string VersionShortInfo = "SMS.inventorymanager 2.0";

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

        void WritePimRunInfo()
        {
            var s = Strings.PimVersion
                + Strings.PimCopyright
                + RunSign.getRunningSign()
                + (LoopManager.IsMaster
                    ? (" Running / " + LoopManager.CurrentInstructionAmount + " inst. per run\ncurrent cycle: " + Properties.Data.CurrentCycleInSec.ToString("0.0") + " sec.\n" + infoString)
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
