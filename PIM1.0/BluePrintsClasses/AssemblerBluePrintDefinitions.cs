using System.Collections.Generic;

namespace IngameScript
{
    // Pure assembler blueprint definitions: no Sandbox/VRage types,
    // so this file can be linked into the test project.
    partial class Program
    {
        static string curmod;
        static Dictionary<string, bool> _bpUsedMods;
        static Dictionary<string, AssemblerBluePrint> _bpTarget;

        static void addBluePrint(string typeID, string subtypeID, string itemName, string alternativItemName)
        {
            if (curmod != Strings.M_Vanilla && !(_bpUsedMods.ContainsKey(curmod) && _bpUsedMods[curmod])) return;
            var definitionSubtype = subtypeID;
            if (subtypeID.StartsWith("Position0")) subtypeID = subtypeID.Substring(subtypeID.IndexOf('_') + 1);
            var bpi = typeID + " " + subtypeID;
            if (!_bpTarget.ContainsKey(bpi)) _bpTarget.Add(bpi, new AssemblerBluePrint(typeID, subtypeID, definitionSubtype, curmod, (itemName == "" ? "" : typeID + " " + itemName), alternativItemName));
        }
        static void C(string s, string astr = "", string astn = "") { addBluePrint("Component", s, astr, astn); }
        static void A(string s, string astr = "", string astn = "") { addBluePrint(IG_Ammo, s, astr, astn); }
        static void T(string s, string astr = "", string astn = "") { addBluePrint(IG_Tools, s, astr, astn); }
        static void K(string s, string astr = "", string astn = "") { addBluePrint(IG_Kits, s, astr, astn); }
        static void O(string s, string astr = "", string astn = "") { addBluePrint(IG_OBottles, s, astr, astn); }
        static void H(string s, string astr = "", string astn = "") { addBluePrint(IG_HBottles, s, astr, astn); }
        static void D(string s, string astr = "", string astn = "") { addBluePrint(IG_Datas, s, astr, astn); }
        static void S(string s, string astr = "", string astn = "") { addBluePrint(IG_Seeds, s, astr, astn); }
        static void I(string blueprintNmae, string itemName = "", string alternativeItemName = "") { addBluePrint(IG_I, blueprintNmae, itemName, alternativeItemName); }
        static void E(string s, string astr = "", string astn = "") { addBluePrint("Ore", s, astr, astn); }

