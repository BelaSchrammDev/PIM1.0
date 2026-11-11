using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PimBluePrintGenerator;
using PimBluePrintTool;

namespace IngameScript.Tests
{
    /// <summary>
    /// PIM1.0.BluePrintTool with small fake game/mod definitions (no Space Engineers needed).
    /// </summary>
    [TestClass]
    public class BluePrintToolTests
    {
        string _root, _gameData, _workshop, _dataDir;

        const string ModId = "123456";

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "pim_tool_" + Guid.NewGuid().ToString("N"));
            _gameData = Path.Combine(_root, "SpaceEngineers", "Content", "Data");
            _workshop = Path.Combine(_root, "workshop");
            _dataDir = Path.Combine(_root, "BluePrintData");
            Directory.CreateDirectory(Path.Combine(_gameData, "Localization"));
            Directory.CreateDirectory(Path.Combine(_workshop, ModId, "Data"));
            Directory.CreateDirectory(_dataDir);

            // --- game: element style ids
            File.WriteAllText(Path.Combine(_gameData, "Blueprints.sbc"), @"<?xml version=""1.0""?>
<Definitions xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <Blueprints>
    <Blueprint>
      <Id><TypeId>BlueprintDefinition</TypeId><SubtypeId>Position0050_FlareGun</SubtypeId></Id>
      <Result Amount=""1"" TypeId=""PhysicalGunObject"" SubtypeId=""FlareGunItem"" />
    </Blueprint>
    <Blueprint>
      <Id><TypeId>BlueprintDefinition</TypeId><SubtypeId>Position0010_SemiAutoPistol</SubtypeId></Id>
      <Result Amount=""1"" TypeId=""PhysicalGunObject"" SubtypeId=""SemiAutoPistolItem"" />
    </Blueprint>
    <Blueprint>
      <Id><TypeId>BlueprintDefinition</TypeId><SubtypeId>SteelPlate</SubtypeId></Id>
      <Result Amount=""1"" TypeId=""MyObjectBuilder_Component"" SubtypeId=""SteelPlate"" />
    </Blueprint>
    <Blueprint>
      <Id><TypeId>BlueprintDefinition</TypeId><SubtypeId>IronOreToIngot</SubtypeId></Id>
      <Prerequisites><Item Amount=""1"" TypeId=""Ore"" SubtypeId=""Iron"" /></Prerequisites>
      <Results><Item Amount=""1"" TypeId=""Ingot"" SubtypeId=""Iron"" /></Results>
    </Blueprint>
  </Blueprints>
  <BlueprintClassEntries>
    <Entry Class=""Tools"" BlueprintSubtypeId=""Position0050_FlareGun"" />
    <Entry Class=""Tools"" BlueprintSubtypeId=""Position0010_SemiAutoPistol"" />
    <Entry Class=""Components"" BlueprintSubtypeId=""SteelPlate"" />
    <Entry Class=""Ingots"" BlueprintSubtypeId=""IronOreToIngot"" />
  </BlueprintClassEntries>
  <CubeBlocks>
    <Definition xsi:type=""MyObjectBuilder_AssemblerDefinition"">
      <Id><TypeId>Assembler</TypeId><SubtypeId>LargeAssembler</SubtypeId></Id>
      <BlueprintClasses><Class>Tools</Class><Class>Components</Class></BlueprintClasses>
    </Definition>
    <Definition xsi:type=""MyObjectBuilder_RefineryDefinition"">
      <Id><TypeId>Refinery</TypeId><SubtypeId>LargeRefinery</SubtypeId></Id>
      <BlueprintClasses><Class>Ingots</Class><Class>ModIngots</Class><Class>Disposal</Class></BlueprintClasses>
    </Definition>
  </CubeBlocks>
</Definitions>");

            // --- mod: attribute style ids, disabled class entry, localization
            File.WriteAllText(Path.Combine(_workshop, ModId, "Data", "Mod_Blueprints.sbc"), @"<?xml version=""1.0""?>
<Definitions>
  <Blueprints>
    <Blueprint>
      <Id Type=""MyObjectBuilder_BlueprintDefinition"" Subtype=""FancyPlateBP"" />
      <Result Amount=""1"" TypeId=""Component"" SubtypeId=""FancyPlate"" />
    </Blueprint>
    <Blueprint>
      <Id Type=""BlueprintDefinition"" Subtype=""GoldOreToIngot"" />
      <Prerequisites><Item Amount=""1"" TypeId=""Ore"" SubtypeId=""Gold"" /></Prerequisites>
      <Result Amount=""1"" TypeId=""Ingot"" SubtypeId=""Gold"" />
    </Blueprint>
    <Blueprint>
      <Id Type=""BlueprintDefinition"" Subtype=""HiddenThing"" />
      <Result Amount=""1"" TypeId=""Component"" SubtypeId=""HiddenThing"" />
    </Blueprint>
    <Blueprint>
      <Id Type=""BlueprintDefinition"" Subtype=""FlareGunDisposal"" />
      <Prerequisites><Item Amount=""1"" TypeId=""PhysicalGunObject"" SubtypeId=""FlareGunItem"" /></Prerequisites>
      <Result Amount=""1"" TypeId=""Ingot"" SubtypeId=""Carbon"" />
    </Blueprint>
  </Blueprints>
  <BlueprintClassEntries>
    <Entry Class=""Components"" BlueprintSubtypeId=""FancyPlateBP"" />
    <Entry Class=""ModIngots"" BlueprintSubtypeId=""GoldOreToIngot"" />
    <Entry Class=""Components"" BlueprintSubtypeId=""HiddenThing"" />
    <Entry Class=""Components"" BlueprintSubtypeId=""HiddenThing"" Enabled=""false"" />
    <Entry Class=""Disposal"" BlueprintSubtypeId=""FlareGunDisposal"" />
  </BlueprintClassEntries>
  <Components>
    <Component>
      <Id><TypeId>Component</TypeId><SubtypeId>FancyPlate</SubtypeId></Id>
      <DisplayName>DisplayName_Item_FancyPlate</DisplayName>
    </Component>
  </Components>
</Definitions>");
            Directory.CreateDirectory(Path.Combine(_workshop, ModId, "Data", "Localization"));
            File.WriteAllText(Path.Combine(_workshop, ModId, "Data", "Localization", "MyTexts.resx"), @"<?xml version=""1.0""?>
<root><data name=""DisplayName_Item_FancyPlate"" xml:space=""preserve""><value>Fancy Plate</value></data></root>");

            // --- PIM data
            File.WriteAllText(Path.Combine(_dataDir, "Mods.txt"),
                "Vanilla\nIndustrialOverhaulMod\nTestMod | " + ModId + "\nNoSourceMod | 999\n");
            File.WriteAllText(Path.Combine(_dataDir, "Vanilla.txt"),
                "# vanilla\n[Assembler]\n# Tools\nPhysicalGunObject | Position0005_FlareGun | FlareGunItem\n" +
                "PhysicalGunObject | Position0010_SemiAutoPistol\nComponent | SteelPlate\nComponent | GoneBlueprint\n" +
                "Ingot | SubFresh | | Algae\n\n[Refinery]\nIronOreToIngot | Ore Iron | Ingot Iron\n");
            File.WriteAllText(Path.Combine(_dataDir, "IndustrialOverhaulMod.txt"), "[Assembler]\n");
            File.WriteAllText(Path.Combine(_dataDir, "TestMod.txt"), "# my comment\n[Assembler]\n");
            File.WriteAllText(Path.Combine(_dataDir, "NoSourceMod.txt"), "[Assembler]\nComponent | Foo\n");
        }

        [TestCleanup]
        public void Cleanup()
        {
            try { Directory.Delete(_root, true); } catch (IOException) { }
        }

        ModChecker Checker(out System.Collections.Generic.List<ModData> mods)
        {
            mods = BluePrintDataFile.Read(_dataDir);
            var paths = new GamePaths { GameData = _gameData };
            paths.WorkshopRoots.Add(_workshop);
            return new ModChecker(paths, mods);
        }

        [TestMethod]
        public void ModList_WorkshopIdsAreRead()
        {
            var mods = BluePrintDataFile.Read(_dataDir);
            CollectionAssert.AreEqual(new[] { ModId }, mods.Single(m => m.Name == "TestMod").WorkshopIds);
            Assert.AreEqual(0, mods.Single(m => m.Name == "Vanilla").WorkshopIds.Count);
        }

        [TestMethod]
        public void SbcSource_ReadsBothIdNotationsClassesAndTexts()
        {
            var source = SbcSource.Read(ModId, Path.Combine(_workshop, ModId));
            Assert.AreEqual(4, source.Blueprints.Count);
            Assert.AreEqual("Component FancyPlate", source.Blueprints[0].Results.Single().ToString());
            Assert.AreEqual("Ore Gold", source.Blueprints[1].Inputs.Single().ToString());
            Assert.IsTrue(source.ClassEntries.Any(e => e.Blueprint == "HiddenThing" && !e.Enabled));
            Assert.AreEqual("Fancy Plate", source.Texts["DisplayName_Item_FancyPlate"]);

            var game = SbcSource.Read("Vanilla", _gameData);
            Assert.AreEqual("Component SteelPlate", game.Blueprints.Single(b => b.Subtype == "SteelPlate").Results.Single().ToString());
            Assert.AreEqual(2, game.Blocks.Count);
        }

        [TestMethod]
        public void Check_Vanilla_FindsRenamedWrongItemAndMissing()
        {
            System.Collections.Generic.List<ModData> mods;
            var findings = Checker(out mods).Check(mods.Single(m => m.Name == "Vanilla"));

            var renamed = findings.Single(f => f.Kind == FindingKind.RenamedBlueprint);
            CollectionAssert.AreEqual(new[] { "PhysicalGunObject", "Position0050_FlareGun", "FlareGunItem", "" }, renamed.Fix);

            var item = findings.Single(f => f.Kind == FindingKind.WrongItem);
            Assert.AreEqual("SemiAutoPistolItem", item.Fix[2]);

            Assert.AreEqual("Component | GoneBlueprint", string.Join(" | ", findings.Single(f => f.Kind == FindingKind.MissingBlueprint).Row.Fields));
            Assert.IsFalse(findings.Any(f => f.Row != null && f.Row.Field(1) == "SubFresh"), "PIM-internal blueprint is not reported");
            Assert.IsFalse(findings.Any(f => f.Row != null && f.Row.Field(1) == "SteelPlate"), "correct rows are not reported");
        }

        [TestMethod]
        public void Check_Mod_ProposesNewBlueprintsWithDisplayNames()
        {
            System.Collections.Generic.List<ModData> mods;
            var findings = Checker(out mods).Check(mods.Single(m => m.Name == "TestMod"));
            var added = findings.Where(f => f.Kind == FindingKind.NewBlueprint).ToList();

            Assert.AreEqual(2, added.Count, string.Join("\n", findings.Select(f => f.Message)));
            CollectionAssert.AreEqual(new[] { "Component", "FancyPlateBP", "FancyPlate", "Fancy Plate" }, added.Single(f => !f.IsRefinery).Fix);
            CollectionAssert.AreEqual(new[] { "GoldOreToIngot", "Ore Gold", "Ingot Gold" }, added.Single(f => f.IsRefinery).Fix);
            // HiddenThing: class entry disabled; FlareGunDisposal: input is not ore/ingot
        }

        [TestMethod]
        public void Check_ModWithoutSource_IsSkipped()
        {
            System.Collections.Generic.List<ModData> mods;
            var findings = Checker(out mods).Check(mods.Single(m => m.Name == "NoSourceMod"));
            Assert.AreEqual(FindingKind.NoSource, findings.Single().Kind);
        }

        [TestMethod]
        public void Update_WritesFixesKeepsCommentsAndStaysReadable()
        {
            System.Collections.Generic.List<ModData> mods;
            var checker = Checker(out mods);
            foreach (var mod in mods.Where(m => m.Name == "Vanilla" || m.Name == "TestMod"))
                DataFileEditor.Apply(mod, checker.Check(mod));

            var vanilla = File.ReadAllText(Path.Combine(_dataDir, "Vanilla.txt"));
            StringAssert.Contains(vanilla, "# Tools\nPhysicalGunObject | Position0050_FlareGun | FlareGunItem\n");
            StringAssert.Contains(vanilla, "PhysicalGunObject | Position0010_SemiAutoPistol | SemiAutoPistolItem\n");
            StringAssert.Contains(vanilla, "Component | GoneBlueprint\n");   // missing: reported, not deleted

            var testMod = File.ReadAllText(Path.Combine(_dataDir, "TestMod.txt"));
            StringAssert.StartsWith(testMod, "# my comment\n[Assembler]\n" + DataFileEditor.NewRowsComment + "\nComponent | FancyPlateBP | FancyPlate | Fancy Plate\n");
            StringAssert.Contains(testMod, "[Refinery]\n" + DataFileEditor.NewRowsComment + "\nGoldOreToIngot | Ore Gold | Ingot Gold\n");

            // the generator accepts the result
            StringAssert.Contains(BluePrintDataGenerator.Generate(_dataDir), "FancyPlateBP");

            // second run: nothing left to fix
            checker = Checker(out mods);
            Assert.IsFalse(mods.Where(m => m.Name == "Vanilla" || m.Name == "TestMod").SelectMany(checker.Check).Any(f => f.IsFixable));
        }

        [TestMethod]
        public void New_CreatesModFileAndModListEntry()
        {
            System.Collections.Generic.List<ModData> mods;
            var checker = Checker(out mods);
            var mod = new ModData { Name = "OtherMod", File = Path.Combine(_dataDir, "OtherMod.txt") };
            mod.WorkshopIds.Add(ModId);
            var rows = checker.Check(mod).Where(f => f.Kind == FindingKind.NewBlueprint).ToList();

            DataFileEditor.CreateModFile(mod.File, mod.Name, mod.WorkshopIds, rows);
            DataFileEditor.AddToModList(_dataDir, mod.Name, mod.WorkshopIds);

            var reread = BluePrintDataFile.Read(_dataDir).Single(m => m.Name == "OtherMod");
            CollectionAssert.AreEqual(new[] { ModId }, reread.WorkshopIds);
            Assert.AreEqual(2, reread.Sections.Sum(s => s.Rows.Count));
        }
    }
}
