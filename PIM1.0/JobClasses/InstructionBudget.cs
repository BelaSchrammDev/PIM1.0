namespace IngameScript
{
    partial class Program
    {
        public static class InstructionBudget
        {
            public const int INSTRUCTION_MIN = 300;
            public const int INSTRUCTION_MAX = 5000;

            private const double TARGET_CYCLE_LOWER_BOUND_SEC = 3.5;
            private const double TARGET_CYCLE_UPPER_BOUND_SEC = 4.5;
            private const int STEP = 100;

            public static int Adjust(int current, double cycleSeconds)
            {
                var adjusted = current;

                if (cycleSeconds < TARGET_CYCLE_LOWER_BOUND_SEC)
                {
                    adjusted -= STEP;
                }
                else if (cycleSeconds > TARGET_CYCLE_UPPER_BOUND_SEC)
                {
                    adjusted += STEP;
                }

                if (adjusted < INSTRUCTION_MIN)
                {
                    adjusted = INSTRUCTION_MIN;
                }
                else if (adjusted > INSTRUCTION_MAX)
                {
                    adjusted = INSTRUCTION_MAX;
                }

                return adjusted;
            }
        }
    }
}
