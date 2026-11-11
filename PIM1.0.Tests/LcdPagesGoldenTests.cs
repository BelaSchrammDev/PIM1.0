using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IngameScript.Tests
{
    /// <summary>
    /// Golden-master test for the LCD pages (autocrafting, oreprio, refining).
    ///
    /// Golden/LcdPages.golden.txt was generated right after extracting parsing and rendering
    /// from LcdManager into LcdPages.cs, before any change of the rendering itself.
    /// Every later refactoring must reproduce it, intended output changes are reviewed in the git diff.
    ///
    /// The hidden row keys ("|BlueprintID|", "|OrePrio:Name|") are read back from the player LCDs,
    /// the round-trip tests make sure an unchanged LCD is read back without changes.
    ///
    /// Intended changes: run once with PIM_UPDATE_GOLDEN=1, then review the diff of the golden file.
    /// </summary>
    [TestClass]
    public class LcdPagesGoldenTests
    {
        const string GoldenFileName = "LcdPages.golden.txt";

        // ------------------------------------------------------------------------------------ test data

        static void ResetGlobals()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            Program.ingotprio.Clear();
            Program.OrePrioConfig.Clear();
            string types = "";
            Program.ParseAutocrafting(Program.Strings.X_Autocrafting_treshold + " = 80%", new Dictionary<string, Program.AssemblerBluePrint>(), ref types, new Dictionary<string, string>());
        }

        /// <summary>Vanilla + Industrial Overhaul (renamed resources), amounts derived from the index.</summary>
        static void LoadBluePrints(out Dictionary<string, Program.AssemblerBluePrint> assembler, out List<Program.RefineryBlueprint> refinery)
        {
            var usedMods = Program.BluePrintMods.Where(m => m != Program.Strings.M_Vanilla)
                .ToDictionary(m => m, m => m == Program.Strings.M_IndustrialOverhaulMod);
            assembler = new Dictionary<string, Program.AssemblerBluePrint>();
            refinery = new List<Program.RefineryBlueprint>();
            Program.DefineBluePrints(usedMods, assembler, refinery);

            int i = 0;
            foreach (var bp in assembler.Values)
            {
                bp.SetMaximumAmount(i % 4 == 0 ? 0 : i * 150);
                bp.SetCurrentAmount(i * 37 % 5000);
                bp.AssemblyAmount = i % 3 == 0 ? i : 0;
                bp.RefineryAmount = i % 5 == 0 ? 2 * i : 0;
                i++;
            }
        }

        static List<string> AutocraftingTypes(Dictionary<string, Program.AssemblerBluePrint> assembler) =>
            assembler.Values.Select(b => b.AutoCraftingType).Distinct().ToList();

        static Dictionary<string, List<Program.RefineryBlueprint>> AcceptedBlueprints(List<Program.RefineryBlueprint> refinery)
        {
            var accepted = new Dictionary<string, List<Program.RefineryBlueprint>>
            {
                { "LargeRefinery", refinery.ToList() },
                { "Blast Furnace", refinery.Take(4).ToList() },
                { "Tiny", refinery.Take(1).ToList() },   // fewer than 2 recipes: not displayed
            };
            foreach (var type in accepted)
            {
                var prios = new List<Program.IPrio>();
                int i = 0;
                foreach (var bp in type.Value) prios.Add(new Program.IPrio(bp, i++ * 13 % 250));
                Program.ingotprio[type.Key] = prios;
            }
            return accepted;
        }

        /// <summary>Replaces column <paramref name="column"/> of the row with the given key ("|key|" / "|OrePrio:key|").</summary>
        static string EditRow(string text, string key, int column, string value)
        {
            var lines = text.Split('\n');
            var i = Array.FindIndex(lines, l => l.EndsWith("|" + key + "|"));
            Assert.IsTrue(i >= 0, "row not found: " + key);
            var parts = lines[i].Split('|');
            parts[column] = value;
            lines[i] = string.Join("|", parts);
            return string.Join("\n", lines);
        }

        // ------------------------------------------------------------------------------------ renderers as string

        static string RenderAutocrafting(string acTypes, Dictionary<string, string> typeFilters, List<string> allTypes, IEnumerable<Program.AssemblerBluePrint> active, Program.Filter filter)
        {
            // same parts as AutocraftingPage: header, one block per type
            var sb = new StringBuilder();
            Program.RenderAutocraftingHeader(sb, acTypes, allTypes);
            foreach (var type in Program.AutocraftingTypeList(acTypes)) Program.RenderAutocraftingType(sb, type, typeFilters, active, filter);
            return sb.ToString();
        }

        static string RenderOrePrio(string filterString, Dictionary<string, List<Program.RefineryBlueprint>> accepted, Program.Filter filter)
        {
            // same parts as OrePrioPage: header, one block per refinery type
            var sb = new StringBuilder();
            Program.RenderOrePrioHeader(sb, filterString, filter);
            foreach (var key in accepted.Keys.ToArray()) Program.RenderOrePrioType(sb, key, accepted[key], filter);
            return sb.ToString();
        }

        static string RenderRefining(List<Program.RefineryBlueprint> blueprints)
        {
            var sb = new StringBuilder();
            Program.RenderRefining(sb, blueprints);
            return sb.ToString();
        }

        // ------------------------------------------------------------------------------------ snapshot

        static string GenerateSnapshot()
        {
            ResetGlobals();
            var sb = new StringBuilder();
            Dictionary<string, Program.AssemblerBluePrint> assembler;
            List<Program.RefineryBlueprint> refinery;
            LoadBluePrints(out assembler, out refinery);
            var allTypes = AutocraftingTypes(assembler);
            var filter = new Program.Filter();

            // autocrafting: empty panel -> default types
            sb.Append("## autocrafting-empty\n");
            var acTypes = "Component,AmmoMagazine";
            var typeFilters = new Dictionary<string, string>();
            Program.ParseAutocrafting("", assembler, ref acTypes, typeFilters);
            sb.Append(RenderAutocrafting(acTypes, typeFilters, allTypes, assembler.Values, filter)).Append('\n');

            // autocrafting: all types, threshold, type filters
            sb.Append("## autocrafting-all-types-filtered\n");
            acTypes = "Component,AmmoMagazine";
            typeFilters = new Dictionary<string, string>();
            var input = Program.Strings.X_Autocrafting_treshold + " = 75%\n"
                + Program.Strings.AutoCraftingTypeStringName + "=" + string.Join(",", allTypes) + "\n"
                + " Type : Component = Steel, Plate, -Large\n"
                + " Type : PhysicalGunObject = \n";
            Program.ParseAutocrafting(input, assembler, ref acTypes, typeFilters);
            sb.Append(RenderAutocrafting(acTypes, typeFilters, allTypes, assembler.Values, filter)).Append('\n');

            // ore priority: empty panels, then edited values and filter
            var accepted = AcceptedBlueprints(refinery);
            sb.Append("## oreprio-empty\n");
            var oreFilter = Program.ParseOrePrio("", accepted, refinery);
            var orePage = RenderOrePrio(oreFilter, accepted, filter);
            sb.Append(orePage).Append('\n');

            sb.Append("## oreprio-edited\n");
            var edited = EditRow(orePage, "OrePrio:" + refinery[1].Name, 1, " 500 ");
            edited = EditRow(edited, "OrePrio:" + refinery[2].Name, 1, " 0 ");
            edited = edited.Replace(" Filter: *", " Filter: Large");
            oreFilter = Program.ParseOrePrio(edited, accepted, refinery);
            Program.ApplyOrePrioConfig();
            sb.Append(RenderOrePrio(oreFilter, accepted, filter)).Append('\n');

            // refining progress: amounts in all mass ranges, a name that has to be shortened
            sb.Append("## refining\n");
            var amounts = new[] { 0f, 0.5f, 12f, 1500f, 2500000f };
            for (int i = 0; i < refinery.Count; i++)
            {
                var bp = refinery[i];
                bp.RefineryCount = i % 3 == 0 ? 0 : i;
                bp.InputAmount = amounts[i % amounts.Length];
                bp.OutputAmount = amounts[(i + 2) % amounts.Length];
                bp.ETA_String = i % 2 == 0 ? "..." : (i + 1) + " h";
            }
            var longName = new Program.RefineryBlueprint("AVeryLongRefineryBlueprintNameThatDoesNotFitIntoTheColumnToIngot", "Ore Iron", "Ingot Iron", false)
            {
                RefineryCount = 12, InputAmount = 123456f, OutputAmount = 0.25f, ETA_String = "2 days"
            };
            sb.Append(RenderRefining(refinery.Concat(new[] { longName }).ToList())).Append('\n');

            ResetGlobals();
            return sb.ToString();
        }

        static string[] Lines(string text) =>
            text.Replace("\r\n", "\n").TrimEnd('\n').Split('\n');

        static string SourceGoldenPath([CallerFilePath] string thisFile = "") =>
            Path.Combine(Path.GetDirectoryName(thisFile), "Golden", GoldenFileName);

        [TestMethod]
        public void LcdPages_MatchGoldenSnapshot()
        {
            var actual = GenerateSnapshot();

            if (Environment.GetEnvironmentVariable("PIM_UPDATE_GOLDEN") == "1")
            {
                File.WriteAllText(SourceGoldenPath(), actual);
                Assert.Inconclusive("Golden file updated: " + SourceGoldenPath());
            }

            var expectedLines = Lines(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Golden", GoldenFileName)));
            var actualLines = Lines(actual);
            var scenario = "";
            for (int i = 0; i < Math.Min(expectedLines.Length, actualLines.Length); i++)
            {
                if (expectedLines[i].StartsWith("## ")) scenario = expectedLines[i];
                if (expectedLines[i] != actualLines[i])
                {
                    File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "LcdPages.actual.txt"), actual);
                    Assert.Fail($"Line {i + 1} differs (scenario '{scenario}'):\n  expected: {expectedLines[i]}\n  actual:   {actualLines[i]}");
                }
            }
            Assert.AreEqual(expectedLines.Length, actualLines.Length, "Number of lines differs");
        }

        // ------------------------------------------------------------------------------------ round trips

        [TestMethod]
        public void Autocrafting_RenderedPageIsReadBackUnchanged()
        {
            ResetGlobals();
            Dictionary<string, Program.AssemblerBluePrint> assembler;
            List<Program.RefineryBlueprint> refinery;
            LoadBluePrints(out assembler, out refinery);
            var allTypes = AutocraftingTypes(assembler);
            var maxBefore = assembler.Values.Select(b => b.MaximumItemAmount).ToList();

            var acTypes = string.Join(",", allTypes);
            var page = RenderAutocrafting(acTypes, new Dictionary<string, string>(), allTypes, assembler.Values, new Program.Filter());

            // the first page shows the missing type filters as " * ", read back they are "*":
            // from the second page on the content is stable
            for (int pass = 0; pass < 2; pass++)
            {
                var readTypes = "Component";
                var readFilters = new Dictionary<string, string>();
                Assert.IsFalse(Program.ParseAutocrafting(page, assembler, ref readTypes, readFilters), "threshold must not change");
                Assert.AreEqual(acTypes, readTypes);
                CollectionAssert.AreEqual(maxBefore, assembler.Values.Select(b => b.MaximumItemAmount).ToList());
                var next = RenderAutocrafting(readTypes, readFilters, allTypes, assembler.Values, new Program.Filter());
                if (pass == 1) Assert.AreEqual(page, next);
                page = next;
            }
        }

        [TestMethod]
        public void Autocrafting_EditedMaxIsRead()
        {
            ResetGlobals();
            Dictionary<string, Program.AssemblerBluePrint> assembler;
            List<Program.RefineryBlueprint> refinery;
            LoadBluePrints(out assembler, out refinery);
            var allTypes = AutocraftingTypes(assembler);
            var acTypes = string.Join(",", allTypes);
            var page = RenderAutocrafting(acTypes, new Dictionary<string, string>(), allTypes, assembler.Values, new Program.Filter());

            var keys = assembler.Values.Where(b => b.AutoCraftingType == "Component").Select(b => b.BlueprintID).Take(2).ToArray();
            page = EditRow(page, keys[0], 2, " 5 k ");
            page = EditRow(page, keys[1], 2, " 1234 ");
            Program.ParseAutocrafting(page, assembler, ref acTypes, new Dictionary<string, string>());

            Assert.AreEqual(5000, assembler[keys[0]].MaximumItemAmount);
            Assert.AreEqual(1234, assembler[keys[1]].MaximumItemAmount);
        }

        [TestMethod]
        public void OrePrio_RenderedPageIsReadBackUnchanged()
        {
            ResetGlobals();
            Dictionary<string, Program.AssemblerBluePrint> assembler;
            List<Program.RefineryBlueprint> refinery;
            LoadBluePrints(out assembler, out refinery);
            var accepted = AcceptedBlueprints(refinery);
            var page = RenderOrePrio(null, accepted, new Program.Filter());

            Assert.AreEqual("*", Program.ParseOrePrio(page, accepted, refinery));
            Assert.IsTrue(Program.OrePrioConfig["LargeRefinery"].Values.All(v => v == -1));
            Assert.AreEqual(page, RenderOrePrio("*", accepted, new Program.Filter()));
            ResetGlobals();
        }
    }
}
