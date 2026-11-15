using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PimBluePrintTool;
using SmsItemDataGenerator;
using ItemDataParser = IngameScript.Program.ItemDataParser;
using ItemNames = IngameScript.Program.ItemNames;

namespace SmsItemDataTool
{
    /// <summary>
    /// SMS1.ItemDataTool: keeps SMS1/ItemData/*.txt in line with the item definitions of the game and the mods.
    ///
    ///   check  [mod ...]              report items missing in the data files and data items the game/mod does not have
    ///   update [mod ...] [--write]    add the missing items to the data files (without --write: only show them)
    ///
    /// Options: --data dir (ItemData folder, default: found from the current directory),
    ///          --game dir (SpaceEngineers or its Content/Data folder), --workshop dir (more workshop roots)
    ///
    /// Vanilla is compared with the game data, a mod with its workshop folder (ids in Mods.txt).
    /// New items are appended with a default max and the English display name: check them.
    /// After changes: build SMS1 (regenerates ItemData.g.cs), review the golden test diff and
    /// update it with SMS_UPDATE_GOLDEN=1.
    /// </summary>
    static class EntryPoint
    {
        sealed class Options
        {
            public string Command, DataDir, GameDir;
            public readonly List<string> Mods = new List<string>();
            public readonly List<string> Workshop = new List<string>();
            public bool Write;
        }

        // max of a new item (the fillbar of the inventory command), by item type
        static readonly Dictionary<string, int> DefaultMax = new Dictionary<string, int>
        {
            { "Ore", 1000 }, { "Ingot", 1000 }, { "Component", 100 }, { "PhysicalGunObject", 2 }, { "ConsumableItem", 5 },
            { "AmmoMagazine", 100 }, { "GasContainerObject", 5 }, { "OxygenContainerObject", 5 }, { "PhysicalObject", 100 },
            { "Datapad", 5 }, { "SeedItem", 10 },
        };

        static int Main(string[] args)
        {
            Options o;
            try { o = Parse(args); }
            catch (ArgumentException e) { Console.Error.WriteLine(e.Message); Usage(); return 2; }

            try
            {
                return Run(o);
            }
            catch (ItemDataException e)
            {
                Console.Error.WriteLine($"{e.Location}: error: {e.Message}");
                return 1;
            }
        }

        static void Usage()
        {
            Console.Error.WriteLine(
@"usage: SMS1.ItemDataTool <command> [mod ...] [options]
  check  [mod ...]              report missing and unknown items
  update [mod ...] [--write]    add missing items to the data files (dry run without --write)
options: --data dir  --game dir  --workshop dir");
        }

        static Options Parse(string[] args)
        {
            var o = new Options();
            for (var i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--data": o.DataDir = Value(args, ref i); break;
                    case "--game": o.GameDir = Value(args, ref i); break;
                    case "--workshop": o.Workshop.Add(Value(args, ref i)); break;
                    case "--write": o.Write = true; break;
                    default:
                        if (args[i].StartsWith("--")) throw new ArgumentException($"unknown option {args[i]}");
                        if (o.Command == null) o.Command = args[i]; else o.Mods.Add(args[i]);
                        break;
                }
            }
            if (o.Command != "check" && o.Command != "update") throw new ArgumentException("command check or update expected");
            return o;
        }

        static string Value(string[] args, ref int i)
        {
            if (i + 1 >= args.Length) throw new ArgumentException($"{args[i]} needs a value");
            return args[++i];
        }

