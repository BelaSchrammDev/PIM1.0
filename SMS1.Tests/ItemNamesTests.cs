using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IngameScript.Tests
{
    /// <summary>
    /// Tests for ItemNames.Build: the display name of an item.
    /// </summary>
    [TestClass]
    public class ItemNamesTests
    {
        [TestMethod]
        public void EmptyName_IsDerivedFromTheSubtype()
        {
            Assert.AreEqual("Ironore", Program.ItemNames.Build("", "Iron", isOre: true, isIngot: false));
            Assert.AreEqual("Ironingot", Program.ItemNames.Build("", "Iron", isOre: false, isIngot: true));
            Assert.AreEqual("SteelPlate", Program.ItemNames.Build("", "SteelPlate", isOre: false, isIngot: false));
        }

        [TestMethod]
        public void At_IsTheSubtype_UnderscoreIsASpace()
        {
            Assert.AreEqual("Magnesiumpowder", Program.ItemNames.Build("@powder", "Magnesium", false, true));
            Assert.AreEqual("Trinium Plate", Program.ItemNames.Build("@ Plate", "Trinium", false, false));
            Assert.AreEqual("Emergency Ration", Program.ItemNames.Build("Emergency_Ration", "Emergency_Ration", false, false));
            Assert.AreEqual("Ice", Program.ItemNames.Build("@", "Ice", true, false));
        }

        [TestMethod]
        public void Name_DoesNotDependOnAPreviousName()
        {
            // SMS 1.4 appended the name of a later mod to the old one ("BulletproofGlass" + "Glass");
            // SetName now always uses the result of Build
            Assert.AreEqual("Glass", Program.ItemNames.Build("Glass", "BulletproofGlass", false, false));
        }
    }
}
