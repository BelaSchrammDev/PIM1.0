namespace IngameScript
{
    partial class Program
    {
        // A clicked terminal is rendered before the cycle goes on, so it reacts fast (not only when DisplayJob gets to it).
        // Only with complete data: not while the scan is running (some sums are set to 0 at its start and counted up).

        // from StartCycle (first job) to FinishBlockScan (the job before DisplayJob); true until the first scan is done
        static bool scanRunning = true;

        // every run, before the cycle: the clicked terminals, the one that waits longest first, with the full budget
        void RenderUrgentTerminals()
        {
            if (scanRunning) return;
            var limit = instructionLimit;
            instructionLimit = InstructionBudget.INSTRUCTION_MAX;
            while (!IsBudgetUsedUp())
            {
                LCD_Terminal next = null;
                foreach (var t in LCDts) if (t.urgent > 0 && (next == null || t.urgent < next.urgent)) next = t;
                if (next == null) break;
                // done = written to the panel (RenderStep also returns true when the width is adjusted: render again)
                if (next.RenderStep() && next.renderPhase == TextRenderEngine.RenderPhase.ResetPages) next.urgent = 0;
            }
            instructionLimit = limit;
        }
    }
}
