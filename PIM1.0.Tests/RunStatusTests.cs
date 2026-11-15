using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IngameScript.Tests
{
    /// <summary>
    /// Tests for RunStatus.Running: the PB info line of PIM and SMS.
    /// </summary>
    [TestClass]
    public class RunStatusTests
    {
        [TestMethod]
        public void Running_WithCycleTime_ShowsSeconds()
        {
            Assert.AreEqual(" Running / 1200 inst. per run\ncurrent cycle: 4.0 sec.\n", Program.RunStatus.Running(1200, 4.0));
        }

        [TestMethod]
        public void Running_RoundsToOneDecimal()
        {
            StringAssert.Contains(Program.RunStatus.Running(300, 1.1666), "current cycle: 1.2 sec.\n");
        }

        [TestMethod]
        public void Running_NotMeasuredYet_ShowsMeasuring()
        {
            Assert.AreEqual(" Running / 5000 inst. per run\ncurrent cycle: measuring...\n", Program.RunStatus.Running(5000, 0));
        }
    }
}
