using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IngameScript.Tests
{
    /// <summary>
    /// Tests for Refinery.TypeDefinitions, which classifies a refinery block's
    /// SubtypeId (e.g. "LargeRefinery", "Hydroponics") into a RefreshType.
    /// </summary>
    [TestClass]
    public class TypeDefinitionsTests
    {
        [TestMethod]
        public void CompareTypeName_ExactNameConfigured_MatchesOnlyExactString()
        {
            var def = new Program.Refinery.TypeDefinitions(
                "LargeRefinery", Program.Refinery.RefreshType.VanillaRefinery, "Large Refinery");

            Assert.IsTrue(def.CompareTypeName("LargeRefinery"));
            Assert.IsFalse(def.CompareTypeName("LargeRefineryIndustrial"));
            Assert.IsFalse(def.CompareTypeName("SomethingElse"));
        }

        [TestMethod]
        public void CompareTypeName_PrefixConfigured_MatchesAnyStringStartingWithIt()
        {
            var def = new Program.Refinery.TypeDefinitions(
                iExactMatch: false, iTypeName: "Hydroponics",
                iTypeID: Program.Refinery.RefreshType.HydroponicsFarm, iAlternativName: "Hydroponics Farm");

            Assert.IsTrue(def.CompareTypeName("Hydroponics"));
            Assert.IsTrue(def.CompareTypeName("HydroponicsMk2"));
            Assert.IsFalse(def.CompareTypeName("SmallHydroponics"));
        }

        [TestMethod]
        public void CompareTypeName_DefaultEntry_NoLongerActsAsCatchAll()
        {
            // The Unknow-short-circuit was deliberately removed from
            // CompareTypeName; the "unknown" fallback is now handled
            // explicitly by the caller (Refinery.UnknownTypeDefinition via
            // `TypeDefs.Find(...) ?? UnknownTypeDefinition`), not by a hidden
            // always-true branch in this method. A bare TypeDefinitions() now
            // behaves like any other entry: TypeIDName is "" and ExactMatch
            // is true, so it only matches an empty compareString.
            var fallback = new Program.Refinery.TypeDefinitions();

            Assert.IsFalse(fallback.CompareTypeName("AnyUnrecognizedSubtypeId"));
            Assert.IsTrue(fallback.CompareTypeName(""));
        }

        [TestMethod]
        public void GetAlternativeOrDefaultName_WithAlternativeName_ReturnsAlternative()
        {
            var def = new Program.Refinery.TypeDefinitions(
                "LargeRefinery", Program.Refinery.RefreshType.VanillaRefinery, "Large Refinery");

            Assert.AreEqual("Large Refinery", def.GetAlternativeOrDefaultName());
        }

        [TestMethod]
        public void GetAlternativeOrDefaultName_WithoutAlternativeName_FallsBackToTypeIDName()
        {
            var def = new Program.Refinery.TypeDefinitions("RockCrusher", Program.Refinery.RefreshType.VanillaRefinery);

            Assert.AreEqual("RockCrusher", def.GetAlternativeOrDefaultName());
        }

        [TestMethod]
        public void GetAlternativeOrDefaultName_DefaultUnknownEntry_ReturnsEmptyStringNotNull()
        {
            // The parameterless constructor never assigns AlternativName, so
            // it stays at its default (null). The original check compared
            // against "" only, so this returned null instead of "" - callers
            // assign this straight into a Dictionary<string,...> key
            // elsewhere, where a null key throws on Add.
            var fallback = new Program.Refinery.TypeDefinitions();

            Assert.AreEqual(string.Empty, fallback.GetAlternativeOrDefaultName());
        }

        [TestMethod]
        public void GetTypeID_DefaultEntry_IsUnknow()
        {
            // the fallback for unknown blocks: BlockManagerFactory gives it the UnknownRefineryManager
            var fallback = new Program.Refinery.TypeDefinitions();

            Assert.AreEqual(Program.Refinery.RefreshType.Unknown, fallback.GetTypeID());
        }

        [TestMethod]
        public void GetTypeID_ReturnsTheTypeOfTheDefinition()
        {
            var def = new Program.Refinery.TypeDefinitions("Reprocessor", Program.Refinery.RefreshType.Reprocessor);

            Assert.AreEqual(Program.Refinery.RefreshType.Reprocessor, def.GetTypeID());
        }
    }
}