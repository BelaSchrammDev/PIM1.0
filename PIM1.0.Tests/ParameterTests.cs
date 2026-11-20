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
            Program.Properties.Data.AutoCraftingSettingsInvalid = false;
        }

        // ------------------------------------------------------------------
        // Untagged blocks
        // ------------------------------------------------------------------

        [TestMethod]
        public void ParseArgs_WithoutTag_IsNotPimControlled()
        {
            var result = _parameter.ParseArgs("Large Cargo Container");

            Assert.IsFalse(result);
            Assert.IsFalse(_parameter.Tagged);
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
            Assert.IsTrue(_parameter.Tagged);
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
            Assert.IsFalse(_parameter.Tagged);
            Assert.AreEqual(0, _parameter.ParameterList.Count);
            Assert.AreEqual("Box", _parameter.Name.ToString());
        }

        [TestMethod]
        public void GetParameter_ReturnsTheValueOrEmpty()
        {
            // SMS style: values after ':' and spaces after the commas
            _parameter.ParseArgs("Panel (sms, infolcd:2, lines:10)");

            Assert.AreEqual("2", _parameter.GetParameter("Infolcd"));
            Assert.AreEqual("10", _parameter.GetParameter("Lines"));
            Assert.AreEqual("", _parameter.GetParameter("Maxlines"));
        }

        [TestMethod]
        public void ParseArgs_SoftHyphen_SeparatesWords()
        {
            // the soft hyphen can come with texts copied from an LCD
            _parameter.ParseArgs("Panel (sms,max­lines:5)");

            Assert.IsTrue(_parameter.IsParameter("Max­Lines"));
        }

        [TestMethod]
        public void ParseArgs_BecomingControlled_InvalidatesAutocraftingSettings()
        {
            Program.ParseProductionTag(_parameter, "Box (sms,food)");

            Assert.IsTrue(Program.Properties.Data.AutoCraftingSettingsInvalid);
        }

        [TestMethod]
        public void ParseArgs_LosingControl_InvalidatesAutocraftingSettings()
        {
            Program.ParseProductionTag(_parameter, "Box (sms,food)");
            Program.Properties.Data.AutoCraftingSettingsInvalid = false;

            Program.ParseProductionTag(_parameter, "Box");

            Assert.IsTrue(Program.Properties.Data.AutoCraftingSettingsInvalid);
        }

        [TestMethod]
        public void ParseArgs_UnchangedName_DoesNotInvalidateAutocraftingSettings()
        {
            Program.ParseProductionTag(_parameter, "Box (sms,food)");
            Program.Properties.Data.AutoCraftingSettingsInvalid = false;

            Program.ParseProductionTag(_parameter, "Box (sms,food)");

            Assert.IsFalse(Program.Properties.Data.AutoCraftingSettingsInvalid,
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

        // ------------------------------------------------------------------
        // HasSmsTag: the same decision for places that only have the name
        // ------------------------------------------------------------------

        [DataTestMethod]
        [DataRow("Assembler (sms)", true)]
        [DataRow("Assembler (SMS)", true)]
        [DataRow("Assembler (Sms,nooff)", true)]
        [DataRow("(sms) Assembler", true)]
        [DataRow("Assembler(sms)", true)]
        [DataRow("Assembler", false)]
        [DataRow("Assembler sms", false)]
        [DataRow("Assembler (sms", false)]
        [DataRow("Assembler (food)", false)]
        [DataRow("", false)]
        public void HasSmsTag_RecognisesTheTagInAnyCase(string name, bool expected)
        {
            Assert.AreEqual(expected, Program.HasSmsTag(name));
        }

        [DataTestMethod]
        [DataRow("Assembler (sms)")]
        [DataRow("Assembler (SMS)")]
        [DataRow("Assembler (sMs,NoOff)")]
        [DataRow("Assembler")]
        [DataRow("Assembler (sms")]
        [DataRow("Assembler (sms (x)")]
        [DataRow("Assembler (x) (sms)")]
        [DataRow("sms) Assembler (sms")]
        [DataRow("Assembler (food) (sms,keep) 2")]
        public void HasSmsTag_DecidesLikeParseArgs(string name)
        {
            Assert.AreEqual(new Program.Parameter().ParseArgs(name), Program.HasSmsTag(name));
        }

        // the menu actions of SMS (ControlActions): PIM control on/off by the tag in the name
        [TestMethod]
        [DataRow("Refinery 1", true, "Refinery 1 (sms)")]
        [DataRow("Refinery 1 ", true, "Refinery 1 (sms)")]
        [DataRow("Refinery 1 (sms)", true, "Refinery 1 (sms)")]
        [DataRow("Refinery 1 (SMS,keep)", true, "Refinery 1 (SMS,keep)")]
        [DataRow("Refinery 1 (sms)", false, "Refinery 1")]
        [DataRow("Refinery 1 (SMS,nocraft)", false, "Refinery 1")]
        [DataRow("Refinery 1 (sms) left", false, "Refinery 1 left")]
        [DataRow("Refinery 1", false, "Refinery 1")]
        public void WithSmsTag_SetsOrRemovesTheTag(string name, bool on, string expected)
        {
            Assert.AreEqual(expected, Program.WithSmsTag(name, on));
            Assert.AreEqual(on, Program.HasSmsTag(Program.WithSmsTag(name, on)));
        }
    }
}
