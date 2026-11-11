using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IngameScript.Tests
{
    /// <summary>
    /// Golden-master test for the assembler and refinery blueprint definitions.
    ///
    /// Golden/BluePrints.golden.txt was generated from the blueprint code *before*
    /// the refactoring (original InitAssemblerBluePrints/InitRefineryBlueprints,
    /// run against stubbed SE types). Every refactoring of the blueprint creation
    /// must reproduce it exactly: keys, item names, autocrafting names/types,
    /// mod assignment, order and refinery input/output names.
    /// Intended change since then: the refinery list is sorted stable (equal inputs
    /// keep their definition order).
    ///
    /// The test covers the complete path: BluePrintData/*.txt -> generator ->
    /// BluePrintData.g.cs (kept up to date, see BluePrintDataGeneratorTests) -> DefineBluePrints().
    ///
    /// The keys (BlueprintID, refinery Name) are persisted on player LCDs and sent
    /// via SMS networking, so they must never change silently.
    ///
    /// Intended data changes: run once with environment variable
    /// PIM_UPDATE_GOLDEN=1, then review the diff of the golden file in git.
    /// </summary>
    [TestClass]
    public class BluePrintGoldenTests
    {
        const string GoldenFileName = "BluePrints.golden.txt";

        static string[] AllMods() =>
            Program.BluePrintMods
                .Where(m => m != Program.Strings.M_Vanilla)
                .OrderBy(s => s, StringComparer.Ordinal)
                .ToArray();

        static IEnumerable<KeyValuePair<string, Func<string, bool>>> Scenarios()
        {
            yield return new KeyValuePair<string, Func<string, bool>>("none", m => false);
            yield return new KeyValuePair<string, Func<string, bool>>("all", m => true);
            yield return new KeyValuePair<string, Func<string, bool>>(
                "all-except-IndustrialOverhaulMod-StarGateMod_Ores",
                m => m != Program.Strings.M_IndustrialOverhaulMod && m != "StarGateMod_Ores");
            foreach (var mod in AllMods())
            {
                var only = mod;
                yield return new KeyValuePair<string, Func<string, bool>>("only-" + only, m => m == only);
            }
        }

        static string RunScenario(string name, Func<string, bool> enabled)
        {
            var usedMods = new Dictionary<string, bool>();
            foreach (var m in AllMods()) usedMods[m] = enabled(m);

            var assembler = new Dictionary<string, Program.AssemblerBluePrint>();
            var refinery = new List<Program.RefineryBlueprint>();
            Program.DefineBluePrints(usedMods, assembler, refinery);

            var sb = new StringBuilder();
            sb.Append("## ").Append(name).Append('\n');
            foreach (var b in assembler.Values)
                sb.Append("A|").Append(b.BlueprintID).Append('|').Append(b.DefinitionSubtype).Append('|').Append(b.ItemName)
                  .Append('|').Append(b.AutoCraftingType).Append('|').Append(b.AutoCraftingName).Append('|').Append(b.ModName)
                  .Append('|').Append(b.MaximumItemAmount).Append('|').Append(b.MinimumAmount).Append('\n');
            foreach (var r in refinery)
                sb.Append("R|").Append(r.Name).Append('|').Append(r.InputID).Append('|').Append(r.InputIDName)
                  .Append('|').Append(r.OutputID).Append('|').Append(r.OutputIDName).Append('|').Append(r.IsScrap).Append('\n');
            return sb.ToString();
        }

        static string GenerateSnapshot()
        {
            var sb = new StringBuilder();
            foreach (var s in Scenarios()) sb.Append(RunScenario(s.Key, s.Value));
            return sb.ToString();
        }

        static string[] Lines(string text) =>
            text.Replace("\r\n", "\n").TrimEnd('\n').Split('\n');

        static string SourceGoldenPath([CallerFilePath] string thisFile = "") =>
            Path.Combine(Path.GetDirectoryName(thisFile), "Golden", GoldenFileName);

        [TestMethod]
        public void BluePrintDefinitions_MatchGoldenSnapshot()
        {
            var actual = GenerateSnapshot();

            if (Environment.GetEnvironmentVariable("PIM_UPDATE_GOLDEN") == "1")
            {
                File.WriteAllText(SourceGoldenPath(), actual);
                Assert.Inconclusive("Golden file updated: " + SourceGoldenPath());
            }

            var expected = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Golden", GoldenFileName));
            var expectedLines = Lines(expected);
            var actualLines = Lines(actual);

            var scenario = "";
            for (int i = 0; i < Math.Min(expectedLines.Length, actualLines.Length); i++)
            {
                if (expectedLines[i].StartsWith("## ")) scenario = expectedLines[i];
                if (expectedLines[i] != actualLines[i])
                {
                    File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "BluePrints.actual.txt"), actual);
                    Assert.Fail($"Line {i + 1} differs (scenario '{scenario}'):\n  expected: {expectedLines[i]}\n  actual:   {actualLines[i]}");
                }
            }
            Assert.AreEqual(expectedLines.Length, actualLines.Length, "Number of lines differs");
        }

        [TestMethod]
        public void AssemblerBluePrints_KeysAreUniqueAndPositionPrefixIsStripped()
        {
            var usedMods = AllMods().ToDictionary(m => m, m => false);
            var assembler = new Dictionary<string, Program.AssemblerBluePrint>();
            Program.DefineBluePrints(usedMods, assembler, new List<Program.RefineryBlueprint>());

            Assert.IsTrue(assembler.ContainsKey("OxygenContainerObject OxygenBottle"));
            var bottle = assembler["OxygenContainerObject OxygenBottle"];
            Assert.AreEqual("Position0010_OxygenBottle", bottle.DefinitionSubtype);
            Assert.IsFalse(assembler.Keys.Any(k => k.Contains("Position0")));
        }

        [TestMethod]
        public void RefineryBlueprints_DuplicateNamesAreAddedOnlyOnce()
        {
            var usedMods = AllMods().ToDictionary(m => m, m => true);
            var refinery = new List<Program.RefineryBlueprint>();
            Program.DefineBluePrints(usedMods, new Dictionary<string, Program.AssemblerBluePrint>(), refinery);

            var duplicates = refinery.GroupBy(r => r.Name).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            Assert.AreEqual(0, duplicates.Count, "duplicates: " + string.Join(", ", duplicates));
        }

        [TestMethod]
        public void DefineBluePrints_CanRunTwice()
        {
            // The old code left curmod == "" after the first run, so a second run silently skipped
            // all vanilla blueprints. Needed for re-initialisation / later mod loading at runtime.
            var usedMods = AllMods().ToDictionary(m => m, m => false);
            var first = new Dictionary<string, Program.AssemblerBluePrint>();
            var second = new Dictionary<string, Program.AssemblerBluePrint>();
            Program.DefineBluePrints(usedMods, first, new List<Program.RefineryBlueprint>());
            Program.DefineBluePrints(usedMods, second, new List<Program.RefineryBlueprint>());

            CollectionAssert.AreEqual(first.Keys.ToList(), second.Keys.ToList());
        }
    }
}
