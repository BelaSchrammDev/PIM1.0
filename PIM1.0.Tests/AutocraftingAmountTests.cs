using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IngameScript.Tests
{
    /// <summary>
    /// Tests for the autocrafting amounts: the stock below which an item is crafted
    /// and how the missing amount is split between the assemblers.
    /// </summary>
    [TestClass]
    public class AutocraftingAmountTests
    {
        [DataTestMethod]
        [DataRow(0, 80, 0)]       // no maximum: never crafted
        [DataRow(1, 80, 1)]       // was 0: never crafted
        [DataRow(2, 80, 2)]
        [DataRow(3, 80, 3)]       // 2.4 -> crafted at 2 (below 80%)
        [DataRow(5, 80, 4)]
        [DataRow(10, 80, 8)]
        [DataRow(1000, 80, 800)]
        [DataRow(1000, 100, 1000)]
        [DataRow(1, 1, 1)]
        [DataRow(250, 50, 125)]
        public void MinimumAmount_IsTheThresholdRoundedUp(int maximum, int threshold, int expected)
        {
            Assert.AreEqual(expected, Program.AssemblerBluePrint.MinimumAmountFor(maximum, threshold));
        }

        static Program.AssemblerBluePrint BluePrint(int threshold, int maximum, int current, int assembling = 0)
        {
            string types = "";
            Program.ParseAutocrafting(Program.Strings.X_Autocrafting_threshold + " = " + threshold + "%",
                new Dictionary<string, Program.AssemblerBluePrint>(), ref types, new Dictionary<string, string>());
            var bp = new Program.AssemblerBluePrint("Component", "SteelPlate", "SteelPlate", "Vanilla", "", "");
            bp.SetMaximumAmount(maximum);
            bp.SetCurrentAmount(current);
            bp.AssemblyAmount = assembling;
            return bp;
        }

        [TestCleanup]
        public void TearDown()
        {
            BluePrint(80, 0, 0);   // back to the default threshold
        }

        [TestMethod]
        public void MaximumOfOne_IsCraftedWhenNothingIsInStock()
        {
            Assert.IsTrue(BluePrint(80, maximum: 1, current: 0).NeedsAssembling());
        }

        [TestMethod]
        public void MaximumOfOne_IsNotCraftedWhenInStockOrQueued()
        {
            Assert.IsFalse(BluePrint(80, maximum: 1, current: 1).NeedsAssembling());
            Assert.IsFalse(BluePrint(80, maximum: 1, current: 0, assembling: 1).NeedsAssembling());
        }

        [TestMethod]
        public void NoMaximum_IsNeverCrafted()
        {
            Assert.IsFalse(BluePrint(80, maximum: 0, current: 0).NeedsAssembling());
        }

        [DataTestMethod]
        [DataRow(1000, 799, true)]
        [DataRow(1000, 800, false)]
        [DataRow(3, 2, true)]
        [DataRow(3, 3, false)]
        public void Stock_IsCraftedBelowTheThreshold(int maximum, int current, bool expected)
        {
            Assert.AreEqual(expected, BluePrint(80, maximum, current).NeedsAssembling());
        }

        [DataTestMethod]
        [DataRow(10, 2, new[] { 5, 5 })]
        [DataRow(10, 3, new[] { 4, 3, 3 })]
        [DataRow(1, 4, new[] { 1, 0, 0, 0 })]     // was 0 for every assembler
        [DataRow(3, 4, new[] { 1, 1, 1, 0 })]
        [DataRow(7, 1, new[] { 7 })]
        [DataRow(0, 3, new[] { 0, 0, 0 })]
        public void SplitAmount_DistributesEverything(int amount, int count, int[] expected)
        {
            var shares = new int[count];
            var sum = 0;
            for (int i = 0; i < count; i++) sum += shares[i] = Program.AssemblerBluePrint.SplitAmount(amount, count, i);

            CollectionAssert.AreEqual(expected, shares);
            Assert.AreEqual(amount, sum);
        }
    }
}
