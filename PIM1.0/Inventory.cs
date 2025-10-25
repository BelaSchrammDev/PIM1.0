using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        static string[] seed_cast = new string[]
        {
            IG_S + "Fruit",
            IG_S + "Grain",
            IG_S + "Vegetables",
            IG_S + "Mushrooms",
        };

        static string[] waste_cast = new string[]
        {
                Ore.Organic,
                Ingot.GreyWater
        };

        static string[] food_cast = new string[]
        {
            // Vanilla
            IG_K + "MammalMeatCooked",
            IG_K + "InsectMeatCooked",
            IG_K + "MealPack_KelpCrisp",
            IG_K + "MealPack_FruitBar",
            IG_K + "MealPack_GardenSlaw",
            IG_K + "MealPack_RedPellets",
            IG_K + "MealPack_Chili",
            IG_K + "MealPack_Flatbread",
            IG_K + "MealPack_Ramen",
            IG_K + "MealPack_FruitPastry",
            IG_K + "MealPack_VeggieBurger",
            IG_K + "MealPack_Curry",
            IG_K + "MealPack_GreenPellets",
            IG_K + "MealPack_Dumplings",
            IG_K + "MealPack_Spaghetti",
            IG_K + "MealPack_Lasagna",
            IG_K + "MealPack_Burrito",
            IG_K + "MealPack_FrontierStew",
            IG_K + "MealPack_SearedSabiroid",
            IG_K + "MealPack_SteakDinner",
            IG_K + "Fruit",
            IG_P + "Grain",
            IG_K + "Vegetables",
            IG_K + "Mushrooms",

            // Daily Needs Survival
            Ingot.WaterFood,
            Ingot.CleanWater,
            Ingot.SubFresh,
            Ingot.Nutrients,
            IG_Ingot + "ArtificialFood",
            IG_Ingot + "LuxuryMeal",
            IG_Ingot + "SabiroidSteak",
            IG_Ingot + "VeganFood",
            IG_Ingot + "WolfSteak",
            IG_Ingot + "WolfBouillon",
            IG_Ingot + "SabiroidBouillon",
            IG_Ingot + "CoffeeFood",
            IG_Ingot + "Potatoes",
            IG_Ingot + "Tomatoes",
            IG_Ingot + "Carrots",
            IG_Ingot + "Cucumbers",
            IG_Ingot + "PotatoSeeds",
            IG_Ingot + "TomatoSeeds",
            IG_Ingot + "CarrotSeeds",
            IG_Ingot + "CucumberSeeds",
            IG_Ingot + "Ketchup",
            IG_Ingot + "MartianSpecial",
            "Ore WolfMeat",
            "Ore SabiroidMeat",
            IG_Ingot + "Fertilizer",
            IG_Ingot + "NotBeefBurger",
            IG_Ingot + "ToFurkey",
            IG_Ingot + "SpaceMealBar",
            IG_Ingot + "HotChocolate",
            IG_Ingot + "SpacersBreakfast",
            IG_Ingot + "ProteinShake",
            IG_Ingot + "EmergencyFood",
            // Eat, Drink, Sleep & Repeat
            IG_K + "SparklingWater",
            IG_K + "Emergency_Ration",
            IG_K + "AppleJuice",
            IG_K + "ApplePie",
            IG_K + "Tofu",
            IG_K + "MeatRoasted",
            IG_K + "ShroomSteak",
            IG_K + "Bread",
            IG_K + "Burger",
            IG_K + "Soup",
            IG_K + "MushroomSoup",
            IG_K + "TofuSoup",
            IG_K + "EuropaTea",
            IG_K + "Mushrooms",
            IG_K + "Apple",
            IG_K + "PrlnglesChips",
            IG_K + "LaysChips",
            IG_K + "InterBeer",
            IG_K + "CosmicCoffee",
            IG_K + "ClangCola",
            IG_K + "Meat",
            IG_K + "MeatRoasted",
            IG_Ingot + "Soya",
            IG_Ingot + "Herbs",
            IG_Ingot + "Wheat",
            IG_Ingot + "Pumpkin",
            IG_Ingot + "Cabbage",
        };

        static string TypeCast(string t)
        {
            if (t.Contains("RifleItem") || t.Contains(IG_Ammo) || t.Contains("PistolItem") || t.Contains("LauncherItem")) return "Armory";
            if (seed_cast.Contains(t)) return "Seeds";
            if (food_cast.Contains(t)) return "Food";
            if (waste_cast.Contains(t)) return "Waste";
            return "";
        }

        // ----------------------------------------------------------------------------------
        static Dictionary<string, string> IngameToPIM = new Dictionary<string, string>
        {
            { "Ammo",  IG_Ammo },
            { Resources.RStone,  Ore.Stone},
            { "Gravel",  Ingot.Stone},
            { "Tools",  IG_Tools},
            { "Kits",  IG_Kits},
            { "Cash",  IG_Phys},
            { "Datapads",  IG_Datas},
            { "H-Bottles",  IG_HBottles},
            { "O-Bottles",  IG_OBottles},
            { "Ice",  Ore.Ice},
            { "Water",  Ingot.WaterFood},
            { "Greywater",  Ingot.GreyWater},
            { "Deuterium",  Ingot.DeuteriumContainer},
            { "Organic", Ore.Organic },
            { "Seeds", IG_Seeds }
        };

        static string Ingame2Tag(string ingame)
        {
            foreach (var x in IngameToPIM) if (x.Value == ingame) return x.Key;
            return ingame;
        }

        string Tag2Ingame(string ststr)
        {
            if (ststr.Contains("Dock")) return "";
            if (IngameToPIM.ContainsKey(ststr)) return IngameToPIM[ststr];
            switch (ststr)
            {
                case "Steelplate": return IG_Com + "SteelPlate";
                case "Metalgrid": return IG_Com + "MetalGrid";
                case "Interiorplate": return IG_Com + "InteriorPlate";
                case "Smalltube": return IG_Com + "SmallTube";
                case "Largetube": return IG_Com + "LargeTube";
                case "Glass": return IG_Com + "BulletproofGlass";
                case "Gravity": return IG_Com + "GravityGenerator";
                case "Radio": return IG_Com + "RadioCommunication";
                case "Solar": return IG_Com + "SolarCell";
                case "Power": return IG_Com + "PowerCell";
                case "Zonechip": return IG_Com + "ZoneChip";
                case "Reactor":
                case "Thrust":
                case "Medical":
                case "Detector":
                case "Explosives":
                case "Construction":
                case "Motor":
                case "Display":
                case "Girder":
                case "Computer":
                case "Canvas": return IG_Com + ststr;
                default: return ststr;
            }
        }

        static void ClearInventory(IMyInventory quelle, List<string> typeID_l = null)
        {
            var von = new List<MyInventoryItem>();
            quelle.GetItems(von);
            if (von.Count() == 0) return;
            for (int j = von.Count() - 1; j >= 0; j--)
            {
                bool success = false;
                var vcon = von[j].Type;
                var clr = true;
                if (typeID_l != null)
                {
                    clr = false;
                    foreach (var nt in typeID_l)
                        if (vcon.TypeId.ToString() == nt)
                        {
                            clr = true;
                            break;
                        }
                }
                if (!clr) continue;
                var idstr = vcon.TypeId.ToString().Split('_')[1];
                var stype = vcon.SubtypeId.ToString();
                var fullid = idstr + " " + stype;
                var atype = TypeCast(fullid);
                if (Lists.Data.InventoryManagerList.ContainsKey(fullid)) success = SendItemByNum(quelle, j, Lists.Data.InventoryManagerList[fullid]);
                if (!success && atype != "" && Lists.Data.InventoryManagerList.ContainsKey(atype)) success = SendItemByNum(quelle, j, Lists.Data.InventoryManagerList[atype]);
                if (!success && Lists.Data.InventoryManagerList.ContainsKey(idstr)) SendItemByNum(quelle, j, Lists.Data.InventoryManagerList[idstr]);

                var idstrPIM = Ingame2Tag(idstr);
                if (Lists.Data.InventoryManagerList.ContainsKey(idstr)) ClearWarning(Warning.ID.CARGOMISSING, idstrPIM);
                else SetWarning(Warning.ID.CARGOMISSING, idstrPIM);
            }
        }

        static bool clearItemByType(IMyInventory quelle, string type, MyInventoryItem item, float amount = 0)
        {
            var typeID = type.Substring(0, type.IndexOf(' '));
            var stypeID = type.Substring(type.IndexOf(' ') + 1);
            var atype = TypeCast(type);
            var trans = false;
            if (amount == 0) amount = (float)item.Amount;
            if (Lists.Data.InventoryManagerList.ContainsKey(type)) trans = SendItemByIItem(quelle, item, amount, Lists.Data.InventoryManagerList[type]);
            else if (atype != "" && Lists.Data.InventoryManagerList.ContainsKey(atype)) trans = SendItemByIItem(quelle, item, amount, Lists.Data.InventoryManagerList[atype]);
            else if (Lists.Data.InventoryManagerList.ContainsKey(typeID)) trans = SendItemByIItem(quelle, item, amount, Lists.Data.InventoryManagerList[typeID]);
            return trans;
        }
        static bool SendItemByIItem(IMyInventory quelle, MyInventoryItem item, float amount, List<IMyInventory> ziele)
        {
            var volume = (MyFixedPoint)amount * item.Type.GetItemInfo().Volume;
            if (ziele.Count > 0)
            {
                foreach (var zinv in ziele) if (quelle == zinv) return true;
                for (int i = 0; i < ziele.Count; i++)
                {
                    var ziel_inv = ziele[i];
                    if (volume < (ziel_inv.MaxVolume - ziel_inv.CurrentVolume))
                    {
                        if (quelle.TransferItemTo(ziel_inv, item, (MyFixedPoint)amount)) return true;
                    }
                }
            }
            return false;
        }
        static bool SendItemByNum(IMyInventory quelle, int itemnum, List<IMyInventory> ziele)
        {
            var trans = false;
            if (ziele.Count != 0)
            {
                foreach (var zinv in ziele) if (quelle == zinv) return true;
                for (int i = 0; i < ziele.Count; i++)
                {
                    var ziel_inv = ziele[i];
                    if (!ziel_inv.IsFull)
                    {
                        trans = quelle.TransferItemTo(ziel_inv, itemnum, null, true, null);
                    }
                }
            }
            return trans;
        }
        static bool SendItemByType(string iType, float itemAmount, IMyInventory ziel, int? p = null)
        {
            return SendItemByTypeAndSubtype(IG_ + iType.Substring(0, iType.IndexOf(' ')), iType.Substring(iType.IndexOf(' ') + 1), itemAmount, ziel);
        }
        static bool SendItemByTypeAndSubtype(string itemType, string itemSubType, float itemAmount, IMyInventory ziel, int? p = null)
        {
            List<IMyInventory> quellen = null;
            var idstr = itemType.Split('_')[1];
            var idstrPIM = Ingame2Tag(idstr);
            var atype = TypeCast(idstr[1] + " " + itemSubType);
            if (Lists.Data.InventoryManagerList.ContainsKey(idstr[1] + " " + itemSubType)) quellen = Lists.Data.InventoryManagerList[idstr[1] + " " + itemSubType];
            else if (atype != "" && Lists.Data.InventoryManagerList.ContainsKey(atype)) quellen = Lists.Data.InventoryManagerList[atype];
            else if (Lists.Data.InventoryManagerList.ContainsKey(idstr)) quellen = Lists.Data.InventoryManagerList[idstr];
            else
            {
                SetWarning(Warning.ID.CARGOMISSING, idstrPIM);
                return false;
            }
            ClearWarning(Warning.ID.CARGOMISSING, idstrPIM);
            for (int i = 0; i < quellen.Count; i++)
            {
                var von = new List<MyInventoryItem>();
                quellen[i].GetItems(von);
                if (von.Count() > 0)
                {
                    for (int j = von.Count() - 1; j >= 0; j--)
                    {
                        if (von[j].Type.TypeId.ToString() == itemType)
                        {
                            if (von[j].Type.SubtypeId.ToString() == itemSubType)
                            {
                                var menge = (MyFixedPoint)itemAmount;
                                if (quellen[i].TransferItemTo(ziel, j, p, true, menge)) return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        static void ClearInventoryList(Dictionary<string, float> invList)
        {
            var keys = invList.Keys.ToArray();
            for (int i = 0; i < keys.Length; i++) invList[keys[i]] = 0;
        }

        static void CountItemsToSummaryDictionary(IMyInventory box, Dictionary<string, float> ilist = null)
        {
            var boxl = new List<MyInventoryItem>();
            box.GetItems(boxl);
            foreach (var boxi in boxl)
            {
                string index = GetPIMItemID(boxi.Type);
                var boxia = (float)boxi.Amount;
                if (inventar.ContainsKey(index)) inventar[index] += boxia;
                else inventar.Add(index, boxia);
                if (ilist != null)
                {
                    if (ilist.ContainsKey(index)) ilist[index] += boxia;
                    else ilist.Add(index, boxia);
                }
            }
        }

        static void CountItemsToDictionary(IMyInventory box, Dictionary<string, float> ilist = null)
        {
            var boxl = new List<MyInventoryItem>();
            box.GetItems(boxl);
            foreach (var boxi in boxl)
            {
                string index = GetPIMItemID(boxi.Type);
                var boxia = (float)boxi.Amount;
                if (ilist != null)
                {
                    if (ilist.ContainsKey(index)) ilist[index] += boxia;
                    else ilist.Add(index, boxia);
                }
            }
        }

        void AddInventoryToInventoryManagerList(IMyInventory inv, Dictionary<string, string> tags)
        {
            foreach (var tag in tags.Keys)
            {
                var ingame = Tag2Ingame(tag);
                if (ingame != "")
                {
                    if (!Lists.Data.InventoryManagerList.ContainsKey(ingame)) Lists.Data.InventoryManagerList.Add(ingame, new List<IMyInventory>());
                    if (!Lists.Data.InventoryManagerList[ingame].Contains(inv)) Lists.Data.InventoryManagerList[ingame].Add(inv);
                    if (!Lists.Data.CargoUseList.ContainsKey(tag)) Lists.Data.CargoUseList.Add(tag, new CargoUse(tag));
                    Lists.Data.CargoUseList[tag].AddCurrentAndMaxCargocapacity(inv.CurrentVolume.RawValue / 1000, inv.MaxVolume.RawValue / 1000);
                }
            }
        }
    }
}
