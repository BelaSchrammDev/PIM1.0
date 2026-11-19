using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using C = IngameScript.Program.ItemTypesConverter;

namespace IngameScript.Tests
{
    /// <summary>
    /// Tests for ItemTypesConverter: container tags -> InventoryManagerList keys and back,
    /// cargo categories, which cargo takes an item (CargoFor) and the parts of an item id.
    /// </summary>
    [TestClass]
    public class ItemTypesConverterTests
    {
        const string Nato = "AmmoMagazine NATO_5p56x45mm", Plate = "Component SteelPlate", Motor = "Component Motor";

        static string[] Cargo(string item, params string[] keys)
        {
            return C.CargoFor(keys.ToDictionary(k => k, k => k), item).ToArray();
        }

        // ------------------------------------------------------------------ ToIngame / ToTag

        // tag (as written by ToArgStr) -> key, as returned by the former Tag2Ingame
        static readonly Dictionary<string, string> Tags = new Dictionary<string, string>
        {
            { "Ammo", "AmmoMagazine" },
            { "Stone", "Ore Stone" },
            { "Gravel", "Ingot Stone" },
            { "Tools", "PhysicalGunObject" },
            { "Kits", "ConsumableItem" },
            { "Cash", "PhysicalObject" },
            { "Datapads", "Datapad" },
            { "H-Bottles", "GasContainerObject" },
            { "O-Bottles", "OxygenContainerObject" },
            { "Ice", "Ore Ice" },
            { "Water", "Ingot WaterFood" },
            { "Greywater", "Ingot GreyWater" },
            { "Deuterium", "Ingot DTFuelCapsule" },
            { "Organic", "Ore Organic" },
            { "Seeds", "SeedItem" },
            { "Steelplate", "Component SteelPlate" },
            { "Metalgrid", "Component MetalGrid" },
            { "Interiorplate", "Component InteriorPlate" },
            { "Smalltube", "Component SmallTube" },
            { "Largetube", "Component LargeTube" },
            { "Glass", "Component BulletproofGlass" },
            { "Gravity", "Component GravityGenerator" },
            { "Radio", "Component RadioCommunication" },
            { "Solar", "Component SolarCell" },
            { "Power", "Component PowerCell" },
            { "Zonechip", "Component ZoneChip" },
            { "Reactor", "Component Reactor" },
            { "Thrust", "Component Thrust" },
            { "Medical", "Component Medical" },
            { "Detector", "Component Detector" },
            { "Explosives", "Component Explosives" },
            { "Construction", "Component Construction" },
            { "Motor", "Component Motor" },
            { "Display", "Component Display" },
            { "Girder", "Component Girder" },
            { "Computer", "Component Computer" },
            { "Canvas", "Component Canvas" },
        };

        [TestMethod]
        public void ToIngame_KnownTags()
        {
            foreach (var t in Tags) Assert.AreEqual(t.Value, C.ToIngame(t.Key), t.Key);
        }

        [TestMethod]
        public void ToIngame_OtherTagsUnchanged()
        {
            Assert.AreEqual("Ore", C.ToIngame("Ore"));
            Assert.AreEqual("Component", C.ToIngame("Component"));
            Assert.AreEqual("Armory", C.ToIngame("Armory"));
        }

        [TestMethod]
        public void ToIngame_DockingTag_Empty()
        {
            Assert.AreEqual("", C.ToIngame("Dock"));
            Assert.AreEqual("", C.ToIngame("Docking"));
        }

        [TestMethod]
        public void ToTag_IsTheInverseOfToIngame()
        {
            foreach (var t in Tags) Assert.AreEqual(t.Key, C.ToTag(t.Value), t.Value);
        }

        [TestMethod]
        public void ToTag_NoOwnTag_Null()
        {
            Assert.IsNull(C.ToTag("Ore"));
            Assert.IsNull(C.ToTag("Component"));
        }

        // ------------------------------------------------------------------ Category

