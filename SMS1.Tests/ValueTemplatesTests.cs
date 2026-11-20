using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IngameScript.Tests
{
    /// <summary>
    /// Golden-master test for the field templates of the value, gas, solar, inventory, resource and power commands.
    /// Golden/ValueTemplates.golden.txt was created once from the string builders of SMS 1.5a in TextRenderEngine
    /// (AddValueTemplates, addFieldTemplates_HOXY, addFieldTemplates_NonBlock, AddPowerLines, PowerTableHeadline/Row),
    /// copied as pure functions into this test for that purpose (removed again afterwards).
    /// The templates are shown on the LCDs of the players: they must not change silently.
    /// Intended changes: run once with environment variable SMS_UPDATE_GOLDEN=1, then review the diff in git.
    /// </summary>
    [TestClass]
    public class ValueTemplatesTests
    {
        const string GoldenFileName = "ValueTemplates.golden.txt";
        const string Header = "# Field templates of the value/power commands per style (from SMS 1.5a); update with SMS_UPDATE_GOLDEN=1.\n";
        static readonly string[] TableOptions = { "SMEI", "S", "EI", "F01", "SMEIF01" };

        static string GenerateSnapshot()
        {
            var lines = new List<string>();
            void Add(string name, IEnumerable<string> templates)
            {
                var number = 0;
                foreach (var t in templates) lines.Add($"{name} | {++number} | {t}");
            }
            for (var style = 1; style <= 4; style++)
            {
                foreach (var (field, prefix) in new[] { ("@fS", "@fN,@fG,"), ("@fH", "@fG,"), ("@X+THRUST", "Right(30),[X+],") })
                    Add($"Value {style} {field} {prefix}", Program.StyleLines(Program.ValueTemplate, style, field, prefix));
                foreach (var c in "HOC") Add($"Hoxy {style} {c}", Program.StyleLines(Program.HoxyTemplate, style, "@fV" + c));
                Add($"Solar {style}", Program.StyleLines(Program.SolarTemplate, style));
                Add($"Inventory {style}", Program.StyleLines(Program.InventoryTemplate, style));
                Add($"Resources {style}", Program.StyleLines(Program.ResourcesTemplate, style));
                foreach (var input in new[] { false, true })
                    foreach (var stored in new[] { false, true })
                        foreach (var line in new[] { false, true })
                            Add($"PowerLines {style} in={input} stored={stored} line={line}", Program.PowerLines(style, input, stored, line));
                foreach (var options in TableOptions)
                {
                    foreach (var grid in new[] { false, true })
                        Add($"PowerTableHead {style} {options} grid={grid}", new[] { Program.PowerTableHeadline(style, options, grid) });
                    foreach (var present in new[] { "SMEIF01", "", "SE" })
                        Add($"PowerTableRow {style} {options} present={present}", new[] { Program.PowerTableRow(style, options, c => present.IndexOf(c) >= 0) });
                }
            }
            return Header + string.Join("\n", lines) + "\n";
        }

        static string[] Lines(string text) => text.Replace("\r\n", "\n").TrimEnd('\n').Split('\n');

        static string SourceGoldenPath([CallerFilePath] string thisFile = "") =>
            Path.Combine(Path.GetDirectoryName(thisFile), "Golden", GoldenFileName);

        [TestMethod]
        public void ValueTemplates_MatchGolden()
        {
            var actual = GenerateSnapshot();
            if (Environment.GetEnvironmentVariable("SMS_UPDATE_GOLDEN") == "1")
            {
                File.WriteAllText(SourceGoldenPath(), actual.Replace("\n", "\r\n"));
                Assert.Inconclusive("golden file updated: review the diff in git");
            }
            var golden = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Golden", GoldenFileName));
            CollectionAssert.AreEqual(Lines(golden), Lines(actual));
        }
    }
}
