using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        public class LoopManager : Job
        {
            // decided by the networking (lowest address of all PIMs on the construct); true until the first decision,
            // so the startup job runs with the full budget
            public static bool IsMaster = true;

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
                var wasMaster = IsMaster;
                IsMaster = Net.IsMaster;
                if (!IsMaster)
                {
                    // standby runs are no real cycle either: the first cycle as master is not measured
                    _cycleHadStartup = true;
                    SetSlaveBehavior();
                    MyProgram.SetStandbyStatus();
                    return RunJobResult.Continue;
                }

                var runs = _runsInCycle;
                var measured = !_cycleHadStartup;
                _runsInCycle = 0;
                _cycleHadStartup = false;
                // a cycle with startup runs is no real cycle: stays 0 ("measuring...")
                if (measured)
                {
                    Properties.Data.CurrentCycleInSec = InstructionBudget.CycleSeconds(runs);
                    CurrentInstructionAmount = InstructionBudget.Adjust(CurrentInstructionAmount, runs);
                }

                if (!wasMaster) MyProgram.PimStatus = " starting...\n";
                SetMasterBehavior();
                return RunJobResult.Finished;
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
        }
    }
}
