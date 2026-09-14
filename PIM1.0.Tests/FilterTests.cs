using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IngameScript.Tests
{
    /// <summary>
    /// Tests for Filter, which decides whether a block name passes a
    /// user-configured white-/blacklist tag filter (e.g. "food,-ore").
    /// </summary>
    [TestClass]
    public class FilterTests
    {
        [TestMethod]
        public void IfFilter_WithoutInit_BlocksEverything()
        {
            var filter = new Program.Filter();

            Assert.IsFalse(filter.IfFilter("Large Cargo Container"));
        }

        [TestMethod]
        public void SetFilterToAll_LetsAnyNameThrough()
        {
            var filter = new Program.Filter();
            filter.SetFilterToAll();

            Assert.IsTrue(filter.IfFilter("Anything At All"));
        }

        [TestMethod]
        public void InitFilter_Whitelist_MatchesOnlyContainedSubstring()
        {
            var filter = new Program.Filter();
            filter.InitFilter("food,ore");

            Assert.IsTrue(filter.IfFilter("food"));
            Assert.IsFalse(filter.IfFilter("ammo"));
        }

        [TestMethod]
        public void InitFilter_BlacklistEntry_BlocksMatchingName()
        {
            var filter = new Program.Filter();
            filter.InitFilter("*,-ore");

            Assert.IsFalse(filter.IfFilter("ore"));
            Assert.IsTrue(filter.IfFilter("food"));
        }

        [TestMethod]
        public void InitFilter_EmptyEntryBetweenCommas_DoesNotWhitelistEverything()
        {
            // "a,,b" splits into "a", "", "b". The empty trimmed entry must be
            // skipped, not added to the whitelist - otherwise
            // testName.Contains("") is always true and the filter lets
            // everything through.
            var filter = new Program.Filter();
            filter.InitFilter("a,,b");

            Assert.IsFalse(filter.IfFilter(""));
        }
    }
}