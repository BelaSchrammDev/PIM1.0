namespace IngameScript
{
    partial class Program
    {
        // The cycle of SMS: one pass over all jobs. It is measured in runs (start to start, the waiting included),
        // the instruction budget per run is adjusted at its end, and a cycle faster than the target cycle time
        // (TargetCycleSeconds) waits until the target is reached.
        // Part of Program (no own class): the members of a nested class all need 'public static', that costs chars.

        // instructions per run; the first cycle (script start, after standby or an error) uses the maximum
        static int instructionLimit = InstructionBudget.INSTRUCTION_MIN;
        // start to start of the last measured cycle, 0 = measuring
        double cycleSeconds;
        bool isNewCycle = true;
        // runs since the start of the current cycle, the current run included
        int cycleRuns;
        // the last cycle ran completely: its runs are used for the budget and the waiting
        bool isCycleMeasured;

        static int InstructionCount()
        {
            return rti.CurrentInstructionCount;
        }

        static bool IsBudgetUsedUp()
        {
            return rti.CurrentInstructionCount > instructionLimit;
        }

        // the last cycle was faster than the target cycle time: the next one does not start yet
        bool IsCycleWaiting()
        {
            return isCycleMeasured && cycleRuns <= TargetRuns();
        }

        int TargetRuns()
        {
            return InstructionBudget.RunsForSeconds(TargetCycleSeconds());
        }

        void StartCycleMeasuring()
        {
            // start to start, the waiting included (the current run is the first of the new cycle)
            if (isCycleMeasured) cycleSeconds = InstructionBudget.CycleSeconds(cycleRuns - 1);
            // first cycle: maximum budget, so the displays show up fast; the measured cycles then lower it
            else instructionLimit = InstructionBudget.INSTRUCTION_MAX;
            cycleRuns = 1;
            isNewCycle = false;
        }

        void EndCycleMeasuring()
        {
            // the runs with work, without waiting: a cycle faster than the target lowers the budget,
            // so the work spreads over the target time with less instructions per run
            if (isCycleMeasured) instructionLimit = InstructionBudget.Adjust(instructionLimit, cycleRuns, TargetRuns());
            isCycleMeasured = true;
            isNewCycle = true;
        }

        // standby runs (Update100) or an error: no cycle, the next one is not measured
        void AbortCycle()
        {
            isNewCycle = true;
            isCycleMeasured = false;
            cycleSeconds = 0;
        }
    }
}
