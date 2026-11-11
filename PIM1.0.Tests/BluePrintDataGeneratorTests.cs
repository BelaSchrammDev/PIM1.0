using System;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PimBluePrintGenerator;

namespace IngameScript.Tests
{
    [TestClass]
    public class BluePrintDataGeneratorTests
    {
        static string RepoRoot([CallerFilePath] string thisFile = "") =>
            Path.GetFullPath(Path.Combine(Path.GetDirectoryName(thisFile), ".."));

        static string DataDir => Path.Combine(RepoRoot(), "PIM1.0", "BluePrintData");
        static string GeneratedFile => Path.Combine(RepoRoot(), "PIM1.0", "BluePrintsClasses", "Generated", "BluePrintData.g.cs");

        [TestMethod]
        public void GeneratedFile_IsUpToDateWithDataFiles()
        {
            // The script build regenerates the file automatically. This test catches a stale
            // committed file (e.g. data changed, but only the test project was built).
            var expected = BluePrintDataGenerator.Generate(DataDir);
            var actual = File.ReadAllText(GeneratedFile).Replace("\r\n", "\n");
            Assert.AreEqual(expected, actual,
                "BluePrintData.g.cs is out of date: build PIM1.0 or run the PIM1.0.BluePrintGenerator project.");
        }

        [TestMethod]
        public void Generator_ReportsUnknownItemTypeWithFileAndLine()
        {
            var dir = Path.Combine(Path.GetTempPath(), "pim_bp_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                File.WriteAllText(Path.Combine(dir, "Mods.txt"), "Vanilla\nIndustrialOverhaulMod\n");
                File.WriteAllText(Path.Combine(dir, "Vanilla.txt"), "[Assembler]\nComponent | SteelPlate\nComponnent | Foo\n");
                File.WriteAllText(Path.Combine(dir, "IndustrialOverhaulMod.txt"), "[Assembler]\n");

                var e = Assert.ThrowsException<BluePrintDataException>(() => BluePrintDataGenerator.Generate(dir));
                StringAssert.EndsWith(e.Location, "Vanilla.txt(3)");
                StringAssert.Contains(e.Message, "Componnent");
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }
    }
}
