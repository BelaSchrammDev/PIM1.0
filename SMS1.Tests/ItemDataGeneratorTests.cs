using System;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmsItemDataGenerator;

namespace IngameScript.Tests
{
    [TestClass]
    public class ItemDataGeneratorTests
    {
        static string RepoRoot([CallerFilePath] string thisFile = "") =>
            Path.GetFullPath(Path.Combine(Path.GetDirectoryName(thisFile), ".."));

        static string DataDir => Path.Combine(RepoRoot(), "SMS1", "ItemData");
        static string GeneratedFile => Path.Combine(RepoRoot(), "SMS1", "Definitions", "Generated", "ItemData.g.cs");

        [TestMethod]
        public void GeneratedFile_IsUpToDateWithDataFiles()
        {
            // The script build regenerates the file automatically. This test catches a stale
            // committed file (e.g. data changed, but only the test project was built).
            var expected = ItemDataGenerator.Generate(DataDir);
            var actual = File.ReadAllText(GeneratedFile).Replace("\r\n", "\n");
            Assert.AreEqual(expected, actual,
                "ItemData.g.cs is out of date: build SMS1 or run the SMS1.ItemDataGenerator project.");
        }

        static ItemDataException GenerateError(string vanilla)
        {
            var dir = Path.Combine(Path.GetTempPath(), "sms_items_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                File.WriteAllText(Path.Combine(dir, "Mods.txt"), "Vanilla\n");
                File.WriteAllText(Path.Combine(dir, "Vanilla.txt"), vanilla);
                return Assert.ThrowsException<ItemDataException>(() => ItemDataGenerator.Generate(dir));
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }

        [TestMethod]
        public void Generator_ReportsUnknownItemTypeWithFileAndLine()
        {
            var e = GenerateError("[Component]\nSteelPlate | 100\n[Componnent]\n");
            StringAssert.EndsWith(e.Location, "Vanilla.txt(3)");
            StringAssert.Contains(e.Message, "Componnent");
        }

        [TestMethod]
        public void Generator_RejectsSeparatorCharsInNames()
        {
            var e = GenerateError("[Component]\nSteelPlate | 100 | Steel: Plate\n");
            StringAssert.EndsWith(e.Location, "Vanilla.txt(2)");
        }

        [TestMethod]
        public void Generator_RejectsMissingMax()
        {
            var e = GenerateError("[Component]\nSteelPlate\n");
            StringAssert.EndsWith(e.Location, "Vanilla.txt(2)");
        }

        [TestMethod]
        public void Generator_RejectsResourceWithoutIngotMax()
        {
            var e = GenerateError("[Resource]\nIron | 100\n");
            StringAssert.EndsWith(e.Location, "Vanilla.txt(2)");
        }

        [TestMethod]
        public void Generator_RequiresVanillaFirst()
        {
            var dir = Path.Combine(Path.GetTempPath(), "sms_items_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                File.WriteAllText(Path.Combine(dir, "Mods.txt"), "SomeMod\nVanilla\n");
                var e = Assert.ThrowsException<ItemDataException>(() => ItemDataGenerator.Generate(dir));
                StringAssert.Contains(e.Message, "Vanilla");
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }
    }
}
