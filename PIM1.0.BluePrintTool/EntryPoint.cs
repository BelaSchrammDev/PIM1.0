using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PimBluePrintGenerator;

namespace PimBluePrintTool
{
    /// <summary>
    /// PIM1.0.BluePrintTool: keeps PIM1.0/BluePrintData/*.txt in line with the game and mod definitions.
    ///
    ///   check  [mod ...]                 report differences (exit code 1 if something is fixable or missing)
    ///   update [mod ...] [--write]       fix renamed blueprints / wrong items, add new blueprints
    ///                                    (without --write: only show what would change)
    ///   new    Name id [id ...] [--write] create the data file of a new mod and add it to Mods.txt
    ///   scan   [--titles]                list installed workshop items with blueprints usable by PIM
    ///   download mod|id ... [--steamcmd path]   download workshop items with steamcmd (anonymous)
    ///
    /// Options: --data dir (BluePrintData folder, default: found from the current directory),
    ///          --game dir (SpaceEngineers or its Content/Data folder), --workshop dir (more workshop roots),
    ///          --steamcmd path, --titles (fetch workshop titles from the Steam Web API), --verbose
    ///
    /// After changes: build PIM1.0 (regenerates BluePrintData.g.cs), review the golden test diff and
    /// update it with PIM_UPDATE_GOLDEN=1.
    /// </summary>
    static class EntryPoint
    {
        sealed class Options
        {
            public string Command;
            public readonly List<string> Arguments = new List<string>();
            public string DataDir, GameDir, SteamCmd;
            public readonly List<string> Workshop = new List<string>();
            public bool Write, Titles, Verbose;
        }

        static int Main(string[] args)
        {
            Options o;
            try { o = Parse(args); }
            catch (ArgumentException e) { Console.Error.WriteLine(e.Message); Usage(); return 2; }
            if (o.Command == null) { Usage(); return 2; }

            try
            {
                switch (o.Command)
                {
                    case "check": return Check(o, write: false, alwaysReport: true);
                    case "update": return Check(o, write: o.Write, alwaysReport: false);
                    case "new": return New(o);
                    case "scan": return Scan(o);
                    case "download": return Download(o);
                    default: Console.Error.WriteLine($"unknown command '{o.Command}'"); Usage(); return 2;
                }
            }
            catch (BluePrintDataException e)
            {
                Console.Error.WriteLine($"{e.Location}: error: {e.Message}");
                return 1;
            }
        }

