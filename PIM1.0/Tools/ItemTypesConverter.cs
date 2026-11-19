using System.Collections.Generic;
using System.Linq;

namespace IngameScript
{
    partial class Program
    {
        /// <summary>
        /// Conversions between PIM item ids ("Component SteelPlate"), container tags ("(sms,steelplate)")
        /// and cargo categories ("Armory"). Pure (no SE types), tested in ItemTypesConverterTests.
        /// The parts with SE types are in ItemTypesConverterIngame.cs, the category lists in ItemCasts.cs.
        /// </summary>
        public static partial class ItemTypesConverter
        {
            public const string Armory = "Armory", Seeds = "Seeds", Food = "Food", Waste = "Waste";

            // container tag (as written by ToArgStr) -> item id or item type, the key in InventoryManagerList
            static readonly Dictionary<string, string> TagToIngame = new Dictionary<string, string>
            {
                { "Ammo", TypeAmmo },
                { SubtypeStone, OreStone },
                { "Gravel", IngotStone },
                { "Tools", TypeTool },
                { "Kits", TypeKit },
                { "Cash", TypePhysicalObject },
                { "Datapads", TypeDatapad },
                { "H-Bottles", TypeHBottle },
                { "O-Bottles", TypeOBottle },
                { "Ice", OreIce },
                { "Water", IngotWaterFood },
                { "Greywater", IngotGreyWater },
                { "Deuterium", IngotDTFuelCapsule },
                { "Organic", OreOrganic },
                { "Seeds", TypeSeed },
                { "Steelplate", ComponentPrefix + "SteelPlate" },
                { "Metalgrid", ComponentPrefix + "MetalGrid" },
                { "Interiorplate", ComponentPrefix + "InteriorPlate" },
                { "Smalltube", ComponentPrefix + "SmallTube" },
                { "Largetube", ComponentPrefix + "LargeTube" },
                { "Glass", ComponentPrefix + "BulletproofGlass" },
                { "Gravity", ComponentPrefix + "GravityGenerator" },
                { "Radio", ComponentPrefix + "RadioCommunication" },
                { "Solar", ComponentPrefix + "SolarCell" },
                { "Power", ComponentPrefix + "PowerCell" },
                { "Zonechip", ComponentPrefix + "ZoneChip" },
            };

            static ItemTypesConverter()
            {
                // components whose tag is their subtype
                foreach (var c in new[] { "Reactor", "Thrust", "Medical", "Detector", "Explosives", "Construction", "Motor", "Display", "Girder", "Computer", "Canvas" })
                    TagToIngame.Add(c, ComponentPrefix + c);
            }

            /// <summary>
            /// Key in InventoryManagerList for a container tag: "Steelplate" -> "Component SteelPlate",
            /// "Ammo" -> "AmmoMagazine"; other tags ("Ore", "Armory") unchanged, "" for docking tags.
            /// </summary>
            public static string ToIngame(string tag)
            {
                if (tag.Contains("Dock")) return "";
                string ingame;
                return TagToIngame.TryGetValue(tag, out ingame) ? ingame : tag;
            }

            /// <summary>Container tag of an item id or item type ("AmmoMagazine" -> "Ammo"), null if there is no own tag.</summary>
            public static string ToTag(string ingame)
            {
                foreach (var x in TagToIngame) if (x.Value == ingame) return x.Key;
                return null;
            }

            /// <summary>
            /// Cargo category of an item id ("AmmoMagazine NATO_5p56x45mm" -> "Armory"), "" if none.
            /// For an item type the category of all its items ("AmmoMagazine" -> "Armory", "PhysicalGunObject" -> "").
            /// </summary>
            public static string Category(string id)
            {
                if (id.Contains("RifleItem") || id.Contains(TypeAmmo) || id.Contains("PistolItem") || id.Contains("LauncherItem") || id.Contains("FlareGunItem")) return Armory;
                if (SeedItems.Contains(id)) return Seeds;
                if (FoodItems.Contains(id)) return Food;
                if (WasteItems.Contains(id)) return Waste;
                return "";
            }

            /// <summary>
            /// The cargo lists that take an item ("Component SteelPlate"), most specific first:
            /// the item itself, its category ("Armory") and its type ("Component").
            /// </summary>
            public static IEnumerable<T> CargoFor<T>(Dictionary<string, T> cargo, string id)
            {
                T c;
                if (cargo.TryGetValue(id, out c)) yield return c;
                var cat = Category(id);
                if (cat != "" && cargo.TryGetValue(cat, out c)) yield return c;
                if (cargo.TryGetValue(TypeOf(id), out c)) yield return c;
            }

            /// <summary>Item type of an item id: "Component SteelPlate" -> "Component".</summary>
            public static string TypeOf(string id)
            {
                var i = id.IndexOf(' ');
                return i < 0 ? id : id.Substring(0, i);
            }

            /// <summary>Subtype of an item id: "Component SteelPlate" -> "SteelPlate", "" without subtype.</summary>
            public static string SubtypeOf(string id)
            {
                var i = id.IndexOf(' ');
                return i < 0 ? "" : id.Substring(i + 1);
            }
        }
    }
}
