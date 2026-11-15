using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmsItemDataTool;

namespace IngameScript.Tests
{
    /// <summary>
    /// Tests for the .sbc reader of SMS1.ItemDataTool with fake game data.
    /// </summary>
    [TestClass]
    public class ItemDataToolTests
    {
        const string Sbc = @"<?xml version=""1.0""?>
<Definitions xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <PhysicalItems>
    <PhysicalItem>
      <Id><TypeId>PhysicalGunObject</TypeId><SubtypeId>FlareGunItem</SubtypeId></Id>
      <DisplayName>DisplayName_Item_FlareGun</DisplayName>
    </PhysicalItem>
    <PhysicalItem>
      <Id><TypeId>Ore</TypeId><SubtypeId>Secret</SubtypeId></Id>
      <DisplayName>Secret</DisplayName>
      <Public>false</Public>
    </PhysicalItem>
    <PhysicalItem>
      <Id><TypeId>TreeObject</TypeId><SubtypeId>Tree</SubtypeId></Id>
      <DisplayName>Tree</DisplayName>
    </PhysicalItem>
  </PhysicalItems>
  <AmmoMagazines>
    <AmmoMagazine>
      <Id Type=""AmmoMagazine"" Subtype=""FlareClip"" />
      <DisplayName>Flare Gun Magazine</DisplayName>
      <Capacity>4</Capacity>
    </AmmoMagazine>
  </AmmoMagazines>
</Definitions>";

        const string Resx = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root><data name=""DisplayName_Item_FlareGun"" xml:space=""preserve""><value>Flare Gun</value></data></root>";

        [TestMethod]
        public void Read_ItemTypesOfSms_LocalizedNames_Capacity_Hidden()
        {
            var dir = Path.Combine(Path.GetTempPath(), "sms_sbc_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(dir, "Data", "Localization"));
            try
            {
                File.WriteAllText(Path.Combine(dir, "Data", "Items.sbc"), Sbc);
                File.WriteAllText(Path.Combine(dir, "Data", "Localization", "MyTexts.resx"), Resx);

                var items = SbcItems.Read(dir).Items;

                CollectionAssert.AreEquivalent(new[] { "PhysicalGunObject/FlareGunItem", "Ore/Secret", "AmmoMagazine/FlareClip" },
                    items.Select(i => i.Key).ToArray(), "TreeObject is no SMS item type");
                Assert.AreEqual("Flare Gun", items.Single(i => i.Subtype == "FlareGunItem").DisplayName);
                Assert.AreEqual(4, items.Single(i => i.Subtype == "FlareClip").Capacity);
                Assert.IsTrue(items.Single(i => i.Subtype == "Secret").Hidden);
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }
    }
}