        static void Usage()
        {
            Console.Error.WriteLine(
@"usage: PIM1.0.BluePrintTool <command> [arguments] [options]
  check  [mod ...]                   report differences between data files and game/mod definitions
  update [mod ...] [--write]         apply fixes and add new blueprints (dry run without --write)
  new    Name id [id ...] [--write]  create a data file for a new mod, add it to Mods.txt
  scan   [--titles]                  list installed workshop items with blueprints usable by PIM
  download mod|id ...                download workshop items with steamcmd
options: --data dir  --game dir  --workshop dir  --steamcmd path  --titles  --verbose");
        }

        static Options Parse(string[] args)
        {
            var o = new Options();
            for (int i = 0; i < args.Length; i++)
            {
                Func<string> value = () =>
                {
                    if (i + 1 >= args.Length) throw new ArgumentException($"{args[i]} needs a value");
                    return args[++i];
                };
                switch (args[i])
                {
                    case "--data": o.DataDir = value(); break;
                    case "--game": o.GameDir = value(); break;
                    case "--workshop": o.Workshop.Add(value()); break;
                    case "--steamcmd": o.SteamCmd = value(); break;
                    case "--write": o.Write = true; break;
                    case "--titles": o.Titles = true; break;
                    case "--verbose": o.Verbose = true; break;
                    default:
                        if (args[i].StartsWith("--")) throw new ArgumentException($"unknown option {args[i]}");
                        if (o.Command == null) o.Command = args[i];
                        else o.Arguments.Add(args[i]);
                        break;
                }
            }
            return o;
        }

        static string FindDataDir(Options o)
        {
            if (o.DataDir != null) return o.DataDir;
            for (var dir = new DirectoryInfo(Directory.GetCurrentDirectory()); dir != null; dir = dir.Parent)
            {
                var candidate = Path.Combine(dir.FullName, "PIM1.0", "BluePrintData");
                if (Directory.Exists(candidate)) return candidate;
                if (dir.Name == "BluePrintData" && File.Exists(Path.Combine(dir.FullName, BluePrintDataFile.ModListFileName))) return dir.FullName;
            }
            throw new BluePrintDataException(Directory.GetCurrentDirectory(), "PIM1.0/BluePrintData not found, use --data");
        }

        static GamePaths Paths(Options o)
        {
            var steamCmd = Steam.FindSteamCmd(o.SteamCmd);
            var paths = GamePaths.Detect(o.GameDir, o.Workshop, steamCmd == null ? null : Path.GetDirectoryName(steamCmd));
            if (paths.GameData == null || !Directory.Exists(paths.GameData))
                throw new BluePrintDataException(o.GameDir ?? "Steam", "Space Engineers not found, use --game");
            return paths;
        }

        static List<ModData> SelectMods(List<ModData> mods, List<string> names)
        {
            if (names.Count == 0) return mods;
            var unknown = names.Where(n => mods.All(m => m.Name != n)).ToList();
            if (unknown.Count > 0) throw new BluePrintDataException(BluePrintDataFile.ModListFileName, $"unknown mod(s): {string.Join(", ", unknown)}");
            return mods.Where(m => names.Contains(m.Name)).ToList();
        }

        // ------------------------------------------------------------ check / update

        static int Check(Options o, bool write, bool alwaysReport)
        {
            var dataDir = FindDataDir(o);
            var mods = BluePrintDataFile.Read(dataDir);
            var checker = new ModChecker(Paths(o), mods);
            var selected = SelectMods(mods, o.Arguments);

            var all = new List<Finding>();
            foreach (var mod in selected)
            {
                var findings = checker.Check(mod);
                all.AddRange(findings);
                Report(mod, findings, o.Verbose);
                if (write && findings.Any(f => f.IsFixable))
                    Console.WriteLine($"  -> {DataFileEditor.Apply(mod, findings)} change(s) written to {Path.GetFileName(mod.File)}");
            }

            foreach (var w in checker.Warnings.Distinct()) Console.WriteLine("warning: " + w);

            var fixable = all.Count(f => f.IsFixable);
            var missing = all.Count(f => f.Kind == FindingKind.MissingBlueprint);
            Console.WriteLine();
            Console.WriteLine($"{fixable} fixable, {missing} missing blueprint(s), {all.Count(f => f.Kind == FindingKind.Info)} info, " +
                              $"{all.Count(f => f.Kind == FindingKind.NoSource)} mod(s) without source");
            if (fixable > 0 && !write)
                Console.WriteLine(o.Command == "update" ? "dry run: add --write to apply the fixes" : "run 'update --write' to apply the fixes");
            if (write && fixable > 0)
                Console.WriteLine("now build PIM1.0 and update the golden test (PIM_UPDATE_GOLDEN=1) after reviewing the diff");

            return fixable + missing > 0 ? 1 : 0;
        }

        static void Report(ModData mod, List<Finding> findings, bool verbose)
        {
            var shown = findings.Where(f => verbose || f.Kind != FindingKind.Info).ToList();
            var infos = findings.Count - shown.Count;
            Console.WriteLine($"== {mod.Name}: {(findings.Count == 0 ? "ok" : $"{shown.Count} finding(s)")}{(infos > 0 ? $" (+{infos} info, --verbose)" : "")}");
            foreach (var f in shown)
            {
                var where = f.Row != null ? $"{Path.GetFileName(f.Row.File)}({f.Row.Line})" : "";
                Console.WriteLine($"  {Label(f.Kind),-9} {where,-30} {f.Message}");
                if (f.IsFixable && f.Row != null) Console.WriteLine($"  {"",-9} {"",-30} => {DataFileEditor.FormatRow(f.Fix)}");
            }
        }

        static string Label(FindingKind kind)
        {
            switch (kind)
            {
                case FindingKind.NoSource: return "SKIPPED";
                case FindingKind.MissingBlueprint: return "MISSING";
                case FindingKind.RenamedBlueprint: return "RENAMED";
                case FindingKind.WrongItem: return "ITEM";
                case FindingKind.WrongType: return "TYPE";
                case FindingKind.WrongRefinery: return "REFINERY";
                case FindingKind.NewBlueprint: return "NEW";
                default: return "INFO";
            }
        }

        // ------------------------------------------------------------ new

        static int New(Options o)
        {
            if (o.Arguments.Count < 2) throw new ArgumentException("new: expected Name and at least one workshop id");
            var name = o.Arguments[0];
            var ids = o.Arguments.Skip(1).ToList();
            if (name.Contains(' ') || ids.Any(i => !i.All(char.IsDigit))) throw new ArgumentException("new: invalid name or workshop id");

            var dataDir = FindDataDir(o);
            var mods = BluePrintDataFile.Read(dataDir);
            if (mods.Any(m => m.Name == name)) throw new BluePrintDataException(BluePrintDataFile.ModListFileName, $"mod '{name}' exists, use update");

            var mod = new ModData { Name = name, File = Path.Combine(dataDir, name + ".txt") };
            mod.WorkshopIds.AddRange(ids);
            var checker = new ModChecker(Paths(o), mods);
            var findings = checker.Check(mod);
            Report(mod, findings, o.Verbose);

            var rows = findings.Where(f => f.Kind == FindingKind.NewBlueprint).ToList();
            if (findings.Any(f => f.Kind == FindingKind.NoSource) || rows.Count == 0)
            {
                Console.WriteLine("nothing to create");
                return 1;
            }
            if (!o.Write)
            {
                Console.WriteLine($"dry run: add --write to create {name}.txt with {rows.Count} blueprint(s) and add it to Mods.txt");
                return 0;
            }
            DataFileEditor.CreateModFile(mod.File, name, ids, rows);
            DataFileEditor.AddToModList(dataDir, name, ids);
            Console.WriteLine($"{mod.File} created ({rows.Count} blueprints), added to Mods.txt. Build PIM1.0 and update the golden test.");
            return 0;
        }

        // ------------------------------------------------------------ scan

        static int Scan(Options o)
        {
            var dataDir = FindDataDir(o);
            var mods = BluePrintDataFile.Read(dataDir);
            var paths = Paths(o);
            var checker = new ModChecker(paths, mods);
            var vanilla = new HashSet<string>(checker.Vanilla.Blueprints.Select(b => b.Subtype));
            var vanillaModel = new GameModel(new[] { checker.Vanilla });

            var rows = new List<string[]>();
            foreach (var dir in paths.AllInstalledMods().OrderBy(d => Path.GetFileName(d)))
            {
                var id = Path.GetFileName(dir);
                var source = checker.Source(id, dir);
                var own = source.Blueprints.Where(b => !vanilla.Contains(b.Subtype)).ToList();
                if (own.Count == 0) continue;

                var model = new GameModel(new[] { checker.Vanilla, source });
                var assembler = own.Count(b => model.InAssembler(b.Subtype));
                var refinery = own.Count(b => model.InRefinery(b.Subtype));
                var pim = string.Join(", ", mods.Where(m => m.WorkshopIds.Contains(id)).Select(m => m.Name));
                rows.Add(new[] { id, pim == "" ? "-" : pim, assembler.ToString(), refinery.ToString(),
                                 string.Join(", ", own.Take(3).Select(b => b.Subtype)), "" });
            }

            if (o.Titles)
            {
                var titles = Steam.FetchTitles(rows.Select(r => r[0]).ToList());
                foreach (var r in rows) { string t; if (titles.TryGetValue(r[0], out t)) r[5] = t; }
            }

            Console.WriteLine($"{"workshop id",-12} {"PIM mod",-32} {"asm",4} {"ref",4}  {(o.Titles ? "title / " : "")}examples");
            foreach (var r in rows)
                Console.WriteLine($"{r[0],-12} {r[1],-32} {r[2],4} {r[3],4}  {(r[5] != "" ? r[5] + " / " : "")}{r[4]}");

            var notInstalled = mods.SelectMany(m => m.WorkshopIds.Select(id => new { m.Name, id })).Where(x => paths.FindMod(x.id) == null).ToList();
            foreach (var x in notInstalled) Console.WriteLine($"not downloaded: {x.id} ({x.Name})");
            foreach (var m in mods.Where(m => m.Name != IngameScript.Program.Strings.M_Vanilla && m.WorkshopIds.Count == 0))
                Console.WriteLine($"no workshop id in Mods.txt: {m.Name}");
            return 0;
        }

        // ------------------------------------------------------------ download

        static int Download(Options o)
        {
            var steamCmd = Steam.FindSteamCmd(o.SteamCmd);
            if (steamCmd == null)
            {
                Console.Error.WriteLine("steamcmd not found: install it (https://developer.valvesoftware.com/wiki/SteamCMD) and use --steamcmd path or add it to PATH");
                return 2;
            }
            var mods = BluePrintDataFile.Read(FindDataDir(o));
            var ids = new List<string>();
            foreach (var arg in o.Arguments)
            {
                if (arg.All(char.IsDigit)) { ids.Add(arg); continue; }
                var mod = mods.FirstOrDefault(m => m.Name == arg);
                if (mod == null) throw new BluePrintDataException(BluePrintDataFile.ModListFileName, $"unknown mod '{arg}'");
                if (mod.WorkshopIds.Count == 0) Console.WriteLine($"{arg}: no workshop id in Mods.txt");
                ids.AddRange(mod.WorkshopIds);
            }
            if (ids.Count == 0) { Console.Error.WriteLine("download: nothing to download"); return 2; }
            return Steam.Download(steamCmd, ids.Distinct());
        }
    }
}
