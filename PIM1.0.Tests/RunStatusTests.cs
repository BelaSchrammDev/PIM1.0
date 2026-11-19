using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IngameScript.Tests
{
    /// <summary>
    /// Tests for RunStatus: the PB info texts of PIM and SMS (header, running, standby).
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

        [TestMethod]
        public void Header_IsVersionAndCopyright()
        {
            Assert.AreEqual("SMS v1.5a (c) BelaOkuma\n", Program.RunStatus.Header("SMS v1.5a"));
        }

        [TestMethod]
        public void Standby_ShowsMasterAndGrid()
        {
            Assert.AreEqual(" Standby\nMaster: PB SMS\nat Grid: Station\n", Program.RunStatus.Standby("PB SMS", "Station"));
        }

        [TestMethod]
        public void Standby_MasterPbNotFound_NoGridLine()
        {
            Assert.AreEqual(" Standby\nMaster: 12345\n", Program.RunStatus.Standby("12345", null));
        }
    }
}
