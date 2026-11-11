using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PimBluePrintGenerator
{
    public class BluePrintDataException : Exception
    {
        /// <summary>"file(line)" or "file", for MSBuild-style error output.</summary>
        public readonly string Location;

        public BluePrintDataException(string file, int line, string message)
            : base(message) { Location = $"{file}({line})"; }

        public BluePrintDataException(string file, string message)
            : base(message) { Location = file; }

        public override string ToString() => $"{Location}: {Message}";
    }

    /// <summary>One data row with its origin, for error messages.</summary>
    public sealed class DataRow
    {
        public string[] Fields;
        public string File;
        public int Line;

        public string Field(int index) => index < Fields.Length ? Fields[index] : "";
        public BluePrintDataException Error(string message) => new BluePrintDataException(File, Line, message);
    }

    public sealed class DataSection
    {
        /// <summary>Line of the section header (1-based).</summary>
        public int HeaderLine;
        public bool IsRefinery;
        /// <summary>null = unconditional.</summary>
        public string ConditionMod;
        /// <summary>true: section is used if ConditionMod is enabled, false: if it is disabled.</summary>
        public bool ConditionEnabled;
        public readonly List<DataRow> Rows = new List<DataRow>();
    }

    public sealed class ModData
    {
        public string Name;
        /// <summary>Full path of the mod data file.</summary>
        public string File;
        /// <summary>Steam workshop ids from Mods.txt (only used by PIM1.0.BluePrintTool).</summary>
        public readonly List<string> WorkshopIds = new List<string>();
        public readonly List<DataSection> Sections = new List<DataSection>();
    }

    /// <summary>
    /// Reads the blueprint data directory:
    ///   Mods.txt       load order, one mod per line: Name [| WorkshopId, WorkshopId, ...]
    ///   &lt;Mod&gt;.txt      one file per mod (see the header of the files for the format)
    /// Lines starting with '#' are comments.
    /// </summary>
    public static class BluePrintDataFile
    {
        public const string ModListFileName = "Mods.txt";

        public static readonly Dictionary<string, char> AssemblerTypeCodes = new Dictionary<string, char>
        {
            // must match BpTypeCodes / BpTypeNames in PIM1.0/BluePrintsClasses/BluePrintDefinitions.cs
            { "Component", 'C' },
            { "AmmoMagazine", 'A' },
            { "PhysicalGunObject", 'T' },
            { "ConsumableItem", 'K' },
            { "OxygenContainerObject", 'O' },
            { "GasContainerObject", 'H' },
            { "Datapad", 'D' },
            { "SeedItem", 'S' },
            { "Ingot", 'I' },
            { "Ore", 'E' },
        };

        public static List<ModData> Read(string dataDir)
        {
            var modListPath = Path.Combine(dataDir, ModListFileName);
            if (!File.Exists(modListPath)) throw new BluePrintDataException(modListPath, "file not found");

            var modNames = new List<string>();
            var workshopIds = new Dictionary<string, List<string>>();
            foreach (var row in ReadLines(modListPath))
            {
                var name = row.Fields[0];
                if (row.Fields.Length > 2 || name.Length == 0 || name.Contains(' ')) throw row.Error("expected 'ModName [| WorkshopId, WorkshopId, ...]'");
                if (modNames.Contains(name)) throw row.Error($"mod '{name}' listed twice");
                if (!File.Exists(Path.Combine(dataDir, name + ".txt"))) throw row.Error($"file {name}.txt not found");

                var ids = row.Field(1).Split(',').Select(i => i.Trim()).Where(i => i.Length > 0).ToList();
                if (ids.Any(i => !i.All(char.IsDigit))) throw row.Error("workshop ids must be numbers");
                modNames.Add(name);
                workshopIds[name] = ids;
            }

            if (modNames.Count == 0 || modNames[0] != IngameScript.Program.Strings.M_Vanilla)
                throw new BluePrintDataException(modListPath, 1, $"the first mod must be '{IngameScript.Program.Strings.M_Vanilla}'");

            foreach (var file in Directory.GetFiles(dataDir, "*.txt"))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                if (name != Path.GetFileNameWithoutExtension(ModListFileName) && !modNames.Contains(name))
                    throw new BluePrintDataException(file, $"mod '{name}' is missing in {ModListFileName}");
            }

            var mods = modNames.Select(n => ReadMod(Path.Combine(dataDir, n + ".txt"), n, modNames)).ToList();
            foreach (var mod in mods) mod.WorkshopIds.AddRange(workshopIds[mod.Name]);

            if (!modNames.Contains(IngameScript.Program.Strings.M_IndustrialOverhaulMod))
                throw new BluePrintDataException(modListPath, $"mod '{IngameScript.Program.Strings.M_IndustrialOverhaulMod}' is required by the script code");

            return mods;
        }

        static ModData ReadMod(string path, string name, List<string> allMods)
        {
            var mod = new ModData { Name = name, File = Path.GetFullPath(path) };
            DataSection section = null;

            foreach (var row in ReadLines(path))
            {
                var first = row.Fields[0];
                if (first.StartsWith("["))
                {
                    section = ParseSectionHeader(row, allMods, name);
                    section.HeaderLine = row.Line;
                    mod.Sections.Add(section);
                    continue;
                }
                if (section == null) throw row.Error("data before the first section header ([Assembler] or [Refinery])");

                if (section.IsRefinery) ValidateRefineryRow(row);
                else ValidateAssemblerRow(row);
                section.Rows.Add(row);
            }
            return mod;
        }

        static DataSection ParseSectionHeader(DataRow row, List<string> allMods, string modName)
        {
            var text = string.Join("|", row.Fields);
            if (row.Fields.Length != 1 || !text.EndsWith("]")) throw row.Error("invalid section header");

            var parts = text.Substring(1, text.Length - 2).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var section = new DataSection();

            if (parts.Length == 0) throw row.Error("invalid section header");
            if (parts[0] == "Assembler") section.IsRefinery = false;
            else if (parts[0] == "Refinery") section.IsRefinery = true;
            else throw row.Error($"unknown section '{parts[0]}', expected Assembler or Refinery");

            if (parts.Length == 1) return section;
            if (parts.Length != 3 || parts[1] != "if") throw row.Error("expected [Section] or [Section if Mod] or [Section if !Mod]");

            var condition = parts[2];
            section.ConditionEnabled = !condition.StartsWith("!");
            section.ConditionMod = condition.TrimStart('!');
            if (!allMods.Contains(section.ConditionMod)) throw row.Error($"unknown mod '{section.ConditionMod}' in condition");
            if (section.ConditionMod == modName) throw row.Error("a section cannot depend on its own mod");
            return section;
        }

        static void ValidateAssemblerRow(DataRow row)
        {
            if (row.Fields.Length < 2 || row.Fields.Length > 4)
                throw row.Error("assembler row: expected 'ItemType | BlueprintSubtype [| ItemSubtype] [| DisplayName]'");
            if (!AssemblerTypeCodes.ContainsKey(row.Fields[0]))
                throw row.Error($"unknown item type '{row.Fields[0]}', known: {string.Join(", ", AssemblerTypeCodes.Keys)}");

            var subtype = row.Field(1);
            if (subtype.Length < 2) throw row.Error("blueprint subtype must have at least 2 characters");
            if (subtype.StartsWith("#") || subtype.StartsWith("[")) throw row.Error("blueprint subtype must not start with '#' or '['");
            if (subtype.Contains(' ')) throw row.Error("blueprint subtype must not contain spaces");
            if (subtype.StartsWith("Position0") && subtype.IndexOf('_') < 0) throw row.Error("'Position0...' subtype without '_'");

            int dummy;
            if (int.TryParse(row.Field(2), out dummy)) throw row.Error("item subtype must not be a number");

            for (int i = 1; i < row.Fields.Length; i++) CheckChars(row, row.Fields[i]);
        }

        static void ValidateRefineryRow(DataRow row)
        {
            if (row.Fields.Length != 3) throw row.Error("refinery row: expected 'BlueprintSubtype | InputItem | OutputItem'");
            foreach (var f in row.Fields)
            {
                if (f.Length == 0) throw row.Error("empty field");
                CheckChars(row, f);
            }
            if (row.Fields[0].Contains(' ')) throw row.Error("blueprint subtype must not contain spaces");
        }

        static void CheckChars(DataRow row, string field)
        {
            foreach (var c in new[] { ',', '=', '*', '"', '\\' })
                if (field.Contains(c)) throw row.Error($"character '{c}' is not allowed");
        }

        static IEnumerable<DataRow> ReadLines(string path)
        {
            var lines = File.ReadAllLines(path);
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;
                yield return new DataRow
                {
                    Fields = line.Split('|').Select(f => f.Trim()).ToArray(),
                    File = Path.GetFullPath(path),
                    Line = i + 1,
                };
            }
        }
    }
}
