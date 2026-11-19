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
        //DP
        class DP
        {
            enum Options { AllTanks, Hydrogen, Oxygen, Table, Name, };
            public List<dyBlockDisplayable> listDy = new List<dyBlockDisplayable>();
            public double ms = 100;
            public string titleString = "", dock = "", headLineString = "", filter = "*", sortMode = "", cm = "";
            private string fgrid = "";
            public IMyCubeGrid grid = null;
            public int dyn, error, c = -1, style = 2;
            public bool is_Line = true, is_HeadLine = true, is_Title = true, is_All, is_Table, is_Fillbar, is_Sort, is_Zero;
            public List<string> options = new List<string>(), FieldTemplates = new List<string>();
            List<string> fwl = new List<string>(), fbl = new List<string>();
            public float with;
            List<changeStat> udt = new List<changeStat>();

            public DP(float iw, string args, IMyCubeGrid lcdGrid)
            {
                if (args == "") return; sortColumn = "";
                var argtrim = args.Trim().ToUpper();
                if (argtrim.StartsWith("WRITEX"))
                {
                    with = iw;
                    cm = "Writex";
                    if (argtrim.StartsWith("WRITEX_I")) cm += "_I";
                    int cp = args.IndexOf('(');
                    if (cp != 0)
                    {
                        FieldTemplates.Add(args.Substring(cp + 1, args.Length - cp - 2));
                        return;
                    }
                }
                var fls = new List<string>(args.Split('{'));
                var alo = new List<string>(fls[0].Split('(', ')'));
                var alf = new List<string>(alo[0].Split('[', ']'));
                cm = ToArgStr(alf[0].Trim().ToLower());
                if (fls.Count > 1) for (int i = 1; i < fls.Count; i++) FieldTemplates.Add(fls[i].Split('}')[0]);
                if (alf.Count > 1) filter = alf[1].Trim();
                if (alo.Count > 1)
                {
                    var argl = new List<string>(alo[1].Split(','));
                    for (int i = 0; i < argl.Count; i++)
                    {
                        string astr = argl[i].Trim().ToUpper();
                        if (astr.StartsWith("SORT")) { is_Sort = true; var colstr = astr.Split(' ', ':'); if (colstr.Length > 1) { sortColumn = colstr[1]; sortMode = sortColumn; } }
                        else if (astr.StartsWith("MAXSPEED")) { var cs = astr.Split(' ', ':'); if (cs.Length > 1) ms = getIntegerWithBounds(cs[1], 100, 1, 9999); }
                        else if (astr.StartsWith("COL")) { var cs = astr.Split(' ', ':'); if (cs.Length > 1) c = getIntegerWithBounds(cs[1], 1, 1, 4); }
                        else if (astr.StartsWith("STYLE")) { var colstr = astr.Split(' ', ':'); if (colstr.Length > 1) style = getIntegerWithBounds(colstr[1], 2, 1, 4); }
                        else if (astr == "ALL") is_All = true;
                        else if (astr == "ZERO") is_Zero = true;
                        else if (astr == OptFillbar) is_Fillbar = true;
                        else if (astr == "TABLE") is_Table = true;
                        else if (astr == "NOLINE") is_Line = false;
                        else if (astr == "NOTITLE") is_Title = false;
                        else if (astr == "NOHEADLINE") is_HeadLine = false;
                        else if (astr == OptMainGrid) grid = Mgrid;
                        else if (astr == OptLcdGrid) grid = lcdGrid;
                        options.Add(astr);
                    }
                }
                if (c == -1) { if (cm == "Inventory" && !is_Fillbar && !is_Table) c = 2; else c = 1; }
                fls = new List<string>(filter.Split(','));
                foreach (var s in fls)
                {
                    args = s.Trim();
                    if (args.Length > 0 && args[0] == '-') fbl.Add(args.Substring(1));
                    else if (args.Length > 5 && args.ToUpper().StartsWith("DOCK:")) { dock = args.Substring(5); if (fls.Count == 1) fwl.Add("*"); }
                    else if (args.Length > 5 && args.ToUpper().StartsWith("GRID:")) fgrid = args.Substring(5);
                    else fwl.Add(args);
                }
                if (dock != "" || fgrid != "" || grid != null) options.Add("GRID");
                with = iw / c;
            }
            public void sF(string nf) { filter = nf; fwl.Clear(); fwl.Add(nf); }
            public bool ifFilter() { return filter != "" && filter != "*"; }
            public bool ifGridOption() { if (ifOption("GRID") || ifOption(OptGridName)) return true; else return false; }
            public bool ifFilter(string s1)
            {
                foreach (var s in fbl) if (s1.Contains(s)) return false;
                foreach (var s in fwl) if (s == "*" || s1.Contains(s)) return true;
                return false;
            }
            public bool ifDisplayGrid(IMyCubeGrid ig)
            {
                if (grid == ig || (grid == null && dock == "" && fgrid == "")) return true;
                if (dock != "")
                {
                    var d = getDock(dock);
                    if (d != null)
                    {
                        if (d.dockedGrid != null)
                        {
                            if (d.dockedGrid == ig) return true;
                        }
                        else error = 3;
                    }
                    else error = -1;
                }
                ;
                return false;
            }
            // the connector tag is normalized by ToArgStr ("dock:FakeShip" -> "Fakeship"), the filter is not: ignore the case
            dyConnector getDock(string name) { foreach (var d in docks) if (string.Equals(d.dockName, name, StringComparison.OrdinalIgnoreCase)) return d; return null; }
            public bool ifOption(string key) { if (options.Contains(key)) return true; else return false; }
            public bool checkChangeStats(DateTime c) { foreach (changeStat u in udt) if (u.ifUpdateNeed(c)) return true; return false; }
            public void addBlockChangeTypes(ChangeType add_ct, ChangeType name_ct)
            {
                addChangeType(add_ct);
                if (is_Sort || ifFilter()) addChangeType(name_ct);
            }
            public void addChangeType(ChangeType type)
            {
                var newChangeStat = getChangeStat(type);
                if (!udt.Contains(newChangeStat)) udt.Add(newChangeStat);
            }
            public void addChangeType(changeStat u)
            {
                if (!udt.Contains(u)) udt.Add(u);
                if (dock != "")
                {
                    addChangeType(ChangeType.Dockings_add);
                    addChangeType(ChangeType.Dockings_name);
                }
            }
        }
    }
}
