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
            public TerminalSite mainmenu;
            public float maxMenuWith;
            // the parsed definition (errors: hint in the title line, comment lines in the CustomData)
            public MenuDef def;
            public Dictionary<string, TerminalSite> terminalSites = new Dictionary<string, TerminalSite>();
            public bool parseMenuDefinition(string definition)
            {
                if (definition.Trim() == "") return false;
                if (definition == oldMenuDef) return true;
                oldMenuDef = definition;
                terminalSites.Clear();
                mainmenu = null;
                maxMenuWith = 0;
                def = ParseMenu(definition, tHeadLine);
                foreach (var s in def.Sites)
                {
                    var site = new TerminalSite(this, s);
                    terminalSites[s.Id] = site;
                    if (mainmenu == null) mainmenu = site;
                }
                // the menu entries: "L" links to a site, "A" actions
                var entries = new Dictionary<string, MenuPunkt>();
                foreach (var t in def.Targets) entries[t.Key] = t.Key[0] == 'L' ? (MenuPunkt)new MenuSite(this, t.Value[0], t.Value[1]) : new MenuCommand(this, t.Value[0], t.Value[1], t.Value[2]);
                MenuPunkt entry;
                foreach (var site in terminalSites.Values)
                    foreach (var e in site.Def.Entries) site.commands.Add(entries.TryGetValue(e, out entry) ? entry : null);
                maxMenuWith += 2.58332f;
                return mainmenu != null;
            }
        }
        public abstract class MenuPunkt
        {
            public TerminalMenu menuObject;
            float maxWith;
            public List<string> menuStrings;
            public MenuPunkt(TerminalMenu menu, string text)
            {
                menuObject = menu;
                menuStrings = new List<string>(text.Split(' '));
                foreach (var s in menuStrings)
                {
                    var menuCom = StringWidth(s);
                    if (menuCom > maxWith) maxWith = menuCom;
                }
                if (maxWith > menuObject.maxMenuWith) menuObject.maxMenuWith = maxWith;
            }
            public abstract TerminalSite scheduleCommand();
        }
        class MenuSite : MenuPunkt
        {
            string siteID;
            public MenuSite(TerminalMenu mid, string text, string sID) : base(mid, text)
            {
                siteID = sID;
            }
            public override TerminalSite scheduleCommand()
            {
                TerminalSite site;
                return menuObject.terminalSites.TryGetValue(siteID, out site) ? site : null;
            }
        }
        class MenuCommand : MenuPunkt
        {
            string command, blocks;
            public MenuCommand(TerminalMenu mid, string text, string com, string iblocks) : base(mid, text)
            {
                command = com;
                blocks = iblocks;
            }
            public override TerminalSite scheduleCommand()
            {
                // an action of another script ("PIM:ctrl_ref"): to the master of that script type (nothing without it)
                var colon = command.IndexOf(':');
                if (colon > 0)
                {
                    var master = Net.MasterOf(command.Substring(0, colon).ToUpper());
                    if (master != 0) Net.Send(master, NetWorkingScripts.RunAction, command.Substring(colon + 1));
                    return null;
                }
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
        public class TerminalSite
        {
            // the parsed site: id, title line, body, entries
            public SiteDef Def;
            TerminalMenu menuObject;
            // the menu entries (null = gap), the first 4 are on the buttons 1-4
            public List<MenuPunkt> commands = new List<MenuPunkt>();
            public List<string> mel = new List<string>();
            public TerminalSite(TerminalMenu menu, SiteDef def)
            {
                menuObject = menu;
                Def = def;
            }
            public void setMenu(int maxlines)
            {
                if (mel.Count > 0) return;
                var git = Math.Round((double)maxlines / 6.0);
                for (int i = 0; i < maxlines; i++)
                {
                    var ams = "";
                    if (i == 3) ams = "Menu";
                    else if (i == 4) ams = "\\/      ";
                    int seg = (int)(i / git);
                    if (seg >= 1 && seg <= 4)
                    {
                        var mp = seg - 1;
                        if (mp < commands.Count && commands[mp] != null && commands[mp].menuStrings.Count > 0)
                        {
                            int offset = (int)((git * (mp + 1)) + ((git - commands[mp].menuStrings.Count) / 2));
                            if (i >= offset && i <= (offset + commands[mp].menuStrings.Count - 1)) ams = commands[mp].menuStrings[i - offset];
                        }
                    }
                    mel.Add(fmstr(menuObject.maxMenuWith, ams) + "\n");
                }
            }
        }
    }
}
