using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IngameScript.Tests
{
    /// <summary>
    /// Tests for CargoUse, which aggregates cargo container capacities to
    /// compute a fill ratio for display purposes.
    /// </summary>
    [TestClass]
    public class CargoUseTests
    {
        [TestMethod]
        public void GetCarcoCapacityUseRatio_HalfFull_ReturnsFifty()
        {
            var cargoUse = new Program.CargoUse("Container");
            cargoUse.AddCapacityValues(50, 100);

            Assert.AreEqual(50, cargoUse.GetCarcoCapacityUseRatio());
        }

        [TestMethod]
        public void GetCarcoCapacityUseRatio_AccumulatesMultipleContainers()
        {
            var cargoUse = new Program.CargoUse("Container");
            cargoUse.AddCapacityValues(25, 100);
            cargoUse.AddCapacityValues(25, 100);

            Assert.AreEqual(25, cargoUse.GetCarcoCapacityUseRatio());
        }

        [TestMethod]
        public void GetCarcoCapacityUseRatio_NoMaximumCapacity_ReturnsZeroInsteadOfDividingByZero()
        {
            // No containers were ever added, so MaximumCapacity is still 0.
            // This must not produce Infinity/NaN or overflow the int cast.
            var cargoUse = new Program.CargoUse("Container");

            Assert.AreEqual(0, cargoUse.GetCarcoCapacityUseRatio());
        }

        [TestMethod]
        public void GetCarcoCapacityUseRatio_ZeroCapacityContainerAddedExplicitly_ReturnsZero()
        {
            var cargoUse = new Program.CargoUse("Container");
            cargoUse.AddCapacityValues(0, 0);

            Assert.AreEqual(0, cargoUse.GetCarcoCapacityUseRatio());
        }
    }
}