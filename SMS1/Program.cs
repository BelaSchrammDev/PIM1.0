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
        int minIC = 500;
        int maxIC = 5000;
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
        const string X_KeepMenu = "KeepMenuToReturn", X_MinIC = "MinInstruction", X_MaxIC = "MaxInstruction", X_tHeadLine = "TerminalHeadLine", X_tMainPage = "TerminalMainPageBody", X_tMainPageEnd = X_tMainPage + "End", X_tItemDefs = "### ItemDefinitions ###", X_tItemdefsEnd = "### ItemDefinitionsEnd ###", X_tNewMod = "### MOD";
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
        static String iss = "", castTypeName = "", css = "", ccs = "", BlockSubtypeFilter = "", oldMeCustomData = "";
        DateTime lastStart = DateTime.Now;
        static double tsm = 0, brn = 0, mss = 0, rsp = 0, ss = 1.5f;
        int msm = 0, lastst = 0;
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
                MAXIC = minIC;
            }
            else
            {
                ccs = "";
                rti.UpdateFrequency = UpdateFrequency.Update10;
                if (ss < 1) MAXIC -= (int)((1.5 - ss) * 500);
                else if (ss > 3) MAXIC += (int)ss * 500;
                else MAXIC = MAXIC - (int)(200 * (1.5 - ss));

                if (MAXIC < minIC) MAXIC = minIC;
                else if (MAXIC > maxIC) MAXIC = maxIC;
                else MAXIC = (int)(Math.Round(MAXIC / 100d) * 100);
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
            MAXIC = maxIC;
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
                    do
                    {
                        switch (msm)
                        {
                            case 0: if (isStandby()) { writeInfo(); return; } break;
                            case 1:
                                gg = Mgrid;
                                rsp = 0;
                                mdi_time.sT(DateTime.Now.ToString("HH:mm"));
                                mdi_date.sT(DateTime.Now.ToString(" MM-dd-yy"));
                                mdi_dateGER.sT(DateTime.Now.ToString("dd.MM.yyyy"));
                                mdi_mgrid.sT(Mgrid.CustomName);
                                foreach (dyDisplayableVariable pv in pvl) pv.clearAllVCs();
                                break;
                            case 2:
                                var iik = new List<MyItemType>(inventar.Keys);
                                for (int i = 0; i < iik.Count; i++) inventar[iik[i]] = 0;
                                foreach (IMyCubeGrid grid in dict_inv.Keys)
                                {
                                    var ik = new List<MyItemType>(dict_inv[grid].Keys);
                                    for (int i = 0; i < ik.Count; i++)
                                        dict_inv[grid][ik[i]] = 0;
                                }
                                break;
                            case 3: if (refreshBlockList(BlockType.Connector, "ShipConnector")) msm--; break;
                            case 4: if (refreshBlockList(BlockType.Weapon, "", BlockWeaponsCheck)) msm--; break;
                            case 5: if (refreshBlockList(BlockType.Cargocontainer, "Cargo")) msm--; break;
                            case 6: break;
                            case 7: loadCustomData(Me.CustomData); css = ss.ToString("0.0"); ss = 0; break;
                            case 8: if (Step(textPanelsJob)) msm--; break;
                            case 9: if (refreshBlockList(BlockType.Battery, "BatteryBlock")) msm--; break;
                            case 10: if (Step(multiBlockJob)) msm--; break;
                            case 11: if (refreshBlockList(BlockType.Tank, "OxygenTank")) msm--; break;
                            case 12: if (refreshBlockList(BlockType.Hydrogenengine, "HydrogenEngine")) msm--; break;
                            case 13: if (refreshBlockList(BlockType.Reactor, "Reactor")) msm--; break;
                            case 14: if (refreshBlockList(BlockType.Assembler, "Assembler")) msm--; break;
                            case 15: foreach (dyResourceOreIngot o in oiil.Values) o.of = 0; break;
                            case 16: if (refreshBlockList(BlockType.Refinery, "Refinery")) msm--; break;
                            case 17: if (refreshBlockList(BlockType.Airvent, "AirVent")) msm--; break;
                            case 18: if (refreshBlockList(BlockType.Gasgenerator, "OxygenGenerator")) msm--; break;
                            case 19: if (cm == ScriptMode.STATION) msm += 2; break;
                            case 20: if (refreshBlockList(BlockType.Jumpdrive, "JumpDrive")) msm--; break;
                            case 21: if (Step(thrustJob)) msm--; break;
                            case 22: if (refreshBlockList(BlockType.Damage, "", BlockDamageCheck)) msm--; break;
                            case 23: if (Step(inventoryInitJob)) msm--; break;
                            case 24:
                                foreach (IMyCubeGrid grid in dict_inv.Keys)
                                {
                                    foreach (var key in dict_inv[grid].Keys)
                                    {
                                        if (!gridInventoryItems[grid].ContainsKey(key)) gridInventoryItems[grid].Add(key, new dyInventory(key, grid));
                                    }
                                }
                                foreach (var k in inventar.Keys)
                                    if (!invitemsg.ContainsKey(k)) invitemsg.Add(k, new dyInventory(k));
                                foreach (var k in invitemsg.Keys)
                                {
                                    var y = GetSMSItemType(k);
                                    if (y.IsItemResource())
                                    {
                                        var tks = y.GetResourceName();
                                        if (oiil.ContainsKey(tks)) oiil[tks].Refresh();
                                        else oiil.Add(tks, new dyResourceOreIngot(tks));
                                    }
                                }
                                break;
                            case 25: if (Step(inventoryRefreshJob)) msm--; break;
                            case 26:
                                foreach (var pv in pvl)
                                    if (pv.Name == "Damageblocks")
                                    {
                                        VC v = pv.getVC('h');
                                        v.C = v.M;
                                        v.M = gn(pv.pcg);
                                    }
                                dyDisplayableVariable.RefreshVar();
                                PT();
                                lastst = 0;
                                gg = Mgrid;
                                break;
                            default:
                                if (sms_disp.Count > 0 && (msm - 27) < sms_disp.Count)
                                {
                                    int curic = IC();
                                    bool scpu = sms_disp[lastst].c();
                                    sms_disp[lastst].icount += (IC() - curic);
                                    if (scpu) lastst++;
                                    else msm--;
                                }
                                else
                                {
                                    ss = (DateTime.Now - lastStart).TotalSeconds;
                                    lastStart = DateTime.Now;
                                    msm = -1;
                                }
                                break;
                        }
                        msm++;
                    } while (!cI());
                }
            }
            catch (NullReferenceException ex) { msm = 0; }
            writeInfo();
        }
        readonly RunningSign RunSign = new RunningSign();
        void writeInfo()
        {
            var s = SI1 + SI2 + RunSign.Next() +(master == null ? ((ccs == "" ? "Running in " + getMode() : ccs) + "\ncurrent cycle: " + css + " sec.\nInstructions per run: " + MAXIC) : "Standby\nMaster: " + master.CustomName + "\nat Grid: " + master.CubeGrid.CustomName);
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
        // transition to the Job loop: the msm switch schedules the jobs one step at a time
        // (Step true = not finished yet, the switch calls again after the budget check)
        static bool Step(Job job) { return job.Schedule() == Job.ScheduleResult.InProgress; }
        Job thrustJob, multiBlockJob, inventoryInitJob, inventoryRefreshJob, textPanelsJob;
        void InitJobs()
        {
            thrustJob = new ForEachJob<IMyThrust>(CollectThrusters, ProcessThruster);
            multiBlockJob = MultiBlockJob();
            inventoryInitJob = new ForEachJob<IMyCubeGrid>(CollectInventoryGrids, InitGridInventoryItems);
            inventoryRefreshJob = new ForEachJob<dyInventory>(CollectInventoryItems, i => i.Refresh(true));
            textPanelsJob = new SequentialJob(new ForEachJob<IMyTerminalBlock>(CollectTextPanels, ProcessTextPanel), new ActionJob(RemoveObsoleteDisplays));
        }
        // one job per BlockType
        readonly Dictionary<BlockType, Job> refreshJobs = new Dictionary<BlockType, Job>();
        bool refreshBlockList(BlockType type, string subtypeText = "", Func<IMyTerminalBlock, bool> checkFunction = null)
        {
            Job job;
            if (!refreshJobs.TryGetValue(type, out job))
            {
                job = new RefreshDyBlockListJob(this, type, subtypeText, checkFunction);
                refreshJobs.Add(type, job);
            }
            return Step(job);
        }

    }
}

