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

            // while this job is active (blueprint loading at script start) the maximum budget is used
            public static Job StartupJob;

            private static bool IsStartupRunning
            {
                get { return StartupJob != null && StartupJob.Active; }
            }

            public static bool IsMaxInstructionsArrived()
            {
                return MyProgram.Runtime.CurrentInstructionCount > (IsStartupRunning ? InstructionBudget.INSTRUCTION_MAX : CurrentInstructionAmount);
            }

            public static int CurrentInstructionAmount = InstructionBudget.INSTRUCTION_MIN;

            // runs since the last cycle start; a cycle containing startup runs (max budget) is not used for the budget
            private static int _runsInCycle;
            private static bool _cycleHadStartup = true;

            // once per Main run, before the jobs run
            public static void CountRun()
            {
                _runsInCycle++;
                if (IsStartupRunning) _cycleHadStartup = true;
            }

            public override RunJobResult RunJob()
            {
                var runs = _runsInCycle;
                var measured = !_cycleHadStartup;
                _runsInCycle = 0;
                _cycleHadStartup = false;
                // a cycle with startup runs is no real cycle: stays 0 ("measuring...")
                if (measured) Properties.Data.CurrentCycleInSec = InstructionBudget.CycleSeconds(runs);

                if (IfMeIsMaster())
                {
                    SetMasterBehavior();
                    if (measured) CurrentInstructionAmount = InstructionBudget.Adjust(CurrentInstructionAmount, runs);
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
                CurrentInstructionAmount = InstructionBudget.INSTRUCTION_MIN;
                SetMasterBehavior();
            }

            private static void SetMasterBehavior()
            {
                MyProgram.Runtime.UpdateFrequency = UpdateFrequency.Update10;
            }

            private static void SetSlaveBehavior()
            {
                MyProgram.Runtime.UpdateFrequency = UpdateFrequency.Update100;
                CurrentInstructionAmount = InstructionBudget.INSTRUCTION_MIN;
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
