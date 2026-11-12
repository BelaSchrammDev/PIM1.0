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
            double days = Math.Round(h / 24, 1);
            if (days > 365)
            {
                return Math.Round(days / 365, 1) + " years";
            }
            if (days < 1.1) return days + " day";
            else return days + " days";
        }

        /// <summary>
        /// Remaining refining time of an ore: the amount that is left, divided by what was used up
        /// since the last snapshot. "..." if the amount did not shrink (nothing refined or ore added),
        /// there is no meaningful time then (the division gave "Infinity years" / "NaN days").
        /// </summary>
        public static string GetEtaString(float oldAmount, float amount, double hoursSinceOldAmount)
        {
            var diff = oldAmount - amount;
            return diff <= 0 ? "..." : GetTimeStringFromHours((amount / diff) * hoursSinceOldAmount);
        }

        public static Dictionary<string, List<IPrio>> ingotprio =new Dictionary<string, List<IPrio>>();
        public static Dictionary<string, Dictionary<RefineryBlueprint, int>> OrePrioConfig = new Dictionary<string, Dictionary<RefineryBlueprint, int>>();

        public class IPrio : IComparable<IPrio>
        {
            public RefineryBlueprint refineryBP;
            public int prio = 0;
            public int initp = 0;
            /// <summary>
            /// Highest current priority first (lowered by LowerAfterFilling, so the next refinerys take
            /// another ore), the same priority by name, so the order does not change from cycle to cycle.
            /// </summary>
            public int CompareTo(IPrio other)
            {
                if (other.prio != prio) return other.prio > prio ? 1 : -1;
                return string.CompareOrdinal(refineryBP.Name, other.refineryBP.Name);
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
            /// <summary>
            /// A refinery was filled with this ore: lowers a calculated priority (below 100) by one
            /// share per PIM controlled refinery, so the following refinerys prefer other ores.
            /// Returns false if the priority is not changed.
            /// </summary>
            public bool LowerAfterFilling(int refineryCount)
            {
                if (prio >= 100) return false;
                prio -= prio / (refineryCount > 1 ? refineryCount : 1);
                return true;
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
