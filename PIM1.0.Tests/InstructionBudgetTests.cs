using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IngameScript.Tests
{
    /// <summary>
    /// Tests for InstructionBudget.Adjust: proportional control on the cycle length
    /// measured in runs (target 24 runs = 4 s at Update10, no change within 21..27 runs).
    /// </summary>
    [TestClass]
    public class InstructionBudgetTests
    {
        [TestMethod]
        public void Adjust_CycleWithinTargetRange_LeavesBudgetUnchanged()
        {
            Assert.AreEqual(1000, Program.InstructionBudget.Adjust(current: 1000, cycleRuns: 24));
        }

        [TestMethod]
        public void Adjust_AtRangeBounds_LeavesBudgetUnchanged()
        {
            Assert.AreEqual(1000, Program.InstructionBudget.Adjust(current: 1000, cycleRuns: 21));
            Assert.AreEqual(1000, Program.InstructionBudget.Adjust(current: 1000, cycleRuns: 27));
        }

        [TestMethod]
        public void Adjust_CycleTooLong_IncreasesProportionallyDamped()
        {
            // 36 runs: ratio 1.5, ideal 1500, 60% of the way -> 1300
            Assert.AreEqual(1300, Program.InstructionBudget.Adjust(current: 1000, cycleRuns: 36));
        }

        [TestMethod]
        public void Adjust_CycleTooShort_DecreasesProportionallyDamped()
        {
            // 18 runs: ratio 0.75, ideal 750, 60% of the way -> 850
            Assert.AreEqual(850, Program.InstructionBudget.Adjust(current: 1000, cycleRuns: 18));
        }

        [TestMethod]
        public void Adjust_VeryLongCycle_IsLimitedToRatioTwo()
        {
            // ratio clamped to 2.0 -> factor 1.6
            Assert.AreEqual(1600, Program.InstructionBudget.Adjust(current: 1000, cycleRuns: 240));
        }

        [TestMethod]
        public void Adjust_ZeroRuns_IsLimitedToRatioHalf()
        {
            // cycle finished within one run: ratio clamped to 0.5 -> factor 0.7
            Assert.AreEqual(700, Program.InstructionBudget.Adjust(current: 1000, cycleRuns: 0));
        }

        [TestMethod]
        public void Adjust_NeverGoesBelowMinimum()
        {
            Assert.AreEqual(300, Program.InstructionBudget.Adjust(current: 300, cycleRuns: 1));
        }

        [TestMethod]
        public void Adjust_NeverGoesAboveMaximum()
        {
            Assert.AreEqual(5000, Program.InstructionBudget.Adjust(current: 4000, cycleRuns: 240));
        }

        [TestMethod]
        public void Adjust_ConvergesWithinFewCycles()
        {
            // work needing 900 inst. per run for a 24-run cycle: runs = 24 * 900 / budget
            var budget = 300;
            var cycles = 0;
            int runs;
            while ((runs = 24 * 900 / budget) > 27 || runs < 21)
            {
                budget = Program.InstructionBudget.Adjust(budget, runs);
                cycles++;
                Assert.IsTrue(cycles < 10, "no convergence, budget " + budget);
            }

            Assert.IsTrue(cycles <= 4, "cycles: " + cycles);
        }

        [TestMethod]
        public void CycleSeconds_UsesTenTicksPerRun()
        {
            Assert.AreEqual(4.0, Program.InstructionBudget.CycleSeconds(24), 1e-9);
        }

        // --- target runs as parameter (SMS: TargetCycleTime) ---

        [TestMethod]
        public void Adjust_DefaultTargetIs24Runs()
        {
            foreach (var runs in new[] { 0, 18, 20, 21, 24, 27, 28, 36, 240 })
                Assert.AreEqual(
                    Program.InstructionBudget.Adjust(1000, runs, 24),
                    Program.InstructionBudget.Adjust(1000, runs),
                    "runs " + runs);
        }

        [TestMethod]
        public void Adjust_SmallTarget_ToleratesOneRun()
        {
            // target 6 runs (1 s): 5 .. 7 runs leave the budget unchanged
            Assert.AreEqual(1000, Program.InstructionBudget.Adjust(1000, 5, 6));
            Assert.AreEqual(1000, Program.InstructionBudget.Adjust(1000, 7, 6));
            // 4 runs: ratio 2/3, 60% of the way -> 800
            Assert.AreEqual(800, Program.InstructionBudget.Adjust(1000, 4, 6));
            // 9 runs: ratio 1.5 -> 1300
            Assert.AreEqual(1300, Program.InstructionBudget.Adjust(1000, 9, 6));
        }

        [TestMethod]
        public void Adjust_LargeTarget_ToleratesOneEighth()
        {
            // target 18 runs (3 s): tolerance 2 -> 16 .. 20
            Assert.AreEqual(1000, Program.InstructionBudget.Adjust(1000, 16, 18));
            Assert.AreEqual(1000, Program.InstructionBudget.Adjust(1000, 20, 18));
            Assert.AreNotEqual(1000, Program.InstructionBudget.Adjust(1000, 15, 18));
            Assert.AreNotEqual(1000, Program.InstructionBudget.Adjust(1000, 21, 18));
        }

        [TestMethod]
        public void RunsForSeconds_RoundsAndIsAtLeastOne()
        {
            Assert.AreEqual(6, Program.InstructionBudget.RunsForSeconds(1));
            Assert.AreEqual(18, Program.InstructionBudget.RunsForSeconds(3));
            Assert.AreEqual(3, Program.InstructionBudget.RunsForSeconds(0.5));
            Assert.AreEqual(1, Program.InstructionBudget.RunsForSeconds(0));
        }
    }
}
