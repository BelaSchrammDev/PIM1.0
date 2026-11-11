using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        public class LoopManager : Job
        {
            private static IMyProgrammableBlock Master = null;

            public static bool IsMaster
            {
                get { return Master == MyProgram.Me; }
            }

            public static bool IsMaxInstructionsArrived()
            {
                return MyProgram.Runtime.CurrentInstructionCount > CurrentInstructionAmount;
            }

            public const int INSTRUCTION_MIN = 300, INSTRUCTION_MAX = 5000;
            public static int CurrentInstructionAmount = 1000;

            public override RunJobResult RunJob()
            {
                Properties.Data.CurrentCycleInSec = (DateTime.Now - Properties.Data.LastStart).TotalSeconds;
                Properties.Data.LastStart = DateTime.Now;

                if (IfMeIsMaster())
                {
                    SetMasterBehavior(Properties.Data.CurrentCycleInSec);
                    return RunJobResult.Finished;
                }
                else
                {
                    SetSlaveBehavior();
                    return RunJobResult.Continue;
                }
            }

            public static void Init()
            {
                Config.Instance.LoadConfig();
                MyProgram.InitBluePrints();
                CurrentInstructionAmount = LoopManager.INSTRUCTION_MIN;
                SetMasterBehavior();
            }

            private static void SetMasterBehavior(double currentCycleInSec = 0.0)
            {
                MyProgram.Runtime.UpdateFrequency = UpdateFrequency.Update10;
                CurrentInstructionAmount = InstructionBudget.Adjust(CurrentInstructionAmount, currentCycleInSec);
            }

            private static void SetSlaveBehavior()
            {
                MyProgram.Runtime.UpdateFrequency = UpdateFrequency.Update100;
                CurrentInstructionAmount = INSTRUCTION_MIN;
            }

            private static bool IfMeIsMaster()
            {
                MyProgram.GridTerminalSystem.GetBlocksOfType(Lists.Data.ProgrammableBlocks, block => block.IsMainConstruct());
                Master = MyProgram.Me;
                return true;

                // TODO: detecting other PIM PRGs; with block.TryRun();
                // ============================================================================
                //Program.Instance.Echo("prg.count = " + Lists.Data.ProgrammableBlocks.Count);
                //Program.Instance.Echo("myself.entityID = " + MySelf.EntityId);

                //Master = null;

                //foreach (var p in Lists.Data.ProgrammableBlocks)
                //{
                //    if (p.Enabled && p.DetailedInfo.StartsWith(SI1))
                //    {
                //        Program.Instance.Echo("found.entity = " + p.EntityId);
                //        if (MySelf.EntityId <= p.EntityId)
                //        {
                //            Master = p;
                //        }
                //    }
                //}
                //return Master == MySelf;
            }
        }
    }
}
