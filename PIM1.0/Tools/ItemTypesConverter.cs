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
                { "Ammo", IG_Ammo },
                { IngameStrings.RStone, Ore.Stone },
                { "Gravel", Ingot.Stone },
                { "Tools", IG_Tools },
                { "Kits", IG_Kits },
                { "Cash", IG_Phys },
                { "Datapads", IG_Datas },
                { "H-Bottles", IG_HBottles },
                { "O-Bottles", IG_OBottles },
                { "Ice", Ore.Ice },
                { "Water", Ingot.WaterFood },
                { "Greywater", Ingot.GreyWater },
                { "Deuterium", Ingot.DTFuelCapsule },
                { "Organic", Ore.Organic },
                { "Seeds", IG_Seeds },
                { "Steelplate", IG_Com + "SteelPlate" },
                { "Metalgrid", IG_Com + "MetalGrid" },
                { "Interiorplate", IG_Com + "InteriorPlate" },
                { "Smalltube", IG_Com + "SmallTube" },
                { "Largetube", IG_Com + "LargeTube" },
                { "Glass", IG_Com + "BulletproofGlass" },
                { "Gravity", IG_Com + "GravityGenerator" },
                { "Radio", IG_Com + "RadioCommunication" },
                { "Solar", IG_Com + "SolarCell" },
                { "Power", IG_Com + "PowerCell" },
                { "Zonechip", IG_Com + "ZoneChip" },
            };

            static ItemTypesConverter()
            {
                // components whose tag is their subtype
                foreach (var c in new[] { "Reactor", "Thrust", "Medical", "Detector", "Explosives", "Construction", "Motor", "Display", "Girder", "Computer", "Canvas" })
                    TagToIngame.Add(c, IG_Com + c);
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
                if (id.Contains("RifleItem") || id.Contains(IG_Ammo) || id.Contains("PistolItem") || id.Contains("LauncherItem") || id.Contains("FlareGunItem")) return Armory;
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
