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
        static Dictionary<string, string> ResourcesNameCastList = new Dictionary<string, string>
            {
                { Ore.Stone, IngameStrings.RStone },
                { Ingot.Magnesium, Ingot.Magnesiumpowder },
                { Ingot.Stone, "Gravel" },
                { Ingot.DeuteriumContainer, IngameStrings.RDeuterium },
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
