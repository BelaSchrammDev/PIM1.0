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
        public class TerminalMenu
        {
            public List<LCD_Terminal> terminals = new List<LCD_Terminal>();
            // alle menuIDs sind GROSS, #timer bleibt so
            public string oldMenuDef = "nix";
            public TerminalSite mainmenu = null;
            public string headline = tHeadLine;
            public float maxMenuWith = 0;
            public Dictionary<string, MenuPunkt> menuCommands = new Dictionary<string, MenuPunkt>();
            public Dictionary<string, TerminalSite> terminalSites = new Dictionary<string, TerminalSite>();
            public bool parseMenuDefinition(string def)
            {
                if (def == "") return false;
                if (def == oldMenuDef) return true;
                oldMenuDef = def;
                terminalSites.Clear();
                menuCommands.Clear();
                mainmenu = null;
                maxMenuWith = 0;
                var menulist = def.Split('\n');
                TerminalSite curTerminalmenu = null;
                bool sitebody = false;
                foreach (var s in menulist)
                {
                    var line = s.Trim();
                    if (line.StartsWith("menu=")) continue;
                    else if (line.StartsWith("/")) continue;
                    else if (line.StartsWith("@newsite"))
                    {
                        var kstring = getKlammerOptions(line);
                        var termID = kstring[0] == "" ? "Unknow" : kstring[0].ToUpper();
                        if (termID != "" && !terminalSites.ContainsKey(termID))
                        {
                            curTerminalmenu = new TerminalSite(this, termID, kstring[1]);
                            terminalSites.Add(termID, curTerminalmenu);
                            curTerminalmenu.titleline = headline;
                            if (mainmenu == null) mainmenu = curTerminalmenu;
                        }
                    }
                    else if (line.StartsWith("@endsite")) { curTerminalmenu = null; sitebody = false; }
                    else if (line.StartsWith("@titleline"))
                    {
                        var sline1 = line.Split('(', ')');
                        var sline = sline1.Length > 1 ? sline1[1] : " ";
                        if (curTerminalmenu == null) headline = sline;
                        else curTerminalmenu.titleline = sline;
                    }
                    else if (line.StartsWith("@alternativ") && curTerminalmenu != null)
                    {
                        var kstring = getKlammerOptions(line);
                        var termID = kstring[0] == "" ? "Unknow" : kstring[0].ToUpper();
                        if (!menuCommands.ContainsKey(termID)) menuCommands.Add(termID, new MenuSite(this, termID, kstring[1], curTerminalmenu.siteID));
                    }
                    else if (line.StartsWith("@command"))
                    {
                        var kstring = getKlammerOptions(line);
                        var termID = kstring[0].ToUpper();
                        if (!menuCommands.ContainsKey(termID)) menuCommands.Add(termID, new MenuCommand(this, termID, kstring[1], kstring[2], kstring[3]));
                    }
                    else if (line.StartsWith("@body")) sitebody = true;
                    else if (line.StartsWith("@menu") && curTerminalmenu != null) curTerminalmenu.setMenuPunkte(getKlammerOptions(line));
                    else if (sitebody && curTerminalmenu != null) curTerminalmenu.siteBody += s + "\n";
                }
                // menucommands setzen
                foreach (var term in terminalSites.Values) term.setMenuCommands();
                maxMenuWith += 2.58332f;
                return mainmenu != null;
            }
        }
        public abstract class MenuPunkt
        {
            public string commandID = "";
            public TerminalMenu menuObject = null;
            public float maxWith = 0;
            public List<string> menuStrings = null;
            public MenuPunkt(TerminalMenu menu, string id, string text)
            {
                commandID = id;
                menuObject = menu;
                menuStrings = new List<string>(text.Split(' '));
                foreach (var s in menuStrings)
                {
                    var menuCom = Sm(s);
                    if (menuCom > maxWith) maxWith = menuCom;
                }
                if (maxWith > menuObject.maxMenuWith) menuObject.maxMenuWith = maxWith;
            }
            public abstract TerminalSite scheduleCommand();
        }
        public class MenuSite : MenuPunkt
        {
            public string siteID = "";
            public MenuSite(TerminalMenu mid, string id, string text, string sID) : base(mid, id, text)
            {
                siteID = sID;
            }
            public override TerminalSite scheduleCommand()
            {
                if (menuObject.terminalSites.ContainsKey(siteID)) return menuObject.terminalSites[siteID];
                return null;
            }
        }
        public class MenuCommand : MenuPunkt
        {
            string command = "";
            string blocks = "";
            public MenuCommand(TerminalMenu mid, string id, string text, string com, string iblocks) : base(mid, id, text)
            {
                command = com;
                blocks = iblocks;
            }
            public override TerminalSite scheduleCommand()
            {
                var d = getDyBlockList(BlockType.Refinery);
                var o = getDyBlockList(BlockType.Assembler);
                switch (command)
                {
                    // block on
                    case "#blocks_on#":
                        foreach (var b in getBlockList(blocks)) { if (b is IMyFunctionalBlock) { var f = b as IMyFunctionalBlock; f.Enabled = true; } }
                        break;
                    // block off
                    case "#blocks_off#":
                        foreach (var b in getBlockList(blocks)) { if (b is IMyFunctionalBlock) { var f = b as IMyFunctionalBlock; f.Enabled = false; } }
                        break;
                    // block on/off
                    case "#blocks_on_off#":
                        foreach (var b in getBlockList(blocks)) { if (b is IMyFunctionalBlock) { var f = b as IMyFunctionalBlock; f.Enabled = !f.Enabled; } }
                        break;
                    // timer_start
                    case "#timer_start#":
                        foreach (var b in getBlockList(blocks)) { if (b is IMyTimerBlock) { var t = b as IMyTimerBlock; t.StartCountdown(); } }
                        break;
                    case "#ctrl_all#":
                        if (d.ContainsKey(Mgrid)) foreach (Ofen ofen in d[Mgrid]) ofen.pms.TakeControl();
                        if (o.ContainsKey(Mgrid)) foreach (Okuma oku in o[Mgrid]) oku.pms.TakeControl();
                        break;
                    case "#ctrl_ref#": if (d.ContainsKey(Mgrid)) foreach (Ofen ofen in d[Mgrid]) ofen.pms.TakeControl(); break;
                    case "#ctrl_ass#": if (o.ContainsKey(Mgrid)) foreach (Okuma oku in o[Mgrid]) oku.pms.TakeControl(); break;
                    case "#noctrl_ref#": if (d.ContainsKey(Mgrid)) foreach (Ofen ofen in d[Mgrid]) ofen.pms.EraseControl(); break;
                    case "#noctrl_ass#": if (o.ContainsKey(Mgrid)) foreach (Okuma oku in o[Mgrid]) oku.pms.EraseControl(); break;
                }
                return null;
            }
            List<IMyTerminalBlock> getBlockList(string blockname)
            {
                var bl = new List<IMyTerminalBlock>();
                var group = gts.GetBlockGroupWithName(blockname);
                if (group == null)
                {
                    var tb = gts.GetBlockWithName(blockname);
                    if (tb == null)
                    {
                        // fehler
                    }
                    else
                    {
                        bl.Add(tb);
                    }
                }
                else
                {
                    group.GetBlocks(bl);
                    if (bl.Count == 0)
                    {
                        // fehler
                    }
                }
                return bl;
            }
        }
        static string[] getKlammerOptions(string line)
        {
            var klammerstrings = new string[4];
            var linestring = new List<string>(line.Split('(', ')', ','));
            for (int i = 0; i < 4; i++)
            {
                if (linestring.Count > i + 1) klammerstrings[i] = linestring[i + 1];
            }
            return klammerstrings;
        }
        public class TerminalSite
        {
            public string siteID = "";
            public string titleline;
            public string siteBody = "";
            TerminalMenu menuObject = null;
            public string[] menupunktStrings = new string[4];
            public MenuPunkt[] commands = new MenuPunkt[4];
            public List<string> mel = new List<string>();
            public TerminalSite(TerminalMenu menu, string id, string text)
            {
                menuObject = menu;
                siteID = id;
                if (!menuObject.menuCommands.ContainsKey(id)) menuObject.menuCommands.Add(id, new MenuSite(menu, id, text, id));
            }
            public void setMenuCommands()
            {
                for (int i = 0; i < menupunktStrings.Length; i++)
                {
                    if (menuObject.menuCommands.ContainsKey(menupunktStrings[i])) commands[i] = menuObject.menuCommands[menupunktStrings[i]];
                }
            }
            public void setMenuPunkte(string[] mpunkte)
            {
                for (int i = 0; (i < mpunkte.Length && i < 4); i++) menupunktStrings[i] = mpunkte[i] == null ? "" : mpunkte[i].ToUpper();
            }
            public void setMenu(int maxlines)
            {
                if (mel.Count > 0) return;
                var git = Math.Round((double)maxlines / 6.0);
                for (int i = 0; i < maxlines; i++)
                {
                    string ams = "";
                    if (i == 3) ams = "Menu";
                    else if (i == 4) ams = "\\/      ";
                    int seg = (int)(i / git);
                    if (seg >= 1 && seg <= 4)
                    {
                        var mp = seg - 1;
                        if (commands[mp] != null && commands[mp].menuStrings.Count > 0)
                        {
                            int offset = (int)((git * (mp + 1)) + ((git - commands[mp].menuStrings.Count) / 2));
                            if (i >= offset && i <= (offset + commands[mp].menuStrings.Count - 1)) ams = commands[mp].menuStrings[i - offset];
                        }
                    }
                    mel.Add(fmstr(menuObject.maxMenuWith, ams) + "\n");
                }
            }
            public TerminalSite getAction(int num)
            {
                if (num < 0 || num > 3) return null;
                if (commands[num] == null) return null;
                return commands[num].scheduleCommand();
            }
        }
    }
}
