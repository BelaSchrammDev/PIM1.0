using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblerBluePrint = IngameScript.Program.AssemblerBluePrint;

namespace PimBluePrintGenerator
{
    /// <summary>
    /// Encodes the blueprint data into the compact string format read by
    /// DefineBluePrints() in PIM1.0/BluePrintsClasses/BluePrintDefinitions.cs.
    /// Every encoding decision must be mirrored exactly by that parser.
    /// </summary>
    public static class BluePrintDataEncoder
    {
        // must match PIM1.0/BluePrintsClasses/BluePrintDefinitions.cs
        public const string PositionPrefix = "Position0";
        public const string DefaultRefineryTemplate = "*OreToIngot=Ore *=Ingot *";

        public static string EncodeMod(ModData mod)
        {
            var entries = new List<string>();
            foreach (var section in mod.Sections.Where(s => !s.IsIgnore))
            {
                entries.Add(SectionHeader(section));
                if (section.IsRefinery) EncodeRefinerySection(section, entries);
                else EncodeAssemblerSection(mod.Name, section, entries);
            }
            return string.Join(",", entries);
        }

        static string SectionHeader(DataSection section)
        {
            var header = section.IsRefinery ? "[R" : "[A";
            if (section.ConditionMod != null) header += (section.ConditionEnabled ? "+" : "!") + section.ConditionMod;
            return header;
        }

        // ---------------------------------------------------------------- assembler

        static void EncodeAssemblerSection(string modName, DataSection section, List<string> entries)
        {
            string currentType = null;   // type entries are repeated in each section (sections may be skipped)
            var keys = new HashSet<string>();
            foreach (var row in section.Rows)
            {
                var type = row.Fields[0];
                var definitionSubtype = row.Field(1);
                var subtype = StripPosition(definitionSubtype);
                if (!keys.Add(type + " " + subtype)) throw row.Error($"blueprint '{type} {subtype}' defined twice in this section");

                if (type != currentType)
                {
                    entries.Add(BluePrintDataFile.AssemblerTypeCodes[type].ToString());
                    currentType = type;
                }
                entries.Add(EncodeAssemblerEntry(modName, type, definitionSubtype, row.Field(2), row.Field(3)));
            }
        }

        static string StripPosition(string definitionSubtype) =>
            definitionSubtype.StartsWith(PositionPrefix) ? definitionSubtype.Substring(definitionSubtype.IndexOf('_') + 1) : definitionSubtype;

        /// <summary>
        /// Chooses the shortest encoding that produces the same ItemName, AutoCraftingName
        /// and AutoCraftingType as the row with all its fields.
        /// </summary>
        static string EncodeAssemblerEntry(string modName, string type, string definitionSubtype, string item, string display)
        {
            var subtype = StripPosition(definitionSubtype);
            var target = Create(modName, type, subtype, definitionSubtype, item, display);

            var itemCandidates = new List<string> { "" };
            if (item != "") itemCandidates.Add(item);
            for (int n = 1; n < subtype.Length; n++)
            {
                itemCandidates.Add(n.ToString());
                itemCandidates.Add((-n).ToString());
            }
            var displayCandidates = display == "" ? new[] { "" } : new[] { "", display };

            var encodedSubtype = definitionSubtype.StartsWith(PositionPrefix)
                ? "#" + definitionSubtype.Substring(PositionPrefix.Length)
                : definitionSubtype;

            string best = null;
            foreach (var d in displayCandidates)
                foreach (var i in itemCandidates)
                {
                    AssemblerBluePrint candidate;
                    try { candidate = Create(modName, type, subtype, definitionSubtype, i, d); }
                    catch (ArgumentException) { continue; }   // the name rules fail on some stripped subtypes
                    if (candidate.ItemName != target.ItemName
                        || candidate.AutoCraftingName != target.AutoCraftingName
                        || candidate.AutoCraftingType != target.AutoCraftingType) continue;

                    var encoded = encodedSubtype + (d != "" ? "=" + i + "=" + d : i != "" ? "=" + i : "");
                    if (best == null || encoded.Length < best.Length) best = encoded;
                }

            if (best == null) throw new InvalidOperationException($"no encoding found for {type} {definitionSubtype}");
            return best;
        }

        /// <summary>Same item decoding as the script parser.</summary>
        static AssemblerBluePrint Create(string modName, string type, string subtype, string definitionSubtype, string itemField, string display)
        {
            string itemName;
            int strip;
            if (itemField == "") itemName = "";
            else if (int.TryParse(itemField, out strip))
                itemName = AssemblerBluePrint.BluePrintNameToItemName(type,
                    strip > 0 ? subtype.Substring(strip) : subtype.Substring(0, subtype.Length + strip));
            else itemName = type + " " + itemField;

            return new AssemblerBluePrint(type, subtype, definitionSubtype, modName, itemName, display);
        }