        static int Run(Options o)
        {
            var dataDir = o.DataDir ?? FindDataDir();
            var mods = ItemDataFile.Read(dataDir);
            var paths = GamePaths.Detect(o.GameDir, o.Workshop, null);
            if (paths.GameData == null || !Directory.Exists(paths.GameData))
            {
                Console.Error.WriteLine("Space Engineers not found, use --game");
                return 2;
            }

            foreach (var name in o.Mods)
                if (!mods.Any(m => m.Name == name)) throw new ArgumentException($"mod '{name}' is not in Mods.txt");

            var vanilla = SbcItems.Read(paths.GameData);
            var vanillaKeys = new HashSet<string>(vanilla.Items.Select(i => i.Key));
            var result = 0;

            foreach (var mod in mods.Where(m => o.Mods.Count == 0 || o.Mods.Contains(m.Name)))
            {
                SbcItems sbc;
                if (mod.Name == "Vanilla") sbc = vanilla;
                else
                {
                    var dirs = mod.WorkshopIds.Select(paths.FindMod).ToList();
                    if (mod.WorkshopIds.Count == 0 || dirs.Any(d => d == null))
                    {
                        Console.WriteLine($"== {mod.Name}: not downloaded (workshop id {string.Join(", ", mod.WorkshopIds)}), skipped");
                        continue;
                    }
                    sbc = new SbcItems();
                    foreach (var dir in dirs)
                    {
                        var part = SbcItems.Read(dir);
                        sbc.Items.AddRange(part.Items);
                        sbc.Warnings.AddRange(part.Warnings);
                    }
                }

                var dataKeys = new HashSet<string>();
                ItemDataParser.Parse(ItemDataEncoder.EncodeMod(mod), (type, subtype, max, n, category, capacity) => dataKeys.Add(type + "/" + subtype));

                // mod items that are vanilla items too (a mod changes them) are no new items of the mod
                var missing = sbc.Items.Where(i => !i.Hidden && !dataKeys.Contains(i.Key) && (mod.Name == "Vanilla" || !vanillaKeys.Contains(i.Key))).ToList();
                var sbcKeys = new HashSet<string>(sbc.Items.Select(i => i.Key));
                var unknown = dataKeys.Where(k => !sbcKeys.Contains(k) && !vanillaKeys.Contains(k)).ToList();
                var hidden = sbc.Items.Where(i => i.Hidden && dataKeys.Contains(i.Key)).ToList();

                Console.WriteLine($"== {mod.Name}: {dataKeys.Count} items in the data file, {sbc.Items.Count(i => !i.Hidden)} in the {(mod.Name == "Vanilla" ? "game" : "mod")}");
                foreach (var w in sbc.Warnings) Console.WriteLine("  warning: " + w);
                foreach (var k in unknown) Console.WriteLine($"  unknown:  {k} (not defined by the {(mod.Name == "Vanilla" ? "game" : "mod")}: removed or renamed?)");
                foreach (var i in hidden) Console.WriteLine($"  hidden:   {i.Key} (Public=false)");

                if (missing.Count == 0) continue;
                result = 1;
                var rows = ProposedRows(missing);
                Console.WriteLine($"  missing:  {missing.Count} items" + (o.Command == "update" && o.Write ? ", added to " + Path.GetFileName(mod.File) : ""));
                foreach (var line in rows) Console.WriteLine("    " + line);

                if (o.Command == "update" && o.Write)
                {
                    var text = File.ReadAllText(mod.File).TrimEnd('\r', '\n');
                    var nl = text.Contains("\r\n") ? "\r\n" : "\n";
                    File.WriteAllText(mod.File, text + nl + nl
                        + "# added by SMS1.ItemDataTool " + DateTime.Now.ToString("yyyy-MM-dd") + ": check max, names and categories" + nl
                        + string.Join(nl, rows) + nl);
                }
            }
            if (result == 1 && !(o.Command == "update" && o.Write)) Console.WriteLine("add the missing items with: update [mod ...] --write");
            return result;
        }

        /// <summary>Rows in the data file format, grouped by item type in the order of the definitions.</summary>
        static List<string> ProposedRows(List<SbcItem> items)
        {
            var rows = new List<string>();
            foreach (var group in items.GroupBy(i => i.Type))
            {
                if (rows.Count > 0) rows.Add("");
                rows.Add("[" + group.Key + "]");
                foreach (var i in group)
                {
                    var name = Clean(i.DisplayName);
                    // the script derives "Ironore" / "SteelPlate": a name is only needed if it is different;
                    // the subtype itself is "@" ("Organic" instead of the derived "Organicore")
                    if (name == ItemNames.Build("", i.Subtype, i.Type == "Ore", i.Type == "Ingot")) name = "";
                    else if (name == i.Subtype) name = "@";
                    var fields = new List<string> { i.Subtype, DefaultMax[i.Type].ToString(), name };
                    if (i.Type == "AmmoMagazine" && i.Capacity > 0) fields.Add(i.Capacity.ToString());
                    while (fields.Count > 2 && fields[fields.Count - 1] == "") fields.RemoveAt(fields.Count - 1);
                    rows.Add(string.Join(" | ", fields));
                }
            }
            return rows;
        }

        // the characters that separate fields in the compact format are not allowed in names
        static string Clean(string name) => string.Join(" ", name.Split(new[] { ',', ':', '"', '\\', '|' }, StringSplitOptions.RemoveEmptyEntries)).Trim();

        static string FindDataDir()
        {
            for (var dir = new DirectoryInfo(Directory.GetCurrentDirectory()); dir != null; dir = dir.Parent)
            {
                var candidate = Path.Combine(dir.FullName, "SMS1", "ItemData");
                if (File.Exists(Path.Combine(candidate, ItemDataFile.ModListFileName))) return candidate;
            }
            throw new ArgumentException("SMS1/ItemData not found, use --data");
        }
    }
}
