using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IngameScript.Tests
{
    /// <summary>
    /// Tests for IPrio.LowerAfterFilling: after a refinery was filled with an ore, its calculated
    /// priority drops by one share per refinery.
    /// </summary>
    [TestClass]
    public class OrePrioTests
    {
        private static Program.IPrio Prio(int prio)
        {
            return new Program.IPrio(new Program.RefineryBlueprint("IronOreToIngot", "Ore Iron", "Ingot Iron", false), prio);
        }

        [DataTestMethod]
        [DataRow(80, 4, 60)]
        [DataRow(80, 2, 40)]
        [DataRow(80, 1, 0)]
        [DataRow(1, 4, 1)]
        [DataRow(99, 3, 66)]
        public void CalculatedPriority_DropsByOneSharePerRefinery(int prio, int refineryCount, int expected)
        {
            var p = Prio(prio);

            Assert.IsTrue(p.LowerAfterFilling(refineryCount));

            Assert.AreEqual(expected, p.prio);
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        public void NoRefineryCounted_DoesNotDivideByZero(int refineryCount)
        {
            var p = Prio(80);

            Assert.IsTrue(p.LowerAfterFilling(refineryCount));

            Assert.AreEqual(0, p.prio);
        }

        [DataTestMethod]
        [DataRow(100)]
        [DataRow(150)]
        [DataRow(200)]
        [DataRow(9999)]
        public void HighPriorities_AreKept(int prio)
        {
            var p = Prio(prio);

            Assert.IsFalse(p.LowerAfterFilling(4));

            Assert.AreEqual(prio, p.prio);
        }

        [TestMethod]
        public void ConfiguredPriority_IsNotTouched()
        {
            var p = Prio(80);

            p.LowerAfterFilling(4);

            Assert.AreEqual(80, p.initp);
        }
    }
}
