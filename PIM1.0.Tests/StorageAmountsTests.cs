using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IngameScript.Tests
{
    /// <summary>
    /// Tests for SurplusOfStack: the wanted amount of an item against the stacks of a storage inventory.
    /// </summary>
    [TestClass]
    public class StorageAmountsTests
    {
        /// <summary>Runs all stacks of one item like DisposeOfSurplusItems does (last stack first).</summary>
        static float[] Surplus(float wanted, out float missing, params float[] stacks)
        {
            missing = wanted;
            var surplus = new List<float>();
            for (int i = stacks.Length - 1; i >= 0; i--) surplus.Insert(0, Program.SurplusOfStack(stacks[i], ref missing));
            return surplus.ToArray();
        }

        [TestMethod]
        public void StackSmallerThanWanted_IsKept_TheRestIsMissing()
        {
            float missing;
            var surplus = Surplus(5, out missing, 2);

            CollectionAssert.AreEqual(new[] { 0f }, surplus);
            Assert.AreEqual(3f, missing);
        }

        [TestMethod]
        public void StackEqualToWanted_NothingMissingNothingTooMuch()
        {
            float missing;
            var surplus = Surplus(5, out missing, 5);

            CollectionAssert.AreEqual(new[] { 0f }, surplus);
            Assert.AreEqual(0f, missing);
        }

        [TestMethod]
        public void StackBiggerThanWanted_TheDifferenceIsTooMuch()
        {
            float missing;
            var surplus = Surplus(5, out missing, 8);

            CollectionAssert.AreEqual(new[] { 3f }, surplus);
            Assert.AreEqual(0f, missing);
        }

        [TestMethod]
        public void TwoStacksBelowWanted_AddUp()
        {
            // two bottles in the container, five wanted: three are missing (not four, counted per stack)
            float missing;
            var surplus = Surplus(5, out missing, 1, 1);

            CollectionAssert.AreEqual(new[] { 0f, 0f }, surplus);
            Assert.AreEqual(3f, missing);
        }

        [TestMethod]
        public void SeveralStacksAboveWanted_OnlyTheWantedAmountIsKept()
        {
            float missing;
            var surplus = Surplus(3, out missing, 2, 2, 2);

            // last stack kept (2), middle stack: 1 kept and 1 too much, first stack: all too much
            CollectionAssert.AreEqual(new[] { 2f, 1f, 0f }, surplus);
            Assert.AreEqual(0f, missing);
            Assert.AreEqual(3f, 6f - surplus.Sum());
        }

        [TestMethod]
        public void NothingWanted_TheWholeStackIsTooMuch()
        {
            float missing;
            var surplus = Surplus(0, out missing, 4);

            CollectionAssert.AreEqual(new[] { 4f }, surplus);
            Assert.AreEqual(0f, missing);
        }

        [TestMethod]
        public void FractionalAmounts_AreHandled()
        {
            float missing;
            var surplus = Surplus(16.5f, out missing, 10.25f, 10.25f);

            Assert.AreEqual(0f, surplus[1]);
            Assert.AreEqual(4f, surplus[0]);
            Assert.AreEqual(0f, missing);
        }
    }
}
