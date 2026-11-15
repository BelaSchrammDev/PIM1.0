using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IngameScript.Tests
{
    /// <summary>
    /// Tests for ItemDataParser.Parse: the compact format of ItemData.g.cs.
    /// </summary>
    [TestClass]
    public class ItemDataParserTests
    {
        static List<string> Parse(string data)
        {
            var items = new List<string>();
            Program.ItemDataParser.Parse(data, (type, subtype, max, name, category, capacity) =>
                items.Add($"{type}|{subtype}|{max}|{name}|{category}|{capacity}"));
            return items;
        }

        [TestMethod]
        public void Entry_WithoutMax_UsesSectionDefault()
        {
            CollectionAssert.AreEqual(new[] { "Component|Motor|16000|||0", "Component|Display|500|||0" },
                Parse("#C16000,Motor,Display:500"));
        }

        [TestMethod]
        public void Section_WithCategory()
        {
            CollectionAssert.AreEqual(new[] { "ConsumableItem|ClangCola|5||Food|0" }, Parse("#K5:Food,ClangCola"));
        }

        [TestMethod]
        public void Entry_NameAndAmmoCapacity()
        {
            CollectionAssert.AreEqual(new[]
            {
                "AmmoMagazine|NATO_25x184mm|100|Ammobox 25x184mm||140",
                "AmmoMagazine|AutocannonClip|100|||16",
            }, Parse("#A100,NATO_25x184mm::Ammobox 25x184mm:140,AutocannonClip:::16"));
        }

        [TestMethod]
        public void Resource_GivesOreAndIngot()
        {
            CollectionAssert.AreEqual(new[]
            {
                "Ore|Stone|40000|@||0", "Ingot|Stone|20000|Gravel||0",
                "Ore|Magnesium|50000|||0", "Ingot|Magnesium|50000|@powder||0",
            }, Parse("#R50000,Stone:40000:20000:@:Gravel,Magnesium::::@powder"));
        }

        [TestMethod]
        public void Tiers_GiveFourTools()
        {
            CollectionAssert.AreEqual(new[]
            {
                "PhysicalGunObject|AngleGrinderItem|2|Grinder||0",
                "PhysicalGunObject|AngleGrinder2Item|2|Grinder MK2||0",
                "PhysicalGunObject|AngleGrinder3Item|2|Grinder MK3||0",
                "PhysicalGunObject|AngleGrinder4Item|2|Grinder MK4||0",
            }, Parse("#T2,AngleGrinder*::Grinder"));
        }

        [TestMethod]
        public void Tiers_WithoutName_UseTheSubtype()
        {
            CollectionAssert.AreEqual(new[]
            {
                "PhysicalGunObject|WelderItem|2|Welder||0",
                "PhysicalGunObject|Welder2Item|2|Welder MK2||0",
                "PhysicalGunObject|Welder3Item|2|Welder MK3||0",
                "PhysicalGunObject|Welder4Item|2|Welder MK4||0",
            }, Parse("#T2,Welder*"));
        }

        [TestMethod]
        public void NewSection_ResetsCategory()
        {
            CollectionAssert.AreEqual(new[] { "Ingot|Wheat|100|@|Food|0", "Component|Foam|100|||0" },
                Parse("#I100:Food,Wheat::@,#C100,Foam"));
        }

        [TestMethod]
        public void AllTypeCodes_AreKnown()
        {
            Assert.AreEqual(Program.ItemDataParser.TypeCodes.Length, Program.ItemDataParser.TypeNames.Length);
            for (var i = 0; i < Program.ItemDataParser.TypeCodes.Length; i++)
            {
                var type = Program.ItemDataParser.TypeNames[i];
                var items = Parse("#" + Program.ItemDataParser.TypeCodes[i] + "1,X");
                // Resource gives ore and ingot, every other type the item of that type
                var expected = type == "Resource" ? new[] { "Ore|X|1|||0", "Ingot|X|1|||0" } : new[] { type + "|X|1|||0" };
                CollectionAssert.AreEqual(expected, items, "type code " + Program.ItemDataParser.TypeCodes[i]);
            }
        }
    }
}
