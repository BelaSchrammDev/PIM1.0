using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        void itemdefinitionen()
        {
            curMod = X_M_Vanilla;
            //R( resources, oreamount, [ingotamount] , [alernativ ore name], [alernativ ingot name])
            Resource("Uranium", 5000, 15000);
            Resource("Platinum", 5000, 20000);
            Resource("Silicon", 20000, 100000);
            Resource("Iron", 300000, 300000);
            Resource("Cobalt", 50000, 70000);
            Resource("Nickel", 100000, 100000);
            Resource("Silver", 10000, 50000);
            Resource("Gold", 10000, 50000);
            Resource("Magnesium", 50000, 50000, "", "@powder");
            Resource("Stone", 40000, 20000, "@", "Gravel");
            ResourceOreNameWithoutOre("Ice", 100000);
            ResourceOre("Scrap", 1000, "@Metal");

            // C( component, amount, [alernativ name] )
            Component("Construction", 50000);
            Component("MetalGrid", 15500);
            Component("InteriorPlate", 55000);
            Component("SteelPlate", 300000);
            Component("Girder", 3500);
            Component("SmallTube", 26000);
            Component("LargeTube", 6000);
            Component("Motor", 16000);
            Component("Display", 500);
            Component("BulletproofGlass", 12000);
            Component("Computer", 6500);
            Component("Reactor", 10000);
            Component("Thrust", 16000);
            Component("GravityGenerator", 250, "GravityParts");
            Component("Medical", 120);
            Component("RadioCommunication", 250, "RadioParts");
            Component("Detector", 400);
            Component("Explosives", 500);
            Component("SolarCell", 2800);
            Component("PowerCell", 2800);
            Component("Superconductor", 3000);
            Component("Canvas", 300);
            Component("ZoneChip", 50);

            // W( tool, amounts item 1-4, [alernativ name] )
            Werkzeug("Welder", 2, 2, 2, 2);
            Werkzeug("HandDrill", 2, 2, 2, 2);
            Werkzeug("AngleGrinder", 2, 2, 2, 2, "Grinder");

            // T( guns, amount, [alernativ name] )
            castTypeName = "Weapons";
            Tools("AutomaticRifleItem", 2, "Assault Rifle");
            Tools("RapidFireAutomaticRifleItem", 2, "RapidFire Rifle");
            Tools("PreciseAutomaticRifleItem", 2, "Precise Rifle");
            Tools("UltimateAutomaticRifleItem", 2, "Ultimate Rifle");
            Tools("SemiAutoPistolItem", 2, "SemiAuto Pistol");
            Tools("FullAutoPistolItem", 2, "FullAuto Pistol");
            Tools("ElitePistolItem", 2, "Elite Pistol");
            Tools("BasicHandHeldLauncherItem", 2, "Basic RocketLauncher");
            Tools("AdvancedHandHeldLauncherItem", 2, "Advanced RocketLauncher");
            castTypeName = "";

            // M( ammo, capacity, amount, [alernativ name] )
            Ammo("Missile200mm", 1, 50, "Missile 200mm");
            Ammo("NATO_5p56x45mm", 30, 1000, "Magazine 5.56x45mm");
            Ammo("NATO_25x184mm", 140, 100, "Ammobox 25x184mm");
            Ammo("SemiAutoPistolMagazine", 10, 100);
            Ammo("FullAutoPistolMagazine", 20, 100);
            Ammo("ElitePistolMagazine", 10, 100);
            Ammo("AutomaticRifleGun_Mag_20rd", 20, 100, "AssaultRifleMagazine");
            Ammo("RapidFireAutomaticRifleGun_Mag_50rd", 50, 100, "RapidFireRifleMagazine");
            Ammo("PreciseAutomaticRifleGun_Mag_5rd", 8, 100, "PreciseRifleMagazine");
            Ammo("UltimateAutomaticRifleGun_Mag_30rd", 30, 10, "UltimateRifleMagazine");
            Ammo("AutocannonClip", 16, 100);
            Ammo("LargeCalibreAmmo", 1, 50, "Big Calibre");
            Ammo("MediumCalibreAmmo", 1, 30, "Medium Calibre");
            Ammo("LargeRailgunAmmo", 1, 20, "Big Railgun Spike");
            Ammo("SmallRailgunAmmo", 1, 20, "Small Railgun Spike");

            // miscellaneous
            // addItemType( type, name, amount, [alernativ name] )
            Kits("Powerkit", 5);
            Kits("Medkit", 5);
            addItemType(IG_HBottles, "HydrogenBottle", 5);
            addItemType(IG_OBottles, "OxygenBottle", 5);
            addItemType(IG_Cash, "SpaceCredit", 10000);
            addItemType(IG_Datas, "Datapad", 5);
        }
        void loadModsItemDefs()
        {
            foreach (var mod in usemods.Keys)
            {
                if (usemods[mod])
                {
                    curMod = mod;
                    switch (mod)
                    {
                        case X_M_Daily: dailyNeedsSurvivalMod(); break;
                        case X_M_AziThrust: azimuthThrusterMod(); break;
                        case X_M_SG_Gates: starGateMod_Gates(); break;
                        case X_M_SG_Ores: starGateMod_Ore(); break;
                        case X_M_Paintgun: paintGunMod(); break;
                        case X_M_DeuteriumReactor: deuteriumReactorMod(); break;
                        case X_M_Shield: defenseShieldMod(); break;
                        case X_M_MCRNRailgun: mcrnRailGunMod(); break;
                        case X_M_MWIHoming: MWIHomingWeaponryMod(); break;
                        case X_M_IndustrialOverhaul: IndustrialOverhaulMod(); break;
                        case X_M_IndustrialOverhaulWater: IndustrialOverhaulWaterMod(); break;
                        case X_M_EatDrinkSleepRepeat: EatDrinkSleepRepeatMod(); break;
                        case X_M_PlantAndCook: PlantAndCookMod(); break;
                        case X_M_AryxEpsteinDrive: AryxEpsteinDrive(); break;
                        case X_M_NorthWindWeapons: NorthWindWeapons(); break;
                        case X_M_HSR: HSR_mod(); break;
                    }
                }
            }
        }
        void azimuthThrusterMod()
        {
            Component("AzimuthSupercharger", 1600, "Supercharger");
        }
        void dailyNeedsSurvivalMod()
        {
            ResourceOreNameWithoutOre("Organic", 10000);
            Resource("Carbon", 2000, 500, "", "@");
            Resource("Phosphorus", 2000, 500, "", "@powder");
            Resource("Potassium", 2000, 500, "", "@powder");
            castTypeName = "Food";
            ResourceIngotNameWithoutIngot("ArtificialFood", 100);
            ResourceIngotNameWithoutIngot("SpaceMealBar", 10);
            ResourceIngotNameWithoutIngot("EmergencyFood", 10);
            ResourceIngot("WaterFood", 500, "Water Packets");
            ResourceIngotNameWithoutIngot("Nutrients", 200);
            ResourceIngot("SubFresh", 200, "Algae-Soy Product");
            ResourceIngotNameWithoutIngot("GreyWater", 20);
            ResourceIngotNameWithoutIngot("CleanWater", 100);
            ResourceIngotNameWithoutIngot("LuxuryMeal", 20);
            ResourceIngotNameWithoutIngot("SabiroidSteak", 20);
            ResourceIngot("VeganFood", 20, "Tofu");
            ResourceIngotNameWithoutIngot("WolfSteak", 20);
            ResourceIngotNameWithoutIngot("WolfBouillon", 20);
            ResourceIngotNameWithoutIngot("SabiroidBouillon", 20);
            ResourceIngot("CoffeeFood", 20, "Coffee");
            ResourceIngotNameWithoutIngot("HotChocolate", 20);
            ResourceIngotNameWithoutIngot("Fertilizer", 20);
            ResourceIngotNameWithoutIngot("Potatoes", 20);
            ResourceIngotNameWithoutIngot("PotatoSeeds", 20);
            ResourceIngotNameWithoutIngot("Tomatoes", 20);
            ResourceIngotNameWithoutIngot("TomatoSeeds", 20);
            ResourceIngotNameWithoutIngot("Cucumbers", 20);
            ResourceIngotNameWithoutIngot("CucumberSeeds", 20);
            ResourceIngotNameWithoutIngot("Carrots", 20);
            ResourceIngotNameWithoutIngot("CarrotSeeds", 20);
            ResourceIngotNameWithoutIngot("Ketchup", 20);
            ResourceIngotNameWithoutIngot("MartianSpecial", 20);
            ResourceOreNameWithoutOre("WolfMeat", 10);
            ResourceOreNameWithoutOre("SabiroidMeat", 10);
            ResourceIngotNameWithoutIngot("NotBeefBurger", 20);
            ResourceIngotNameWithoutIngot("ToFurkey", 20);
            ResourceIngotNameWithoutIngot("SpacersBreakfast", 20);
            ResourceIngotNameWithoutIngot("ProteinShake", 20);
            ResourceIngotNameWithoutIngot("ProcessedMeat", 20);
            ResourceIngotNameWithoutIngot("ProcessedSabiroid", 20);
            ResourceIngotNameWithoutIngot("ProcessedEgg", 20);
            ResourceIngotNameWithoutIngot("SabiroidOmelette", 20);
            castTypeName = "";
        }
        void starGateMod_Ore()
        {
            Resource("Trinium", 5000, 20000);
            Resource("Neutronium", 5000, 30000);
            Resource("Naquadah", 5000, 40000);
            Component("Trinium", 500, "@ Plate");
            Component("Neutronium", 500, "@ Crate");
            starGateMod_Gates();
        }
        void starGateMod_Gates()
        {
            Component("Naquadah", 500, "@parts");
        }
        void paintGunMod()
        {
            Tools("PhysicalPaintGun", 5, "PaintGun");
            Ammo("PaintGunMag", 1, 100);
        }
        void deuteriumReactorMod()
        {
            Component("Magnetron_Component", 10000, "MagnetronParts");
            ResourceIngot("DeuteriumContainer", 20000, "Deuteriumparts");
            ResourceOre("Deuterium", 10000);
        }
        void defenseShieldMod()
        {
            Component("ShieldComponent", 1000, "Field Emitter");
        }
        void mcrnRailGunMod()
        {
            Ammo("RailGunAmmoMag", 1, 100, "RailGun Spike");
        }
        void MWIHomingWeaponryMod()
        {
            Ammo("TorpedoMk1", 1, 100);
            Ammo("SwarmMissile50mm", 12, 100);
            Ammo("DestroyerMissileX", 8, 100);
            Ammo("DestroyerMissileMk1", 2, 100);
        }
        void IndustrialOverhaulMod()
        {
            ResourceOre("Heat", 100);
            ResourceOre("Coal", 100);
            Resource("Sulfur", 100, 100);
            Resource("Copper", 100, 100);
            Resource("Lithium", 100, 100);
            ResourceOre("Bauxite", 100);
            Resource("Titanium", 100, 100);
            Resource("Tantalum", 100, 100);
            Resource("Niter", 100, 100);
            ResourceIngot("Polymer", 100);
            ResourceOre("Aluminum", 100);
            ResourceIngot("Carbon", 100);
            Resource("Uranium", 100, 100);
            ResourceIngot("NuclearWaste", 100);
            ResourceIngot("DepletedUranium", 100);

            castTypeName = "Crushed";
            ResourceOreNameWithoutOre("CrushedSulfur", 100);
            ResourceOreNameWithoutOre("CrushedCopper", 100);
            ResourceOreNameWithoutOre("CrushedLithium", 100);
            ResourceOreNameWithoutOre("CrushedBauxite", 100);
            ResourceOreNameWithoutOre("CrushedTitanium", 100);
            ResourceOreNameWithoutOre("CrushedTantalum", 100);
            ResourceOreNameWithoutOre("CrushedIron", 100);
            ResourceOreNameWithoutOre("CrushedNickel", 100);
            ResourceOreNameWithoutOre("CrushedCobalt", 100);
            ResourceOreNameWithoutOre("CrushedSilicon", 100);
            ResourceOreNameWithoutOre("CrushedSilver", 100);
            ResourceOreNameWithoutOre("CrushedGold", 100);
            ResourceOreNameWithoutOre("CrushedPlatinum", 100);
            ResourceOreNameWithoutOre("CrushedUranium", 100);

            castTypeName = "Purified";
            ResourceOreNameWithoutOre("PurifiedSulfur", 100);
            ResourceOreNameWithoutOre("PurifiedCopper", 100);
            ResourceOreNameWithoutOre("PurifiedLithium", 100);
            ResourceOreNameWithoutOre("PurifiedBauxite", 100);
            ResourceOreNameWithoutOre("PurifiedTitanium", 100);
            ResourceOreNameWithoutOre("PurifiedTantalum", 100);
            ResourceOreNameWithoutOre("PurifiedNiter", 100);
            ResourceOreNameWithoutOre("PurifiedIron", 100);
            ResourceOreNameWithoutOre("PurifiedNickel", 100);
            ResourceOreNameWithoutOre("PurifiedCobalt", 100);
            ResourceOreNameWithoutOre("PurifiedSilicon", 100);
            ResourceOreNameWithoutOre("PurifiedSilver", 100);
            ResourceOreNameWithoutOre("PurifiedGold", 100);
            ResourceOreNameWithoutOre("PurifiedPlatinum", 100);
            ResourceOreNameWithoutOre("PurifiedUranium", 100);

            castTypeName = "Oil";
            ResourceOreNameWithoutOre("Oil Sand", 100);
            ResourceOreNameWithoutOre("CrudeOil", 100);
            ResourceIngotNameWithoutIngot("FuelOil", 100);
            ResourceIngotNameWithoutIngot("SpentFuel", 100);
            castTypeName = "";

            Component("FSSolarCell", 100, "Full-Spectrum Solar Cell");
            Component("ElectronMatrix", 100, "Electron-Matrix Cell");
            Component("Capacitor", 100, "Capacitor Cell");
            Component("CompositeArmor", 100, "Composite Plate");
            Component("TokamakBlanket", 100, "Tokamak Blanket Plate");
            Component("SuperMagnet", 100);
            Component("Cryocooler", 100);
            Component("Thermocouple", 100);
            Component("ArmorGlass", 100);
            Component("CopperWire", 100);
            Component("GoldWire", 100);
            Component("Fabric", 100, "Synthetic Fabric");
            Component("Rubber", 100);
            Component("Plastic", 100);
            Component("AdvancedComputer", 100);
            Component("QuantumComputer", 100);
            Component("Concrete", 100);
            Component("Asphalt", 100);
            Component("TitaniumPlate", 100);
            Component("ArmoredPlate", 100);
            Component("Electromagnet", 100);
            Component("Lightbulb", 100);
            Component("AcidPowerCell", 100);
            Component("AlkalinePowerCell", 100);
            Component("Ceramic", 100);
            Component("LaserEmitter", 100);
            Component("HeatingElement", 100);
            Component("BulletproofGlass", 100, "Glass");
            Component("InteriorPlate", 100, "Aluminum Plate");
            Component("Computer", 100, "Basic Computer");
            Component("Detector", 100, "Sensor Cluster");
            Component("PowerCell", 100, "Lithium Power Cell");

            Ammo("DUNATO_25x184mm", 140, 10);
            Ammo("InteriorTurret_Mag_50rd", 50, 10);
            Ammo("AutocannonClipDUAP", 16, 10);
            Ammo("LargeCalibreAmmoHE", 1, 10);
            Ammo("LargeCalibreAmmoDUAP", 1, 10);
            Ammo("MediumCalibreAmmoHE", 1, 10);
            Ammo("MediumCalibreAmmoDUAP", 1, 10);
            Ammo("LargeRailgunAmmoDUAP", 1, 10);
            Ammo("SmallRailgunAmmoDUAP", 1, 10);
        }
        void IndustrialOverhaulWaterMod()
        {
            Component("Foam", 100);
            Component("BuoyancyTube", 100);
        }
        void EatDrinkSleepRepeatMod()
        {
            castTypeName = "Food";
            Kits("ClangCola", 5);
            Kits("CosmicCoffee", 5);
            Kits("SparklingWater", 5);
            Kits("InterBeer", 5);
            Kits("LaysChips", 5);
            Kits("PrlnglesChips", 5, "PringlesChips");
            Kits("Emergency_Ration", 5, "Emergency Ration");
            castTypeName = "";
        }
        void PlantAndCookMod()
        {
            Component("EmptyTinCan", 100);
            ResourceOre("Organic", 100);
            castTypeName = "Food";
            ResourceIngotNameWithoutIngot("Wheat", 100);
            ResourceIngotNameWithoutIngot("Pumpkin", 100);
            ResourceIngotNameWithoutIngot("Cabbage", 100);
            ResourceIngotNameWithoutIngot("Soya", 100);
            ResourceIngotNameWithoutIngot("Herbs", 100);
            Kits("EuropaTea", 5);
            Kits("Apple", 5);
            Kits("AppleJuice", 5);
            Kits("ApplePie", 5);
            Kits("Bread", 5);
            Kits("Burger", 5);
            Kits("Meat", 5);
            Kits("MeatRoasted", 5);
            Kits("Soup", 5);
            Kits("MushroomSoup", 5);
            Kits("TofuSoup", 5);
            Kits("Tofu", 5);
            Kits("Mushrooms", 5);
            Kits("ShroomSteak", 5);
            castTypeName = "";
        }
        void AryxEpsteinDrive()
        {
            Component("AryxLynxon_FusionComponent", 100, "Fusion Coil");
        }
        void NorthWindWeapons()
        {
            Component("C100ShellCasing", 100, "100mm Casing Scrap");
            Ammo("R75ammo", 1, 10);
            Ammo("R150ammo", 1, 10);
            Ammo("R250ammo", 1, 10);
            Ammo("H203Ammo", 1, 10);
            Ammo("H203AmmoAP", 1, 10);
            Ammo("C30Ammo", 120, 10);
            Ammo("C30DUammo", 120, 10);
            Ammo("CRAM30mmAmmo", 120, 10);
            Ammo("C100mmAmmo", 1, 10);
            Ammo("C300AmmoAP", 3, 10);
            Ammo("C300AmmoHE", 3, 10);
            Ammo("C300AmmoG", 3, 10);
            Ammo("C400AmmoAP", 2, 10);
            Ammo("C400AmmoHE", 2, 10);
            Ammo("C400AmmoCluster", 2, 10);
            Ammo("C500AmmoAP", 3, 10);
            Ammo("C500AmmoHE", 3, 10);
            Ammo("C500AmmoCasaba", 3, 10);
            Ammo("PlasmaCell10MJ", 1, 10);
        }

        void HSR_mod()
        {
            Resource("K_HSR_Nanites_Sludge", 100, 100);
            ResourceIngot("K_HSR_Nanites_EnergizedGel", 100);
            ResourceIngot("K_HSR_Nanites_Hexagol", 100);
            ResourceIngot("K_HSR_Nanites_Chromium", 100);
            ResourceIngot("K_HSR_Nanites_IX", 100);
            ResourceIngot("K_HSR_Nanites_KEL", 100);
            ResourceIngot("K_HSR_Nanites_Cerebi", 100);
            Component("K_HSR_RailComponents", 100);
            Component("K_HSR_AssemblerSystem", 100, "Hybrid System Stabilizer");
            Component("K_HSR_Globe", 100, "Fusion-Sphere");
            Component("K_HSR_PulseSystem", 100);
            Component("K_HSR_GelConduit", 100);
            Component("K_HSR_HexagolPlating", 100, "Hexagol Plate");
            Component("K_HSR_RailComponents_II", 100, "Hexagol Rail-Components");
            Component("K_HSR_Globe_II", 100, "Chromium-Sphere");
            Component("K_HSR_RailComponents_III", 100, "Chromium Rail-Components");
            Component("K_HSR_Globe_III", 100, "IX-Sphere");
            Component("K_HSR_HexagolPlating_II", 100, "Hexagolite Plate");
            Component("K_HSR_Mainframe_K3", 100);
            Component("K_HSR_HyperConductiveCircuitry", 100, "Hyper-Conductive Circuitry");
            Ammo("K_HSR_Slug", 1, 10);
            Ammo("K_HSR_Seeker", 1, 10);
        }

        static Dictionary<string, Dictionary<MyItemType, int>> modItemDefsList = new Dictionary<string, Dictionary<MyItemType, int>>();
        static Dictionary<MyItemType, string> itemAlterNameList = new Dictionary<MyItemType, string>();
        static bool writeNewItemDefs = false;
        static string curMod = "Vanilla";
        smsItemType addItemType(string typeName, string subtypeName, int maximumAmount, string alternativerName = "")
        {
            var newItemType = new MyItemType(IG_ + typeName, subtypeName);
            if (!smsItemTypeDefinitions.ContainsKey(newItemType))
            {
                if (!modItemDefsList.ContainsKey(curMod))
                {
                    modItemDefsList.Add(curMod, new Dictionary<MyItemType, int>());
                }
                if (modItemDefsList[curMod].ContainsKey(newItemType))
                    maximumAmount = modItemDefsList[curMod][newItemType];
                else
                {
                    writeNewItemDefs = true;
                    modItemDefsList[curMod].Add(newItemType, maximumAmount);
                }
                if (itemAlterNameList.ContainsKey(newItemType) && itemAlterNameList[newItemType] != "") alternativerName = itemAlterNameList[newItemType];
                // name ummoddeln
                if (alternativerName == "" && subtypeName.StartsWith("K_HSR_")) alternativerName = subtypeName.Substring(6);
                var newSmsItemType = new smsItemType(newItemType, maximumAmount, alternativerName, castTypeName);
                smsItemTypeDefinitions.Add(newItemType, newSmsItemType);
                return newSmsItemType;
            }
            else
            {
                smsItemTypeDefinitions[newItemType].InitMaximumAmount(maximumAmount);
                smsItemTypeDefinitions[newItemType].SetName(alternativerName);
            }
            return null;
        }
        void Component(string s, int d, string x = "") { addItemType("Component", s, d, x); }
        void ResourceOre(string s, int o, string so = "") { addItemType("Ore", s, o, so); }
        void ResourceOreNameWithoutOre(string s, int o) { ResourceOre(s, o, "@"); }
        void ResourceIngot(string s, int i, string si = "") { addItemType("Ingot", s, i, si); }
        void ResourceIngotNameWithoutIngot(string s, int i) { ResourceIngot(s, i, "@"); }
        void Resource(string s, int o, int i = -1, string so = "", string si = "") { if (o != -1) ResourceOre(s, o, so); if (i != -1) ResourceIngot(s, i, si); }
        void Werkzeug(string s, int i, int i2, int i3, int i4, string x = "") { if (x == "") x = s; addItemType(IG_Tools, s + "Item", i, x); addItemType(IG_Tools, s + "2Item", i2, x + " MK2"); addItemType(IG_Tools, s + "3Item", i3, x + " MK3"); addItemType(IG_Tools, s + "4Item", i4, x + " MK4"); }
        void Tools(string s, int d, string x = "") { addItemType(IG_Tools, s, d, x); }
        void Kits(string s, int d, string x = "") { addItemType(IG_Kits, s, d, x); }
        void Ammo(string s, int capa, int d, string x = "") { var sit = addItemType(IG_Ammo, s, d, x); if (sit != null) sit.ammocapacity = capa; }
        //SMS ItemType
        public class smsItemType
        {
            public MyItemType type;
            public MyItemInfo info;
            public string name, typeString, subtypeString, blueprintName;
            public int max, imax, pmax = 0, ammocapacity = 0;
            public float ass = 0;
            public smsItemType(MyItemType _type, int amount = -1, string alternativname = "", string s = "")
            {
                type = _type;
                info = type.GetItemInfo();
                subtypeString = type.SubtypeId;
                typeString = s == "" ? type.TypeId.Substring(type.TypeId.IndexOf('_') + 1) : s;
                SetName(alternativname);
                InitMaximumAmount(amount);
            }
            public void InitMaximumAmount(int initMax)
            {
                if (initMax == -1) imax = IfItemtypeOreOrIngot() ? 1000 : 10;
                else imax = initMax;
                max = imax;
            }
            public void SetMaximumAmount(float m)
            {
                pmax = (int)m;
                max = pmax;
            }
            public void SetName(string displayName)
            {
                if (type == IG_Magnesium && usemods[X_M_IndustrialOverhaul]) name = "Gunpowder";
                else if (displayName != "")
                    foreach (var c in displayName)
                        if (c == '@') name += type.SubtypeId;
                        else if (c == '_') name += ' ';
                        else name += c;
                else
                {
                    name = type.SubtypeId;
                    if (info.IsOre) name += "ore";
                    else if (info.IsIngot) name += "ingot";
                }
            }
            public bool IfItemtypeOreOrIngot() { return info.IsOre || info.IsIngot; }
            public double GetAmount(IMyCubeGrid g = null) { var d = GetInventoryDictionaryByGrid(g); return d.GetValueOrDefault(type, 0); }
            public double GetMass(double a) { return a * info.Mass; }
            public double GetVolume(double a) { return a * info.Volume * 1000; }
            public bool IsItemMasstype() { return info.Mass == 1; }
            public bool IsItemVolumetype() { return info.Volume == 0.001f; }
            public bool IsItemResource() { return (typeString == "Ore" || typeString == "Ingot") && (subtypeString != "Ice" && subtypeString != "Organic" && !subtypeString.StartsWith("K_HSR_")); }
            public string GetResourceName() { return subtypeString.Contains("Deuterium") ? "Deuterium" : subtypeString; }
        }
    }
}
