using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.GUI.TextPanel;
using VRage.Scripting.MemorySafeTypes;

namespace IngameScript
{
    partial class Program
    {
        void loadAutocratingDefinitions()
        {
            var lcds = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(lcds, block => block.CustomName.Contains(Autocrafting) && Me.CubeGrid == block.CubeGrid);
            foreach (var lcd in lcds)
            {
                var ac_Types = IG_Component + "," + IG_Ammo;
                var act_new = false;
                var ac_TypesFilter = new Dictionary<string, string>();
                foreach (var s1 in lcd.GetText().Split('\n'))
                {
                    var acLines = s1.Split('|', '=', '%', ':');
                    for (int i = 0; i < acLines.Count(); i++) acLines[i] = acLines[i].Trim(' ', '\u00AD');
                    if (acLines.Count() == 0 || acLines[0] == "" || acLines[0][0] == '/') continue;
                    else if (acLines[0] == Strings.AutoCraftingTypeStringName && acLines.Count() > 1)
                    {
                        if (acLines[1] != "") ac_Types = acLines[1];
                    }
                    else if (acLines[0] == Strings.X_Autocrafting_treshold)
                    {
                        // threshold loading
                        var oldacf = AutocraftingThreshold;
                        AutocraftingThreshold = getInteger(acLines[1]);
                        if (AutocraftingThreshold == 0) AutocraftingThreshold = 80;
                        if (oldacf != AutocraftingThreshold) act_new = true;
                    }
                    else if (acLines[0] == "Type" && acLines.Count() > 2)
                    {
                        if (!ac_TypesFilter.ContainsKey(acLines[1])) ac_TypesFilter.Add(acLines[1], acLines[2]);
                    }
                    else if (acLines.Count() == 6)
                    {
                        if (Lists.Data.BluePrints_Active.ContainsKey(acLines[4]))
                        {
                            Lists.Data.BluePrints_Active[acLines[4]].SetMaximumAmount(getIntegerWithPräfix(acLines[2]));
                        }
                    }
                }
                if (act_new) SetAutocraftingThresholdNew();
                // write autocrafting config
                var acString = "/ Autocraftingdefinition:\n";
                acString += "/ add '...(sms)' to the name of assemblers to crafting their items,\n/ and set the max quantity as you want\n\n";
                acString += "/ if the quantity of items falls below this percentage value,\n/ then it will be increased to max.\n" + Strings.X_Autocrafting_treshold + " = " + AutocraftingThreshold + "%\n\n";
                acString += "/ possible autocrafting types, please add them separated by comma.\n/ ";
                foreach (var t in Lists.Data.autocrafting_Types) acString += t + ",";
                acString += "\n" + Strings.AutoCraftingTypeStringName + "=" + ac_Types;
                acString += "\n\n/                           Item            |     current    ­|       max      ­|   assembly\n";
                var ac_TypesList = ac_Types.Split(',')
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToArray();
                foreach (var actype in ac_TypesList)
                {
                    acString += line2pur + "\n Type : " + actype + " = ";
                    if (ac_TypesFilter.ContainsKey(actype))
                    {
                        acString += (ac_TypesFilter[actype] == "" ? " * " : ac_TypesFilter[actype]);
                        filter.InitFilter(ac_TypesFilter[actype]);
                    }
                    else
                    {
                        acString += " * ";
                        filter.SetFilterToAll();
                    }
                    acString += line2;
                    var bpList = Lists.Data.BluePrints_Active.Values.ToList().FindAll(b => b.AutoCraftingType == actype && filter.IfFilter(b.AutoCraftingName));
                    bpList.Sort((x, y) => x.AutoCraftingName.CompareTo(y.AutoCraftingName));
                    foreach (var bp in bpList)
                    {
                        acString += " "
                            + GetDisplayBoxString(bp.AutoCraftingName, 60)
                            + " | "
                            + GetDisplayBoxString(bp.CurrentItemAmount, 25)
                            + " | "
                            + GetDisplayBoxString(bp.MaximumItemAmount, 25)
                            + " | "
                            + GetDisplayBoxString((bp.AssemblyAmount > 0 ? bp.AssemblyAmount : (bp.RefineryAmount > 0 ? bp.RefineryAmount : 0)), 25)
                            + bigSpaces
                            + "|"
                            + bp.BlueprintID
                            + "|\n";
                    }
                }
                lcd.Alignment = TextAlignment.LEFT;
                lcd.ContentType = ContentType.TEXT_AND_IMAGE;
                lcd.WriteText(acString);
            }
        }

