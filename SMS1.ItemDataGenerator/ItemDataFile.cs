using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ItemDataParser = IngameScript.Program.ItemDataParser;

namespace SmsItemDataGenerator
{
    public class ItemDataException : Exception
    {
        /// <summary>"file(line)" or "file", for MSBuild-style error output.</summary>
        public readonly string Location;

        public ItemDataException(string file, int line, string message)
            : base(message) { Location = $"{file}({line})"; }

        public ItemDataException(string file, string message)
            : base(message) { Location = file; }

        public override string ToString() => $"{Location}: {Message}";
    }

    /// <summary>One item row: Subtype | Max [| Name] [| AmmoCapacity], Resource: Subtype | OreMax | IngotMax [| OreName] [| IngotName].</summary>
    public sealed class ItemRow
    {
        public string[] Fields;
        public string File;
        public int Line;

        public string Field(int index) => index < Fields.Length ? Fields[index] : "";
        public ItemDataException Error(string message) => new ItemDataException(File, Line, message);
    }

    public sealed class ItemSection
    {
        public string Type;
        /// <summary>"" = none.</summary>
        public string Category = "";
        public int HeaderLine;
        public readonly List<ItemRow> Rows = new List<ItemRow>();
        public bool IsResource => Type == "Resource";
    }

    public sealed class ModItemData
    {
        public string Name;
        public string File;
        /// <summary>Steam workshop ids from Mods.txt (only for the check tool).</summary>
        public readonly List<string> WorkshopIds = new List<string>();
        public readonly List<ItemSection> Sections = new List<ItemSection>();
    }

    /// <summary>
    /// Reads the item data directory:
    ///   Mods.txt     load order, one mod per line: Name [| WorkshopId, WorkshopId, ...]
    ///   &lt;Mod&gt;.txt    one file per mod (see the header of the files for the format)
    /// Lines starting with '#' are comments.
    /// </summary>
    public static class ItemDataFile
    {
        public const string ModListFileName = "Mods.txt";

        // these separate entries/fields in the compact format or would break the C# string
        static readonly char[] ForbiddenChars = { ',', ':', '"', '\\' };

        public static List<ModItemData> Read(string dataDir)
        {
            var modList = Path.Combine(dataDir, ModListFileName);
            if (!File.Exists(modList)) throw new ItemDataException(modList, "file not found");

            var mods = new List<ModItemData>();
            var lineNo = 0;
            foreach (var raw in File.ReadAllLines(modList))
            {
                lineNo++;
                var line = raw.Trim();
                if (line == "" || line.StartsWith("#")) continue;
                var parts = line.Split('|');
                var mod = new ModItemData { Name = parts[0].Trim(), File = Path.Combine(dataDir, parts[0].Trim() + ".txt") };
                if (mods.Any(m => m.Name == mod.Name)) throw new ItemDataException(modList, lineNo, $"mod '{mod.Name}' listed twice");
                if (parts.Length > 1)
                    mod.WorkshopIds.AddRange(parts[1].Split(',').Select(s => s.Trim()).Where(s => s != ""));
                mods.Add(mod);
            }
            if (mods.Count == 0 || mods[0].Name != "Vanilla") throw new ItemDataException(modList, "the first mod must be 'Vanilla'");

            foreach (var mod in mods) ReadMod(mod);
            return mods;
        }

        static void ReadMod(ModItemData mod)
        {
            if (!File.Exists(mod.File)) throw new ItemDataException(mod.File, "file not found (listed in Mods.txt)");
            ItemSection section = null;
            var lineNo = 0;
            foreach (var raw in File.ReadAllLines(mod.File))
            {
                lineNo++;
                var line = raw.Trim();
                if (line == "" || line.StartsWith("#")) continue;

                if (line.StartsWith("["))
                {
                    if (!line.EndsWith("]")) throw new ItemDataException(mod.File, lineNo, $"section header without ']': {line}");
                    var parts = line.Substring(1, line.Length - 2).Split('|').Select(s => s.Trim()).ToArray();
                    if (Array.IndexOf(ItemDataParser.TypeNames, parts[0]) < 0)
                        throw new ItemDataException(mod.File, lineNo, $"unknown item type '{parts[0]}', known: {string.Join(", ", ItemDataParser.TypeNames)}");
                    if (parts.Length > 2) throw new ItemDataException(mod.File, lineNo, "section header: [ItemType] or [ItemType | Category]");
                    section = new ItemSection { Type = parts[0], Category = parts.Length > 1 ? parts[1] : "", HeaderLine = lineNo };
                    CheckChars(mod.File, lineNo, section.Category);
                    mod.Sections.Add(section);
                    continue;
                }

                if (section == null) throw new ItemDataException(mod.File, lineNo, "item row before the first section header");
                var row = new ItemRow { Fields = line.Split('|').Select(s => s.Trim()).ToArray(), File = mod.File, Line = lineNo };
                CheckRow(section, row);
                section.Rows.Add(row);
            }
        }

        static void CheckRow(ItemSection section, ItemRow row)
        {
            var f = row.Fields;
            foreach (var field in f) CheckChars(row.File, row.Line, field);
            if (f[0] == "") throw row.Error("subtype missing");

            if (section.IsResource)
            {
                if (f.Length < 3 || f.Length > 5) throw row.Error("resource row: Subtype | OreMax | IngotMax [| OreName] [| IngotName]");
                CheckNumber(row, 1, "ore max");
                CheckNumber(row, 2, "ingot max");
                if (f[0].EndsWith("*")) throw row.Error("'*' (tools MK1..MK4) is not possible in a Resource section");
                return;
            }

            if (f.Length < 2 || f.Length > 4) throw row.Error("row: Subtype | Max [| DisplayName] [| AmmoCapacity]");
            CheckNumber(row, 1, "max");
            if (f.Length > 3)
            {
                CheckNumber(row, 3, "ammo capacity");
                if (f[0].EndsWith("*")) throw row.Error("'*' (tools MK1..MK4) cannot have an ammo capacity");
            }
        }

        static void CheckNumber(ItemRow row, int index, string what)
        {
            int value;
            if (!int.TryParse(row.Fields[index], out value) || value < 0) throw row.Error($"{what} must be a number >= 0: '{row.Fields[index]}'");
        }

        static void CheckChars(string file, int line, string field)
        {
            if (field.IndexOfAny(ForbiddenChars) >= 0)
                throw new ItemDataException(file, line, $"'{field}': the characters , : \" \\ are not allowed");
        }
    }
}
