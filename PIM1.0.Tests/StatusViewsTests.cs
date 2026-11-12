using Microsoft.VisualStudio.TestTools.UnitTesting;
using ID = IngameScript.Program.Warning.ID;

namespace IngameScript.Tests
{
    /// <summary>
    /// Tests for the hints of the PB display: a hint is shown as long as it is reported
    /// in every cycle, SweepViews at the end of the cycle removes the others.
    /// </summary>
    [TestClass]
    public class StatusViewsTests
    {
        [TestInitialize]
        public void Init()
        {
            // hints of other tests: a hint reported in the current cycle survives one sweep
            Program.SweepViews();
            Program.SweepViews();
        }

        static bool Shown(ID id, string sub)
        {
            return Program.GetWarning(id, sub) != null;
        }

        [TestMethod]
        public void Reported_ShownUntilTheEndOfTheCycle()
        {
            Program.SetWarning(ID.CARGOMISSING, "Ammo");
            Assert.IsTrue(Shown(ID.CARGOMISSING, "Ammo"));
            Program.SweepViews();
            Assert.IsTrue(Shown(ID.CARGOMISSING, "Ammo"), "the PB display is written after the sweep");
        }

        [TestMethod]
        public void NotReportedAnymore_Removed()
        {
            Program.SetWarning(ID.CARGOMISSING, "Ammo");
            Program.SweepViews();
            Program.SweepViews();
            Assert.IsFalse(Shown(ID.CARGOMISSING, "Ammo"));
        }

        [TestMethod]
        public void ReportedEveryCycle_StaysTheSameHint()
        {
            Program.SetWarning(ID.CARGOMISSING, "Ammo");
            var first = Program.GetWarning(ID.CARGOMISSING, "Ammo");
            for (int i = 0; i < 10; i++)
            {
                Program.SweepViews();
                Program.SetWarning(ID.CARGOMISSING, "Ammo");
            }
            Assert.AreSame(first, Program.GetWarning(ID.CARGOMISSING, "Ammo"), "same hint: its age keeps counting");
        }

        [TestMethod]
        public void ReportedTwiceInACycle_OneHint()
        {
            Program.SetWarning(ID.CARGOMISSING, "Component");
            var first = Program.GetWarning(ID.CARGOMISSING, "Component");
            Program.SetWarning(ID.CARGOMISSING, "Component");
            Assert.AreSame(first, Program.GetWarning(ID.CARGOMISSING, "Component"));
        }

        [TestMethod]
        public void OtherItemsOfTheType_CannotClearTheHint()
        {
            // former bug: an item with a cargo cleared the hint that another item of the type had set
            Program.SetWarning(ID.CARGOMISSING, "Component");   // Motor: no cargo
            // SteelPlate: has a cargo, reports nothing
            Program.SweepViews();
            Assert.IsTrue(Shown(ID.CARGOMISSING, "Component"));
        }

        [TestMethod]
        public void ReportedAgainAfterRemoval_ShownAgain()
        {
            Program.SetWarning(ID.CARGOUSEFULL, "Ore");
            Program.SweepViews();
            Program.SweepViews();
            Assert.IsFalse(Shown(ID.CARGOUSEFULL, "Ore"));
            Program.SetWarning(ID.CARGOUSEFULL, "Ore");
            Assert.IsTrue(Shown(ID.CARGOUSEFULL, "Ore"));
        }

        [TestMethod]
        public void HeavyBecomesFull_OnlyFullShown()
        {
            Program.SetWarning(ID.CARGOUSEHEAVY, "Ore");
            Program.SweepViews();
            Program.SetWarning(ID.CARGOUSEFULL, "Ore");
            Program.SweepViews();
            Assert.IsTrue(Shown(ID.CARGOUSEFULL, "Ore"));
            Assert.IsFalse(Shown(ID.CARGOUSEHEAVY, "Ore"));
        }

        [TestMethod]
        public void SubtypesAreSeparateHints()
        {
            Program.SetWarning(ID.CARGOMISSING, "Ammo");
            Program.SetWarning(ID.CARGOMISSING, "Ore");
            Program.SweepViews();
            Program.SetWarning(ID.CARGOMISSING, "Ore");
            Program.SweepViews();
            Assert.IsFalse(Shown(ID.CARGOMISSING, "Ammo"));
            Assert.IsTrue(Shown(ID.CARGOMISSING, "Ore"));
        }

        [TestMethod]
        public void Text_CargoMissing()
        {
            Program.SetWarning(ID.CARGOMISSING, "Component");
            Assert.AreEqual("!! please define cargo for Component. name container like ...(sms,component)",
                Program.GetWarning(ID.CARGOMISSING, "Component").GetInfoText().ToString());
        }

        [TestMethod]
        public void Text_CargoMissing_WithAlternative()
        {
            Program.SetWarning(ID.CARGOMISSING, "Ammo", "Armory");
            Assert.AreEqual("!! please define cargo for Ammo. name container like ...(sms,ammo) or ...(sms,armory)",
                Program.GetWarning(ID.CARGOMISSING, "Ammo").GetInfoText().ToString());
        }
    }
}
