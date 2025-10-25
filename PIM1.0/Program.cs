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

        // Inventory dictionary: maps item types to their total quantities
        private readonly Dictionary<MyItemType, float> _inventory = new Dictionary<MyItemType, float>();

        // Expose inventory to jobs
        public Dictionary<MyItemType, float> Inventory => _inventory;

        // singleton Program Instance
        public static Program Instance;

        // old property section ---------------------------------------------------------------------------------

        bool ShowInfoPBLcd = true;
        static bool delete_queueItem_if_max = true;
        static bool always_recycle_greywater = true;
        static bool assemblers_off = true;
        static bool refinerys_off = true;
        bool collect_all_Ore = true;
        bool collect_all_Ingot = true;
        bool collect_all_Component = true;
        int stacking_cycle = 10;
        StopWatch MainLoopTimeSpan = new StopWatch(3);
        static int AutocraftingThreshold = 80;

        static string LCD_DebugString = "";

        static Dictionary<string, float> inventar = new Dictionary<string, float>();
         
        static Dictionary<string, bool> usedMods = new Dictionary<string, bool>();
        List<string> mods = new List<string>(); string curmod = Strings.M_Vanilla;

        List<IMyTerminalBlock> tbl = new List<IMyTerminalBlock>();
        int m0 = -1; int m1, m2 = 0; List<string> s0;

        static Dictionary<string, DisplayBox> DisplayBoxList = new Dictionary<string, DisplayBox>();

        // old property section --------------------------------------------------------------------------------- END

        List<string> autocrafting_Types = new List<string>();
        void InitAutoCraftingTypes()
        {
            foreach (var bpType in Lists.Data.BluePrints_Active.Values)
                if (!autocrafting_Types.Contains(bpType.AutoCraftingType))
                    autocrafting_Types.Add(bpType.AutoCraftingType);
        }
        string bigSpaces = new string(' ', 85);
        const string AutoCraftingTypeStringName = "AutocraftingTypes";

        
        
        void writeInfo()
        {
            var s = Strings.PimVersion + Strings.PimCopyright + getRunningSign() + (LoopManager.IsMaster ? (" Running / " + LoopManager.CurrentInstructionAmount + " inst. per run\ncurrent cycle: " + Propertys.Data.CurrentCycleInSec.ToString("0.0") + " sec.\n" + infoString) : "Standby\nMaster: " + Tools.MySelf.CustomName);
            Echo(s);
            if (ShowInfoPBLcd)
            {
                var tp = Me.GetSurface(0);
                tp.Alignment = TextAlignment.LEFT;
                tp.ContentType = ContentType.TEXT_AND_IMAGE;
                tp.WriteText(s);
            }
        }

        

        public Program()
        {
            Instance = this;
            
            viewList.Add(new AmmoManagerInfo());
            viewList.Add(new StorageManagerInfo());
            viewList.Add(new RefineryManagerInfo());
            viewList.Add(new AssemblerManagerInfo());

            LoopManager.Init();

            if (collect_all_Ore) Lists.Data.collectAll_List.Add(IG_ + "Ore");
            if (collect_all_Ingot) Lists.Data.collectAll_List.Add(IG_ + IG_I);
            if (collect_all_Component) Lists.Data.collectAll_List.Add(IG_ + IG_Component);

            _jobs = new Job[]
            {
                new LoopManager(this),
                new ChangeAutoCraftingSettingsJob(this),
                new ClearJob(this),
                new GridScanningJob(this),
                new FindControllingGunJob(this),
                new RefreshControllingGunsJob(this),
                new FindStorageContainersJob(this),
                new StackingJob(this, "StackingJob",
                    new StackingSingleJob(this),
                    new StackingAlphaJob(this),
                    new StackingBetaJob(this),
                    new StackingDeltaJob(this),
                    new StackingGammaJob(this)),
                new RefreshRefineryListJob(this),
                new RefreshAssemblerListJob(this),
                new InventoryClearingJob(this, "NoneSMSflaggedClearing", Lists.Data.SmsFlagedInventoryList),
                new InventoryClearingJob(this, "NoneSMSflaggedClearing", Lists.Data.NoneSmsFlagedInventoryList, Lists.Data.collectAll_List),
                new StorageInventoryRefreshJob(this),
            };
        }

        bool maxInstructions()
        {
            return LoopManager.RunTime.CurrentInstructionCount > LoopManager.CurrentInstructionAmount; 
        }

        void Main(string argument, UpdateType updateSource)
        {
            if (argument != "")
            {
                CommandDispatcher.Dispatch(argument);
                return;
            }

            OldMainLoop(updateSource);

            writeInfo();
        }

        void CalcIngotPrio()
        {
            foreach (var pl in ingotprio.Values) foreach (IPrio ip in pl) ip.setPrio(0);
            foreach (var refSubType in Refinery.refineryTypesAcceptedBlueprintsList.Keys)
            {
                foreach (var refBluePrint in Refinery.refineryTypesAcceptedBlueprintsList[refSubType])
                {
                    var inputOre = refBluePrint.InputID;
                    if (inventar.ContainsKey(inputOre) && inventar[inputOre] > 0)
                    {
                        if (refBluePrint.IsScrap) addPrio(refSubType, refBluePrint, 9999);
                        else
                        {
                            var oreamount = inventar[refBluePrint.InputID];
                            var ingotamount = inventar.GetValueOrDefault(refBluePrint.OutputID, 0);
                            if (ingotamount == 0) addPrio(refSubType, refBluePrint, 200);
                            else if (ingotamount < 500) addPrio(refSubType, refBluePrint, 150);
                            else if (ingotamount < oreamount) addPrio(refSubType, refBluePrint, 100 - (int)(ingotamount / (oreamount / 97.0f)));
                            else addPrio(refSubType, refBluePrint, 1);
                        }
                    }
                }
            }
            LoadAndRenderOrePrioDefs();
        }

        static int getIntegerWithPräfix(string cstr)
        {
            if (cstr == "") return 0;
            var cstrlist = cstr.Split(' ');
            if (cstrlist.Count() == 0) return 0;
            float wr;
            if (!float.TryParse(cstrlist[0], out wr)) return 0;
            if (cstrlist.Count() == 2)
            {
                if (cstrlist[1] == "k") wr *= 1000;
                else if (cstrlist[1] == "M") wr *= 1000000;
            }
            return (int)wr;
        }

        static int getInteger(string cstr) { float wr; float.TryParse(cstr.Trim().Split('.')[0], out wr); return (int)wr; }
        // runningsign
        int r = 0;
        int rc = 1;
        int mr = 7;
        StringBuilderExtended getRunningSign()
        {
            runningSign.SetText('|');
            r += rc;
            if (r < 0)
            {
                r = 1;
                rc = 1;
            }
            else if (r > mr)
            {
                r = mr - 1;
                rc = -1;
            }
            for (int i = 0; i <= mr; i++) runningSign.Append(i == r ? (rc < 0 ? '<' : '>') : ' ');
            runningSign.Append("| ");
            return runningSign;
        }

        StringBuilderExtended runningSign = new StringBuilderExtended(10);
    }
}
