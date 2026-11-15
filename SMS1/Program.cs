using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRageMath;


namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        #region mdk preserve
        enum ScriptMode { AUTO, SHIP, SHIP_PLUS, STATION };
        enum MenuMode { Default, Custom, MenuClone, }
        #endregion
        static Dictionary<MyItemType, smsItemType> smsItemTypeDefinitions = new Dictionary<MyItemType, smsItemType>();
        // TargetCycleTime from the config, 0 = automatic (ship 1 s, station 3 s)
        double targetCycleTime = 0;
        ScriptMode s_mode = ScriptMode.AUTO;
        bool ShowInfoPBLcd = true;
        static TerminalMenu defaultMenu = null;
        static bool ShowDamageBlockOnHUD = false;
        static int KEEPMENU = 120;
        static string tHeadLine = "@maingridname,,<,Welcome to SMS Terminal,>,,@time,/,@date";
        static string tMainPage = "\ninvman(sort,style 3,nomessage)";
        ScriptMode cm; const int zW = 20, pW = 31;
        const String SI1 = "SMS v1.4", SI2 = "a (c) BelaOkuma\n", Cfilled = "@-(filled)", HSS = "\u00AD", oMG = "MAINGRID", oLG = "LCDGRID", oGN = "GRIDNAME", IG_Datas = "Datapad", IG_Kits = "ConsumableItem", IG_Cash = "PhysicalObject", IG_Tools = "PhysicalGunObject", IG_HBottles = "GasContainerObject", IG_OBottles = "OxygenContainerObject", IG_Ammo = "AmmoMagazine", IG_ = "MyObjectBuilder_";
        const string X_Config = "### Config ###", X_Config_end = "### Config End ###", X_ShowInfoPBLcd = "ShowInfoOnPBLcd", X_ShowDamageBlockHUD = "ShowDamageBlocksOnHud", X_Line = "  / =================================\n", X_LineNewSite = X_Line + "@newsite(", X_Line2 = "  / ------------\n";
        const string X_KeepMenu = "KeepMenuToReturn", X_TargetCycleTime = "TargetCycleTime", X_tHeadLine = "TerminalHeadLine", X_tMainPage = "TerminalMainPageBody", X_tMainPageEnd = X_tMainPage + "End", X_tItemDefs = "### ItemDefinitions ###", X_tItemdefsEnd = "### ItemDefinitionsEnd ###", X_tNewMod = "### MOD";
        const string X_Scriptmode = "ScriptMode", X_M_Vanilla = "Vanilla", X_M_Daily = "DailyNeedsSurvivalMod", X_M_AziThrust = "AzimuthThrusterMod", X_M_SG_Gates = "StarGateMod_Gates", X_M_SG_Ores = "StarGateMod_Ores";
        const string X_M_NorthWindWeapons = "NorthWindWeaponsMod", X_M_HSR = "HSR_Mod", X_M_AryxEpsteinDrive = "AryxEpsteinDriveMod", X_M_PlantAndCook = "PlantAndCookMod", X_M_EatDrinkSleepRepeat = "EatDrinkSleepRepeatMod", X_M_IndustrialOverhaulWater = "IndustrialOverhaulWaterMod", X_M_IndustrialOverhaul = "IndustrialOverhaulMod", X_M_Paintgun = "PaintGunMod", X_M_DeuteriumReactor = "DeuteriumReactorMod", X_M_Shield = "DefenseShieldMod", X_M_MCRNRailgun = "MCRN_RailGunMod", X_M_MWIHoming = "MWI_HomingWeaponryMod";
        static MyItemType IG_Magnesium = new MyItemType(IG_ + "Ingot", "Magnesium");
        IMyProgrammableBlock master = null; static IMyCubeGrid Mgrid; static IMyGridProgramRuntimeInfo rti; static IMyGridTerminalSystem gts;
        static List<TextSurfaceDisplay> sms_sf = new List<TextSurfaceDisplay>(); static List<TextRenderEngine> sms_disp = new List<TextRenderEngine>(); static List<LCD_Terminal> LCDts = new List<LCD_Terminal>(); static List<LCD_Infolcd> infolcds = new List<LCD_Infolcd>();
        static Dictionary<string, MDI> allMDIs = new Dictionary<string, MDI>();
        static Dictionary<char, float> FCS = new Dictionary<char, float>();
        static Dictionary<float, List<BarRatio>> GPL = new Dictionary<float, List<BarRatio>>();
        static List<SP> SPL = new List<SP>();
        static IMyProgrammableBlock runPB;
        enum BlockType { Damage, Tank, Hydrogenengine, Gasgenerator, Airvent, Assembler, Battery, Reactor, Refinery, Jumpdrive, Cargocontainer, Connector, Weapon, };
        static Dictionary<BlockType, Dictionary<IMyCubeGrid, List<dyBlock>>> _D = new Dictionary<BlockType, Dictionary<IMyCubeGrid, List<dyBlock>>>();
        static Dictionary<IMyCubeGrid, List<dyBlock>> getDyBlockList(BlockType type) { if (!_D.ContainsKey(type)) _D.Add(type, new Dictionary<IMyCubeGrid, List<dyBlock>>()); return _D[type]; }
        dyBlock newDyBlock(IMyTerminalBlock b, BlockType type)
        {
            switch (type)
            {
                case BlockType.Connector: return new dyConnector(b);
                case BlockType.Cargocontainer: return new dyCargoContainer(b);
                case BlockType.Damage: return new dyDamageBlock(b);
                case BlockType.Jumpdrive: return new Jump(b);
                case BlockType.Weapon: return new Weapon(b);
                case BlockType.Assembler: return new Okuma(b);
                case BlockType.Refinery: return new Ofen(b);
                case BlockType.Reactor: return new Reactor(b);
                case BlockType.Hydrogenengine: return new HydrogenEngine(b);
                case BlockType.Battery: return new Akku(b);
                case BlockType.Tank: return new GasTank(b);
                case BlockType.Gasgenerator: return new dyGasgenerator(b);
                case BlockType.Airvent: return new Luefter(b);
            }
            return null;
        }
        static Dictionary<IMyCubeGrid, Dictionary<MyItemType, dyInventory>> gridInventoryItems = new Dictionary<IMyCubeGrid, Dictionary<MyItemType, dyInventory>>();
        static List<dyDisplayableVariable> pvl = new List<dyDisplayableVariable>();
        static List<dyConnector> docks = new List<dyConnector>();
        static Dictionary<MyItemType, float> inventar = new Dictionary<MyItemType, float>();
        static Dictionary<MyItemType, dyInventory> invitemsg = new Dictionary<MyItemType, dyInventory>();
        static Dictionary<string, dyResourceOreIngot> oiil = new Dictionary<string, dyResourceOreIngot>();
        static Dictionary<IMyCubeGrid, Dictionary<MyItemType, float>> dict_inv = new Dictionary<IMyCubeGrid, Dictionary<MyItemType, float>>();
        static MDIs pt = new MDIs(55), sMb = new MDIs(20), sMt = new MDIs(20), sMc = new MDIs(20), grn = new MDIs(20), gra = new MDIs(20), mdi_time = new MDIs(), mdi_date = new MDIs(), mdi_dateGER = new MDIs(), mdi_mgrid = new MDIs();
        static String iss = "", castTypeName = "", ccs = "", BlockSubtypeFilter = "", oldMeCustomData = "";
        static double tsm = 0, brn = 0, mss = 0, rsp = 0;
        Job[] jobs;
        int jobIndex = 0;
        bool isNewCycle = true;
        //static float menuWithGlobal;
        bool nomaster, prgblocklarge;
        static IMyCubeGrid gg;
        static smsItemType GetSMSItemType(MyItemType t)
        {
            if (!smsItemTypeDefinitions.ContainsKey(t)) smsItemTypeDefinitions.Add(t, new smsItemType(t));
            return smsItemTypeDefinitions[t];
        }
        static bool cI() { return IC() > MAXIC; }
        static int IC() { return rti.CurrentInstructionCount; }
        static int MAXIC = 0;
        static bool shipControlled = false;
        void calcFrequency()
        {
            if (master != null || (cm == ScriptMode.SHIP && !shipControlled))
            {
                ccs = "Standby, no Pilot in Ship";
                rti.UpdateFrequency = UpdateFrequency.Update100;
                MAXIC = InstructionBudget.INSTRUCTION_MIN;
            }
            else
            {
                ccs = "";
                // the budget is adjusted at the end of every cycle (EndCycle)
                rti.UpdateFrequency = UpdateFrequency.Update10;
            }
        }
        bool if_true(string s) { return (s == "True" || s == "true"); }
        public Program()
        {
            modItemDefsList.Add(X_M_Vanilla, new Dictionary<MyItemType, int>());
            loadConfig();
            òb.Add(Me);
            runPB = Me;
            addBlockParseFunction();
            InitJobs();
            MAXIC = InstructionBudget.INSTRUCTION_MIN;
            rti = Runtime;
            gts = GridTerminalSystem;
            itemdefinitionen();
            loadModsItemDefs();
            if (writeNewItemDefs) writeconfig();
            Mgrid = Me.CubeGrid;
            prgblocklarge = Me.BlockDefinition.SubtypeId.Contains("Large");
            isStandby();
            gFSD();
            defaultMenu = new TerminalMenu();
            defaultMenu.parseMenuDefinition(defMenu);
            allMDIs.Add("@fgA", gra);
            allMDIs.Add("@fChT", pt);
            allMDIs.Add("@fgN", grn);
            allMDIs.Add("@fsMc", sMc);
            allMDIs.Add("@fsMt", sMt);
            allMDIs.Add("@fsMb", sMb);
            allMDIs.Add("@TIME", mdi_time);
            allMDIs.Add("@DATE", mdi_date);
            allMDIs.Add("@DATEGER", mdi_dateGER);
            allMDIs.Add("@MAINGRIDNAME", mdi_mgrid);
            rti.UpdateFrequency = UpdateFrequency.Update10;
        }
        void Main(string argument, UpdateType updateSource)
        {
            try
            {
                if ((updateSource & UpdateType.Trigger) != 0)
                {
                    var argl = argument.Split(':');
                    if (argl.Length > 1)
                    {
                        if (master == null)
                            foreach (LCD_Terminal smst in LCDts)
                            {
                                if (argl[0] == smst.lcd_id)
                                {
                                    smst.Action(argl[1]);
                                    break;
                                }
                            }
                        else master.TryRun(argument);
                    }
                }
                else
                {
                    cycleRuns++;
                    do
                    {
                        if (isNewCycle)
                        {
                            // the last cycle was faster than the target cycle time: wait
                            if (isCycleMeasured && cycleRuns <= TargetRuns()) break;
                            // at the start of every cycle: master/standby and the script mode (cm) are determined
                            if (isStandby())
                            {
                                // standby runs (Update100) are no cycle: the next cycle is not measured
                                isCycleMeasured = false;
                                cycleSeconds = 0;
                                break;
                            }
                            StartCycleMeasuring();
                        }
                        if (jobs[jobIndex].Schedule() == Job.ScheduleResult.Done && ++jobIndex >= jobs.Length)
                        {
                            EndCycle();
                            // the next cycle starts with the next run: within the same run the jobs would still be cooling down
                            break;
                        }
                    } while (!cI());
                }
            }
            catch (NullReferenceException) { jobIndex = 0; isNewCycle = true; isCycleMeasured = false; cycleSeconds = 0; }
            writeInfo();
        }

        // runs since the start of the current cycle, the current run included
        int cycleRuns;
        // the last cycle ran completely: its runs are used for the budget and the waiting
        bool isCycleMeasured;
        // start to start of the last measured cycle, 0 = measuring
        double cycleSeconds;

        int TargetRuns()
        {
            return InstructionBudget.RunsForSeconds(targetCycleTime > 0 ? targetCycleTime : (cm == ScriptMode.STATION ? 3 : 1));
        }

        void StartCycleMeasuring()
        {
            // start to start, the waiting included (the current run is the first of the new cycle)
            if (isCycleMeasured) cycleSeconds = InstructionBudget.CycleSeconds(cycleRuns - 1);
            // first cycle (script start, after standby or an error): maximum budget, so the displays show up fast;
            // the measured cycles then lower it
            else MAXIC = InstructionBudget.INSTRUCTION_MAX;
            cycleRuns = 1;
            isNewCycle = false;
        }

        void EndCycle()
        {
            // the runs with work, without waiting: a cycle faster than the target lowers the budget,
            // so the work spreads over the target time with less instructions per run
            if (isCycleMeasured) MAXIC = InstructionBudget.Adjust(MAXIC, cycleRuns, TargetRuns());
            isCycleMeasured = true;
            jobIndex = 0;
            isNewCycle = true;
        }
        readonly RunningSign RunSign = new RunningSign();
        void writeInfo()
        {
            var s = SI1 + SI2 + RunSign.Next() +(master != null ? " Standby\nMaster: " + master.CustomName + "\nat Grid: " + master.CubeGrid.CustomName : ccs != "" ? " " + ccs : RunStatus.Running(MAXIC, cycleSeconds) + getMode());
            Echo(s);
            if (ShowInfoPBLcd)
            {
                var tp = Me.GetSurface(0);
                tp.Alignment = TextAlignment.LEFT;
                tp.ContentType = ContentType.TEXT_AND_IMAGE;
                tp.WriteText(s);
            }
        }
        string getMode() { if (cm == ScriptMode.STATION) return "Stationmode"; else if (cm == ScriptMode.SHIP) return "Shipmode"; else return "Shipmode (plus)"; }
        bool isDockports()
        {
            if (docks.Count > 0) return true;
            GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(listTerminalBlock, block => block.CubeGrid == Me.CubeGrid);
            foreach (var d in listTerminalBlock)
            {
                if (d.CustomName.ToUpper().Contains(",DOCK:")) return true;
            }
            return false;
        }
        bool isStandby()
        {
            if (s_mode == ScriptMode.AUTO)
            {
                if (Me.CubeGrid.IsStatic) cm = ScriptMode.STATION;
                else cm = isDockports() ? ScriptMode.SHIP_PLUS : ScriptMode.SHIP;
            }
            else cm = s_mode;
            var comp = new List<IMyProgrammableBlock> { };
            GridTerminalSystem.GetBlocksOfType<IMyProgrammableBlock>(comp, b => b.IsSameConstructAs(Me));
            master = null;
            var largePBinlist = false;
            foreach (var p in comp) if (p.BlockDefinition.SubtypeId.Contains("Large")) largePBinlist = true;
            foreach (var p in comp)
            {
                var pblarge = p.BlockDefinition.SubtypeId.Contains("Large");
                if (!largePBinlist && pblarge) largePBinlist = true;
                if (p.Enabled && p.DetailedInfo.StartsWith(SI1))
                {
                    if (prgblocklarge)
                    {
                        if (pblarge && Me.EntityId < p.EntityId && (master == null ? true : master.EntityId < p.EntityId)) master = p;
                    }
                    else if (largePBinlist)
                    {
                        if (master == null ? true : master.EntityId < p.EntityId) master = p;
                    }
                    else
                    {
                        if (Me.EntityId < p.EntityId && (master == null ? true : master.EntityId < p.EntityId)) master = p;
                    }
                }
            }
            getShipController();
            calcFrequency();
            return ((master != null) || (cm == ScriptMode.SHIP && !shipControlled));
        }
        IMyShipController currentShipController = null;
        void getShipController()
        {
            var cl = new List<IMyShipController>();
            GridTerminalSystem.GetBlocksOfType<IMyShipController>(cl, block => block.CubeGrid == Me.CubeGrid);
            currentShipController = null;
            mss = 0;
            brn = 0;
            shipControlled = false;
            foreach (var c in cl)
            {
                if (c.IsUnderControl)
                {
                    shipControlled = true;
                    if (currentShipController == null) currentShipController = c;
                }
                else if (c.IsMainCockpit)
                {
                    if (currentShipController == null) currentShipController = c;
                }
            }
            if (currentShipController != null)
            {
                mss = currentShipController.GetShipSpeed();
                var m = currentShipController.CalculateShipMass();
                tsm = m.TotalMass;
                sMb.sM(m.BaseMass);
                sMt.sM(m.TotalMass);
                sMc.sM(m.TotalMass - m.BaseMass);
                brn = currentShipController.GetNaturalGravity().Length();
                grn.sG(currentShipController.GetNaturalGravity().Length());
                gra.sG(currentShipController.GetArtificialGravity().Length());
            }
        }
        List<IMyThrust> th = new List<IMyThrust>();
        List<IMyThrust> CollectThrusters()
        {
            gg = Mgrid;
            GridTerminalSystem.GetBlocksOfType<IMyThrust>(th, BlockGridCheck);
            foreach (VC v in dyDisplayableVariable.getPV(null, "WtoT").vcl) v.sF(Fm);
            return th;
        }
        void ProcessThruster(IMyThrust thrustBlock)
        {
            var typAzimuth = "Azimuth";
            var typHydro = "Hydro";
            var maxThrust = thrustBlock.MaxEffectiveThrust;
            var curThrust = thrustBlock.CurrentThrust;
            var blockDef = thrustBlock.BlockDefinition.ToString();
            var typThrust = "Ion";
            var a = blockDef.Contains(typAzimuth) ? typAzimuth : "";
            if (blockDef.Contains("Atmo")) typThrust = "Atmo";
            else if (blockDef.Contains(typHydro)) typThrust = typHydro;
            dyDisplayableVariable p = dyDisplayableVariable.getPV(null, "Thrust Summary", true, "xXyYzZ");
            dyDisplayableVariable pv = dyDisplayableVariable.getPV(null, a + typThrust + "Thrust", true, "xXyYzZ");
            dyDisplayableVariable pvw = dyDisplayableVariable.getPV(null, a + typThrust + "Thrusters");
            char o = ' ';
            var O = thrustBlock.GridThrustDirection;
            if (O.X != 0) o = O.X == 1 ? 'X' : 'x';
            else if (O.Y != 0) o = O.Y == 1 ? 'Y' : 'y';
            else if (O.Z != 0) o = O.Z == 1 ? 'Z' : 'z';
            pv.aV(o, maxThrust, curThrust);
            p.aV(o, maxThrust, curThrust);
            pvw.aV('t', thrustBlock.MaxThrust, maxThrust);
            if (typThrust != typHydro)
            {
                var maxPowerusage = gP(thrustBlock);
                var curPowerusage = maxThrust == 0 ? 0.000002 : (maxPowerusage * (curThrust / maxThrust));
                pvw.aV('I', maxPowerusage, thrustBlock.Enabled ? curPowerusage : 0);
            }
        }
        int gn(IMyCubeGrid g) { gg = g; GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(listTerminalBlock, BlockGridCheck); return listTerminalBlock.Count; }
        bool BlockGridCheck(IMyTerminalBlock t) { if (gg == t.CubeGrid) return true; return false; }
        bool BlockConstructCheck(IMyTerminalBlock t) { if (cm == ScriptMode.SHIP && !t.IsSameConstructAs(Me)) return false; return true; }
        static List<IMyTerminalBlock> òb = new List<IMyTerminalBlock>();
        bool BlockConstructDockingCheck(IMyTerminalBlock t) { if (!(cm == ScriptMode.SHIP)) return true; foreach (var x in òb) if (t.IsSameConstructAs(x)) return true; return false; }
        bool BlockSutypeCheck(IMyTerminalBlock t) { if (BlockConstructCheck(t)) return t.BlockDefinition.TypeIdString.Contains(BlockSubtypeFilter); return false; }
        bool BlockDamageCheck(IMyTerminalBlock t) { if (BlockConstructCheck(t)) return !t.CubeGrid.GetCubeBlock(t.Position).IsFullIntegrity; return false; }
        bool BlockWeaponsCheck(IMyTerminalBlock t) { if (BlockConstructCheck(t)) return (t is IMyUserControllableGun); return false; }

        List<IMyTerminalBlock> listTerminalBlock = new List<IMyTerminalBlock>();
    }
}