        // ---------------------------------------------------------------- refinery

        sealed class Run
        {
            public string Template;          // null: no template possible, literal only
            public readonly List<DataRow> Rows = new List<DataRow>();
            public readonly List<string> Resources = new List<string>();
        }

        /// <summary>
        /// Rows that differ only in one resource name (e.g. CrushIronOre, CrushNickelOre, ...)
        /// are written as template + resource list. Only consecutive rows are grouped, so the
        /// definition order is kept. A small dynamic program picks literal or template per run,
        /// taking into account that the parser remembers the last template.
        /// </summary>
        static void EncodeRefinerySection(DataSection section, List<string> entries)
        {
            var runs = new List<Run>();
            foreach (var row in section.Rows)
            {
                string resource;
                var template = FindTemplate(row.Fields[0], row.Fields[1], row.Fields[2], out resource);
                var last = runs.LastOrDefault();
                if (template == null || last == null || last.Template != template)
                {
                    last = new Run { Template = template };
                    runs.Add(last);
                }
                last.Rows.Add(row);
                if (template != null) last.Resources.Add(resource);
            }

            // dp: state = current parser template -> (cost, encoded entries)
            var states = new Dictionary<string, Tuple<int, List<string>>>
            {
                { DefaultRefineryTemplate, Tuple.Create(0, new List<string>()) }
            };
            foreach (var run in runs)
            {
                var next = new Dictionary<string, Tuple<int, List<string>>>();
                foreach (var state in states)
                {
                    // literal rows, template unchanged
                    var literal = run.Rows.Select(r => string.Join("=", r.Fields)).ToList();
                    Offer(next, state.Key, state.Value, literal);

                    if (run.Template == null) continue;
                    var withTemplate = new List<string>();
                    if (run.Template != state.Key) withTemplate.Add(run.Template);
                    withTemplate.Add(string.Join(" ", run.Resources));
                    Offer(next, run.Template, state.Value, withTemplate);
                }
                states = next;
            }
            entries.AddRange(states.Values.OrderBy(v => v.Item1).First().Item2);
        }

        static void Offer(Dictionary<string, Tuple<int, List<string>>> states, string state,
            Tuple<int, List<string>> previous, List<string> added)
        {
            var cost = previous.Item1 + added.Sum(e => e.Length + 1);
            Tuple<int, List<string>> existing;
            if (states.TryGetValue(state, out existing) && existing.Item1 <= cost) return;
            states[state] = Tuple.Create(cost, previous.Item2.Concat(added).ToList());
        }

        /// <summary>
        /// Longest resource name (at least 3 chars, no spaces) contained in all three fields,
        /// e.g. CrushedIronOreToIngot | Ore CrushedIron | Ingot Iron -> "Crushed*OreToIngot=Ore Crushed*=Ingot *", "Iron".
        /// </summary>
        static string FindTemplate(string name, string input, string output, out string resource)
        {
            resource = null;
            for (int length = name.Length; length >= 3; length--)
                for (int start = 0; start + length <= name.Length; start++)
                {
                    var x = name.Substring(start, length);
                    if (!input.Contains(x) || !output.Contains(x)) continue;
                    var template = name.Replace(x, "*") + "=" + input.Replace(x, "*") + "=" + output.Replace(x, "*");
                    if (template.Replace("*", x) != name + "=" + input + "=" + output) continue;
                    resource = x;
                    return template;
                }
            return null;
        }

        // ---------------------------------------------------------------- output file

        public static string GenerateSource(IList<ModData> mods)
        {
            var sb = new StringBuilder();
            sb.Append("// <auto-generated>\n");
            sb.Append("// Generated by PIM1.0.BluePrintGenerator from PIM1.0/BluePrintData/*.txt. Do not edit,\n");
            sb.Append("// change the data files instead. Format: see BluePrintsClasses/BluePrintDefinitions.cs\n");
            sb.Append("// </auto-generated>\n");
            sb.Append("namespace IngameScript\n{\n    partial class Program\n    {\n");
            sb.Append("        public static readonly string[] BluePrintMods =\n        {\n");
            foreach (var mod in mods) sb.Append("            ").Append(Quote(mod.Name)).Append(",\n");
            sb.Append("        };\n\n");
            sb.Append("        public static readonly string[] BluePrintData =\n        {\n");
            foreach (var mod in mods)
            {
                sb.Append("            // ").Append(mod.Name).Append('\n');
                sb.Append("            ").Append(Quote(EncodeMod(mod))).Append(",\n");
            }
            sb.Append("        };\n    }\n}\n");
            return sb.ToString();
        }

        static string Quote(string s) => "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
    }
}
