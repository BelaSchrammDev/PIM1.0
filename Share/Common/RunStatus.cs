namespace IngameScript
{
    partial class Program
    {
        /// <summary>
        /// The texts of the PB info, the same for PIM and SMS (written by PbInfo):
        /// Header, then the running sign, then Running or Standby.
        /// Pure (no SE types), tested in RunStatusTests.
        /// </summary>
        public static class RunStatus
        {
            // "SMS v1.5a (c) BelaOkuma\n"
            public static string Header(string version)
            {
                return version + " (c) BelaOkuma\n";
            }

            // " Running / 1200 inst. per run\ncurrent cycle: 4.0 sec.\n", before the first measured cycle "measuring..."
            public static string Running(int instructionsPerRun, double cycleSeconds)
            {
                return " Running / " + instructionsPerRun + " inst. per run\ncurrent cycle: "
                    + (cycleSeconds > 0 ? cycleSeconds.ToString("0.0") + " sec.\n" : "measuring...\n");
            }

            // " Standby\nMaster: <master>\nat Grid: <grid>\n"; grid null: no grid line (master PB not found)
            public static string Standby(string master, string grid)
            {
                return " Standby\nMaster: " + master + "\n" + (grid != null ? "at Grid: " + grid + "\n" : "");
            }
        }
    }
}
