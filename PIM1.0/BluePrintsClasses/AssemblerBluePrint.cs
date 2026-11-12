using System;
using System.Linq;

namespace IngameScript
{
    // Pure part of AssemblerBluePrint: no Sandbox/VRage types,
    // so this file can be linked into the test project.
    // The ingame part (MyDefinitionId, lookups) lives in AssemblerBluePrintIngame.cs.
    partial class Program
    {
        const string BluePrintDefinitionPrefix = "MyObjectBuilder_BlueprintDefinition/";
        static int AutocraftingThreshold = 80;

        public partial class AssemblerBluePrint : IComparable<AssemblerBluePrint>
        {
            static readonly string[] ToolsAndGunsTypes = { IG_Tools, IG_Datas, IG_HBottles, IG_OBottles, };

            public long ItemPriority = 0;
            public bool valid = true;
            public int CurrentItemAmount = 0;
            public int AssemblingDeltaAmount = 0;
            public int AssemblyAmount = 0;
            public int RefineryAmount = 0;
            public int MinimumAmount = 0;
            public int MaximumItemAmount = 1000;
            public string ModName = "";
            public string ItemName = "";
            public string AutoCraftingName = "";
            public string AutoCraftingType = "";
            public string BlueprintID = "";
            // full blueprint subtype as known by the game (incl. "Position0010_" prefix),
            // used to build the MyDefinitionId
            public string DefinitionSubtype = "";
            string type = "";
            public string subtype = "";
            string subtypename = "";

            /// <summary>Item name derived from the blueprint subtype, e.g. "Component ConstructionComponent" -> "Component Construction".</summary>
            public static string BluePrintNameToItemName(string t, string s)
            {
                var cs = IngameStrings.RComponent;
                // components: "...Component" suffix removed
                if (t == cs && s.EndsWith(cs)) return t + " " + s.Substring(0, s.Length - cs.Length);
                // NATO magazines: "Magazine" suffix removed
                if (s.StartsWith(IngameStrings.RNATO_25)) return t + " " + s.Substring(0, s.Length - IngameStrings.RMagazine.Length);
                // hand tools and rifles: "Item" added
                if (t == IG_Tools && (s.Contains("Drill") || s.Contains("Grinder") || s.Contains("Welder") || s.Contains("Rifle"))) return t + " " + s + "Item";
                return t + " " + s;
            }

            public bool IfRefineryBluePrint()
            {
                return BlueprintID == Ingot.SubFresh || BlueprintID == (Ingot.SpentFuelReprocessing);
            }

            void ConvertAutoCraftingName()
            {
                AutoCraftingType = DeriveAutoCraftingType();
                AutoCraftingName = (subtypename != "" ? subtypename : DeriveAutoCraftingName()).Replace('_', ' ');
            }

            string DeriveAutoCraftingType()
            {
                if (ToolsAndGunsTypes.Contains(type)) return Strings.AC_ToolsAndGuns;
                if (ItemTypesConverter.SeedItems.Contains(ItemName)) return IngameStrings.RSeeds;
                if (ItemTypesConverter.FoodItems.Contains(ItemName)) return IG_Food;
                if (subtype == IngameStrings.RSpentFuelReprocessing) return IngameStrings.RReprocessing;
                if (subtype.Contains(IngameStrings.RDeuterium)) return IngameStrings.RDeuterium;
                return type;
            }

            /// <summary>Short name for the autocrafting LCD, derived from the item subtype.</summary>
            string DeriveAutoCraftingName()
            {
                var name = ItemTypesConverter.SubtypeOf(ItemName);
                if (name.StartsWith("MealPack")) name = name.Substring(9);   // "MealPack_Chili" -> "Chili"
                else if (name.Contains(IngameStrings.RAutomaticRifleGunMag))
                {
                    // "AutomaticRifleGun_Mag_20rd" -> "AutoRifleGunMag", "PreciseAutomaticRifleGun_Mag_5rd" -> "PreciseRifleGunMag"
                    var i = name.IndexOf(IngameStrings.RAutomaticRifleGunMag);
                    name = i == 0 ? IngameStrings.RAutoRifleGunMagazine : name.Substring(0, i) + IngameStrings.RRifleGunMagazine;
                }
                else if (type == IG_Seeds) name += " " + IngameStrings.RSeeds;
                else if (type == IG_Tools && ModName == Strings.M_Vanilla) name = VanillaToolName(name);

                // "...Magazine" -> "...Mag"
                return name.EndsWith(IngameStrings.RMagazine) ? name.Substring(0, name.Length - 5) : name;
            }

            static readonly string[] VanillaTools = { "HandDrill", "Grinder", "Welder" };

            /// <summary>"AngleGrinder4Item" -> "EliteGrinder", "Welder2Item" -> "UltimateWelder", "SemiAutoPistolItem" -> "SemiAutoPistol".</summary>
            static string VanillaToolName(string name)
            {
                foreach (var tool in VanillaTools)
                    if (name.Contains(tool))
                        return (name.Contains('4') ? "Elite" : name.Contains('3') ? "Professional" : name.Contains('2') ? "Ultimate" : "") + tool;
                return name.EndsWith("Item") ? name.Substring(0, name.Length - 4) : name;
            }

            public AssemblerBluePrint(string iTypeID, string iSubTypeID, string definitionSubtype, string modName, string alter, string astype)
            {
                subtypename = astype;
                type = iTypeID;
                subtype = iSubTypeID;
                DefinitionSubtype = definitionSubtype;
                BlueprintID = type + " " + subtype;
                ItemName = alter == "" ? BluePrintNameToItemName(type, subtype) : alter;
                ModName = modName;
                ConvertAutoCraftingName();
                SetMaximumAmount(0);
            }
            public int CompareTo(AssemblerBluePrint other)
            {
                if (other.ItemPriority < ItemPriority) return -1;
                else if (other.ItemPriority > ItemPriority) return 1;
                return 0;
            }
            public bool NeedsAssembling() { return (MinimumAmount > CurrentItemAmount + AssemblyAmount); }
            public bool IfMax() { return (MaximumItemAmount <= CurrentItemAmount); }
            public void SetMaximumAmount(int m) { MaximumItemAmount = m; CalcMinimumAmount(); }
            public void CalcMinimumAmount() { MinimumAmount = MinimumAmountFor(MaximumItemAmount, AutocraftingThreshold); }

            /// <summary>
            /// Stock below which an item is crafted: the threshold percentage of the maximum, rounded up.
            /// Rounded down, a maximum of 1 gave 0 and the item was never crafted.
            /// </summary>
            public static int MinimumAmountFor(int maximum, int thresholdPercent)
            {
                return (maximum * thresholdPercent + 99) / 100;
            }

            /// <summary>
            /// Share of assembler number <paramref name="index"/> (0, 1, ...) if <paramref name="amount"/> is
            /// split between <paramref name="count"/> assemblers. The remainder goes to the first ones,
            /// so nothing is lost if the amount is smaller than the number of assemblers.
            /// </summary>
            public static int SplitAmount(int amount, int count, int index)
            {
                return amount / count + (index < amount % count ? 1 : 0);
            }
            public void SetCurrentAmount(int amount)
            {
                CurrentItemAmount = amount;
                if (MaximumItemAmount > 0) AssemblingDeltaAmount = MaximumItemAmount - amount;
                else AssemblingDeltaAmount = -1;
            }
            public void CalcPriority()
            {
                if (AssemblingDeltaAmount <= 0) ItemPriority = 0;
                else ItemPriority = 100 - (CurrentItemAmount * 100 / MaximumItemAmount);
            }
        }
    }
}
