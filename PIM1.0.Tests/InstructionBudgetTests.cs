using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IngameScript.Tests
{
    /// <summary>
    /// Tests for InstructionBudget.Adjust, extracted from
    /// LoopManager.SetMasterBehavior so the budget arithmetic is testable
    /// without any Space Engineers dependency.
    /// </summary>
    [TestClass]
    public class InstructionBudgetTests
    {
        [TestMethod]
        public void Adjust_CycleWithinTargetRange_LeavesBudgetUnchanged()
        {
            var result = Program.InstructionBudget.Adjust(current: 1000, cycleSeconds: 4.0);

            Assert.AreEqual(1000, result);
        }

        [TestMethod]
        public void Adjust_CycleFasterThanTarget_DecreasesBudget()
        {
            var result = Program.InstructionBudget.Adjust(current: 1000, cycleSeconds: 3.0);

            Assert.AreEqual(900, result);
        }

        [TestMethod]
        public void Adjust_CycleSlowerThanTarget_IncreasesBudget()
        {
            var result = Program.InstructionBudget.Adjust(current: 1000, cycleSeconds: 5.0);

            Assert.AreEqual(1100, result);
        }

        [TestMethod]
        public void Adjust_AtLowerBound_DoesNotChangeBudget()
        {
            // 3.5s is the boundary itself (< 3.5 decreases), so exactly 3.5
            // must count as "within range".
            var result = Program.InstructionBudget.Adjust(current: 1000, cycleSeconds: 3.5);

            Assert.AreEqual(1000, result);
        }

        [TestMethod]
        public void Adjust_AtUpperBound_DoesNotChangeBudget()
        {
            var result = Program.InstructionBudget.Adjust(current: 1000, cycleSeconds: 4.5);

            Assert.AreEqual(1000, result);
        }

        [TestMethod]
        public void Adjust_NeverGoesBelowMinimum()
        {
            var result = Program.InstructionBudget.Adjust(current: 300, cycleSeconds: 1.0);

            Assert.AreEqual(300, result);
        }

        [TestMethod]
        public void Adjust_NeverGoesAboveMaximum()
        {
            var result = Program.InstructionBudget.Adjust(current: 5000, cycleSeconds: 10.0);

            Assert.AreEqual(5000, result);
        }
    }
}