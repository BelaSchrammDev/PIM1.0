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
        static string debugString = "";
        static Dictionary<string, float> inventar = new Dictionary<string, float>();
        static List<StorageInventory> storageinvs = new List<StorageInventory>();
        
        static List<Assembler> AssemblerList = new List<Assembler>();
        List<IMyUserControllableGun> ugun = new List<IMyUserControllableGun>();
        List<IMyTerminalBlock> tbl = new List<IMyTerminalBlock>();
        List<string> collectAll_List = new List<string>();
        string gungroupName = "PIM controlled Guns";
        static List<Gun> guns = new List<Gun>();
        static List<StorageCargo> storageCargos = new List<StorageCargo>();
        Dictionary<string, CargoUse> CargoUseList = new Dictionary<string, CargoUse>();
        static Dictionary<string, bool> usedMods = new Dictionary<string, bool>();
        List<string> mods = new List<string>(); string curmod = M_Vanilla;
        int m0 = -1; int m1, m2 = 0; List<string> s0;
        const string SI1 = "PIM v1.1", SI2 = "c (c) BelaOkuma\n", SMS = "SMS v1.4", X_StorageTag = "(sms,storage)";
        const string X_Config = "### Config ###", X_Config_end = "### Config End ###", X_Line = "  / =================================\n", X_UseConveyor = "UseConveyor";
        const string X_Autocrafting_treshold = "Autocrafting_threshold";
        const string M_Vanilla = "Vanilla", M_SigmaDraconisCore = "SigmaDraconisCoreMod", M_HSR = "HSR_Mod", M_NorthWindWeapons = "NorthWindWeaponsMod", M_AryxEpsteinDrive = "AryxEpsteinDriveMod", M_PlantCook = "PlantAndCookMod", M_EatDrinkSleep = "EatDrinkSleepRepeatMod", M_IndustrialOverhaulLLMod = "IndustrialOverhaulLockLoadMod", M_IndustrialOverhaulWaterMod = "IndustrialOverhaulWaterMod", M_IndustrialOverhaulMod = "IndustrialOverhaulMod", M_DailyNeedsSurvival = "DailyNeedsSurvivalMod", M_AzimuthThruster = "AzimuthThrusterMod", M_SG_Gates = "StarGateMod_Gates", M_SG_Ores = "StarGateMod_Ores", M_PaintGun = "PaintGunMod", M_DeuteriumReactor = "DeuteriumReactorMod", M_Shield = "DefenseShieldMod", M_RailGun = "MCRN_RailGunMod", M_HomingWeaponry = "MWI_HomingWeaponryMod";
        const string AC_ToolsAndGuns = "Tools&Guns", IG_Food = "Food", IG_Component = "Component", IG_I = "Ingot", IG_Ingot = IG_I + " ", IG_Com = IG_Component + " ", IG_Datas = "Datapad", IG_Kits = "ConsumableItem", IG_K = IG_Kits + " " , IG_Phys = "PhysicalObject", IG_P = IG_Phys + " ", IG_Tools = "PhysicalGunObject", IG_HBottles = "GasContainerObject", IG_OBottles = "OxygenContainerObject", IG_Ammo = "AmmoMagazine", IG_ = "MyObjectBuilder_", IG_Seeds = "SeedItem", IG_S = IG_Seeds + " ";
        static Dictionary<string, AmmoDefs> ammoDefs = new Dictionary<string, AmmoDefs>();
        static Dictionary<string, DisplayBox> DisplayBoxList = new Dictionary<string, DisplayBox>();
        
        DateTime StackingCounter = DateTime.Now;
        static List<StackItem> StackItemList = new List<StackItem>();
        int stack_mode = 0;
        string stack_type = "";
        int cur_stack_type = 0;
        static string[] stack_types = new string[] { "Component", "Ore", "Ingot" };
        bool if_true(string str) { return Convert.ToBoolean(str); }
        List<string> autocrafting_Types = new List<string>();
        void InitAutoCraftingTypes()
        {
            foreach (var bpType in Lists.BluePrints_Active.Values)
                if (!autocrafting_Types.Contains(bpType.AutoCraftingType))
                    autocrafting_Types.Add(bpType.AutoCraftingType);
        }
        string bigSpaces = new string(' ', 85);
        const string AutoCraftingTypeStringName = "AutocraftingTypes";

        
        
        void writeInfo()
        {
            var s = SI1 + SI2 + getRunningSign() + (LoopManager.Master == null ? (" Running / " + LoopManager.CurrentInstructionAmount + " inst. per run\ncurrent cycle: " + Propertys.CurrentCycleInSec.ToString("0.0") + " sec.\n" + infoString) : "Standby\nMaster: " + LoopManager.Master.CustomName);
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
            viewList.Add(new AmmoManagerInfo());
            viewList.Add(new StorageManagerInfo());
            viewList.Add(new RefineryManagerInfo());
            viewList.Add(new AssemblerManagerInfo());

            LoadConfig();
            InitAssemblerBluePrints();
            InitRefineryBlueprints();
            LoopManager.Init(this);

            if (collect_all_Ore) collectAll_List.Add(IG_ + "Ore");
            if (collect_all_Ingot) collectAll_List.Add(IG_ + IG_I);
            if (collect_all_Component) collectAll_List.Add(IG_ + IG_Component);
            stack_type = stack_types[0];

            _jobs = new Job[]
            {
                new LoopManager(this),
                new ChangeAutoCraftingSettingsJob(this),
                new ClearJob(this),
                new GridInventoryScanningJob(this, "GridInventoryScanningJob"),

            };
        }
        
        bool maxInstructions()
        {
            return LoopManager.rti.CurrentInstructionCount > LoopManager.CurrentInstructionAmount; 
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

        StringBuilderExtended runningSign = new StringBuilderExtended(10);

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
    }
}
