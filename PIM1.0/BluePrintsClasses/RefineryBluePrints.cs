using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using VRage.Game;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        static Dictionary<string, string> ResourcesNameCastList = new Dictionary<string, string>
            {
                { Ore.Stone, Resources.RStone },
                { Ingot.Magnesium, Ingot.Magnesiumpowder },
                { Ingot.Stone, "Gravel" },
                { Ingot.DeuteriumContainer, Resources.RDeuterium },
                { Ore.Ice, "Ice"},
                { Ingot.Carbon, Resources.RCarbon },
            };
        static Dictionary<string, string> ResourcesNameCastListIOMod = new Dictionary<string, string>
            {
                { Ore.Coal, Resources.RCoal },
                { Ore.Bauxite, Resources.RBauxite },
                { Ore.Niter, Resources.RNiter },
                { Ingot.Lithium, Resources.RLithium + " Paste" },
                { Ingot.Sulfur, Resources.RSulfur },
                { Ingot.Niter, Resources.RPotassium + " Nitrate" },
                { Ore.Magnesium, "Crushed Niter" },
                { Ingot.Magnesium, Ingot.Gunpowder},
            };

        static string GetTimeStringFromHours(double h)
        {
            if (h < 1)
            {
                if ((h * 60) > 1) return Math.Round(h * 60, 0) + " min.";
                else return Math.Round(h * 60 * 60, 0) + " s";
            }
            if (h < 24)
            {
                return Math.Round(h, 1) + " h";
            }
            double tage = Math.Round(h / 24, 1);
            if (tage > 365)
            {
                return Math.Round(tage / 365, 1) + " years";
            }
            if (tage < 1.1) return tage + " day";
            else return tage + " days";
        }

        // to be removed ---------------------------------------------------------------------------------
        void addPrio(string reftype, RefineryBlueprint type, int prio)
        {
            if (prio == 0) prio = 1;
            if (ingotprio.ContainsKey(reftype))
            {
                IPrio.GetBlueprintPrio(ingotprio[reftype], type, true).setPrio(prio);
            }
        }

        static string CastResourceName(string name)
        {
            if (Config.Instance.usedMods[Strings.M_IndustrialOverhaulMod] && ResourcesNameCastListIOMod.ContainsKey(name)) return ResourcesNameCastListIOMod[name];
            if (ResourcesNameCastList.ContainsKey(name)) return ResourcesNameCastList[name];
            if (name.StartsWith("Ore Crushed")) return "Crushed " + name.Substring(11);
            if (name.StartsWith("Ore Purified")) return "Purified " + name.Substring(12);
            return name;
        }

        static Dictionary<string, List<IPrio>> ingotprio = new Dictionary<string, List<IPrio>>();
        //Ip
        public class IPrio : IComparable<IPrio>
        {
            public RefineryBlueprint refineryBP;
            public int prio = 0;
            public int initp = 0;
            public int CompareTo(IPrio other)
            {
                if (other.initp == initp) return 0;
                return other.initp > initp ? 1 : -1;
            }
            public IPrio(RefineryBlueprint bluePrint, int pr = 0)
            {
                refineryBP = bluePrint;
                prio = pr;
                initp = pr;
            }
            public void setPrio(int np)
            {
                if (np > 10000) np = 10000;
                prio = np;
                if (np == 0 || initp != np) initp = np;
            }
            public static IPrio GetBlueprintPrio(List<IPrio> ingotprioList, RefineryBlueprint bp, bool newip = false)
            {
                foreach (IPrio ip in ingotprioList)
                {
                    if (ip.refineryBP == bp) return ip;
                }
                if (newip)
                {
                    IPrio np = new IPrio(bp);
                    ingotprioList.Add(np);
                    return np;
                }
                else return null;
            }
        }
        // to be removed # end  ---------------------------------------------------------------------------------


        static List<RefineryBlueprint> RefineryBlueprints = new List<RefineryBlueprint>();
        static Dictionary<string, Dictionary<RefineryBlueprint, int>> OrePrioConfig = new Dictionary<string, Dictionary<RefineryBlueprint, int>>();


        static void AddRefineryBlueprint(string bpName, string inputItem, string outputItem)
        {
            MyDefinitionId id;
            if (!MyDefinitionId.TryParse("MyObjectBuilder_BlueprintDefinition/" + bpName, out id)) return;
            if (RefineryBlueprints.Find(b => b.Definition_id == id) == null)
            {
                RefineryBlueprints.Add(new RefineryBlueprint(id, inputItem, outputItem));
            }
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


        void InitRefineryBlueprints()
        {

            RefineryBlueprint.InitScrapTypeBlueprintTypes();

            // Ingots
            AddRefineryBlueprint("StoneOreToIngotBasic", Ore.Stone, Ingot.Stone);
            AddRefineryBlueprint("ScrapToIronIngot", Ore.Scrap, Ingot.Iron);
            AddRefineryBlueprint("ScrapIngotToIronIngot", Ingot.Scrap, Ingot.Iron);
            AddRefineryBlueprintOreToIngot(Resources.RGold);
            AddRefineryBlueprintOreToIngot(Resources.RPlatinum);
            AddRefineryBlueprintOreToIngot(Resources.RStone);
            AddRefineryBlueprintOreToIngot(Resources.RSilver);
            AddRefineryBlueprintOreToIngot(Resources.RIron);
            AddRefineryBlueprintOreToIngot(Resources.RNickel);
            AddRefineryBlueprintOreToIngot(Resources.RCobalt);
            AddRefineryBlueprintOreToIngot(Resources.RSilicon);
            AddRefineryBlueprintOreToIngot(Resources.RUranium);

            if (Config.Instance.usedMods[Strings.M_SigmaDraconisCore])
            {
                // Ingots
                AddRefineryBlueprint("TungstenToIngot", "Ore Tungsten", "Ingot TungstenIngot");
                AddRefineryBlueprint("CopperToIngot", "Ore Copper", "Ingot CopperIngot");
                AddRefineryBlueprint("LeadToIngot", "Ore Lead", "Ingot LeadIngot");
                AddRefineryBlueprint("TitaniumToIngot", "Ore Titanium", "Ingot TitaniumIngot");
                AddRefineryBlueprint("GraphiteOreToIngot", "Ore Graphite", "Ingot Carbon");
                // AdminIngots
                AddRefineryBlueprint("LithiumToIngot", "Ore Lithium", "Ingot LithiumIngot");
                // CommonMetals
                AddRefineryBlueprint("TungstenToIngot", "Ore Tungsten", "Ingot TungstenIngot");
                AddRefineryBlueprint("CopperToIngot", "Ore Copper", "Ingot CopperIngot");
                AddRefineryBlueprint("LeadToIngot", "Ore Lead", "Ingot LeadIngot");
                AddRefineryBlueprint("GraphiteOreToIngot", "Ore Graphite", "Ingot Carbon");
            }

            if (Config.Instance.usedMods[Strings.M_DeuteriumReactor])
            {
                AddRefineryBlueprint("StonetoDeuterium", Ore.Stone, Ingot.DeuteriumContainer);
                AddRefineryBlueprint("IcetoDeuterium", Ore.Ice, Ingot.DeuteriumContainer);
                AddRefineryBlueprint("DeuteriumOreToIngot", Ore.Deuterium, Ingot.DeuteriumContainer);
            }


            if (Config.Instance.usedMods[Strings.M_DailyNeedsSurvival])
            {
                AddRefineryBlueprintOreToIngot(Resources.RCarbon);
                AddRefineryBlueprintOreToIngot(Resources.RPotassium);
                AddRefineryBlueprintOreToIngot(Resources.RPhosphorus);
            }


            if (Config.Instance.usedMods[Strings.M_SG_Ores])
            {
                AddRefineryBlueprintOreToIngot(Resources.RNaquadah);
                AddRefineryBlueprintOreToIngot(Resources.RTrinium);
                AddRefineryBlueprintOreToIngot(Resources.RNeutronium);
            }


            if (!Config.Instance.usedMods[Strings.M_IndustrialOverhaulMod])
            {
                AddRefineryBlueprintOreToIngot(Resources.RMagnesium);
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
            RefineryBlueprints.Sort((x, y) => x.InputIDName.CompareTo(y.InputIDName));
        }


        public class RefineryBlueprint
        {
            static DateTime FillTimestamp = DateTime.Now;
            static string[] ScrapTypeBlueprintNames = { "Component C100ShellCasing", Ore.Scrap, Ingot.Scrap };
            static MyItemType[] ScrapItemTypes;
            static Dictionary<MyItemType, RefineryBlueprint> KnowScrapTypes = new Dictionary<MyItemType, RefineryBlueprint>();


            public static void FillInputOutputAmountAndETA()
            {
                if ((DateTime.Now - FillTimestamp).TotalSeconds > 12)
                {
                    FillTimestamp = DateTime.Now;
                    foreach (var bluePrint in RefineryBlueprints)
                    {
                        var OldInputAmount = bluePrint.InputAmount;
                        bluePrint.InputAmount = Lists.Data.inventar.GetValueOrDefault(bluePrint.InputID, 0);
                        bluePrint.OutputAmount = Lists.Data.inventar.GetValueOrDefault(bluePrint.OutputID, 0);
                        var OldAmountSnapshot = bluePrint.AmountSnapshot;
                        bluePrint.AmountSnapshot = DateTime.Now;
                        var diff = (OldInputAmount - bluePrint.InputAmount);
                        bluePrint.ETA_String = diff < 0 ? "..." : GetTimeStringFromHours((bluePrint.InputAmount / diff) * (bluePrint.AmountSnapshot - OldAmountSnapshot).TotalHours);
                    }
                }
            }


            public static bool IsKnowScrapType(MyItemType scrapType) { return ScrapItemTypes.Contains(scrapType); }


            public static RefineryBlueprint GetScrapBlueprintByItemtypeOrCreateNew(MyItemType scrapType)
            {
                if (!KnowScrapTypes.ContainsKey(scrapType))
                {
                    KnowScrapTypes.Add(scrapType, new RefineryBlueprint(scrapType));
                    RefineryBlueprints.Add(KnowScrapTypes[scrapType]);
                }
                return KnowScrapTypes[scrapType];
            }


            public static void InitScrapTypeBlueprintTypes()
            {
                ScrapItemTypes = new MyItemType[ScrapTypeBlueprintNames.Length];
                for (int i = 0; i < ScrapTypeBlueprintNames.Length; i++)
                {
                    var t = ScrapTypeBlueprintNames[i];
                    var parts = t.Split(' ');
                    var newType = MyItemType.Parse(IG_ + parts[0] + "/" + parts[1]);
                    ScrapItemTypes[i] = newType;
                }
            }


            // ----------------------------------------------------------------------------------------------------------------
            public MyDefinitionId Definition_id;
            public string Name = "";
            public string InputID = "";
            public string InputIDName = "";
            public string OutputID = "";
            public string OutputIDName = "";
            public float InputAmount = 0;
            public float OutputAmount = 0;
            DateTime AmountSnapshot = DateTime.Now;
            public string ETA_String = "";
            public int RefineryCount = 0;
            public bool IsScrap = false;


            public RefineryBlueprint(MyDefinitionId iDefinitionID, string iInputID, string iOutputID)
            {
                Definition_id = iDefinitionID;
                Name = iDefinitionID.SubtypeName;
                InputID = iInputID;
                InputIDName = CastResourceName(InputID);
                OutputID = iOutputID;
                OutputIDName = CastResourceName(OutputID);
                IsScrap = ScrapTypeBlueprintNames.Contains(InputID);
            }


            // special constructor for blueprints from the AWWScrap mod
            // the blueprints wont be realtime created and canot be predefined
            public RefineryBlueprint(MyItemType iScrapType)
            {
                InputID = GetPIMItemID(iScrapType);
                InputIDName = iScrapType.SubtypeId;
                Name = InputIDName + "ToIngots";
                IsScrap = true;
            }
        }
    }
}
