namespace IngameScript
{
    partial class Program
    {
        /// <summary>
        /// PB info after the running sign, the same for PIM and SMS:
        /// " Running / 1200 inst. per run\ncurrent cycle: 4.0 sec.\n", before the first measured cycle "measuring...".
        /// Pure (no SE types), tested in RunStatusTests.
        /// </summary>
        public static class RunStatus
        {
            public static string Running(int instructionsPerRun, double cycleSeconds)
            {
                return " Running / " + instructionsPerRun + " inst. per run\ncurrent cycle: "
                    + (cycleSeconds > 0 ? cycleSeconds.ToString("0.0") + " sec.\n" : "measuring...\n");
            }
        }
    }
}
