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
        bool ShowInfoPBLcd = true;
        static TerminalMenu defaultMenu = null;
        static bool ShowDamageBlockOnHUD = false;
        static int KEEPMENU = 120;
        static string tHeadLine = "@maingridname,,<,Welcome to SMS Terminal,>,,@time,/,@date";
        static string tMainPage = "\ninvman(sort,style 3,nomessage)";
        // script-to-script networking: master/standby among the SMS, the PIM data from the PIM master
        NetWorkingIgc netIgc; static NetWorkingNode Net;
        // the last PIM data received ("" = none or PIM is gone), read by the job ReadPimData
        static string pimData = "";
        static IMyCubeGrid Mgrid; static IMyGridProgramRuntimeInfo rti; static IMyGridTerminalSystem gts;
        static IMyProgrammableBlock runPB;
        Job[] jobs;
        int jobIndex = 0;
        bool nomaster;
        public Program()
        {
            // master priority: a PB on a large grid is preferred (it is the SMS of the station or of the mother ship)
            netIgc = new NetWorkingIgc(IGC, GridTerminalSystem, Me, NetWorkingScripts.Sms, SmsVersion, Me.BlockDefinition.SubtypeId.Contains("Large") ? 1 : 0);
            Net = netIgc.Node;
            Net.Register(NetWorkingScripts.PimData, (source, body) => pimData = body);
            Net.Register(NetWorkingScripts.Command, (source, body) => RunLcdCommand(body));
            modItemDefsList.Add(X_M_Vanilla, new Dictionary<MyItemType, int>());
            loadConfig();
            shipConstructBlocks.Add(Me);
            runPB = Me;
            addBlockParseFunction();
            InitJobs();
            rti = Runtime;
            gts = GridTerminalSystem;
            LoadItemDefinitions();
            if (writeNewItemDefs) writeconfig();
            Mgrid = Me.CubeGrid;
            IsStandby();
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
        // "<terminal id>:<action>" of a button panel etc.: runs on the master (the standby SMS have no terminals)
        void RunLcdCommand(string argument)
        {
            var argl = argument.Split(':');
            if (argl.Length > 1)
                foreach (LCD_Terminal smst in LCDts)
                {
                    if (argl[0] == smst.lcd_id)
                    {
                        smst.Action(argl[1]);
                        break;
                    }
                }
        }
        void Main(string argument, UpdateType updateSource)
        {
            netIgc.Poll(Runtime.TimeSinceLastRun);
            try
            {
                if ((updateSource & UpdateType.Trigger) != 0)
                {
                    if (Net.MasterAddress == Net.Me) RunLcdCommand(argument);
                    else Net.Send(Net.MasterAddress, NetWorkingScripts.Command, argument);
                }
                else
                {
                    cycleRuns++;
                    do
                    {
                        if (isNewCycle)
                        {
                            if (IsCycleWaiting()) break;
                            // at the start of every cycle: master/standby and the script mode are determined
                            if (IsStandby())
                            {
                                AbortCycle();
                                break;
                            }
                            StartCycleMeasuring();
                        }
                        if (jobs[jobIndex].Schedule() == Job.ScheduleResult.Done && ++jobIndex >= jobs.Length)
                        {
                            EndCycleMeasuring();
                            jobIndex = 0;
                            // the next cycle starts with the next run: within the same run the jobs would still be cooling down
                            break;
                        }
                    } while (!IsBudgetUsedUp());
                }
            }
            catch (NullReferenceException) { jobIndex = 0; AbortCycle(); }
            writeInfo();
        }
        readonly RunningSign RunSign = new RunningSign();
        void writeInfo()
        {
            var status = state == SmsState.Starting ? " starting...\n"
                : state == SmsState.Standby ? PbInfo.Standby(Net, GridTerminalSystem)
                : state == SmsState.NoPilot ? " Standby, no Pilot in Ship\n"
                : RunStatus.Running(instructionLimit, cycleSeconds) + ModeName();
            PbInfo.Write(Echo, Me.GetSurface(0), ShowInfoPBLcd, new Color(40, 90, 190), RunStatus.Header(SmsVersion) + RunSign.Next() + status);
        }
    }
}

