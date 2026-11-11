using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IngameScript
{
    // Pure part of the LCD pages (autocrafting, oreprio, refining): parsing and rendering
    // without Sandbox/VRage types, so this file can be linked into the test project.
    // Finding and writing the panels lives in LcdManager.cs.
    //
    // The hidden keys at the end of the rows ("|BlueprintID|", "|OrePrio:Name|") are read back
    // from the player LCDs, so their format must not change.
    partial class Program
    {
        const string OrePrioDefString = "(sms,oreprio)";
        const string ResourcenOverview = "(sms,refining)";
        const string Autocrafting = "(sms,autocrafting)";

        const string line = "\n" + linepur + "\n";
        const string linepur = "/-------------------------------------------------------------------------";
        const string line2 = "\n" + line2pur + "\n";
        const string line2pur = "/" + line2lineonly;
        const string line2lineonly = "-------------------------------------------------------------------------------------------";

        static readonly string bigSpaces = new string(' ', 85);

        static int getIntegerWithPräfix(string cstr)
        {
            if (cstr == "") return 0;
            var cstrlist = cstr.Split(' ');
            float wr;
            if (!float.TryParse(cstrlist[0], out wr)) return 0;
            if (cstrlist.Length == 2)
            {
                if (cstrlist[1] == "k") wr *= 1000;
                else if (cstrlist[1] == "M") wr *= 1000000;
            }
            return (int)wr;
        }

        static int getInteger(string cstr) { float wr; float.TryParse(cstr.Trim().Split('.')[0], out wr); return (int)wr; }

        /// <summary>Splits one LCD line and trims spaces and soft hyphens (used as fine spaces) from all parts.</summary>
        static string[] SplitLcdLine(string s, string separators)
        {
            var parts = s.Split(separators.ToCharArray());
            for (int i = 0; i < parts.Length; i++) parts[i] = parts[i].Trim(' ', '­');
            return parts;
        }

        // ---------------------------------------------------------------------------------------- autocrafting

        /// <summary>
        /// Reads an autocrafting LCD: autocrafting types, threshold, type filters and the max amounts
        /// (rows " name | current | max | assembly ...|BlueprintID|"). Returns true if the threshold changed.
        /// </summary>
        public static bool ParseAutocrafting(string text, Dictionary<string, AssemblerBluePrint> active, ref string acTypes, Dictionary<string, string> typeFilters)
        {
            var thresholdChanged = false;
            foreach (var s in text.Split('\n'))
            {
                var l = SplitLcdLine(s, "|=%:");
                if (l[0] == "" || l[0][0] == '/') continue;
                if (l[0] == Strings.AutoCraftingTypeStringName && l.Length > 1)
                {
                    if (l[1] != "") acTypes = l[1];
                }
                else if (l[0] == Strings.X_Autocrafting_treshold && l.Length > 1)
                {
                    var old = AutocraftingThreshold;
                    AutocraftingThreshold = getInteger(l[1]);
                    if (AutocraftingThreshold == 0) AutocraftingThreshold = 80;
                    if (old != AutocraftingThreshold) thresholdChanged = true;
                }
                else if (l[0] == "Type" && l.Length > 2)
                {
                    if (!typeFilters.ContainsKey(l[1])) typeFilters.Add(l[1], l[2]);
                }
                else if (l.Length == 6 && active.ContainsKey(l[4]))
                {
                    active[l[4]].SetMaximumAmount(getIntegerWithPräfix(l[2]));
                }
            }
            return thresholdChanged;
        }

        // The pages are rendered in parts (header, one block per type), one part per job step.
        // One concatenation per line: linear (no growing string), and fewer calls than single Appends.

        /// <summary>The autocrafting types of the page ("Component, AmmoMagazine"), one block each.</summary>
        public static string[] AutocraftingTypeList(string acTypes)
        {
            return acTypes.Split(',').Select(s => s.Trim()).Where(s => s != "").ToArray();
        }

        public static void RenderAutocraftingHeader(StringBuilder sb, string acTypes, List<string> allTypes)
        {
            sb.Append("/ Autocraftingdefinition:\n/ add '...(sms)' to the name of assemblers to crafting their items,\n/ and set the max quantity as you want\n\n/ if the quantity of items falls below this percentage value,\n/ then it will be increased to max.\n"
                + Strings.X_Autocrafting_treshold + " = " + AutocraftingThreshold + "%\n\n/ possible autocrafting types, please add them separated by comma.\n/ "
                + string.Concat(allTypes.Select(t => t + ","))
                + "\n" + Strings.AutoCraftingTypeStringName + "=" + acTypes
                + "\n\n/                           Item            |     current    ­|       max      ­|   assembly\n");
        }

        public static void RenderAutocraftingType(StringBuilder sb, string actype, Dictionary<string, string> typeFilters, IEnumerable<AssemblerBluePrint> active, Filter filter)
        {
            string typeFilter;
            if (typeFilters.TryGetValue(actype, out typeFilter)) filter.InitFilter(typeFilter);
            else filter.SetFilterToAll();
            sb.Append(line2pur + "\n Type : " + actype + " = " + (typeFilter == null || typeFilter == "" ? " * " : typeFilter) + line2);
            var bpList = active.Where(b => b.AutoCraftingType == actype && filter.IfFilter(b.AutoCraftingName)).ToList();
            bpList.Sort((x, y) => x.AutoCraftingName.CompareTo(y.AutoCraftingName));
            foreach (var bp in bpList)
                sb.Append(" "
                    + GetDisplayBoxString(bp.AutoCraftingName, 60) + " | "
                    + GetDisplayBoxString(bp.CurrentItemAmount, 25) + " | "
                    + GetDisplayBoxString(bp.MaximumItemAmount, 25) + " | "
                    + GetDisplayBoxString(bp.AssemblyAmount > 0 ? bp.AssemblyAmount : (bp.RefineryAmount > 0 ? bp.RefineryAmount : 0), 25)
                    + bigSpaces + "|" + bp.BlueprintID + "|\n");
        }

        // ---------------------------------------------------------------------------------------- ore priority

        /// <summary>
        /// Reads an oreprio LCD into OrePrioConfig
        /// (rows " recipe | value | current ...|OrePrio:Name|" below "RefineryType: subtype").
        /// Returns the refinery type filter of the panel, null if there is none.
        /// </summary>
        public static string ParseOrePrio(string text, Dictionary<string, List<RefineryBlueprint>> accepted, List<RefineryBlueprint> blueprints)
        {
            string filterString = null;
            Dictionary<RefineryBlueprint, int> curPrioList = null;
            foreach (var s in text.Split('\n'))
            {
                var l = SplitLcdLine(s, ":=|");
                if (l[0].Length == 0 || l[0][0] == '/') continue;
                if (l.Length >= 5 && l[3].ToLower().StartsWith("oreprio") && curPrioList != null)
                {
                    var blueprint = blueprints.Find(b => b.Name == l[4]);
                    if (blueprint != null)
                    {
                        int prio;
                        if (!int.TryParse(l[1], out prio)) prio = -1;
                        curPrioList[blueprint] = (prio < 0 ? -1 : (prio > 10000 ? 10000 : prio));
                    }
                }
                else if (l.Length >= 2 && l[0].ToLower().StartsWith("refinerytype"))
                {
                    curPrioList = null;
                    if (l[1] != "" && accepted.ContainsKey(l[1]) && !OrePrioConfig.TryGetValue(l[1], out curPrioList))
                    {
                        curPrioList = new Dictionary<RefineryBlueprint, int>();
                        OrePrioConfig.Add(l[1], curPrioList);
                    }
                }
                else if (l.Length > 1 && l[0] == "Filter")
                {
                    if (filterString == null) filterString = l[1];
                }
            }
            return filterString;
        }

        /// <summary>
        /// The values of the oreprio LCDs (OrePrioConfig) override the priorities calculated by PIM,
        /// only for ores that are in stock (calculated prio > 0). Called after the calculation, before rendering.
        /// </summary>
        public static void ApplyOrePrioConfig()
        {
            foreach (var type in OrePrioConfig)
            {
                List<IPrio> prios;
                if (!ingotprio.TryGetValue(type.Key, out prios)) continue;
                foreach (var c in type.Value)
                {
                    if (c.Value < 0) continue;
                    var ni = IPrio.GetBlueprintPrio(prios, c.Key);
                    if (ni != null && ni.prio > 0) ni.setPrio(c.Value);
                }
            }
        }

        /// <param name="filterString">refinery type filter of the panel, null = all</param>
        public static void RenderOrePrioHeader(StringBuilder sb, string filterString, Filter filter)
        {
            if (filterString != null) filter.InitFilter(filterString);
            else filter.SetFilterToAll();
            sb.Append("/ Orepriorityconfig:\n/ only refinerytypes with '(sms)' in the name are displayed.\n\n/ set the 'value' between 1 and 10000\n/ if value = 0 then the ore will be ignored\n/ if value empty then prio will be calculated by PIM.\n/ any type of scrap is always refined first\n\n/ Refinerytypefilter, separated by comma, '*' for all\n Filter: "
                + (filterString ?? "*") + "\n\n/                Recipe                       |    Value   |       Current\n");
        }

        /// <summary>Block of one refinery type (only with at least 2 recipes and if not filtered out), uses the filter of the header.</summary>
        public static void RenderOrePrioType(StringBuilder sb, string key, List<RefineryBlueprint> accepted, Filter filter)
        {
            Dictionary<RefineryBlueprint, int> config;
            if (!OrePrioConfig.TryGetValue(key, out config)) OrePrioConfig.Add(key, config = new Dictionary<RefineryBlueprint, int>());
            var blueprintList = accepted.FindAll(o => !o.IsScrap);
            if (blueprintList.Count < 2 || !filter.IfFilter(key)) return;
            sb.Append(linepur + "\n RefineryType: " + key + line);
            var curIngotPrioList = ingotprio[key];
            blueprintList.Sort((x, y) => x.InputIDName.CompareTo(y.InputIDName));
            foreach (var bp in blueprintList)
            {
                int prio;
                if (!config.TryGetValue(bp, out prio)) config.Add(bp, prio = -1);
                var curPrio = IPrio.GetBlueprintPrio(curIngotPrioList, bp);
                sb.Append(" "
                    + GetDisplayBoxString(bp.Name, 65, true) + " | "
                    + GetDisplayBoxString(prio == -1 ? "  |  " : prio + "  |  ", 23)
                    + (curPrio != null && curPrio.initp > 0 ? GetDisplayBoxString(curPrio.initp.ToString(), 23) + "  " : "")
                    + bigSpaces + "|OrePrio:" + bp.Name + "|\n");
            }
        }

        // ---------------------------------------------------------------------------------------- refining progress

        public static void RenderRefining(StringBuilder sb, List<RefineryBlueprint> blueprints)
        {
            sb.Append(" Refiningprogress:\n" + line2lineonly + "\n");
            foreach (var bp in blueprints)
                if (bp.RefineryCount > 0)
                    sb.Append(GetDisplayBoxString(bp.Name, 70, true) + " | "
                        + GetDisplayBoxString(bp.InputIDName, bp.InputAmount, 70) + "\n"
                        + GetDisplayBoxString(bp.RefineryCount + " Refinerys.", 30, true)
                        + GetDisplayBoxString("-> " + bp.ETA_String, 40, true) + " | "
                        + GetDisplayBoxString(bp.OutputIDName, bp.OutputAmount, 70) + "\n"
                        + line2lineonly + "\n");
        }
    }
}