        /// <summary>
        /// Creates all assembler blueprints of the enabled mods and adds them to <paramref name="target"/>
        /// (key = "Type Subtype"; the first definition of a key wins).
        /// </summary>
        public static void DefineAssemblerBluePrints(Dictionary<string, bool> usedMods, Dictionary<string, AssemblerBluePrint> target)
        {
            _bpUsedMods = usedMods;
            _bpTarget = target;
            curmod = Strings.M_Vanilla;

            if (!usedMods[Strings.M_IndustrialOverhaulMod])
            {
                // blueprint vanilla
                // Components
                C("ConstructionComponent");
                C("GirderComponent");
                C("MetalGrid");
                C("InteriorPlate");
                C("SteelPlate");
                C("SmallTube");
                C("LargeTube");
                C("MotorComponent");
                C("Display");
                C("BulletproofGlass");
                C("ComputerComponent");
                C("ReactorComponent");
                C("ThrustComponent");
                C("GravityGeneratorComponent");
                C("MedicalComponent");
                C("RadioCommunicationComponent");
                C("DetectorComponent");
                C("ExplosivesComponent");
                C("SolarCell");
                C("PowerCell");
                C("Superconductor");
                // PrototechComponents
                C("PrototechPanel");
                C("PrototechCapacitor");
                C("PrototechPropulsionUnit");
                C("PrototechMachinery");
                C("PrototechCircuitry");
                C("PrototechCoolingUnit");
                // Equipment
                O("Position0010_OxygenBottle");
                H("Position0020_HydrogenBottle");
                C("Position0030_Canvas");
                D("Position0040_Datapad");
                T("Position0005_FlareGun", "FlareGunItem");
                A("Position0005_FlareGunMagazine", "FlareClip");
                A("Position0007_FireworksBoxBlue");
                A("Position00071_FireworksBoxGreen");
                A("Position00072_FireworksBoxRed");
                A("Position00073_FireworksBoxYellow");
                A("Position00074_FireworksBoxPink");
                A("Position00075_FireworksBoxRainbow");
                // Tools
                T(IngameStrings.GetPositionString(10, Component.AngleGrinder));
                T(IngameStrings.GetPositionString(20, Component.AngleGrinder + "2"));
                T(IngameStrings.GetPositionString(30, Component.AngleGrinder + "3"));
                T(IngameStrings.GetPositionString(40, Component.AngleGrinder + "4"));
                T("Position0050_HandDrill");
                T("Position0060_HandDrill2");
                T("Position0070_HandDrill3");
                T("Position0080_HandDrill4");
                T("Position0090_Welder");
                T("Position0100_Welder2");
                T("Position0110_Welder3");
                T("Position0120_Welder4");
                // Weapons
                T("Position0010_SemiAutoPistol");
                T("Position0020_FullAutoPistol");
                T("Position0030_EliteAutoPistol");
                T("Position0040_AutomaticRifle");
                T("Position0050_RapidFireAutomaticRifle");
                T("Position0060_PreciseAutomaticRifle");
                T("Position0070_UltimateAutomaticRifle");
                T("Position0080_BasicHandHeldLauncher");
                T("Position0090_AdvancedHandHeldLauncher");
                // Ammo
                A("Position0010_SemiAutoPistolMagazine");
                A("Position0020_FullAutoPistolMagazine");
                A("Position0030_ElitePistolMagazine");
                A("Position0040_AutomaticRifleGun_Mag_20rd");
                A("Position0050_RapidFireAutomaticRifleGun_Mag_50rd");
                A("Position0060_PreciseAutomaticRifleGun_Mag_5rd");
                A("Position0080_NATO_25x184mmMagazine");
                A("Position0070_UltimateAutomaticRifleGun_Mag_30rd");
                A("Position0090_AutocannonClip");
                A("Position0100_Missile200mm");
                A("Position0110_MediumCalibreAmmo");
                A("Position0120_LargeCalibreAmmo");
                A("Position0130_SmallRailgunAmmo");
                A("Position0140_LargeRailgunAmmo");
                // FoodProcessorRations
                K("Position0010_CookMammalMeat", "MammalMeatCooked");
                K("Position0020_CookSpiderMeat", "InsectMeatCooked");
                K("Position0030_MealPack_KelpCrisp");
                K("Position0040_MealPack_FruitBar");
                K("Position0050_MealPack_GardenSlaw");
                K("Position0060_MealPack_RedPellets");
                K("Position0070_MealPack_Chili");
                K("Position0080_MealPack_Flatbread");
                K("Position0090_MealPack_Ramen");
                K("Position0100_MealPack_FruitPastry");
                K("Position0110_MealPack_VeggieBurger");
                K("Position0120_MealPack_Curry");
                K("Position0130_MealPack_GreenPellets");
                K("Position0140_MealPack_Dumplings");
                K("Position0150_MealPack_Spaghetti");
                K("Position0160_MealPack_Lasagna");
                K("Position0170_MealPack_Burrito");
                K("Position0180_MealPack_FrontierStew");
                K("Position0190_MealPack_SearedSabiroid");
                K("Position0200_MealPack_SteakDinner");
                // FoodProcessorSeeds
                S("Position0010_Seeds_Fruit", "Fruit");
                S("Position0020_Seeds_Grain", "Grain");
                S("Position0030_Seeds_Vegetables", "Vegetables");
                S("Position0040_Spores_Mushrooms", "Mushrooms");
            }

            curmod = Strings.M_SigmaDraconisCore;
            // TradeGoods
            C("Composting", "Compost");
            C("CrateofTomatoes", "CrateTomato");
            C("BottleWater", "DrinkingWater");
            C("ToSmallArms", "SmallArms");
            C("ToHeavyArms", "HeavyArms");
            C("ToTools", "ToolPack");
            C("ToAmmo", "AmmoCache");
            C("ToMedicalCache", "MedicalCache");
            C("ToRepairCache", "RepairCache");
            // NPTradeGoods
            C("toMachinePart", "UpgradedBelterComponent");
            C("toUpgradedBelterComp", "UpgradedBelterComponent");
            C("toOPAExpComponent", "OPAExpComponent");
            C("IcetoNPTradeGoods", "IceBox");
            C("RawMaterialsCraft", "RawMaterials");
            C("RawMaterialsUse", "InnerComponent");
            C("ComponentCreationTycho", "TychoComponent");
            C("ComponentCreationCeres", "CeresComponent");
            C("ComponentCreationVesta", "VestaComponent");
            C("ComponentCreationMedina", "MedinaComponent");
            C("PassengerCreationTycho", "CeresPassenger");
            C("PassengerCreationCeres", "TychoPassenger");
            C("PassengerCreationVesta", "CeresPassenger");
            C("PassengerCreationGanymede", "CeresPassenger");
            C("PassengerCreationMedina", "CeresPassenger");
            C("SurviviorConsume", "CeresPassenger");
            C("WelWalaConsume", "CeresPassenger");
            C("toInnerComp", "InnerComponent");
            C("toMCRNComponent", "MCRNComponent");
            C("toMCRNAdvComponent", "MCRNAdvComponent");
            C("toMCRNExpComponent", "MCRNExpComponent");
            C("toUNNComponent", "UNNComponent");
            C("toUNNAdvComponent", "UNNAdvComponent");
            C("toUNNExpComponent", "UNNExpComponent");
            // AdminComponents
            C("ToLithiumPowerCell", "LithiumCell");
            C("toResearchInnerUNN", "Research");
            C("toResearchInnerMCRN", "Research");
            C("toResearchBelter", "Research");
            // NPFuelGoods
            C("Fuel_Processing", "Fuel_Tank");
            // BulkPackaging
            C("BulkTungsten", "BulkTungstenCrate");
            C("BulkCopper", "BulkCopperCrate");
            C("BulkLead", "BulkLeadCrate");
            C("BulkTitanium", "BulkTitaniumCrate");
            C("BulkIron", "BulkIronCrate");
            C("BulkNickel", "BulkNickelCrate");
            C("BulkCobalt", "BulkCobaltCrate");
            C("BulkMagnesium", "BulkMagnesiumCrate");
            C("BulkSilicon", "BulkSiliconCrate");
            C("BulkSilver", "BulkSilverCrate");
            C("BulkGold", "BulkGoldCrate");
            C("BulkUranium", "BulkUraniumCrate");


            /* DailyNeedsSurvivalMod */
            curmod = Strings.M_DailyNeedsSurvival;
            I("SubFresh", "", "Algae-Soy Product");
            I("WaterFood", "", "Drinking Water Packet");
            I("OrganicToNutrients", "Nutrients", "Nutrients");
            I("ArtificialFood");
            I("LuxuryMeal");
            I("SabiroidSteak");
            I("VeganFood");
            I("WolfSteak");
            I("WolfBouillon");
            I("SabiroidBouillon");
            I("CoffeeFood", "", "Coffee");
            I("EmergencyWater", "WaterFood", "Emergency Water Packets");
            I("EmergencyFood");
            E("ClonedAnimalMeat", "WolfMeat");
            E("ClonedSabiroidMeat", "SabiroidMeat");
            I("Potatoes");
            I("Tomatoes");
            I("Carrots");
            I("Cucumbers");
            I("PotatoSeeds");
            I("TomatoSeeds");
            I("CarrotSeeds");
            I("CucumberSeeds");
            I("Ketchup");
            I("MartianSpecial");
            I("OrganicToFertilizer", "Fertilizer", "Organic Fertilizer");
            I("IngotsToFertilizer", "Fertilizer", "Artificial Fertilizer");
            I("NotBeefBurger");
            I("ToFurkey", "", "ToFurkey Dinner");
            I("SpaceMealBar");
            I("HotChocolate");
            I("SpacersBreakfast");
            I("ProteinShake");

            /* AzimuthThrusterMod */
            curmod = Strings.M_AzimuthThruster;
            C("AzimuthSuperchargerComponent", "AzimuthSupercharger");

            /* StargateMods */
            curmod = Strings.M_SG_Ores;
            C("Naquadah", "", "Naquadah Bars");
            C("Trinium", "", "Trinium Plate");
            C("Neutronium", "", "Neutronium Crate");

            if (!usedMods[Strings.M_SG_Ores])
            {
                curmod = Strings.M_SG_Gates;
                C("Naquadah", "", "Naquadah Bars");
            }

            /* PaintGunMod */
            curmod = Strings.M_PaintGun;
            T("Blueprint_PaintGun", "PhysicalPaintGun");
            A("Blueprint_PaintGunMag", "PaintGunMag");

            /* DeuteriumReactorMod */
            curmod = Strings.M_DeuteriumReactor;
            C("Magnetron_Component");
            I("DeuteriumOreToIngot", "DeuteriumContainer", "Deuterium");
            I("StonetoDeuterium", "DeuteriumContainer", "Deuterium (Stone)");
            I("IcetoDeuterium", "DeuteriumContainer", "Deuterium (Ice)");

            /* DefenseShieldMod */
            curmod = Strings.M_Shield;
            C("ShieldComponentBP", "ShieldComponent", "Field Emitter");

            /* MCRN RailGunMod */
            curmod = Strings.M_RailGun;
            A("RailGunAmmoMag");

            /* MWI Homing Weaponry Mod */
            curmod = Strings.M_HomingWeaponry;
            A("TorpedoMk1_Blueprint", "TorpedoMk1");
            A("SwarmMissileMk1_Blueprint", "SwarmMissile50mm");
            A("DestroyerMissileX_Blueprint", "DestroyerMissileX");
            A("DestroyerMissileMk1_Blueprint", "DestroyerMissileMk1");

            /* Industrial Overhaul Mod */
            curmod = Strings.M_IndustrialOverhaulMod;
            // Reprocessor
            I(IngameStrings.RSpentFuelReprocessing, "Uranium", "Nuclear Fuel");
            // AssemblingBenchComponents
            C("CopperWire");
            C("Electromagnet");
            C("Lightbulb");
            C("AcidPowerCell");
            C("AlkalinePowerCell");
            C("HeatingElement");
            C("POConstructionComponent", "Construction");
            C("POSteelPlate", "SteelPlate");
            C("POSmallTube", "SmallTube");
            C("POLargeTube", "LargeTube");
            C("POMotorComponent", "Motor");
            C("POComputerComponent", "Computer");
            C("PORadioCommunicationComponent", "RadioCommunication");
            // FabricatorComponents (+ Electromagnet, Lightbulb, AlkalinePowerCell, HeatingElement, see AssemblingBench)
            C("POMetalGrid", "MetalGrid");
            // WireDrawerComponents
            C("GoldWire");
            C("POSuperconductor", "Superconductor");
            // PlateStampComponents
            C("CompositeArmor");
            C("TitaniumPlate");
            C("ArmoredPlate");
            C("POInteriorPlate", "InteriorPlate");
            // ExtruderComponents
            C("POGirderComponent", "Girder");
            // SiliconFuserComponents
            C("ArmorGlass");
            C("Ceramic");
            C("POBulletproofGlass", "BulletproofGlass");
            // CementKilnComponents
            C("Concrete");
            // AssemblerComponents
            C("Capacitor");
            C("Cryocooler");
            C("POMedicalComponent", "Medical");
            C("POSolarCell", "SolarCell");
            // MicroelectronicsFactoryComponents
            C("Thermocouple");
            C("AdvancedComputer");
            C("PODisplay", "Display");
            C("PODetectorComponent", "Detector");
            // AutoLoomComponents
            C("Fabric");
            C("POCanvas", "Canvas");
            // AdvancedAssemblerComponents
            C("TokamakBlanket");
            C("SuperMagnet");
            C("LaserEmitter");
            C("POReactorComponent", "Reactor");
            C("POPowerCell", "PowerCell");
            // NanoAssemblerComponents
            C("ElectronMatrix");
            C("QuantumComputer");
            C("POThrustComponent", "Thrust");
            C("POGravityGeneratorComponent", "GravityGenerator");
            C("FSSolarCell");
            // MunitionsFactory
            C("POExplosivesComponent", "Explosives");
            I("Gunpowder", "Magnesium");
            A("POInteriorTurret_Mag_50rd", "InteriorTurret_Mag_50rd");
            A("PONATO_25x184mmMagazine", "NATO_25x184mm");
            A("DUNATO_25x184mmMagazine", "DUNATO_25x184mm");
            A("POMissile200mm", "Missile200mm");
            A("POFullAutoPistolMagazine", "FullAutoPistolMagazine");
            A("POElitePistolMagazine", "ElitePistolMagazine");
            A("POAutomaticRifleGun_Mag_20rd", "AutomaticRifleGun_Mag_20rd");
            A("PORapidFireAutomaticRifleGun_Mag_50rd", "RapidFireAutomaticRifleGun_Mag_50rd");
            A("POPreciseAutomaticRifleGun_Mag_5rd", "PreciseAutomaticRifleGun_Mag_5rd");
            A("POUltimateAutomaticRifleGun_Mag_30rd", "UltimateAutomaticRifleGun_Mag_30rd");
            A("AutocannonClipDUAP");
            A("MediumCalibreAmmoHE");
            A("MediumCalibreAmmoDUAP");
            A("LargeCalibreAmmoHE");
            A("LargeCalibreAmmoDUAP");
            A("SmallRailgunAmmoDUAP");
            // ChemicalPlantComponents
            C("PolymerToPlastic", "Plastic");
            C("Rubber");
            I("SyntheticPolymer", "Polymer");
            C("Asphalt");
            // T1Tools
            T("POWelder", "WelderItem");
            T("POAngleGrinder", "AngleGrinderItem");
            T("POHandDrill", "HandDrillItem");
            T("POSemiAutoPistol", "SemiAutoPistolItem");
            A("POSemiAutoPistolMagazine", "SemiAutoPistolMagazine");
            // Bottles
            O("POOxygenBottle", "OxygenBottle");
            H("POHydrogenBottle", "HydrogenBottle");
            // T2Tools
            T("POWelder2", "Welder2Item");
            T("POAngleGrinder2", "AngleGrinder2Item");
            T("POHandDrill2", "HandDrill2Item");
            T("POAutomaticRifle", "AutomaticRifleItem");
            T("POPreciseAutomaticRifle", "PreciseAutomaticRifleItem");
            T("PORapidFireAutomaticRifle", "RapidFireAutomaticRifleItem");
            T("POBasicHandHeldLauncher", "BasicHandHeldLauncherItem");
            T("POFullAutoPistol", "FullAutoPistolItem");
            // T3Tools
            T("POWelder3", "Welder3Item");
            T("POAngleGrinder3", "AngleGrinder3Item");
            T("POHandDrill3", "HandDrill3Item");
            T("POUltimateAutomaticRifle", "UltimateAutomaticRifleItem");
            T("POAdvancedHandHeldLauncher", "AdvancedHandHeldLauncherItem");
            T("POEliteAutoPistol", "ElitePistolItem");
            // T4Tools
            T("POWelder4", "Welder4Item");
            T("POAngleGrinder4", "AngleGrinder4Item");
            T("POHandDrill4", "HandDrill4Item");

            // IndustrialOverhaulWater Mod
            curmod = Strings.M_IndustrialOverhaulWaterMod;
            C("Foam");
            C("BuoyancyTube");

            // IndustrialOverhaulLockLoad Mod
            curmod = Strings.M_IndustrialOverhaulLLMod;
            // CompressedGravel
            A("GravelMag");
            A("GravelMagBig");
            // MunitionsFactory
            A("155mmAPShell");
            A("155mmDUAPShell");
            A("155mmHEShell");
            A("305mmAPShell");
            A("305mmDUAPShell");
            A("305mmHEShell");
            A("APCoilgunShell");
            A("DUAPCoilgunShell");
            A("CLGGMag", "CLGG");

            curmod = Strings.M_EatDrinkSleep;
            // Emergency
            K("SparklingWater");
            K("Emergency_Ration");

            curmod = Strings.M_PlantCook;
            // Farming
            I("Soya");
            I("Herbs");
            // Cooking
            K("AppleJuice");
            K("ApplePie");
            K("Tofu");
            K("MeatRoasted");
            K("ShroomSteak");
            K("Bread");
            K("Burger");
            K("Soup");
            K("MushroomSoup");
            K("TofuSoup");
            K("SparklingWaterCan", "SparklingWater");
            K("EuropaTea");
            // MushroomsFarm
            K("FarmedMushrooms", "Mushrooms");
            // AppleFarm
            K("FarmedApples", "Apple");
            // WheatFarm
            I("FarmedWheat", "Wheat");
            // HerbsFarm
            I("FarmedHerbs", "Herbs");
            // Soya_Farm
            I("FarmedSoya", "Soya");
            // VegetablesFarm
            I("FarmedPumpkin", "Pumpkin");
            I("FarmedCabbage", "Cabbage");

            curmod = Strings.M_AryxEpsteinDrive;
            C("AryxLynxon_FusionComponentBP", "AryxLynxon_FusionComponent", "Fusion Coil");

            curmod = Strings.M_HSR;
            // Components
            C("K_HSR_Component_Rail_Vanilla", "K_HSR_RailComponents");
            A("K_HSR_Ammuntion_Recipe_Slug", "K_HSR_Slug");
            C("K_HSR_Component_AssemblerStabilizer_Vanilla", "K_HSR_AssemblerSystem");
            // HSR_BlueprintClassFolder
            C("K_HSR_Component_Rail", "K_HSR_RailComponents");
            C("K_HSR_Component_PulseSystem", "K_HSR_PulseSystem");
            C("K_HSR_Component_Globe", "K_HSR_Globe");
            C("K_HSR_Component_Circuitry", "K_HSR_HyperConductiveCircuitry");
            C("K_HSR_Component_HexPlate", "K_HSR_HexagolPlating");
            C("K_HSR_Component_Conduit", "K_HSR_GelConduit");
            C("K_HSR_Component_GlobeII", "K_HSR_Globe_II");
            C("K_HSR_Component_RailII", "K_HSR_RailComponents_II");
            C("K_HSR_Component_HexPlateII", "K_HSR_HexagolPlating_II");
            C("K_HSR_Component_RailIII", "K_HSR_RailComponents_III");
            // HSR_BlueprintClassFolderII
            A("K_HSR_Ammuntion_Recipe_Slug_Automated", "K_HSR_Slug");
            // HSR_BlueprintClassFolderIII
            I("K_HSR_NaniteSludge_Recipe", "K_HSR_Nanites_Sludge");
            I("K_HSR_EnergizedGel_Recipe", "K_HSR_Nanites_EnergizedGel");
            I("K_HSR_Hexagol_Recipe", "K_HSR_Nanites_Hexagol");
            I("K_HSR_Chromium_Recipe", "K_HSR_Nanites_Chromium");

            curmod = Strings.M_NorthWindWeapons;
            // Ammo
            A("R75ammo", "", "75mm Railgun Ammo");
            A("R150ammo", "", "150mm Railgun Ammo");
            A("R250ammo", "", "250mm Railgun Ammo");
            A("H203Ammo", "", "203mm HE Ammo");
            A("H203AmmoAP", "", "203mm AP Ammo");
            A("C30Ammo", "", "30mm Standard Ammo");
            A("C30DUammo", "", "30mm DU Ammo");
            A("CRAM30mmAmmo", "", "30mm C-RAM Ammo");
            A("C100mmAmmo", "", "100mm HE Ammo");
            A("C300AmmoAP", "", "300mm AP Ammo");
            A("C300AmmoHE", "", "300mm HE Ammo");
            A("C300AmmoG", "", "300mm Guided Ammo");
            A("C400AmmoAP", "", "400mm AP Ammo");
            A("C400AmmoHE", "", "400mm HE Ammo");
            A("C400AmmoCluster", "", "400mm Cluster Ammo");
            A("C500AmmoAP", "", "500mm AP Ammo");
            A("C500AmmoHE", "", "500mm HE Ammo");
            A("C500AmmoCasaba", "", "500mm Casaba Ammo");
            A("PlasmaCell10MJ", "", "10MJ PlasmaCannon Cell");

            curmod = null;
            _bpUsedMods = null;
            _bpTarget = null;
        }
    }
}
