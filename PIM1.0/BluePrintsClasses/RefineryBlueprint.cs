using System;
using System.Collections.Generic;
using System.Linq;

namespace IngameScript
{
    // Pure part of RefineryBlueprint: no Sandbox/VRage types,
    // so this file can be linked into the test project.
    // The ingame part (MyDefinitionId, scrap types, ETA) lives in RefineryBlueprintIngame.cs.
    partial class Program
    {
        static List<RefineryBlueprint> RefineryBlueprints = new List<RefineryBlueprint>();

        static Dictionary<string, string> ResourcesNameCastList = new Dictionary<string, string>
            {
                { Ore.Stone, IngameStrings.RStone },
                { Ingot.Magnesium, Ingot.Magnesiumpowder },
                { Ingot.Stone, "Gravel" },
                { Ingot.DTFuelCapsule, IngameStrings.RDeuterium },
                { Ore.Ice, "Ice"},
                { Ingot.Carbon, IngameStrings.RCarbon },
            };
        static Dictionary<string, string> ResourcesNameCastListIOMod = new Dictionary<string, string>
            {
                { Ore.Coal, IngameStrings.RCoal },
                { Ore.Bauxite, IngameStrings.RBauxite },
                { Ore.Niter, IngameStrings.RNiter },
                { Ingot.Lithium, IngameStrings.RLithium + " Paste" },
                { Ingot.Sulfur, IngameStrings.RSulfur },
                { Ingot.Niter, IngameStrings.RPotassium + " Nitrate" },
                { Ore.Magnesium, "Crushed Niter" },
                { Ingot.Magnesium, Ingot.Gunpowder},
            };

        static string CastResourceName(string name, bool ioMod)
        {
            if (ioMod && ResourcesNameCastListIOMod.ContainsKey(name)) return ResourcesNameCastListIOMod[name];
            if (ResourcesNameCastList.ContainsKey(name)) return ResourcesNameCastList[name];
            if (name.StartsWith("Ore Crushed")) return "Crushed " + name.Substring(11);
            if (name.StartsWith("Ore Purified")) return "Purified " + name.Substring(12);
            return name;
        }

        public static Dictionary<string, List<IPrio>> ingotprio = new Dictionary<string, List<IPrio>>();
        public static Dictionary<string, Dictionary<RefineryBlueprint, int>> OrePrioConfig = new Dictionary<string, Dictionary<RefineryBlueprint, int>>();

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

        public partial class RefineryBlueprint
        {
            static string[] ScrapTypeBlueprintNames = { "Component C100ShellCasing", Ore.Scrap, Ingot.Scrap };

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

            /// <param name="name">blueprint subtype, also used to build the MyDefinitionId</param>
            /// <param name="ioMod">IndustrialOverhaulMod active (changes the resource display names)</param>
            public RefineryBlueprint(string name, string iInputID, string iOutputID, bool ioMod)
            {
                Name = name;
                InputID = iInputID;
                InputIDName = CastResourceName(InputID, ioMod);
                OutputID = iOutputID;
                OutputIDName = CastResourceName(OutputID, ioMod);
                IsScrap = ScrapTypeBlueprintNames.Contains(InputID);
            }
        }
    }
}
