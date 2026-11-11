using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IngameScript.Tests
{
    /// <summary>
    /// Tests for GetEtaString: the remaining refining time on the refining LCD.
    /// </summary>
    [TestClass]
    public class RefiningEtaTests
    {
        const double TwelveSeconds = 12.0 / 3600;

        CultureInfo _culture;

        [TestInitialize]
        public void SetUp()
        {
            // the numbers are formatted with the current culture
            _culture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        }

        [TestCleanup]
        public void TearDown()
        {
            CultureInfo.CurrentCulture = _culture;
        }

        [TestMethod]
        public void AmountUnchanged_NoTime()
        {
            // was "Infinity years"
            Assert.AreEqual("...", Program.GetEtaString(1000, 1000, TwelveSeconds));
        }

        [TestMethod]
        public void NothingThereAndNothingChanged_NoTime()
        {
            // was "NaN days"
            Assert.AreEqual("...", Program.GetEtaString(0, 0, TwelveSeconds));
        }

        [TestMethod]
        public void AmountGrew_NoTime()
        {
            Assert.AreEqual("...", Program.GetEtaString(1000, 1500, TwelveSeconds));
        }

        [DataTestMethod]
        [DataRow(2000f, 1000f, "12 s")]       // 1000 per 12 s, 1000 left
        [DataRow(1100f, 1000f, "2 min.")]     // 100 per 12 s
        [DataRow(1010f, 1000f, "20 min.")]    // 10 per 12 s
        [DataRow(1001f, 1000f, "3.3 h")]      // 1 per 12 s
        [DataRow(1000.1f, 1000f, "1.4 days")]
        [DataRow(100f, 0f, "0 s")]            // just used up
        public void AmountShrank_TimeFromTheConsumption(float oldAmount, float amount, string expected)
        {
            Assert.AreEqual(expected, Program.GetEtaString(oldAmount, amount, TwelveSeconds));
        }
    }
}
