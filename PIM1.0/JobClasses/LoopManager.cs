using Sandbox.ModAPI.Ingame;
using System;

namespace IngameScript
{
    partial class Program
    {
        public class LoopManager : Job
        {
            public static IMyProgrammableBlock Master = null, MySelf = null;
            public static IMyGridProgramRuntimeInfo rti = null;

            public const int INSTRUCTION_MIN = 300, INSTRUCTION_MAX = 5000;
            public static int CurrentInstructionAmount = 1000;
            public static bool firstRun = true;

            public LoopManager(Program program) : base(program, "OldMainLoop")
            {
            }

            public override RunJobResult RunJob()
            {
                Propertys.CurrentCycleInSec = (DateTime.Now - Propertys.LastStart).TotalSeconds;
                Propertys.LastStart = DateTime.Now;
                Program.GridTerminalSystem.GetBlocksOfType(Lists.ProgrammableBlocks, block => block.IsSameConstructAs(Program.Me));

                if (IfMeIsMaster())
                {
                    SetMasterBehavior();
                    return RunJobResult.Finished;
                }
                else
                {
                    SetSlaveBehavior();
                    return RunJobResult.Continue;
                }
            }

            public static void Init(Program prg)
            {
                rti = prg.Runtime;
                CurrentInstructionAmount = LoopManager.INSTRUCTION_MIN;
                rti.UpdateFrequency = UpdateFrequency.Update10;
                SetMasterBehavior();
            }

            private static void SetMasterBehavior()
            {
                rti.UpdateFrequency = UpdateFrequency.Update10;
                if (Propertys.CurrentCycleInSec < 3.5) CurrentInstructionAmount -= 100;
                else if (Propertys.CurrentCycleInSec > 4.5) CurrentInstructionAmount += 100;
                if (CurrentInstructionAmount < INSTRUCTION_MIN) CurrentInstructionAmount = INSTRUCTION_MIN;
                else if (CurrentInstructionAmount > INSTRUCTION_MAX) CurrentInstructionAmount = INSTRUCTION_MAX;
            }

            private static void SetSlaveBehavior()
            {
                rti.UpdateFrequency = UpdateFrequency.Update100;
                CurrentInstructionAmount = INSTRUCTION_MIN;
            }

            private static bool IfMeIsMaster() {
                Master = null;
                foreach (var p in Lists.ProgrammableBlocks)
                {
                    if (p.Enabled && p.DetailedInfo.StartsWith(SI1))
                    {
                        if (MySelf.EntityId <= p.EntityId)
                        {
                            Master = p;
                        }
                    }
                }
                return Master == MySelf;
            }
        }
    }
}
