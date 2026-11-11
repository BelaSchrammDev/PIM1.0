using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PimBluePrintGenerator;

namespace PimBluePrintTool
{
    /// <summary>
    /// Changes data files line by line: comments, conditions, display names and the order of
    /// existing rows stay untouched. Fixed rows are replaced in place, new rows are appended
    /// to the end of the first unconditional section of the right kind.
    /// </summary>
    public static class DataFileEditor
    {
        public const string NewRowsComment = "# new (BluePrintTool)";

        public static string FormatRow(string[] fields)
        {
            var list = fields.ToList();
            while (list.Count > 0 && list[list.Count - 1] == "") list.RemoveAt(list.Count - 1);
            return string.Join(" | ", list);
        }

        /// <summary>Applies all fixable findings of one mod, returns the number of changed rows.</summary>
        public static int Apply(ModData mod, IEnumerable<Finding> findings)
        {
            var lines = File.ReadAllLines(mod.File).ToList();
            var fixes = findings.Where(f => f.IsFixable && f.Mod == mod).ToList();

            // replacements (line numbers stay valid, nothing is inserted yet)
            foreach (var f in fixes.Where(f => f.Row != null))
                lines[f.Row.Line - 1] = FormatRow(f.Fix);

            // new rows: grouped by target section, inserted bottom-up
            var inserts = new List<Tuple<int, List<string>>>();
            var newSections = new List<List<string>>();
            foreach (var group in fixes.Where(f => f.Row == null).GroupBy(f => f.IsRefinery).OrderBy(g => g.Key))
            {
                var rows = new List<string> { NewRowsComment };
                rows.AddRange(group.Select(f => FormatRow(f.Fix)));

                var section = mod.Sections.FirstOrDefault(s => !s.IsIgnore && s.IsRefinery == group.Key && s.ConditionMod == null);
                if (section == null)
                {
                    rows.Insert(0, group.Key ? "[Refinery]" : "[Assembler]");
                    newSections.Add(rows);
                }
                else
                {
                    // after the last row of the section (or its header)
                    var last = section.Rows.Count > 0 ? section.Rows.Max(r => r.Line) : section.HeaderLine;
                    inserts.Add(Tuple.Create(last, rows));
                }
            }
            foreach (var insert in inserts.OrderByDescending(i => i.Item1))
                lines.InsertRange(insert.Item1, insert.Item2);
            // missing sections at the end of the file
            foreach (var rows in newSections)
            {
                while (lines.Count > 0 && lines[lines.Count - 1].Trim() == "") lines.RemoveAt(lines.Count - 1);
                lines.Add("");
                lines.AddRange(rows);
            }

            File.WriteAllText(mod.File, string.Join("\n", lines).TrimEnd('\n') + "\n");
            return fixes.Count;
        }

        /// <summary>Header of a new mod data file (same as the converted files).</summary>
        public static string NewFileHeader(string modName) =>
$@"# PIM blueprint data for mod '{modName}'.
# Converted into BluePrintsClasses/Generated/BluePrintData.g.cs by PIM1.0.BluePrintGenerator.
#
# Sections:  [Assembler]  [Refinery]  optionally with condition: [Assembler if !OtherMod] / [Assembler if OtherMod]
# Assembler: ItemType | BlueprintSubtype [| ItemSubtype] [| DisplayName]
#            ItemSubtype empty = derived from the blueprint subtype; DisplayName empty = derived from the item
# Refinery:  BlueprintSubtype | InputItem | OutputItem
# [Ignore]:  BlueprintSubtype (deliberately not used: not generated, not proposed by BluePrintTool)
# Created by PIM1.0.BluePrintTool from workshop item(s) {{0}}.
";

        public static void CreateModFile(string path, string modName, IEnumerable<string> workshopIds, List<Finding> newRows)
        {
            var text = string.Format(NewFileHeader(modName), string.Join(", ", workshopIds)) + "\n";
            foreach (var refinery in new[] { false, true })
            {
                var rows = newRows.Where(f => f.IsRefinery == refinery).ToList();
                if (rows.Count == 0 && refinery) continue;
                text += (refinery ? "[Refinery]" : "[Assembler]") + "\n";
                text += string.Concat(rows.Select(f => FormatRow(f.Fix) + "\n")) + "\n";
            }
            File.WriteAllText(path, text);
        }

        /// <summary>Appends "Name | ids" to Mods.txt.</summary>
        public static void AddToModList(string dataDir, string modName, IEnumerable<string> workshopIds)
        {
            var path = Path.Combine(dataDir, BluePrintDataFile.ModListFileName);
            var text = File.ReadAllText(path).Replace("\r\n", "\n").TrimEnd('\n');
            File.WriteAllText(path, text + "\n" + modName + " | " + string.Join(", ", workshopIds) + "\n");
        }
    }
}
