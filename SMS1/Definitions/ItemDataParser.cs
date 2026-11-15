using System;

namespace IngameScript
{
    partial class Program
    {
        /// <summary>
        /// Reads the compact item data of one mod (ItemData in Generated/ItemData.g.cs, made by SMS1.ItemDataGenerator
        /// from SMS1/ItemData/*.txt). Pure (no SE types): linked into the generator and SMS1.Tests.
        ///
        /// Format: entries separated by ','; fields by ':'; trailing empty fields are left out.
        ///   section: '#' type code, default max [: category]           e.g. "#C100", "#K5:Food"
        ///   entry:   subtype [: max [: name [: ammo capacity]]]           max empty = the default of the section
        ///   Resource section (R): subtype [: ore max [: ingot max [: ore name [: ingot name]]]]
        ///   subtype ending with '*': tools MK1..MK4 (subtype + Item, 2Item, 3Item, 4Item; name + MK2..MK4)
        /// Every item is passed to add(type, subtype, max, name, category, ammo capacity) in the data order.
        /// </summary>
        public static class ItemDataParser
        {
            // must match SMS1.ItemDataGenerator (it uses these arrays)
            public const string TypeCodes = "REICTKAHOPDS";
            public static readonly string[] TypeNames =
            {
                "Resource", "Ore", "Ingot", "Component", "PhysicalGunObject", "ConsumableItem",
                "AmmoMagazine", "GasContainerObject", "OxygenContainerObject", "PhysicalObject", "Datapad", "SeedItem"
            };

            public static void Parse(string data, Action<string, string, int, string, string, int> add)
            {
                string type = "", category = "";
                var defaultMax = 0;
                foreach (var entry in data.Split(','))
                {
                    var fields = entry.Split(':');
                    if (entry[0] == '#')
                    {
                        type = TypeNames[TypeCodes.IndexOf(entry[1])];
                        defaultMax = int.Parse(fields[0].Substring(2));
                        category = Field(fields, 1);
                        continue;
                    }

                    var subtype = fields[0];
                    var max = Number(fields, 1, defaultMax);
                    if (type == "Resource")
                    {
                        add("Ore", subtype, max, Field(fields, 3), category, 0);
                        add("Ingot", subtype, Number(fields, 2, defaultMax), Field(fields, 4), category, 0);
                    }
                    else if (subtype.EndsWith("*"))
                    {
                        subtype = subtype.TrimEnd('*');
                        var name = Field(fields, 2);
                        if (name == "") name = subtype;
                        for (var tier = 1; tier <= 4; tier++)
                            add(type, subtype + (tier > 1 ? tier.ToString() : "") + "Item", max, name + (tier > 1 ? " MK" + tier : ""), category, 0);
                    }
                    else add(type, subtype, max, Field(fields, 2), category, Number(fields, 3, 0));
                }
            }

            static string Field(string[] fields, int index)
            {
                return index < fields.Length ? fields[index] : "";
            }

            static int Number(string[] fields, int index, int defaultValue)
            {
                return index < fields.Length && fields[index] != "" ? int.Parse(fields[index]) : defaultValue;
            }
        }
    }
}