        void LoadAndRenderOrePrioDefs()
        {
            var lcds = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(lcds, block => block.CustomName.Contains(OrePrioDefString));
            if (lcds.Count > 0) // ToDo: mehrere LCDs, einlesen und ausgeben trennen
            {
                Refinery.RemoveUnusedRefinerytypeBlueprintLists();
                Dictionary<RefineryBlueprint, int> curPrioList = null;
                Dictionary<IMyTextPanel, string> filterStrings = new Dictionary<IMyTextPanel, string>();
                foreach (var lcd in lcds)
                {
                    var curRefineryType = "";
                    var pstrs = lcd.GetText().Split('\n');
                    // prio laden
                    foreach (var s in pstrs)
                    {
                        var line = s.Split(':', '=', '|');
                        for (int i = 0; i < line.Count(); i++) { line[i] = line[i].Trim(' ', '\u00AD'); }
                        if (line.Length == 0 || line[0].Length == 0 || line[0][0] == '/') continue;
                        if (line.Length >= 5 && line[3].ToLower().StartsWith("oreprio") && (curPrioList != null))
                        {
                            var blueprint = RefineryBlueprints.Find(b => b.Name == line[4]);
                            if (blueprint != null)
                            {
                                int prio = -1;
                                if (!int.TryParse(line[1], out prio)) prio = -1;
                                if (!curPrioList.ContainsKey(blueprint)) curPrioList.Add(blueprint, -1);
                                curPrioList[blueprint] = (prio < 0 ? -1 : (prio > 10000 ? 10000 : prio));
                                if (prio >= 0)
                                {
                                    IPrio ni = IPrio.GetBlueprintPrio(ingotprio[curRefineryType], blueprint);
                                    if (ni != null && ni.prio > 0) ni.setPrio(prio);
                                }
                            }
                        }
                        else if (line.Length >= 2 && line[0].ToLower().StartsWith("refinerytype"))
                        {
                            curPrioList = null;
                            curRefineryType = "";
                            if ((line[1] != "") && Refinery.refineryTypesAcceptedBlueprintsList.ContainsKey(line[1]))
                            {
                                if (OrePrioConfig.ContainsKey(line[1]))
                                {
                                    curPrioList = OrePrioConfig[line[1]];
                                }
                                else
                                {
                                    curPrioList = new Dictionary<RefineryBlueprint, int>();
                                    OrePrioConfig.Add(line[1], curPrioList);
                                }
                                curRefineryType = line[1];
                            }
                        }
                        else if (line.Length > 1 && line[0] == "Filter")
                        {
                            if (!filterStrings.ContainsKey(lcd))
                            {
                                filterStrings.Add(lcd, line[1]);
                            }
                        }
                    }
                }
                RenderOrePrio(lcds, filterStrings);
            }
        }
        void RenderOrePrio(List<IMyTextPanel> lcds, Dictionary<IMyTextPanel, string> filterStrings)
        {
            foreach (var lcd in lcds)
            {
                // prio wieder schreiben
                var fString = "*";
                if (filterStrings.ContainsKey(lcd))
                {
                    filter.InitFilter(filterStrings[lcd]);
                    fString = filterStrings[lcd];
                }
                else
                {
                    filter.SetFilterToAll();
                }
                var priolist = "/ Orepriorityconfig:\n/ only refinerytypes with '(sms)' in the name are displayed.\n\n/ set the 'value' between 1 and 10000\n/ if value = 0 then the ore will be ignored\n/ if value empty then prio will be calculated by PIM.\n/ any type of scrap is always refined first\n\n/ Refinerytypefilter, separated by comma, '*' for all\n Filter: " + fString + "\n\n/                Recipe                       |    Value   |       Current\n";
                foreach (var key in Refinery.refineryTypesAcceptedBlueprintsList.Keys)
                {
                    if (!OrePrioConfig.ContainsKey(key)) OrePrioConfig.Add(key, new Dictionary<RefineryBlueprint, int>());
                    var blueprintList = Refinery.refineryTypesAcceptedBlueprintsList[key].FindAll(o => !o.IsScrap);
                    if (blueprintList.Count < 2 || !filter.IfFilter(key)) continue;
                    priolist += linepur + "\n RefineryType: " + key + line;
                    var curIngotPrioList = ingotprio[key];
                    blueprintList.Sort((x, y) => x.InputIDName.CompareTo(y.InputIDName));
                    foreach (var bp in blueprintList)
                    {
                        var priostr = "-1";
                        var curPrio = IPrio.GetBlueprintPrio(curIngotPrioList, bp);
                        if (OrePrioConfig[key].ContainsKey(bp)) priostr = OrePrioConfig[key][bp].ToString();
                        else OrePrioConfig[key].Add(bp, -1);
                        priolist += " "
                            + (GetDisplayBoxString(bp.Name, 65, true))
                            + " | "
                            + GetDisplayBoxString((priostr == "-1" ? "  |  " : priostr + "  |  "), 23)
                            + ((curPrio != null && curPrio.initp > 0) ? GetDisplayBoxString(curPrio.initp.ToString(), 23) + "  " : "")
                            + bigSpaces
                            + "|OrePrio:"
                            + bp.Name
                            + "|\n";
                    }
                }
                lcd.Alignment = TextAlignment.LEFT;
                lcd.ContentType = ContentType.TEXT_AND_IMAGE;
                lcd.WriteText(priolist);
            }
        }
        void RenderResourceProccesingLCD()
        {
            var lcds = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(lcds, block => block.CustomName.Contains(ResourcenOverview));
            if (lcds.Count > 0)
            {
                RefineryBlueprint.FillInputOutputAmountAndETA();
                var oresList = " Refiningprogress:\n" + line2lineonly + "\n";
                foreach (var bluePrint in RefineryBlueprints)
                {
                    if (bluePrint.RefineryCount > 0)
                    {
                        oresList
                            += GetDisplayBoxString(bluePrint.Name, 70, true)
                            + " | "
                            + GetDisplayBoxString(bluePrint.InputIDName, bluePrint.InputAmount, 70)
                            + "\n"
                            + GetDisplayBoxString(bluePrint.RefineryCount.ToString() + " Refinerys.", 30, true)
                            + GetDisplayBoxString("-> " + bluePrint.ETA_String, 40, true)
                            + " | "
                            + GetDisplayBoxString(bluePrint.OutputIDName, bluePrint.OutputAmount, 70)
                            + "\n"
                            + line2lineonly
                            + "\n";

                    }
                }
                foreach (var lcd in lcds)
                {
                    lcd.Alignment = TextAlignment.LEFT;
                    lcd.ContentType = ContentType.TEXT_AND_IMAGE;
                    lcd.WriteText(oresList);
                }
            }
        }

        const string OrePrioDefString = "(sms,oreprio)";
        const string ResourcenOverview = "(sms,refining)";
        const string Autocrafting = "(sms,autocrafting)";
        const string AmmoPrioDefinition = "(sms,ammoprio)";
        const string line = "\n" + linepur + "\n";
        const string linepur = "/-------------------------------------------------------------------------";
        const string line2 = "\n" + line2pur + "\n";
        const string line2pur = "/" + line2lineonly;
        const string line2lineonly = "-------------------------------------------------------------------------------------------";
    }
}
