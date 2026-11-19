using System;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        // The parser of the terminal menu definitions (pure: only strings and lists, tested in SMS1.Tests).
        // New syntax:  @site(id, Title) ... @link(site, Text) / @action(id, Text) ... @body ... @end, @title(...)
        // Old syntax (still valid, translated): @newsite, @endsite, @titleline, @menu(a,b,c,d), @command, @alternative.
        // Parameters: between the first '(' and the last ')', split at ',', trimmed; the last one gets the rest.

        // a site of a menu definition
        public class SiteDef
        {
            public string Id, TitleLine, Body = "";
            // the keys of the entries in MenuDef.Targets, "" = gap (classic @menu)
            public List<string> Entries = new List<string>();
        }

        // a parsed menu definition
        public class MenuDef
        {
            public List<SiteDef> Sites = new List<SiteDef>();
            // what an entry does: "L" + id -> text, site id (sites, @alternative, @link with own text);
            // "A" + id -> text, command, blocks (actions)
            public Dictionary<string, string[]> Targets = new Dictionary<string, string[]>();
            // the errors as comment lines for the CustomData, before the definition ("" = no error)
            public string Errors = "";
            // the old @menu(a,b,c,d) is used: rendered with the 4 buttons as before
            public bool Classic;
        }

        // count parameters of a directive line, the last one gets the rest of the line, missing ones are ""
        static string[] MenuParams(string line, int count)
        {
            int open = line.IndexOf('('), close = line.LastIndexOf(')');
            var parts = (open < 0 ? "" : line.Substring(open + 1, (close > open ? close : line.Length) - open - 1)).Split(new[] { ',' }, count);
            var result = new string[count];
            for (int i = 0; i < count; i++) result[i] = i < parts.Length ? parts[i].Trim() : "";
            return result;
        }

        static MenuDef ParseMenu(string definition, string headline)
        {
            var menu = new MenuDef();
            var targets = menu.Targets;
            Action<string> error = text => menu.Errors += "  / !! " + text + "\n";
            SiteDef site = null, open = null;
            bool body = false, newEntries = false;
            foreach (var raw in definition.Split('\n'))
            {
                var line = raw.Trim();
                var lower = line.ToLower();
                Func<string, bool> Is = name => lower.StartsWith("@" + name);
                string[] p = MenuParams(line, 4);
                var id = p[0].ToUpper();
                if (line.StartsWith("/") || lower.StartsWith("menu=")) continue;
                if (Is("newsite") || Is("site"))
                {
                    if (open != null) error("no @end: " + open.Id);
                    p = MenuParams(line, 2);
                    open = site = new SiteDef { Id = id, TitleLine = headline };
                    // a second site with the same id: its lines are ignored
                    if (targets.ContainsKey("L" + id)) error("double site: " + id);
                    else
                    {
                        menu.Sites.Add(site);
                        targets["L" + id] = new[] { p[1], id };
                    }
                    body = false;
                }
                else if (Is("end")) site = open = null;
                else if (Is("title"))
                {
                    p = MenuParams(line, 1);
                    if (site == null) headline = p[0];
                    else site.TitleLine = p[0];
                }
                else if (Is("body")) body = true;
                else if (Is("command") || Is("action"))
                {
                    // an action without command (only id and text): the built-in command #id#
                    var command = p[2] == "" ? "#" + p[0].ToLower() + "#" : p[2];
                    // the control of refineries/assemblers is done by PIM: #ctrl_ref# -> PIM:ctrl_ref (also in old menus)
                    if (command.StartsWith("#") && command.Contains("ctrl_")) command = NetWorkingScripts.Pim + ":" + command.Trim('#');
                    if (!targets.ContainsKey("A" + id) && p[1] != "") targets["A" + id] = new[] { p[1], command, p[3] };
                    if (Is("action") && site != null) { site.Entries.Add("A" + id); newEntries = true; }
                }
                else if (site == null) { }
                else if (Is("link"))
                {
                    p = MenuParams(line, 2);
                    // a link with its own text: an own target with that text
                    var key = "L" + id + (p[1] == "" ? "" : "|" + p[1]);
                    if (p[1] != "") targets[key] = new[] { p[1], id };
                    site.Entries.Add(key);
                    newEntries = true;
                }
                else if (Is("alternativ")) targets["L" + id] = new[] { MenuParams(line, 2)[1], site.Id };
                else if (Is("menu"))
                {
                    menu.Classic = true;
                    // the 4 buttons: gaps stay (""), "M": a site or an action (resolved at the end)
                    foreach (var e in p) site.Entries.Add(e == "" ? "" : "M" + e.ToUpper());
                }
                else if (body) site.Body += raw + "\n";
            }
            if (open != null) error("no @end: " + open.Id);
            if (menu.Classic && newEntries) error("mixed syntax: @menu and @link/@action");

            // classic entries: a site first, then an action; unknown ids: the entry becomes a gap
            foreach (var s in menu.Sites)
                for (int i = 0; i < s.Entries.Count; i++)
                {
                    var e = s.Entries[i];
                    if (e != "" && e[0] == 'M') s.Entries[i] = e = (targets.ContainsKey("L" + e.Substring(1)) ? "L" : "A") + e.Substring(1);
                    string[] target;
                    if (e != "" && (!targets.TryGetValue(e, out target) || e[0] == 'L' && !targets.ContainsKey("L" + target[1])))
                    {
                        error("unknown " + e.Substring(1).Split('|')[0] + " in " + s.Id);
                        s.Entries[i] = "";
                    }
                }
            return menu;
        }
    }
}
