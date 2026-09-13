using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IngameScript.Tests
{
    /// <summary>
    /// Tests for the container tag parser. ParseArgs decides whether a block is
    /// managed by PIM at all, so the behaviour pinned here is user facing.
    /// </summary>
    [TestClass]
    public class ParameterTests
    {
        private Program.Parameter _parameter;

        [TestInitialize]
        public void SetUp()
        {
            _parameter = new Program.Parameter();

            // Properties.Data is a static singleton with no reset, so tests would
            // otherwise leak state into each other.
            Program.Properties.Data.AutoCraftingSettingsInValid = false;
        }

        // ------------------------------------------------------------------
        // Untagged blocks
        // ------------------------------------------------------------------

        [TestMethod]
        public void ParseArgs_WithoutTag_IsNotPimControlled()
        {
            var result = _parameter.ParseArgs("Large Cargo Container");

            Assert.IsFalse(result);
            Assert.IsFalse(_parameter.PIMcontrolled);
            Assert.AreEqual(0, _parameter.ParameterList.Count);
        }

        [TestMethod]
        public void ParseArgs_WithoutTag_KeepsTheNameTrimmed()
        {
            _parameter.ParseArgs("  Large Cargo Container  ");

            Assert.AreEqual("Large Cargo Container", _parameter.Name.ToString());
        }

        // ------------------------------------------------------------------
        // Tagged blocks
        // ------------------------------------------------------------------

        [TestMethod]
        public void ParseArgs_WithTag_IsPimControlled()
        {
            var result = _parameter.ParseArgs("Box (sms,food)");

            Assert.IsTrue(result);
            Assert.IsTrue(_parameter.PIMcontrolled);
        }

        [TestMethod]
        public void ParseArgs_WithTag_CapitalisesParameterKeys()
        {
            _parameter.ParseArgs("Box (sms,food)");

            Assert.IsTrue(_parameter.IsParameter("Food"), "keys are normalised to leading upper case");
        }

        [TestMethod]
        public void ParseArgs_WithTag_DoesNotStoreTheSmsMarkerItself()
        {
            _parameter.ParseArgs("Box (sms,food)");

            Assert.IsFalse(_parameter.IsParameter("Sms"));
            Assert.AreEqual(1, _parameter.ParameterList.Count);
        }

        [TestMethod]
        public void ParseArgs_KeyValuePair_IsSplitAtTheColon()
        {
            _parameter.ParseArgs("Box (sms,limit:100)");

            Assert.IsTrue(_parameter.IsParameter("Limit"));
            Assert.AreEqual("100", _parameter.ParameterList["Limit"]);
        }

        [TestMethod]
        public void ParseArgs_TagInUpperCase_IsStillRecognised()
        {
            var result = _parameter.ParseArgs("Box (SMS,FOOD)");

            Assert.IsTrue(result);
            Assert.IsTrue(_parameter.IsParameter("Food"));
        }

        [TestMethod]
        public void ParseArgs_RepeatedParameter_DoesNotThrow()
        {
            _parameter.ParseArgs("Box (sms,food,food)");

            Assert.AreEqual(1, _parameter.ParameterList.Count);
        }

        [TestMethod]
        public void ParseArgs_NameBeforeTag_IsExtracted()
        {
            _parameter.ParseArgs("Box (sms,food)");

            Assert.AreEqual("Box", _parameter.Name.ToString());
        }

        // ------------------------------------------------------------------
        // State transitions and caching
        // ------------------------------------------------------------------

        [TestMethod]
        public void ParseArgs_CalledTwiceWithSameName_KeepsResult()
        {
            _parameter.ParseArgs("Box (sms,food)");
            var second = _parameter.ParseArgs("Box (sms,food)");

            Assert.IsTrue(second);
            Assert.IsTrue(_parameter.IsParameter("Food"), "cached path must not drop parameters");
        }

        [TestMethod]
        public void ParseArgs_TagRemoved_ClearsParametersAndControlFlag()
        {
            _parameter.ParseArgs("Box (sms,food)");
            var result = _parameter.ParseArgs("Box");

            Assert.IsFalse(result);
            Assert.IsFalse(_parameter.PIMcontrolled);
            Assert.AreEqual(0, _parameter.ParameterList.Count);
            Assert.AreEqual("Box", _parameter.Name.ToString());
        }

        [TestMethod]
        public void ParseArgs_BecomingControlled_InvalidatesAutocraftingSettings()
        {
            _parameter.ParseArgs("Box (sms,food)", canChangeAutocraftingStatus: true);

            Assert.IsTrue(Program.Properties.Data.AutoCraftingSettingsInValid);
        }

        [TestMethod]
        public void ParseArgs_LosingControl_InvalidatesAutocraftingSettings()
        {
            _parameter.ParseArgs("Box (sms,food)", canChangeAutocraftingStatus: true);
            Program.Properties.Data.AutoCraftingSettingsInValid = false;

            _parameter.ParseArgs("Box", canChangeAutocraftingStatus: true);

            Assert.IsTrue(Program.Properties.Data.AutoCraftingSettingsInValid);
        }

        [TestMethod]
        public void ParseArgs_UnchangedName_DoesNotInvalidateAutocraftingSettings()
        {
            _parameter.ParseArgs("Box (sms,food)", canChangeAutocraftingStatus: true);
            Program.Properties.Data.AutoCraftingSettingsInValid = false;

            _parameter.ParseArgs("Box (sms,food)", canChangeAutocraftingStatus: true);

            Assert.IsFalse(Program.Properties.Data.AutoCraftingSettingsInValid,
                "an unchanged name must not force a settings rebuild");
        }

        // ------------------------------------------------------------------
        // The four tests below currently FAIL. They document real defects.
        // ------------------------------------------------------------------

        [TestMethod]
        public void ParseArgs_TagAtStart_LeavesTheNameEmpty()
        {
            // IndexOf("(sms") is 0, so the length passed to Substring becomes -1,
            // which Substring reads as "copy everything".
            _parameter.ParseArgs("(sms,food)");

            Assert.AreEqual("", _parameter.Name.ToString());
        }

        [TestMethod]
        public void ParseArgs_SingleCharacterBeforeTag_KeepsThatCharacter()
        {
            // Length becomes 0, which Substring also reads as "copy everything".
            _parameter.ParseArgs("X(sms,food)");

            Assert.AreEqual("X", _parameter.Name.ToString());
        }

        [TestMethod]
        public void ParseArgs_NoSpaceBeforeTag_DoesNotTruncateTheName()
        {
            // The "- 1" assumes exactly one space in front of the tag and eats a
            // real character when there is none.
            _parameter.ParseArgs("Box(sms,food)");

            Assert.AreEqual("Box", _parameter.Name.ToString());
        }

        [TestMethod]
        public void ParseArgs_SpaceAfterComma_StillYieldsACleanKey()
        {
            // ToArgStr treats the space as the start of a new word and keeps it,
            // so the key ends up as " food" and no lookup for "Food" will match.
            _parameter.ParseArgs("Box (sms, food)");

            Assert.IsTrue(_parameter.IsParameter("Food"));
        }
    }
}