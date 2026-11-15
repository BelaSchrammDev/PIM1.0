using System;

namespace IngameScript
{
    partial class Program
    {
        // Proportional budget control: the cycle length is ~ inversely proportional to the budget,
        // so the budget is scaled with (measured runs / target runs), damped and limited per cycle.
        // Measured in runs (game time), so a low sim speed does not raise the budget.
        public static class InstructionBudget
        {
            public const int INSTRUCTION_MIN = 300, INSTRUCTION_MAX = 5000;

            // Update10: one run every 10 ticks, 60 ticks per second
            public const int TICKS_PER_RUN = 10;

            // PIM: target 4 s = 24 runs, no change within 3.5 .. 4.5 s = 21 .. 27 runs
            private const int TARGET_RUNS = 24;

            // move 60% of the way to the ideal budget, at most halve / double the ratio per cycle
            private const double GAIN = 0.6, MIN_RATIO = 0.5, MAX_RATIO = 2.0;

            public static double CycleSeconds(int runs)
            {
                return runs * TICKS_PER_RUN / 60.0;
            }

            // at least one run
            public static int RunsForSeconds(double seconds)
            {
                return Math.Max(1, (int)Math.Round(seconds * 60 / TICKS_PER_RUN));
            }

            public static int Adjust(int current, int cycleRuns)
            {
                return Adjust(current, cycleRuns, TARGET_RUNS);
            }

            // no change within target +- 1/8 (at least +- 1 run): 24 -> 21 .. 27, 6 -> 5 .. 7
            public static int Adjust(int current, int cycleRuns, int targetRuns)
            {
                var tolerance = Math.Max(1, targetRuns / 8);
                if (cycleRuns < targetRuns - tolerance || cycleRuns > targetRuns + tolerance)
                {
                    var ratio = Math.Max(MIN_RATIO, Math.Min(MAX_RATIO, (double)cycleRuns / targetRuns));
                    current = (int)Math.Round(current * (1 + (ratio - 1) * GAIN));
                }

                return Math.Max(INSTRUCTION_MIN, Math.Min(INSTRUCTION_MAX, current));
            }
        }
    }
}