        [TestMethod]
        public void Category_Items()
        {
            Assert.AreEqual(C.Armory, C.Category(Nato));
            Assert.AreEqual(C.Armory, C.Category("PhysicalGunObject AutomaticRifleItem"));
            Assert.AreEqual(C.Armory, C.Category("PhysicalGunObject SemiAutoPistolItem"));
            Assert.AreEqual(C.Armory, C.Category("PhysicalGunObject BasicHandHeldLauncherItem"));
            Assert.AreEqual(C.Armory, C.Category("PhysicalGunObject FlareGunItem"));
            Assert.AreEqual(C.Armory, C.Category("AmmoMagazine FlareClip"));
            // hand tools are no weapons: they go into the Tools cargo (type PhysicalGunObject)
            Assert.AreEqual("", C.Category("PhysicalGunObject WelderItem"));
            Assert.AreEqual("", C.Category("PhysicalGunObject AngleGrinderItem"));
            Assert.AreEqual("", C.Category("PhysicalGunObject HandDrillItem"));
            Assert.AreEqual(C.Seeds, C.Category("SeedItem Fruit"));
            Assert.AreEqual(C.Food, C.Category("ConsumableItem MealPack_Chili"));
            Assert.AreEqual(C.Waste, C.Category("Ore Organic"));
            Assert.AreEqual("", C.Category(Plate));
        }

        [TestMethod]
        public void Category_ItemType_OnlyIfAllItemsHaveIt()
        {
            // used as alternative in the hint "please define cargo for Ammo ... or ...(sms,armory)"
            Assert.AreEqual(C.Armory, C.Category("AmmoMagazine"));
            Assert.AreEqual("", C.Category("PhysicalGunObject"));   // rifles, but also welders
            Assert.AreEqual("", C.Category("ConsumableItem"));
            Assert.AreEqual("", C.Category("Component"));
            Assert.AreEqual("", C.Category("Ore"));
        }

        // ------------------------------------------------------------------ CargoFor

        [TestMethod]
        public void CargoFor_NoCargo_Nothing()
        {
            CollectionAssert.AreEqual(new string[0], Cargo(Nato));
            CollectionAssert.AreEqual(new string[0], Cargo(Nato, "Component", "Ore"));
        }

        [TestMethod]
        public void CargoFor_Ammo_OnlyArmory_FoundArmory()
        {
            // the bug: with only an (sms,armory) container the hint "define cargo for Ammo" appeared
            CollectionAssert.AreEqual(new[] { "Armory" }, Cargo(Nato, "Armory"));
        }

        [TestMethod]
        public void CargoFor_MostSpecificFirst()
        {
            CollectionAssert.AreEqual(new[] { Nato, "Armory", "AmmoMagazine" }, Cargo(Nato, "AmmoMagazine", "Armory", Nato));
        }

        [TestMethod]
        public void CargoFor_ItemCargo_OnlyForThatItem()
        {
            CollectionAssert.AreEqual(new[] { Plate }, Cargo(Plate, Plate));
            CollectionAssert.AreEqual(new string[0], Cargo(Motor, Plate));
            CollectionAssert.AreEqual(new[] { "Component" }, Cargo(Motor, Plate, "Component"));
        }

        [TestMethod]
        public void CargoFor_NoCategory_NoEmptyKeyLookup()
        {
            // items without a category must not find a cargo stored under ""
            CollectionAssert.AreEqual(new string[0], Cargo(Plate, ""));
        }

        [TestMethod]
        public void CargoFor_TagsOfTheContainers()
        {
            // the keys as AddInventoryToInventoryManagerList creates them from the tags
            var cargo = new[] { "Ammo", "Steelplate" }.ToDictionary(C.ToIngame, t => t);
            CollectionAssert.AreEqual(new[] { "Ammo" }, C.CargoFor(cargo, Nato).ToArray());
            CollectionAssert.AreEqual(new[] { "Steelplate" }, C.CargoFor(cargo, Plate).ToArray());
        }

        // ------------------------------------------------------------------ TypeOf / SubtypeOf

        [TestMethod]
        public void TypeOf_SubtypeOf()
        {
            Assert.AreEqual("Component", C.TypeOf(Plate));
            Assert.AreEqual("SteelPlate", C.SubtypeOf(Plate));
            Assert.AreEqual("NATO_5p56x45mm", C.SubtypeOf(Nato));
        }

        [TestMethod]
        public void TypeOf_SubtypeOf_NoSubtype()
        {
            Assert.AreEqual("Ore", C.TypeOf("Ore"));
            Assert.AreEqual("", C.SubtypeOf("Ore"));
        }

        [TestMethod]
        public void SubtypeOf_KeepsEverythingAfterTheFirstSpace()
        {
            Assert.AreEqual("Some Mod Item", C.SubtypeOf("Component Some Mod Item"));
        }
    }
}
