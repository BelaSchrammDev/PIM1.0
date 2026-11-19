using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IngameScript.Tests
{
    /// <summary>
    /// Golden-master test for the SMS item definitions.
    ///
    /// Golden/ItemData.golden.txt was created once from the hand-written item definitions of SMS 1.4
    /// (the calls in ItemDefinitions.cs, expanded: Resource -> ore + ingot, Werkzeug -> MK1..MK4).
    /// The script passes exactly these items, in this order, to addItemType: the order is the order of
    /// the item lists and the CustomData, a later mod overrides max and name.
    /// Intended changes since then: HSR_Mod removed (discontinued), IndustrialOverhaulWaterMod removed (no items anymore); IndustrialOverhaulMod renames
    /// Magnesium powder to Gunpowder by data (before: a special rule in smsItemType.SetName).
    ///
    /// Covers the complete path: ItemData/*.txt -> generator -> ItemData.g.cs (kept up to date,
    /// see ItemDataGeneratorTests) -> ItemDataParser.Parse.
    ///
    /// Intended data changes: run once with environment variable SMS_UPDATE_GOLDEN=1,
    /// then review the diff of the golden file in git.
    /// </summary>
    [TestClass]
    public class ItemDataGoldenTests
    {
        const string GoldenFileName = "ItemData.golden.txt";
        const string Header =
            "# Expanded item definitions per mod, in call order: Mod | Type | Subtype | Max | Name | Category | Capacity\n" +
            "# Created once from the hand-written SMS1/Definitions/ItemDefinitions.cs (SMS 1.4); update with SMS_UPDATE_GOLDEN=1.\n";

        static string GenerateSnapshot()
        {
            var lines = new List<string>();
            for (var i = 0; i < Program.ItemDataMods.Length; i++)
            {
                var mod = Program.ItemDataMods[i];
                Program.ItemDataParser.Parse(Program.ItemData[i], (type, subtype, max, name, category, capacity) =>
                    lines.Add($"{mod} | {type} | {subtype} | {max} | {name} | {category} | {capacity}"));
            }
            return Header + string.Join("\n", lines) + "\n";
        }

        static string[] Lines(string text) => text.Replace("\r\n", "\n").TrimEnd('\n').Split('\n');

        static string SourceGoldenPath([CallerFilePath] string thisFile = "") =>
            Path.Combine(Path.GetDirectoryName(thisFile), "Golden", GoldenFileName);

        [TestMethod]
        public void ItemDefinitions_MatchGolden()
        {
            var actual = GenerateSnapshot();
            if (Environment.GetEnvironmentVariable("SMS_UPDATE_GOLDEN") == "1")
            {
                File.WriteAllText(SourceGoldenPath(), actual.Replace("\n", "\r\n"));
                Assert.Inconclusive("golden file updated: review the diff in git");
            }

            var expected = Lines(File.ReadAllText(SourceGoldenPath()));
            var got = Lines(actual);
            for (var i = 0; i < Math.Min(expected.Length, got.Length); i++)
                Assert.AreEqual(expected[i], got[i], $"line {i + 1} of {GoldenFileName}");
            Assert.AreEqual(expected.Length, got.Length, "number of lines");
        }

        [TestMethod]
        public void ItemDataMods_VanillaFirst_NoDuplicates()
        {
            Assert.AreEqual("Vanilla", Program.ItemDataMods[0]);
            Assert.AreEqual(Program.ItemDataMods.Length, Program.ItemDataMods.Distinct().Count());
            Assert.AreEqual(Program.ItemDataMods.Length, Program.ItemData.Length);
        }

        [TestMethod]
        public void ItemDataMods_AreTheConfigKeysOfSms14()
        {
            // the mod names are the keys in the config (CustomData) and read by PIM 1.1: they must never change silently
            CollectionAssert.AreEqual(new[]
            {
                "Vanilla", "DailyNeedsSurvivalMod", "AzimuthThrusterMod", "DeuteriumReactorMod", "MCRN_RailGunMod",
                "MWI_HomingWeaponryMod", "PaintGunMod", "StarGateMod_Gates", "StarGateMod_Ores", "DefenseShieldMod",
                "IndustrialOverhaulMod", "IndustrialOverhaulLockLoadMod", "EatDrinkSleepRepeatMod", "PlantAndCookMod",
                "AryxEpsteinDriveMod", "NorthWindWeaponsMod",
            }, Program.ItemDataMods);
        }
    }
}
