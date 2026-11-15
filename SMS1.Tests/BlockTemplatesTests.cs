using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IngameScript.Tests
{
    /// <summary>
    /// Golden-master test for the default field templates of the block lists (BlockTemplates).
    ///
    /// Golden/BlockTemplates.golden.txt was created once from the switch in
    /// TextRenderEngine.addFieldTemplates_Block of SMS 1.4 (all block types, styles 1..4,
    /// missing styles = the default of the switch).
    /// The templates are shown on the LCDs of the players: they must not change silently.
    /// Intended change since then: Cargocontainer style 3 "(20" -> "(20)" (missing bracket, same result in MDIbox).
    ///
    /// Intended changes: run once with environment variable SMS_UPDATE_GOLDEN=1,
    /// then review the diff of the golden file in git.
    /// </summary>
    [TestClass]
    public class BlockTemplatesTests
    {
        const string GoldenFileName = "BlockTemplates.golden.txt";
        const string Header =
            "# Default field templates per block type and style, from the switch in TextRenderEngine.addFieldTemplates_Block (SMS 1.4):\n" +
            "# Type | Style | Line (head = headLineString) | Template; update with SMS_UPDATE_GOLDEN=1.\n";

        static string GenerateSnapshot()
        {
            var lines = new List<string>();
            foreach (Program.BlockType type in Enum.GetValues(typeof(Program.BlockType)))
                for (var style = 1; style <= 4; style++)
                {
                    var templates = Program.BlockTemplates.For(type, style);
                    var number = 0;
                    foreach (var t in templates)
                    {
                        if (t.StartsWith("^")) lines.Add($"{type} | {style} | head | {t.Substring(1)}");
                        else lines.Add($"{type} | {style} | {++number} | {t}");
                    }
                }
            return Header + string.Join("\n", lines) + "\n";
        }

        static string[] Lines(string text) => text.Replace("\r\n", "\n").TrimEnd('\n').Split('\n');

        static string SourceGoldenPath([CallerFilePath] string thisFile = "") =>
            Path.Combine(Path.GetDirectoryName(thisFile), "Golden", GoldenFileName);

        [TestMethod]
        public void BlockTemplates_MatchGolden()
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
        public void EveryBlockType_HasFourStyles_AndNoEmptyTemplate()
        {
            foreach (Program.BlockType type in Enum.GetValues(typeof(Program.BlockType)))
                for (var style = 1; style <= 4; style++)
                    foreach (var t in Program.BlockTemplates.For(type, style))
                        Assert.AreNotEqual("", t, $"{type} style {style}");
        }

        [TestMethod]
        public void EmptyStyle_UsesStyle2()
        {
            // Damage has no own style 4
            CollectionAssert.AreEqual(Program.BlockTemplates.For(Program.BlockType.Damage, 2),
                Program.BlockTemplates.For(Program.BlockType.Damage, 4));
        }
    }
}
