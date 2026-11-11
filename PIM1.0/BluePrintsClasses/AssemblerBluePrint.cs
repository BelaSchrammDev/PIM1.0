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

            static string BluePrintNameToItemName(string t, string s)
            {
                var cs = IngameStrings.RComponent;
                if (s == Component.Magnetron) return cs + " " + s; // mod item
                if (t == cs && s.EndsWith(cs)) return t + " " + s.Substring(0, s.Length - cs.Length); // components remove from bottom
                if (s.StartsWith(IngameStrings.RNATO_25)) // NATO Magazine
                {
                    cs = IngameStrings.RMagazine;
                    return t + " " + s.Substring(0, s.Length - cs.Length);
                }
                if (t == IG_Tools) // add 'item' to all PhyicalGunItems
                {
                    if (s.Contains("Drill") || s.Contains("Grinder") || s.Contains("Welder") || s.Contains("Rifle")) return t + " " + s + "Item";
                }
                return t + " " + s;
            }

            public bool IfRefineryBluePrint()
            {
                return BlueprintID == Ingot.SubFresh || BlueprintID == (Ingot.SpentFuelReprocessing);
            }

            void ConvertAutoCraftingName()
            {
                AutoCraftingType = type;
                if (ToolsAndGunsTypes.Contains(type)) AutoCraftingType = Strings.AC_ToolsAndGuns;
                else if (seed_cast.Contains(ItemName)) AutoCraftingType = IngameStrings.RSeeds;
                else if (food_cast.Contains(ItemName)) AutoCraftingType = IG_Food;
                else if (subtype == IngameStrings.RSpentFuelReprocessing) AutoCraftingType = IngameStrings.RReprocessing;
                else if (subtype.Contains(IngameStrings.RDeuterium)) AutoCraftingType = IngameStrings.RDeuterium;
                if (subtypename != "")
                {
                    AutoCraftingName = subtypename.Replace('_', ' ');
                    return;
                }
                AutoCraftingName = ItemName.Split(' ')[1];
                if (AutoCraftingName.StartsWith("Position")) AutoCraftingName = AutoCraftingName.Substring(AutoCraftingName.IndexOf('_') + 1);
                else if (AutoCraftingName.StartsWith("MealPack")) AutoCraftingName = AutoCraftingName.Substring(9);
                else if (AutoCraftingName.StartsWith("K_HSR_")) AutoCraftingName = AutoCraftingName.Substring(6);
                else if (AutoCraftingName.Contains(IngameStrings.RAutomaticRifleGunMag))
                {
                    if (AutoCraftingName.StartsWith(IngameStrings.RAutomaticRifleGunMag)) AutoCraftingName = IngameStrings.RAutoRifleGunMagazine;
                    else AutoCraftingName = AutoCraftingName.Substring(0, AutoCraftingName.IndexOf(IngameStrings.RAutomaticRifleGunMag)) + IngameStrings.RRifleGunMagazine;
                }
                else if (type == IG_Seeds) AutoCraftingName += " " + IngameStrings.RSeeds;
                else if (type == IG_Tools && ModName == Strings.M_Vanilla)
                {
                    string[] Tools = { "HandDrill", "Grinder", "Welder" };
                    var isTool = false;
                    foreach (var toolType in Tools)
                    {
                        if (AutoCraftingName.Contains(toolType))
                        {
                            var preString = "";
                            if (AutoCraftingName.Contains('4')) preString = "Elite";
                            else if (AutoCraftingName.Contains('3')) preString = "Professional";
                            else if (AutoCraftingName.Contains('2')) preString = "Ultimate";
                            AutoCraftingName = preString + toolType;
                            isTool = true;
                            break;
                        }
                    }
                    if (!isTool && AutoCraftingName.EndsWith("Item")) AutoCraftingName = AutoCraftingName.Substring(0, AutoCraftingName.Length - 4);
                }
                if (AutoCraftingName.EndsWith(IngameStrings.RMagazine)) AutoCraftingName = AutoCraftingName.Substring(0, AutoCraftingName.Length - 5);
                AutoCraftingName = AutoCraftingName.Replace('_', ' ');
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
            public void CalcMinimumAmount() { MinimumAmount = (MaximumItemAmount * AutocraftingThreshold) / 100; }
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
