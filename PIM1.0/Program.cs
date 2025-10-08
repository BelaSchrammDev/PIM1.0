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
        const int minIC = 300, maxIC = 5000;
        bool firstRun = true;
        static Dictionary<string, float> inventar = new Dictionary<string, float>();
        static bool changeAutoCraftingSettings = true;
        static List<StorageInventory> storageinvs = new List<StorageInventory>();
        List<IMyRefinery> raff = new List<IMyRefinery>();
        List<IMyAssembler> ass = new List<IMyAssembler>();
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
        static Dictionary<string, AssemblerBluePrint> bprints = new Dictionary<string, AssemblerBluePrint>();
        static Dictionary<string, AssemblerBluePrint> bprints_pool = new Dictionary<string, AssemblerBluePrint>();
        double currentCycleInSec = 0f; int m0 = 1; int m1, m2 = 0; List<string> s0; static IMyGridProgramRuntimeInfo rti; IMyProgrammableBlock master;
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
            foreach (var bpType in bprints.Values)
                if (!autocrafting_Types.Contains(bpType.AutoCraftingType))
                    autocrafting_Types.Add(bpType.AutoCraftingType);
        }
        string bigSpaces = new string(' ', 85);
        const string AutoCraftingTypeStringName = "AutocraftingTypes";
        Filter filter = new Filter();
        void loadAutocratingDefinitions()
        {
            var lcds = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(lcds, block => block.CustomName.Contains(Autocrafting) && Me.CubeGrid == block.CubeGrid);
            foreach (var lcd in lcds)
            {
                var ac_Types = IG_Component + "," + IG_Ammo;
                var act_new = false;
                var ac_TypesFilter = new Dictionary<string, string>();
                foreach (var s1 in lcd.GetText().Split('\n'))
                {
                    var acLines = s1.Split('|', '=', '%', ':');
                    for (int i = 0; i < acLines.Count(); i++) acLines[i] = acLines[i].Trim(' ', '\u00AD');
                    if (acLines.Count() == 0 || acLines[0] == "" || acLines[0][0] == '/') continue;
                    else if (acLines[0] == AutoCraftingTypeStringName && acLines.Count() > 1)
                    {
                        if (acLines[1] != "") ac_Types = acLines[1];
                    }
                    else if (acLines[0] == X_Autocrafting_treshold)
                    {
                        // threshold loading
                        var oldacf = AutocraftingThreshold;
                        AutocraftingThreshold = getInteger(acLines[1]);
                        if (AutocraftingThreshold == 0) AutocraftingThreshold = 80;
                        if (oldacf != AutocraftingThreshold) act_new = true;
                    }
                    else if (acLines[0] == "Type" && acLines.Count() > 2)
                    {
                        if (!ac_TypesFilter.ContainsKey(acLines[1])) ac_TypesFilter.Add(acLines[1], acLines[2]);
                    }
                    else if (acLines.Count() == 6)
                    {
                        if (bprints.ContainsKey(acLines[4])) bprints[acLines[4]].SetMaximumAmount(getIntegerWithPräfix(acLines[2]));
                    }
                }
                if (act_new) AssemblerBluePrint.SetAutocraftingThresholdNew();
                // write autocrafting config
                var acString = "/ Autocraftingdefinition:\n";
                acString += "/ add '...(sms)' to the name of assemblers to crafting their items,\n/ and set the max quantity as you want\n\n";
                acString += "/ if the quantity of items falls below this percentage value,\n/ then it will be increased to max.\n" + X_Autocrafting_treshold + " = " + AutocraftingThreshold + "%\n\n";
                acString += "/ possible autocrafting types, please add them separated by comma.\n/ ";
                foreach (var t in autocrafting_Types) acString += t + ",";
                acString += "\n" + AutoCraftingTypeStringName + "=" + ac_Types;
                acString += "\n\n/                           Item            |     current    ­|       max      ­|   assembly\n";
                var ac_TypesList = ac_Types.Split(',')
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToArray();
                foreach (var actype in ac_TypesList)
                {
                    acString += line2pur + "\n Type : " + actype + " = ";
                    if (ac_TypesFilter.ContainsKey(actype))
                    {
                        acString += (ac_TypesFilter[actype] == "" ? " * " : ac_TypesFilter[actype]);
                        filter.InitFilter(ac_TypesFilter[actype]);
                    }
                    else
                    {
                        acString += " * ";
                        filter.SetFilterToAll();
                    }
                    acString += line2;
                    var bpList = bprints.Values.ToList().FindAll(b => b.AutoCraftingType == actype && filter.IfFilter(b.AutoCraftingName));
                    bpList.Sort((x, y) => x.AutoCraftingName.CompareTo(y.AutoCraftingName));
                    foreach (var bp in bpList)
                    {
                        acString += " "
                            + GetDisplayBoxString(bp.AutoCraftingName, 60)
                            + " | "
                            + GetDisplayBoxString(bp.CurrentItemAmount, 25)
                            + " | "
                            + GetDisplayBoxString(bp.MaximumItemAmount, 25)
                            + " | "
                            + GetDisplayBoxString((bp.AssemblyAmount > 0 ? bp.AssemblyAmount : (bp.RefineryAmount > 0 ? bp.RefineryAmount : 0)), 25)
                            + bigSpaces
                            + "|"
                            + bp.BlueprintID
                            + "|\n";
                    }
                }
                lcd.Alignment = TextAlignment.LEFT;
                lcd.ContentType = ContentType.TEXT_AND_IMAGE;
                lcd.WriteText(acString);
            }
        }
        
        void writeInfo()
        {
            var s = SI1 + SI2 + getRunningSign() + (master == null ? (" Running / " + MAXIC + " inst. per run\ncurrent cycle: " + currentCycleInSec.ToString("0.0") + " sec.\n" + infoString) : "Standby\nMaster: " + master.CustomName);
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
            MAXIC = minIC;
            rti = Runtime;
            Slave();
            rti.UpdateFrequency = UpdateFrequency.Update10;
            if (collect_all_Ore) collectAll_List.Add(IG_ + "Ore");
            if (collect_all_Ingot) collectAll_List.Add(IG_ + IG_I);
            if (collect_all_Component) collectAll_List.Add(IG_ + IG_Component);
            stack_type = stack_types[0];
        }
        bool maxInstructions() { return rti.CurrentInstructionCount > MAXIC; }
        DateTime lastStart = DateTime.Now;
        void Main(string argument, UpdateType updateSource)
        {
            if (argument != "")
            {
                switch (argument.ToLower())
                {
                    case "flushrefinerys_all":
                        foreach (var o in RefineryList) o.FlushAllInventorys();
                        SetInfo("all (" + RefineryList.Count + ") refinerys flushed.");
                        break;
                    default:
                        SetInfo("unknow command: \"" + argument + "\"");
                        break;

                }
                return;
            }
            do
            {
                switch (m0)
                {
                    case 0:
                        if (MainLoopTimeSpan.IfTimeSpanReady()) m0++;  // ToDo: deswegen nur alle 5 sec. cyclus???
                        else { writeInfo(); return; }
                        currentCycleInSec = (DateTime.Now - lastStart).TotalSeconds;
                        lastStart = DateTime.Now;
                        break;
                    case 1:
                        if (!changeAutoCraftingSettings)
                        {
                            debugString += "kein calc_ACDef\n";
                            m0 += 2;
                            break;
                        }
                        debugString += "calc_ACDef\n";
                        GridTerminalSystem.GetBlocksOfType<IMyAssembler>(ass, block => block.CubeGrid == Me.CubeGrid);
                        GridTerminalSystem.GetBlocksOfType<IMyRefinery>(raff, block => block.CubeGrid == Me.CubeGrid);
                        s0 = new List<string>(bprints.Keys);
                        for (int i = s0.Count - 1; i >= 0; i--)
                        {
                            var b = bprints[s0[i]];
                            bprints_pool.Add(s0[i], b);
                            bprints.Remove(s0[i]);
                        }
                        s0 = new List<string>(bprints_pool.Keys);
                        m1 = s0.Count - 1;
                        m0++;
                        changeAutoCraftingSettings = false;
                        break;
                    case 2:
                        for (int i = m1; i >= 0; i--, m1--)
                        {
                            if (maxInstructions()) { writeInfo(); return; }
                            var b = bprints_pool[s0[i]];
                            if (s0[i] == Ingot.SubFresh || s0[i] == (Refinery.BluePrintID_SpentFuelReprocessing))
                            {
                                foreach (var r in raff)
                                {
                                    var subTypeName = r.BlockDefinition.SubtypeId;
                                    if (r.CustomName.Contains("(sms") && (subTypeName.Contains("Hydroponics") || subTypeName.Contains("Reprocessor")))
                                    {
                                        bprints.Add(s0[i], b);
                                        bprints_pool.Remove(s0[i]);
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                foreach (var a in ass)
                                {
                                    if (a.CustomName.Contains("(sms") && a.CanUseBlueprint(b.definition_id))
                                    {
                                        bprints.Add(s0[i], b);
                                        bprints_pool.Remove(s0[i]);
                                        break;
                                    }
                                }
                            }
                        }
                        InitAutoCraftingTypes();
                        m0++;
                        break;
                    case 3:
                        if (!Slave()) { writeInfo(); return; }
                        loadAutocratingDefinitions();
                        DebugPrint();
                        ClearInventoryList(inventar);
                        InventoryList_SMSflagged.Clear();
                        InventoryList_nonSMSflagged.Clear();
                        CargoUseList.Clear();
                        foreach (var ivl in InventoryManagerList.Values) ivl.Clear();
                        InventoryManagerList.Clear();
                        m0++;
                        break;
                    case 4:
                        GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(tbl, block => block.IsSameConstructAs(Me));
                        m1 = 0;
                        m0++;
                        break;
                    case 5:
                        for (int i = m1; i < tbl.Count; i++, m1++)
                        {
                            if (maxInstructions()) { writeInfo(); return; }
                            pushTerminalBlock(tbl[i]);
                        }
                        m0++;
                        break;
                    case 6:
                        GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(tbl, block => block.IsSameConstructAs(Me));
                        m1 = 0;
                        m0++;
                        break;
                    case 7:
                        for (int i = m1; i < tbl.Count; i++, m1++)
                        {
                            if (maxInstructions()) { writeInfo(); return; }
                            pushTerminalBlock(tbl[i]);
                        }
                        m0++;
                        break;
                    case 8:
                        GridTerminalSystem.GetBlocksOfType<IMyShipController>(tbl, block => block.IsSameConstructAs(Me));
                        m1 = 0;
                        m0++;
                        break;
                    case 9:
                        for (int i = m1; i < tbl.Count; i++, m1++)
                        {
                            if (maxInstructions()) { writeInfo(); return; }
                            pushTerminalBlock(tbl[i]);
                        }
                        m0++;
                        break;
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
                            if (maxInstructions()) { writeInfo(); return; }
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
                            if (maxInstructions()) { writeInfo(); return; }
                            guns[i].Refresh();
                        }
                        m0++;
                        break;
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
                            if (maxInstructions()) { writeInfo(); return; }
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
                    case 24:
                        GridTerminalSystem.GetBlocksOfType<IMyShipWelder>(tbl, block => block.IsSameConstructAs(Me));
                        m1 = 0;
                        m0++;
                        break;
                    case 25:
                        for (int i = m1; i < tbl.Count; i++, m1++)
                        {
                            if (maxInstructions()) { writeInfo(); return; }
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
                        GridTerminalSystem.GetBlocksOfType<IMyShipGrinder>(tbl, block => block.IsSameConstructAs(Me));
                        m1 = 0;
                        m0++;
                        break;
                    case 27:
                        for (int i = m1; i < tbl.Count; i++, m1++)
                        {
                            if (maxInstructions()) { writeInfo(); return; }
                            pushTerminalBlock(tbl[i]);
                        }
                        m0++;
                        break;
                    case 28:
                        GridTerminalSystem.GetBlocksOfType<IMyShipDrill>(tbl, block => block.IsSameConstructAs(Me));
                        m1 = 0;
                        m0++;
                        break;
                    case 29:
                        for (int i = m1; i < tbl.Count; i++, m1++)
                        {
                            if (maxInstructions()) { writeInfo(); return; }
                            pushTerminalBlock(tbl[i]);
                        }
                        m0++;
                        break;
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
                                    if (maxInstructions()) { writeInfo(); return; }
                                    StackItemList[i].stacking_single();
                                }
                                m0++;
                                stack_mode++;
                                break;
                            case 1:
                                for (int i = m1; i < StackItemList.Count; i++, m1++)
                                {
                                    if (maxInstructions()) { writeInfo(); return; }
                                    StackItemList[i].stacking_alpha();
                                }
                                m0++;
                                stack_mode++;
                                break;
                            case 2:
                                for (int i = m1; i < StackItemList.Count; i++, m1++)
                                {
                                    if (maxInstructions()) { writeInfo(); return; }
                                    if (!StackItemList[i].stacking_beta()) break;
                                }
                                m0++;
                                stack_mode++;
                                break;
                            case 3:
                                for (int i = m1; i < StackItemList.Count; i++, m1++)
                                {
                                    if (maxInstructions()) { writeInfo(); return; }
                                    StackItemList[i].stacking_delta();
                                }
                                m0++;
                                stack_mode++;
                                break;
                            case 4:
                                if (StackItemList.Count > 0) for (int i = m1; i < 300; i++, m1++)
                                    {
                                        if (maxInstructions()) { writeInfo(); return; }
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
                        foreach (var b in bprints.Values) b.AssemblyAmount = 0;
                        foreach (var b in bprints_pool.Values) b.AssemblyAmount = 0;
                        GridTerminalSystem.GetBlocksOfType<IMyRefinery>(raff, block => block.CubeGrid == Me.CubeGrid);
                        for (int i = RefineryList.Count - 1; i >= 0; i--)
                        {
                            if (raff.Contains(RefineryList[i].RefineryBlock)) raff.Remove(RefineryList[i].RefineryBlock);
                            else
                            {
                                changeAutoCraftingSettings = true;
                                RefineryList.Remove(RefineryList[i]);
                            }
                        }
                        Refinery.priobt = "";
                        m1 = raff.Count - 1;
                        m0++;
                        break;
                    case 34:
                        for (int i = m1; i >= 0; i--, m1--)
                        {
                            if (maxInstructions()) { writeInfo(); return; }
                            RefineryList.Add(new Refinery(raff[i]));
                        }
                        m0++;
                        break;
                    case 35:
                        GridTerminalSystem.GetBlocksOfType<IMyAssembler>(ass, block => block.CubeGrid == Me.CubeGrid);
                        for (int i = AssemblerList.Count - 1; i >= 0; i--)
                        {
                            if (ass.Contains(AssemblerList[i].AssemblerBlock)) ass.Remove(AssemblerList[i].AssemblerBlock);
                            else
                            {
                                changeAutoCraftingSettings = true;
                                AssemblerList.Remove(AssemblerList[i]);
                            }
                        }
                        m1 = ass.Count - 1;
                        m0++;
                        break;
                    case 36:
                        for (int i = m1; i >= 0; i--, m1--)
                        {
                            if (maxInstructions()) { writeInfo(); return; }
                            AssemblerList.Add(new Assembler(ass[i]));
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
                            if (maxInstructions()) { writeInfo(); return; }
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
                            if (maxInstructions()) { writeInfo(); return; }
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
                            if (maxInstructions()) { writeInfo(); return; }
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
                            if (maxInstructions()) { writeInfo(); return; }
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
                            if (maxInstructions()) { writeInfo(); return; }
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
                            if (maxInstructions()) { writeInfo(); return; }
                            AssemblerList[i].Refresh();
                        }
                        m0++;
                        break;
                    case 49:
                        s0 = new List<string>(bprints.Keys);
                        m1 = 0;
                        m0++;
                        break;
                    case 50:
                        for (int i = m1; i < s0.Count; i++, m1++)
                        {
                            if (maxInstructions()) { writeInfo(); return; }
                            var b = bprints[s0[i]];
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
                        s0 = new List<string>(bprints_pool.Keys);
                        m1 = 0;
                        m0++;
                        break;
                    case 52:
                        for (int i = m1; i < s0.Count; i++, m1++)
                        {
                            if (maxInstructions()) { writeInfo(); return; }
                            var b = bprints_pool[s0[i]];
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
                            if (maxInstructions()) { writeInfo(); return; }
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
                        firstRun = false;
                        m0 = 0;
                        break;
                }
            }
            while (!maxInstructions());
            writeInfo();
        }
        static void ClearInventoryList(Dictionary<string, float> invList)
        {
            var keys = invList.Keys.ToArray();
            for (int i = 0; i < keys.Length; i++) invList[keys[i]] = 0;
        }
        static void AddToInventory(IMyInventory box, Dictionary<string, float> ilist = null)
        {
            var boxl = new List<MyInventoryItem>();
            box.GetItems(boxl);
            foreach (var boxi in boxl)
            {
                string index = GetPIMItemID(boxi.Type);
                var boxia = (float)boxi.Amount;
                if (inventar.ContainsKey(index)) inventar[index] += boxia;
                else inventar.Add(index, boxia);
                if (ilist != null)
                {
                    if (ilist.ContainsKey(index)) ilist[index] += boxia;
                    else ilist.Add(index, boxia);
                }
            }
        }
        void addToInventoryList(IMyInventory inv, Dictionary<string, string> tags)
        {
            foreach (var tag in tags.Keys)
            {
                var ingame = Tag2Ingame(tag);
                if (ingame != "")
                {
                    if (!InventoryManagerList.ContainsKey(ingame)) InventoryManagerList.Add(ingame, new List<IMyInventory>());
                    if (!InventoryManagerList[ingame].Contains(inv)) InventoryManagerList[ingame].Add(inv);
                    if (!CargoUseList.ContainsKey(tag)) CargoUseList.Add(tag, new CargoUse(tag));
                    CargoUseList[tag].AddCurrentAndMaxCargocapacity(inv.CurrentVolume.RawValue / 1000, inv.MaxVolume.RawValue / 1000);
                }
            }
        }
        static Dictionary<string, List<IMyInventory>> InventoryManagerList = new Dictionary<string, List<IMyInventory>>();
        static List<IMyInventory> InventoryList_SMSflagged = new List<IMyInventory>();
        static List<IMyInventory> InventoryList_nonSMSflagged = new List<IMyInventory>();
        void pushTerminalBlock(IMyTerminalBlock t)
        {
            if (t.HasInventory)
            {
                var inv = t.GetInventory(0);
                AddToInventory(inv);
                if (t.CustomName.Contains(X_StorageTag)) return;
                Parameter pm = new Parameter();
                if (pm.ParseArgs(t.CustomName))
                {
                    if (!pm.IsParameter("Keep")) InventoryList_SMSflagged.Add(inv);
                    if (!pm.IsParameter("Infolcd")) addToInventoryList(inv, pm.ParameterList);
                }
                else if (t.BlockDefinition.SubtypeId.Contains("Container") || t.BlockDefinition.SubtypeId.Contains("Connector"))
                {
                    InventoryList_SMSflagged.Add(inv);
                }
                else InventoryList_nonSMSflagged.Add(inv);
            }
        }

        int MAXIC;
        void CalculateMaxIC()
        {
            if (master != null)
            {
                rti.UpdateFrequency = UpdateFrequency.Update100;
                MAXIC = minIC;
            }
            else
            {
                rti.UpdateFrequency = UpdateFrequency.Update10; // ToDo: Zeitspanne regulieren!
                if (currentCycleInSec < 3.5) MAXIC -= 100;
                else if (currentCycleInSec > 4.5) MAXIC += 100;
                if (MAXIC < minIC) MAXIC = minIC;
                else if (MAXIC > maxIC) MAXIC = maxIC;
            }
        }
        const string AssemblerQueueNameSemikolon = "@ASSEMBLERQUEUE;";
        const string ItemMaxNameSemikolon = "@ITEMMAX;";
        bool Slave()
        {
            var comp = new List<IMyProgrammableBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyProgrammableBlock>(comp, block => block.IsSameConstructAs(Me));
            master = null;
            foreach (var p in comp)
            {
                if (p.Enabled && p.DetailedInfo.StartsWith(SI1))
                {
                    if (Me.EntityId < p.EntityId)
                    {
                        master = p;
                        break;
                    }
                }
                else if (p.Enabled && p.DetailedInfo.StartsWith(SMS) && !firstRun)
                {
                    var s = "";
                    bool configteil = false;
                    foreach (var cstr in p.CustomData.Split('\n'))
                    {
                        if (cstr.Contains(X_Config)) configteil = true;
                        if (configteil) s += cstr + '\n';
                        if (cstr.Contains(X_Config_end)) configteil = false;
                    }
                    if (s != "") s += "\n\n";
                    foreach (var a in ingotprio.Keys.ToArray()) if (ingotprio.ContainsKey(a) && !Refinery.priobt.Contains("@" + a)) ingotprio.Remove(a);
                    foreach (var sx in ingotprio.Keys)
                    {
                        s += "@INGOTPRIOLIST;" + sx + "\n";
                        foreach (var i in ingotprio[sx]) if (i.initp > 0) s += "@INGOTPRIO;" + i.refineryBP.OutputIDName + ";" + i.initp + "\n";
                    }
                    foreach (var c in CargoUseList.Values) s += "@CARGOUSE;" + c.type + ";" + c.Current + ";" + c.Maximum + "\n";
                    foreach (var b in bprints.Values)
                    {
                        if (b.MaximumItemAmount > 0) s += ItemMaxNameSemikolon + b.ItemName + ";" + b.MaximumItemAmount + ";" + b.subtype + "\n";
                        if (b.AssemblyAmount > 0) s += AssemblerQueueNameSemikolon + b.ItemName + ";" + b.AssemblyAmount + ";" + b.subtype + "\n";
                        else if (b.RefineryAmount > 0) s += AssemblerQueueNameSemikolon + b.ItemName + ";" + b.RefineryAmount + ";" + b.subtype + "\n";
                    }
                    foreach (var b in bprints_pool.Values)
                    {
                        if (b.MaximumItemAmount > 0) s += ItemMaxNameSemikolon + b.ItemName + ";" + b.MaximumItemAmount + ";" + b.subtype + "\n";
                        if (b.AssemblyAmount > 0) s += AssemblerQueueNameSemikolon + b.ItemName + ";" + b.AssemblyAmount + ";" + b.subtype + "\n";
                        else if (b.RefineryAmount > 0) s += AssemblerQueueNameSemikolon + b.ItemName + ";" + b.RefineryAmount + ";" + b.subtype + "\n";
                    }
                    p.CustomData = s;
                }
            }
            CalculateMaxIC();
            return master == null;
        }

        static string[] seed_cast = new string[]
        {
            IG_S + "Fruit",
            IG_S + "Grain",
            IG_S + "Vegetables",
            IG_S + "Mushrooms",
        };

        static string[] waste_cast = new string[]
        {
                Ore.Organic,
                Ingot.GreyWater
        };

        static string[] food_cast = new string[]
        {
            // Vanilla
            IG_K + "MammalMeatCooked",
            IG_K + "InsectMeatCooked",
            IG_K + "MealPack_KelpCrisp",
            IG_K + "MealPack_FruitBar",
            IG_K + "MealPack_GardenSlaw",
            IG_K + "MealPack_RedPellets",
            IG_K + "MealPack_Chili",
            IG_K + "MealPack_Flatbread",
            IG_K + "MealPack_Ramen",
            IG_K + "MealPack_FruitPastry",
            IG_K + "MealPack_VeggieBurger",
            IG_K + "MealPack_Curry",
            IG_K + "MealPack_GreenPellets",
            IG_K + "MealPack_Dumplings",
            IG_K + "MealPack_Spaghetti",
            IG_K + "MealPack_Lasagna",
            IG_K + "MealPack_Burrito",
            IG_K + "MealPack_FrontierStew",
            IG_K + "MealPack_SearedSabiroid",
            IG_K + "MealPack_SteakDinner",
            IG_K + "Fruit",
            IG_P + "Grain",
            IG_K + "Vegetables",
            IG_K + "Mushrooms",

            // Daily Needs Survival
            Ingot.WaterFood,
            Ingot.CleanWater,
            Ingot.SubFresh,
            Ingot.Nutrients,
            IG_Ingot + "ArtificialFood",
            IG_Ingot + "LuxuryMeal",
            IG_Ingot + "SabiroidSteak",
            IG_Ingot + "VeganFood",
            IG_Ingot + "WolfSteak",
            IG_Ingot + "WolfBouillon",
            IG_Ingot + "SabiroidBouillon",
            IG_Ingot + "CoffeeFood",
            IG_Ingot + "Potatoes",
            IG_Ingot + "Tomatoes",
            IG_Ingot + "Carrots",
            IG_Ingot + "Cucumbers",
            IG_Ingot + "PotatoSeeds",
            IG_Ingot + "TomatoSeeds",
            IG_Ingot + "CarrotSeeds",
            IG_Ingot + "CucumberSeeds",
            IG_Ingot + "Ketchup",
            IG_Ingot + "MartianSpecial",
            "Ore WolfMeat",
            "Ore SabiroidMeat",
            IG_Ingot + "Fertilizer",
            IG_Ingot + "NotBeefBurger",
            IG_Ingot + "ToFurkey",
            IG_Ingot + "SpaceMealBar",
            IG_Ingot + "HotChocolate",
            IG_Ingot + "SpacersBreakfast",
            IG_Ingot + "ProteinShake",
            IG_Ingot + "EmergencyFood",
            // Eat, Drink, Sleep & Repeat
            IG_K + "SparklingWater",
            IG_K + "Emergency_Ration",
            IG_K + "AppleJuice",
            IG_K + "ApplePie",
            IG_K + "Tofu",
            IG_K + "MeatRoasted",
            IG_K + "ShroomSteak",
            IG_K + "Bread",
            IG_K + "Burger",
            IG_K + "Soup",
            IG_K + "MushroomSoup",
            IG_K + "TofuSoup",
            IG_K + "EuropaTea",
            IG_K + "Mushrooms",
            IG_K + "Apple",
            IG_K + "PrlnglesChips",
            IG_K + "LaysChips",
            IG_K + "InterBeer",
            IG_K + "CosmicCoffee",
            IG_K + "ClangCola",
            IG_K + "Meat",
            IG_K + "MeatRoasted",
            IG_Ingot + "Soya",
            IG_Ingot + "Herbs",
            IG_Ingot + "Wheat",
            IG_Ingot + "Pumpkin",
            IG_Ingot + "Cabbage",
        };

        static string TypeCast(string t)
        {
            if (t.Contains("RifleItem") || t.Contains(IG_Ammo) || t.Contains("PistolItem") || t.Contains("LauncherItem")) return "Armory";
            if (seed_cast.Contains(t)) return "Seeds";
            if (food_cast.Contains(t)) return "Food";
            if (waste_cast.Contains(t)) return "Waste";
            return "";
        }

        static void ClearInventory(IMyInventory quelle, List<string> typeID_l = null)
        {
            var von = new List<MyInventoryItem>();
            quelle.GetItems(von);
            if (von.Count() == 0) return;
            for (int j = von.Count() - 1; j >= 0; j--)
            {
                bool success = false;
                var vcon = von[j].Type;
                var clr = true;
                if (typeID_l != null)
                {
                    clr = false;
                    foreach (var nt in typeID_l)
                        if (vcon.TypeId.ToString() == nt)
                        {
                            clr = true;
                            break;
                        }
                }
                if (!clr) continue;
                var idstr = vcon.TypeId.ToString().Split('_')[1];
                var stype = vcon.SubtypeId.ToString();
                var fullid = idstr + " " + stype;
                var atype = TypeCast(fullid);
                if (InventoryManagerList.ContainsKey(fullid)) success = SendItemByNum(quelle, j, InventoryManagerList[fullid]);
                if (!success && atype != "" && InventoryManagerList.ContainsKey(atype)) success = SendItemByNum(quelle, j, InventoryManagerList[atype]);
                if (!success && InventoryManagerList.ContainsKey(idstr)) SendItemByNum(quelle, j, InventoryManagerList[idstr]);

                var idstrPIM = Ingame2Tag(idstr);
                if (InventoryManagerList.ContainsKey(idstr)) ClearWarning(Warning.ID.CARGOMISSING, idstrPIM);
                else SetWarning(Warning.ID.CARGOMISSING, idstrPIM);
            }
        }
        void new_stackcount(IMyInventory quelle, string ti)
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
        static bool clearItemByType(IMyInventory quelle, string type, MyInventoryItem item, float amount = 0)
        {
            var typeID = type.Substring(0, type.IndexOf(' '));
            var stypeID = type.Substring(type.IndexOf(' ') + 1);
            var atype = TypeCast(type);
            var trans = false;
            if (amount == 0) amount = (float)item.Amount;
            if (InventoryManagerList.ContainsKey(type)) trans = SendItemByIItem(quelle, item, amount, InventoryManagerList[type]);
            else if (atype != "" && InventoryManagerList.ContainsKey(atype)) trans = SendItemByIItem(quelle, item, amount, InventoryManagerList[atype]);
            else if (InventoryManagerList.ContainsKey(typeID)) trans = SendItemByIItem(quelle, item, amount, InventoryManagerList[typeID]);
            return trans;
        }
        static bool SendItemByIItem(IMyInventory quelle, MyInventoryItem item, float amount, List<IMyInventory> ziele)
        {
            var volume = (MyFixedPoint)amount * item.Type.GetItemInfo().Volume;
            if (ziele.Count > 0)
            {
                foreach (var zinv in ziele) if (quelle == zinv) return true;
                for (int i = 0; i < ziele.Count; i++)
                {
                    var ziel_inv = ziele[i];
                    if (volume < (ziel_inv.MaxVolume - ziel_inv.CurrentVolume))
                    {
                        if (quelle.TransferItemTo(ziel_inv, item, (MyFixedPoint)amount)) return true;
                    }
                }
            }
            return false;
        }
        static bool SendItemByNum(IMyInventory quelle, int itemnum, List<IMyInventory> ziele)
        {
            var trans = false;
            if (ziele.Count != 0)
            {
                foreach (var zinv in ziele) if (quelle == zinv) return true;
                for (int i = 0; i < ziele.Count; i++)
                {
                    var ziel_inv = ziele[i];
                    if (!ziel_inv.IsFull)
                    {
                        trans = quelle.TransferItemTo(ziel_inv, itemnum, null, true, null);
                    }
                }
            }
            return trans;
        }
        static bool SendItemByType(string iType, float itemAmount, IMyInventory ziel, int? p = null)
        {
            return SendItemByTypeAndSubtype(IG_ + iType.Substring(0, iType.IndexOf(' ')), iType.Substring(iType.IndexOf(' ') + 1), itemAmount, ziel);
        }
        static bool SendItemByTypeAndSubtype(string itemType, string itemSubType, float itemAmount, IMyInventory ziel, int? p = null)
        {
            List<IMyInventory> quellen = null;
            var idstr = itemType.Split('_')[1];
            var idstrPIM = Ingame2Tag(idstr);
            var atype = TypeCast(idstr[1] + " " + itemSubType);
            if (InventoryManagerList.ContainsKey(idstr[1] + " " + itemSubType)) quellen = InventoryManagerList[idstr[1] + " " + itemSubType];
            else if (atype != "" && InventoryManagerList.ContainsKey(atype)) quellen = InventoryManagerList[atype];
            else if (InventoryManagerList.ContainsKey(idstr)) quellen = InventoryManagerList[idstr];
            else
            {
                SetWarning(Warning.ID.CARGOMISSING, idstrPIM);
                return false;
            }
            ClearWarning(Warning.ID.CARGOMISSING, idstrPIM);
            for (int i = 0; i < quellen.Count; i++)
            {
                var von = new List<MyInventoryItem>();
                quellen[i].GetItems(von);
                if (von.Count() > 0)
                {
                    for (int j = von.Count() - 1; j >= 0; j--)
                    {
                        if (von[j].Type.TypeId.ToString() == itemType)
                        {
                            if (von[j].Type.SubtypeId.ToString() == itemSubType)
                            {
                                var menge = (MyFixedPoint)itemAmount;
                                if (quellen[i].TransferItemTo(ziel, j, p, true, menge)) return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        

        static string GetTimeStringFromHours(double h)
        {
            if (h < 1)
            {
                if ((h * 60) > 1) return Math.Round(h * 60, 0) + " min.";
                else return Math.Round(h * 60 * 60, 0) + " s";
            }
            if (h < 24)
            {
                return Math.Round(h, 1) + " h";
            }
            double tage = Math.Round(h / 24, 1);
            if (tage > 365)
            {
                return Math.Round(tage / 365, 1) + " years";
            }
            if (tage < 1.1) return tage + " day";
            else return tage + " days";
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
        const string OrePrioDefString = "(sms,oreprio)";
        const string ResourcenOverview = "(sms,refining)";
        const string Autocrafting = "(sms,autocrafting)";
        const string AmmoPrioDefinition = "(sms,ammoprio)";
        void LoadAndRenderOrePrioDefs()
        {
            var lcds = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(lcds, block => block.CustomName.Contains(OrePrioDefString));
            if (lcds.Count > 0) // ToDo: mehrere LCDs, einlesen und ausgeben trennen
            {
                Refinery.RemoveUnusedRefinerytypeBlueprintLists();
                Dictionary<RefineryBlueprint, int> curPrioList = null;
                Dictionary<IMyTextPanel, string> filterStrings = new Dictionary<IMyTextPanel, string>();
                foreach (var lcd in lcds)
                {
                    var curRefineryType = "";
                    var pstrs = lcd.GetText().Split('\n');
                    // prio laden
                    foreach (var s in pstrs)
                    {
                        var line = s.Split(':', '=', '|');
                        for (int i = 0; i < line.Count(); i++) { line[i] = line[i].Trim(' ', '\u00AD'); }
                        if (line.Length == 0 || line[0].Length == 0 || line[0][0] == '/') continue;
                        if (line.Length >= 5 && line[3].ToLower().StartsWith("oreprio") && (curPrioList != null))
                        {
                            var blueprint = RefineryBlueprints.Find(b => b.Name == line[4]);
                            if (blueprint != null)
                            {
                                int prio = -1;
                                if (!int.TryParse(line[1], out prio)) prio = -1;
                                if (!curPrioList.ContainsKey(blueprint)) curPrioList.Add(blueprint, -1);
                                curPrioList[blueprint] = (prio < 0 ? -1 : (prio > 10000 ? 10000 : prio));
                                if (prio >= 0)
                                {
                                    IPrio ni = IPrio.GetBlueprintPrio(ingotprio[curRefineryType], blueprint);
                                    if (ni != null && ni.prio > 0) ni.setPrio(prio);
                                }
                            }
                        }
                        else if (line.Length >= 2 && line[0].ToLower().StartsWith("refinerytype"))
                        {
                            curPrioList = null;
                            curRefineryType = "";
                            if ((line[1] != "") && Refinery.refineryTypesAcceptedBlueprintsList.ContainsKey(line[1]))
                            {
                                if (OrePrioConfig.ContainsKey(line[1]))
                                {
                                    curPrioList = OrePrioConfig[line[1]];
                                }
                                else
                                {
                                    curPrioList = new Dictionary<RefineryBlueprint, int>();
                                    OrePrioConfig.Add(line[1], curPrioList);
                                }
                                curRefineryType = line[1];
                            }
                        }
                        else if (line.Length > 1 && line[0] == "Filter")
                        {
                            if (!filterStrings.ContainsKey(lcd))
                            {
                                filterStrings.Add(lcd, line[1]);
                            }
                        }
                    }
                }
                RenderOrePrio(lcds, filterStrings);
            }
        }
        void RenderOrePrio(List<IMyTextPanel> lcds, Dictionary<IMyTextPanel, string> filterStrings)
        {
            foreach (var lcd in lcds)
            {
                // prio wieder schreiben
                var fString = "*";
                if (filterStrings.ContainsKey(lcd))
                {
                    filter.InitFilter(filterStrings[lcd]);
                    fString = filterStrings[lcd];
                }
                else
                {
                    filter.SetFilterToAll();
                }
                var priolist = "/ Orepriorityconfig:\n/ only refinerytypes with '(sms)' in the name are displayed.\n\n/ set the 'value' between 1 and 10000\n/ if value = 0 then the ore will be ignored\n/ if value empty then prio will be calculated by PIM.\n/ any type of scrap is always refined first\n\n/ Refinerytypefilter, separated by comma, '*' for all\n Filter: " + fString + "\n\n/                Recipe                       |    Value   |       Current\n";
                foreach (var key in Refinery.refineryTypesAcceptedBlueprintsList.Keys)
                {
                    if (!OrePrioConfig.ContainsKey(key)) OrePrioConfig.Add(key, new Dictionary<RefineryBlueprint, int>());
                    var blueprintList = Refinery.refineryTypesAcceptedBlueprintsList[key].FindAll(o => !o.IsScrap);
                    if (blueprintList.Count < 2 || !filter.IfFilter(key)) continue;
                    priolist += linepur + "\n RefineryType: " + key + line;
                    var curIngotPrioList = ingotprio[key];
                    blueprintList.Sort((x, y) => x.InputIDName.CompareTo(y.InputIDName));
                    foreach (var bp in blueprintList)
                    {
                        var priostr = "-1";
                        var curPrio = IPrio.GetBlueprintPrio(curIngotPrioList, bp);
                        if (OrePrioConfig[key].ContainsKey(bp)) priostr = OrePrioConfig[key][bp].ToString();
                        else OrePrioConfig[key].Add(bp, -1);
                        priolist += " "
                            + (GetDisplayBoxString(bp.Name, 65, true))
                            + " | "
                            + GetDisplayBoxString((priostr == "-1" ? "  |  " : priostr + "  |  "), 23)
                            + ((curPrio != null && curPrio.initp > 0) ? GetDisplayBoxString(curPrio.initp.ToString(), 23) + "  " : "")
                            + bigSpaces
                            + "|OrePrio:"
                            + bp.Name
                            + "|\n";
                    }
                }
                lcd.Alignment = TextAlignment.LEFT;
                lcd.ContentType = ContentType.TEXT_AND_IMAGE;
                lcd.WriteText(priolist);
            }
        }
        void RenderAmmoPrioLCDS()
        {
            var lcds = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(lcds, block => block.CustomName.Contains(AmmoPrioDefinition));
            if (lcds.Count != 0)
            {
                var lcd = lcds[0];
                string[] pstr = lcd.GetText().Split('\n');
                MultiAmmoGuns currentMultiGun = null;
                foreach (var s in pstr)
                {
                    var line = s.Split(':', '=', '|');
                    for (int i = 0; i < line.Count(); i++) { line[i] = line[i].Trim(' ', '\u00AD'); }
                    if (line.Length == 0 || line[0].Length == 0 || line[0][0] == '/') continue;
                    if (line.Length >= 3 && line[0] == "GunType")
                    {
                        currentMultiGun = multiAmmoGuns.GetValueOrDefault(line[2], null);
                    }
                    else if (line.Length >= 3 && currentMultiGun != null)
                    {
                        var adef = currentMultiGun.GetAmmoDefs(line[2]);
                        if (adef != null)
                        {
                            adef.SetAmmoPriority(currentMultiGun.MultiAmmoGuntype, int.Parse(line[0]));
                        }
                    }
                }
                // Prio schreiben...
                var ammoprioString = "/ Ammopriodefinitions:\n/ the prio only affects weapons that can use\n/ different ammunition types. this determines\n/ which one is loaded into the inventory first.\n/ 0 means that the ammunition is not used\n";
                var headerString = "\n" + GetDisplayBoxString("Priority", 25) + " | Ammotyp\n";
                foreach (var mAmmoGuns in multiAmmoGuns)
                {
                    ammoprioString += linepur + "\nGunType: " + mAmmoGuns.Value.DisplayName + bigSpaces + "|" + mAmmoGuns.Value.MultiAmmoGuntype + headerString;
                    AmmoDefs.SetCurrentSortGuntype(mAmmoGuns.Key);
                    mAmmoGuns.Value.ammoDefs.Sort();
                    foreach (var aDef in mAmmoGuns.Value.ammoDefs)
                    {
                        ammoprioString += GetDisplayBoxStringDisplayNull(aDef.GetAmmoPriority(mAmmoGuns.Key), 25) + " | " + GetDisplayBoxString(aDef.GetAmmoBluePrintAutocraftingName(), 60, true) + bigSpaces + " | " + aDef.type + "\n";
                    }
                }
                lcd.Alignment = TextAlignment.LEFT;
                lcd.ContentType = ContentType.TEXT_AND_IMAGE;
                lcd.WriteText(ammoprioString);
            }
        }
        void RenderResourceProccesingLCD()
        {
            var lcds = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(lcds, block => block.CustomName.Contains(ResourcenOverview));
            if (lcds.Count > 0)
            {
                RefineryBlueprint.FillInputOutputAmountAndETA();
                var oresList = " Refiningprogress:\n" + line2lineonly + "\n";
                foreach (var bluePrint in RefineryBlueprints)
                {
                    if (bluePrint.RefineryCount > 0)
                    {
                        oresList
                            += GetDisplayBoxString(bluePrint.Name, 70, true)
                            + " | "
                            + GetDisplayBoxString(bluePrint.InputIDName, bluePrint.InputAmount, 70)
                            + "\n"
                            + GetDisplayBoxString(bluePrint.RefineryCount.ToString() + " Refinerys.", 30, true)
                            + GetDisplayBoxString("-> " + bluePrint.ETA_String, 40, true)
                            + " | "
                            + GetDisplayBoxString(bluePrint.OutputIDName, bluePrint.OutputAmount, 70)
                            + "\n"
                            + line2lineonly
                            + "\n";

                    }
                }
                foreach (var lcd in lcds)
                {
                    lcd.Alignment = TextAlignment.LEFT;
                    lcd.ContentType = ContentType.TEXT_AND_IMAGE;
                    lcd.WriteText(oresList);
                }
            }
        }
        static Dictionary<string, string> ResourcesNameCastList = new Dictionary<string, string>
            {
                { Ore.Stone, Resources.RStone },
                { Ingot.Magnesium, Ingot.Magnesiumpowder },
                { Ingot.Stone, "Gravel" },
                { Ingot.DeuteriumContainer, Resources.RDeuterium },
                { Ore.Ice, "Ice"},
                { Ingot.Carbon, Resources.RCarbon },
            };
        static Dictionary<string, string> ResourcesNameCastListIOMod = new Dictionary<string, string>
            {
                { Ore.Coal, Resources.RCoal },
                { Ore.Bauxite, Resources.RBauxite },
                { Ore.Niter, Resources.RNiter },
                { Ingot.Lithium, Resources.RLithium + " Paste" },
                { Ingot.Sulfur, Resources.RSulfur },
                { Ingot.Niter, Resources.RPotassium + " Nitrate" },
                { Ore.Magnesium, "Crushed Niter" },
                { Ingot.Magnesium, Ingot.Gunpowder},
            };
        static string CastResourceName(string name)
        {
            if (usedMods[M_IndustrialOverhaulMod] && ResourcesNameCastListIOMod.ContainsKey(name)) return ResourcesNameCastListIOMod[name];
            if (ResourcesNameCastList.ContainsKey(name)) return ResourcesNameCastList[name];
            if (name.StartsWith("Ore Crushed")) return "Crushed " + name.Substring(11);
            if (name.StartsWith("Ore Purified")) return "Purified " + name.Substring(12);
            return name;
        }
        const string line = "\n" + linepur + "\n";
        const string linepur = "/-------------------------------------------------------------------------";
        const string line2 = "\n" + line2pur + "\n";
        const string line2pur = "/" + line2lineonly;
        const string line2lineonly = "-------------------------------------------------------------------------------------------";
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
        static string ToArgStr(string qstr)
        {
            var zstr = "";
            var tc = true;
            foreach (var ctr in qstr)
            {
                if (tc)
                {
                    tc = false;
                    zstr += Char.ToUpper(ctr);
                    continue;
                }
                if (ctr == ' ' | ctr == ',' | ctr == '-' | ctr == '_' | ctr == '&' | ctr == ':')
                {
                    tc = true;
                    zstr += ctr;
                    continue;
                }
                zstr += Char.ToLower(ctr);
            }
            return zstr;
        }
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
        // ----------------------------------------------------------------------------------
        static Dictionary<string, string> IngameToPIM = new Dictionary<string, string>
        {
            { "Ammo",  IG_Ammo },
            { Resources.RStone,  Ore.Stone},
            { "Gravel",  Ingot.Stone},
            { "Tools",  IG_Tools},
            { "Kits",  IG_Kits},
            { "Cash",  IG_Phys},
            { "Datapads",  IG_Datas},
            { "H-Bottles",  IG_HBottles},
            { "O-Bottles",  IG_OBottles},
            { "Ice",  Ore.Ice},
            { "Water",  Ingot.WaterFood},
            { "Greywater",  Ingot.GreyWater},
            { "Deuterium",  Ingot.DeuteriumContainer},
            { "Organic", Ore.Organic },
            { "Seeds", IG_Seeds }
        };

        static string Ingame2Tag(string ingame)
        {
            foreach (var x in IngameToPIM) if (x.Value == ingame) return x.Key;
            return ingame;
        }

        string Tag2Ingame(string ststr)
        {
            if (ststr.Contains("Dock")) return "";
            if (IngameToPIM.ContainsKey(ststr)) return IngameToPIM[ststr];
            switch (ststr)
            {
                case "Steelplate": return IG_Com + "SteelPlate";
                case "Metalgrid": return IG_Com + "MetalGrid";
                case "Interiorplate": return IG_Com + "InteriorPlate";
                case "Smalltube": return IG_Com + "SmallTube";
                case "Largetube": return IG_Com + "LargeTube";
                case "Glass": return IG_Com + "BulletproofGlass";
                case "Gravity": return IG_Com + "GravityGenerator";
                case "Radio": return IG_Com + "RadioCommunication";
                case "Solar": return IG_Com + "SolarCell";
                case "Power": return IG_Com + "PowerCell";
                case "Zonechip": return IG_Com + "ZoneChip";
                case "Reactor":
                case "Thrust":
                case "Medical":
                case "Detector":
                case "Explosives":
                case "Construction":
                case "Motor":
                case "Display":
                case "Girder":
                case "Computer":
                case "Canvas": return IG_Com + ststr;
                default: return ststr;
            }
        }

    }
}
