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
        public void Sort_ByCurrentPriority_HighestFirst_SameByName()
        {
            var iron = Prio(80);
            var gold = new Program.IPrio(new Program.RefineryBlueprint("GoldOreToIngot", "Ore Gold", "Ingot Gold", false), 50);
            var silver = new Program.IPrio(new Program.RefineryBlueprint("SilverOreToIngot", "Ore Silver", "Ingot Silver", false), 50);
            var list = new System.Collections.Generic.List<Program.IPrio> { silver, gold, iron };

            list.Sort();
            CollectionAssert.AreEqual(new[] { iron, gold, silver }, list);

            // a refinery was filled with iron: the next refinerys take gold first
            iron.LowerAfterFilling(4);
            iron.LowerAfterFilling(4);
            list.Sort();
            CollectionAssert.AreEqual(new[] { gold, silver, iron }, list);
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
