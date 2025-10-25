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
                get { return Master == MySelf; }
            }

            public const int INSTRUCTION_MIN = 300, INSTRUCTION_MAX = 5000;
            public static int CurrentInstructionAmount = 1000;
            public static bool firstRun = true;

            public LoopManager(Program program) : base(program, "OldMainLoop")
            {
            }

            public override RunJobResult RunJob()
            {
                Propertys.Data.CurrentCycleInSec = (DateTime.Now - Propertys.Data.LastStart).TotalSeconds;
                Propertys.Data.LastStart = DateTime.Now;

                if (IfMeIsMaster())
                {
                    SetMasterBehavior(Propertys.Data.CurrentCycleInSec);
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
                ProgramInstance.LoadConfig();
                ProgramInstance.InitAssemblerBluePrints();
                ProgramInstance.InitRefineryBlueprints();
                CurrentInstructionAmount = LoopManager.INSTRUCTION_MIN;
                SetMasterBehavior();
            }

            private static void SetMasterBehavior(double currentCycleInSec = 0.0)
            {
                RunTime.UpdateFrequency = UpdateFrequency.Update10;
                if (currentCycleInSec < 3.5) CurrentInstructionAmount -= 100;
                else if (currentCycleInSec > 4.5) CurrentInstructionAmount += 100;
                if (CurrentInstructionAmount < INSTRUCTION_MIN) CurrentInstructionAmount = INSTRUCTION_MIN;
                else if (CurrentInstructionAmount > INSTRUCTION_MAX) CurrentInstructionAmount = INSTRUCTION_MAX;
            }

            private static void SetSlaveBehavior()
            {
                RunTime.UpdateFrequency = UpdateFrequency.Update100;
                CurrentInstructionAmount = INSTRUCTION_MIN;
            }

            private static bool IfMeIsMaster()
            {
                GridTerminalSystem.GetBlocksOfType(Lists.Data.ProgrammableBlocks, block => BlockConstructMember(block));
                Master = MySelf;
                return true;

                // TODO: detecting other PIM PRGs
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
