using System.Collections.Generic;

namespace IngameScript
{
    // Pure refinery blueprint definitions: no Sandbox/VRage types,
    // so this file can be linked into the test project.
    partial class Program
    {
        static List<RefineryBlueprint> RefineryBlueprints = new List<RefineryBlueprint>();
        static List<RefineryBlueprint> _refTarget;
        static HashSet<string> _refNames;
        static bool _refIOMod;

        static void AddRefineryBlueprint(string bpName, string inputItem, string outputItem)
        {
            if (_refNames.Add(bpName)) _refTarget.Add(new RefineryBlueprint(bpName, inputItem, outputItem, _refIOMod));
        }


        static void AddRefineryBlueprintOreToIngot(string resource)
        {
            AddRefineryBlueprint(resource + "OreToIngot", "Ore " + resource, "Ingot " + resource);
        }


        static void AddRefineryBlueprintsArray(string[] resArray, string bpNamePrefix, string bpNameSuffix, string inputNamePrefix, string outputNamePrefix)
        {
            foreach (var bp in resArray)
            {
                AddRefineryBlueprint(bpNamePrefix + bp + bpNameSuffix, inputNamePrefix + bp, outputNamePrefix + bp);
            }
        }


        /// <summary>
        /// Creates all refinery blueprints of the enabled mods, adds them to <paramref name="target"/>
        /// (blueprint names already in the list are skipped) and sorts the list by input name.
        /// </summary>
        public static void DefineRefineryBlueprints(Dictionary<string, bool> usedMods, List<RefineryBlueprint> target)
        {
            _refTarget = target;
            _refIOMod = usedMods[Strings.M_IndustrialOverhaulMod];
            _refNames = new HashSet<string>();
            foreach (var b in target) _refNames.Add(b.Name);

            // Ingots
            AddRefineryBlueprint("StoneOreToIngotBasic", Ore.Stone, Ingot.Stone);
            AddRefineryBlueprint("ScrapToIronIngot", Ore.Scrap, Ingot.Iron);
            AddRefineryBlueprint("ScrapIngotToIronIngot", Ingot.Scrap, Ingot.Iron);
            AddRefineryBlueprintOreToIngot(IngameStrings.RGold);
            AddRefineryBlueprintOreToIngot(IngameStrings.RPlatinum);
            AddRefineryBlueprintOreToIngot(IngameStrings.RStone);
            AddRefineryBlueprintOreToIngot(IngameStrings.RSilver);
            AddRefineryBlueprintOreToIngot(IngameStrings.RIron);
            AddRefineryBlueprintOreToIngot(IngameStrings.RNickel);
            AddRefineryBlueprintOreToIngot(IngameStrings.RCobalt);
            AddRefineryBlueprintOreToIngot(IngameStrings.RSilicon);
            AddRefineryBlueprintOreToIngot(IngameStrings.RUranium);

            if (usedMods[Strings.M_SigmaDraconisCore])
            {
                // Ingots
                AddRefineryBlueprint("TungstenToIngot", "Ore Tungsten", "Ingot TungstenIngot");
                AddRefineryBlueprint("CopperToIngot", "Ore Copper", "Ingot CopperIngot");
                AddRefineryBlueprint("LeadToIngot", "Ore Lead", "Ingot LeadIngot");
                AddRefineryBlueprint("TitaniumToIngot", "Ore Titanium", "Ingot TitaniumIngot");
                AddRefineryBlueprint("GraphiteOreToIngot", "Ore Graphite", "Ingot Carbon");
                // AdminIngots
                AddRefineryBlueprint("LithiumToIngot", "Ore Lithium", "Ingot LithiumIngot");
                // CommonMetals: Tungsten, Copper, Lead, Graphite (same blueprints as Ingots)
            }

            if (usedMods[Strings.M_DeuteriumReactor])
            {
                AddRefineryBlueprint("StonetoDeuterium", Ore.Stone, Ingot.DeuteriumContainer);
                AddRefineryBlueprint("IcetoDeuterium", Ore.Ice, Ingot.DeuteriumContainer);
                AddRefineryBlueprint("DeuteriumOreToIngot", Ore.Deuterium, Ingot.DeuteriumContainer);
            }


            if (usedMods[Strings.M_DailyNeedsSurvival])
            {
                AddRefineryBlueprintOreToIngot(IngameStrings.RCarbon);
                AddRefineryBlueprintOreToIngot(IngameStrings.RPotassium);
                AddRefineryBlueprintOreToIngot(IngameStrings.RPhosphorus);
            }


            if (usedMods[Strings.M_SG_Ores])
            {
                AddRefineryBlueprintOreToIngot(IngameStrings.RNaquadah);
                AddRefineryBlueprintOreToIngot(IngameStrings.RTrinium);
                AddRefineryBlueprintOreToIngot(IngameStrings.RNeutronium);
            }


            if (!usedMods[Strings.M_IndustrialOverhaulMod])
            {
                AddRefineryBlueprintOreToIngot(IngameStrings.RMagnesium);
            }


            else // IndustrialOverhaulMod
            {
                string[] ioModResources = { "Iron", "Nickel", "Cobalt", "Silicon", "Silver", "Gold", "Platinum", "Uranium", "Copper", "Lithium", "Bauxite", "Titanium", "Tantalum", "Sulfur", };
                // SmelterIngots & RefineryIngots
                AddRefineryBlueprintOreToIngot("Copper");
                AddRefineryBlueprint("BauxiteOreToIngot", Ore.Bauxite, Ingot.Aluminium);
                AddRefineryBlueprintOreToIngot("Titanium");
                AddRefineryBlueprintOreToIngot("Tantalum");
                AddRefineryBlueprint("CoalToCarbonBasic", Ore.Coal, Ingot.Carbon);
                // CrushedToIngot
                AddRefineryBlueprintsArray(ioModResources, "Crushed", "OreToIngot", "Ore Crushed", "Ingot ");
                // PurifiedToIngot
                AddRefineryBlueprintsArray(ioModResources, "Purified", "OreToIngot", "Ore Purified", "Ingot ");
                // Crusher
                AddRefineryBlueprintsArray(ioModResources, "Crush", "Ore", "Ore ", "Ore Crushed");
                AddRefineryBlueprint("CrushNiterOre", Ore.Niter, Ore.Magnesium);
                AddRefineryBlueprint("CrushStoneOre", Ore.Stone, Ingot.Stone);
                // Purifier
                AddRefineryBlueprintsArray(ioModResources, "Purify", "Ore", "Ore Crushed", "Ore Purified");
                AddRefineryBlueprint("PurifyNiterOre", Ore.Magnesium, "Ore PurifiedNiter");
                // ChemicalPlantOre
                AddRefineryBlueprintOreToIngot("Niter");
                AddRefineryBlueprintOreToIngot("Lithium");
                AddRefineryBlueprintOreToIngot("Sulfur");
                AddRefineryBlueprint("CoalToCarbon", Ore.Coal, Ingot.Carbon);
                AddRefineryBlueprint("CrushedNiterOreToIngot", Ore.Magnesium, Ingot.Niter);
                AddRefineryBlueprint("PurifiedNiterOreToIngot", "Ore PurifiedNiter", Ingot.Niter);
                // BitumenExtractor
                AddRefineryBlueprint("OilSandToCrudeOil", "Ore OilSand", "Ore CrudeOil");
                // OilCracking
                AddRefineryBlueprint("CrudeOilCracking", "Ore CrudeOil", "Ingot FuelOil");
                // CentrifugeIngots
                AddRefineryBlueprintOreToIngot("Uranium");
            }
            target.Sort((x, y) => x.InputIDName.CompareTo(y.InputIDName));

            _refTarget = null;
            _refNames = null;
        }
    }
}
