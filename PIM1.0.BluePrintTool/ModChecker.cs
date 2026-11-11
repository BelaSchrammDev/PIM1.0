using System;
using System.Collections.Generic;
using System.Linq;
using PimBluePrintGenerator;
using AssemblerBluePrint = IngameScript.Program.AssemblerBluePrint;

namespace PimBluePrintTool
{
    public enum FindingKind
    {
        /// <summary>The mod has no workshop id or is not downloaded: nothing checked.</summary>
        NoSource,
        /// <summary>Blueprint does not exist (anymore), no unique replacement found.</summary>
        MissingBlueprint,
        /// <summary>Blueprint renamed (e.g. new Position number), fixable.</summary>
        RenamedBlueprint,
        /// <summary>PIM derives another item than the blueprint produces, fixable.</summary>
        WrongItem,
        /// <summary>Item type differs, fixable, but the PIM key (Type Subtype) changes.</summary>
        WrongType,
        /// <summary>Refinery input/output differs, fixable.</summary>
        WrongRefinery,
        /// <summary>Not usable by PIM (several results, unknown item type, not producible), info only.</summary>
        Info,
        /// <summary>Blueprint of the mod that is not in any data file yet, fixable (new row).</summary>
        NewBlueprint,
    }

    public sealed class Finding
    {
        public FindingKind Kind;
        public ModData Mod;
        /// <summary>Affected row, null for new blueprints and mod-level findings.</summary>
        public DataRow Row;
        public string Message;
        /// <summary>Replacement fields for Row, or the fields of the new row. null: no automatic fix.</summary>
        public string[] Fix;
        /// <summary>For new blueprints: target section kind.</summary>
        public bool IsRefinery;

        public bool IsFixable => Fix != null;
    }

    /// <summary>Compares the data files with the game and mod definitions.</summary>
    public sealed class ModChecker
    {
        /// <summary>
        /// Blueprints of refinery-type blocks (Hydroponics, Reprocessor, Water Recycling System)
        /// that PIM manages as assembler blueprints (see AssemblerBluePrint.IfRefineryBluePrint).
        /// </summary>
        public static readonly string[] AssemblerBlueprintsInRefineries = { "SubFresh", "SpentFuelReprocessing", "WaterFood" };

        /// <summary>Refinery blueprints PIM manages: ore/ingot processing (no disposal/incinerator recipes).</summary>
        public static readonly string[] RefineryInputTypes = { "Ore", "Ingot" };

        readonly GamePaths _paths;
        readonly List<ModData> _mods;
        readonly Dictionary<string, SbcSource> _sourceCache = new Dictionary<string, SbcSource>();
        readonly HashSet<string> _knownSubtypes;
        /// <summary>PIM keys ("Type Subtype") of all assembler rows in the data files.</summary>
        readonly Dictionary<string, string> _knownKeys = new Dictionary<string, string>();
        readonly HashSet<string> _proposed = new HashSet<string>();
        GameModel _vanillaModel, _allModsModel;

        public readonly List<string> Warnings = new List<string>();

        public ModChecker(GamePaths paths, List<ModData> mods)
        {
            _paths = paths;
            _mods = mods;
            // every blueprint subtype used in any data file
            _knownSubtypes = new HashSet<string>(mods.SelectMany(m => m.Sections)
                .SelectMany(s => s.Rows.Select(r => s.IsRefinery ? r.Field(0) : r.Field(1))));
            foreach (var mod in mods)
                foreach (var section in mod.Sections.Where(s => !s.IsRefinery))
                    foreach (var row in section.Rows)
                    {
                        var key = row.Field(0) + " " + StripPosition(row.Field(1));
                        if (!_knownKeys.ContainsKey(key)) _knownKeys[key] = mod.Name;
                    }
        }

        public SbcSource Vanilla => Source(IngameScript.Program.Strings.M_Vanilla, _paths.GameData);

        public SbcSource Source(string name, string directory)
        {
            SbcSource source;
            if (!_sourceCache.TryGetValue(name, out source))
            {
                source = SbcSource.Read(name, directory);
                _sourceCache[name] = source;
                Warnings.AddRange(source.Warnings);
            }
            return source;
        }

        /// <summary>Installed sources of a mod (one per downloaded workshop id).</summary>
        public List<SbcSource> SourcesOf(ModData mod) =>
            mod.WorkshopIds.Select(id => new { id, dir = _paths.FindMod(id) })
               .Where(x => x.dir != null)
               .Select(x => Source(x.id, x.dir))
               .ToList();

        GameModel VanillaModel => _vanillaModel ?? (_vanillaModel = new GameModel(new[] { Vanilla }));

        /// <summary>Vanilla + all installed mods of Mods.txt: for blueprints that depend on blocks of other mods.</summary>
        GameModel AllModsModel => _allModsModel ?? (_allModsModel = new GameModel(new[] { Vanilla }.Concat(
            _mods.Where(m => !IsVanilla(m)).SelectMany(SourcesOf).GroupBy(s => s.Name).Select(g => g.First()))));

        static bool IsVanilla(ModData mod) => mod.Name == IngameScript.Program.Strings.M_Vanilla;

        public List<Finding> Check(ModData mod)
        {
            var findings = new List<Finding>();
            if (_paths.GameData == null) throw new InvalidOperationException("game data folder not found, use --game");

            List<SbcSource> own;
            GameModel model;
            if (IsVanilla(mod))
            {
                own = new List<SbcSource> { Vanilla };
                model = VanillaModel;
            }
            else
            {
                if (mod.WorkshopIds.Count == 0)
                    return Single(mod, FindingKind.NoSource, "no workshop id in Mods.txt, not checked");
                own = SourcesOf(mod);
                if (own.Count == 0)
                    return Single(mod, FindingKind.NoSource, $"not downloaded ({string.Join(", ", mod.WorkshopIds)}), not checked");
                model = new GameModel(new[] { Vanilla }.Concat(own));
            }

            Func<string, bool> inAssembler = s => model.InAssembler(s) || AllModsModel.InAssembler(s);
            Func<string, bool> inRefinery = s => model.InRefinery(s) || AllModsModel.InRefinery(s);
            var ownSubtypes = new HashSet<string>(own.SelectMany(s => s.Blueprints).Select(b => b.Subtype));

            foreach (var section in mod.Sections)
                foreach (var row in section.Rows)
                {
                    var f = section.IsRefinery
                        ? CheckRefineryRow(mod, row, model, ownSubtypes, inRefinery)
                        : CheckAssemblerRow(mod, row, model, ownSubtypes, inAssembler, inRefinery);
                    if (f != null) findings.Add(f);
                }

            findings.AddRange(NewBlueprints(mod, own, model, inAssembler, inRefinery));
            return findings;
        }

        static List<Finding> Single(ModData mod, FindingKind kind, string message) =>
            new List<Finding> { new Finding { Mod = mod, Kind = kind, Message = message } };

        // ------------------------------------------------------------ assembler rows

        Finding CheckAssemblerRow(ModData mod, DataRow row, GameModel model, HashSet<string> ownSubtypes,
            Func<string, bool> inAssembler, Func<string, bool> inRefinery)
        {
            var fields = new[] { row.Field(0), row.Field(1), row.Field(2), row.Field(3) };
            var messages = new List<string>();
            var kind = FindingKind.Info;
            var fixable = true;

            var bp = model.Find(fields[1]);
            if (bp == null && AssemblerBlueprintsInRefineries.Contains(StripPosition(fields[1])))
                return null;   // PIM-internal blueprint of Hydroponics/Reprocessor/WRS, activated via the block, needs no definition
            if (bp == null)
            {
                var candidates = RenameCandidates(fields[1], model, ownSubtypes);
                if (candidates.Count != 1)
                    return new Finding
                    {
                        Mod = mod, Row = row, Kind = FindingKind.MissingBlueprint,
                        Message = $"blueprint '{fields[1]}' not found" + (candidates.Count > 1 ? $" (candidates: {string.Join(", ", candidates)})" : ""),
                    };
                bp = model.Find(candidates[0]);
                _proposed.Add(bp.Subtype);   // not proposed again as a new blueprint
                messages.Add($"blueprint renamed: {fields[1]} -> {bp.Subtype}");
                fields[1] = bp.Subtype;
                kind = FindingKind.RenamedBlueprint;
            }

            if (bp.Results.Count != 1)
            {
                messages.Add($"{bp.Results.Count} results ({string.Join(", ", bp.Results)}), PIM supports one");
                fixable = false;
            }
            else
            {
                var result = bp.Results[0];
                if (result.Type != fields[0])
                {
                    if (!BluePrintDataFile.AssemblerTypeCodes.ContainsKey(result.Type))
                    {
                        messages.Add($"produces {result}, item type '{result.Type}' not supported by PIM");
                        fixable = false;
                    }
                    else
                    {
                        messages.Add($"type {fields[0]} -> {result.Type} (the PIM key changes: autocrafting LCD settings of this item are lost)");
                        fields[0] = result.Type;
                        fields[2] = result.Subtype;
                        if (kind == FindingKind.Info) kind = FindingKind.WrongType;
                    }
                }
                else if (ItemNameOf(mod, fields) != result.ToString())
                {
                    messages.Add($"item {ItemNameOf(mod, fields)} -> {result}");
                    fields[2] = result.Subtype;
                    if (kind == FindingKind.Info) kind = FindingKind.WrongItem;
                }
            }

            var subtype = bp.Subtype;
            if (!inAssembler(subtype) && !inRefinery(subtype))
                messages.Add("not in a blueprint class of an assembler or refinery block");
            else if (!inAssembler(subtype) && !AssemblerBlueprintsInRefineries.Contains(StripPosition(subtype)))
                messages.Add("only producible in refinery blocks");

            if (messages.Count == 0) return null;
            if (kind == FindingKind.Info) fixable = false;
            return new Finding { Mod = mod, Row = row, Kind = kind, Message = string.Join("; ", messages), Fix = fixable ? fields : null };
        }

        /// <summary>The item name the script derives for these fields (same rules as the script).</summary>
        static string ItemNameOf(ModData mod, string[] fields)
        {
            var type = fields[0];
            var item = fields[2];
            try
            {
                return new AssemblerBluePrint(type, StripPosition(fields[1]), fields[1], mod.Name, item == "" ? "" : type + " " + item, fields[3]).ItemName;
            }
            catch (ArgumentException) { return "?"; }
        }

        public static string StripPosition(string subtype) =>
            subtype.StartsWith("Position0") && subtype.IndexOf('_') > 0 ? subtype.Substring(subtype.IndexOf('_') + 1) : subtype;

        /// <summary>Blueprints with the same name apart from the "Position0123_" prefix.</summary>
        static List<string> RenameCandidates(string subtype, GameModel model, HashSet<string> ownSubtypes)
        {
            var name = StripPosition(subtype);
            return model.Blueprints.Keys.Where(k => k != subtype && StripPosition(k) == name)
                        .OrderBy(k => ownSubtypes.Contains(k) ? 0 : 1).ThenBy(k => k)
                        .ToList();
        }

        // ------------------------------------------------------------ refinery rows

        Finding CheckRefineryRow(ModData mod, DataRow row, GameModel model, HashSet<string> ownSubtypes, Func<string, bool> inRefinery)
        {
            var fields = new[] { row.Field(0), row.Field(1), row.Field(2) };
            var bp = model.Find(fields[0]);
            if (bp == null)
            {
                var candidates = RenameCandidates(fields[0], model, ownSubtypes);
                return new Finding
                {
                    Mod = mod, Row = row, Kind = FindingKind.MissingBlueprint,
                    Message = $"blueprint '{fields[0]}' not found" + (candidates.Count > 0 ? $" (candidates: {string.Join(", ", candidates)})" : ""),
                };
            }

            var messages = new List<string>();
            var kind = FindingKind.Info;
            if (bp.Inputs.Count == 1 && bp.Results.Count == 1)
            {
                var input = bp.Inputs[0].ToString();
                var output = bp.Results[0].ToString();
                if (input != fields[1] || output != fields[2])
                {
                    messages.Add($"{fields[1]} -> {fields[2]} is {input} -> {output} in the game");
                    fields[1] = input;
                    fields[2] = output;
                    kind = FindingKind.WrongRefinery;
                }
            }
            else
            {
                var inputs = bp.Inputs.Select(i => i.ToString()).ToList();
                var outputs = bp.Results.Select(i => i.ToString()).ToList();
                if (!inputs.Contains(fields[1]) || !outputs.Contains(fields[2]))
                    messages.Add($"{fields[1]} -> {fields[2]} does not match {string.Join(" + ", inputs)} -> {string.Join(" + ", outputs)}");
            }

            if (!inRefinery(bp.Subtype)) messages.Add("not in a blueprint class of a refinery block");

            if (messages.Count == 0) return null;
            return new Finding { Mod = mod, Row = row, Kind = kind, Message = string.Join("; ", messages), Fix = kind == FindingKind.Info ? null : fields };
        }

        // ------------------------------------------------------------ new blueprints

        /// <summary>
        /// Blueprints defined by the mod itself (not vanilla, not in any data file, not proposed for an
        /// earlier mod of Mods.txt) that PIM can use: one result of a supported item type in an
        /// assembler, or one input and one result in a refinery.
        /// </summary>
        IEnumerable<Finding> NewBlueprints(ModData mod, List<SbcSource> own, GameModel model,
            Func<string, bool> inAssembler, Func<string, bool> inRefinery)
        {
            var vanilla = IsVanilla(mod) ? new HashSet<string>() : new HashSet<string>(Vanilla.Blueprints.Select(b => b.Subtype));
            foreach (var subtype in own.SelectMany(s => s.Blueprints).Select(b => b.Subtype).Distinct())
            {
                if (vanilla.Contains(subtype) || _knownSubtypes.Contains(subtype) || _proposed.Contains(subtype)) continue;
                var bp = model.Find(subtype);
                if (subtype.Contains(' ') || subtype.Contains('|')) continue;

                if (inAssembler(subtype) && bp.Results.Count == 1 && BluePrintDataFile.AssemblerTypeCodes.ContainsKey(bp.Results[0].Type))
                {
                    var result = bp.Results[0];
                    var display = Clean(model.DisplayNameOf(result) ?? "");
                    string sameKeyMod;
                    _knownKeys.TryGetValue(bp.Results[0].Type + " " + StripPosition(subtype), out sameKeyMod);
                    _proposed.Add(subtype);
                    yield return new Finding
                    {
                        Mod = mod, Kind = FindingKind.NewBlueprint, IsRefinery = false,
                        Message = $"new assembler blueprint {subtype} -> {result}" + (display != "" ? $" '{display}'" : "")
                                  + (sameKeyMod != null ? $" (same PIM key as a blueprint of {sameKeyMod})" : ""),
                        Fix = new[] { result.Type, subtype, result.Subtype, display },
                    };
                }
                else if (inRefinery(subtype) && bp.Inputs.Count == 1 && bp.Results.Count == 1
                         && RefineryInputTypes.Contains(bp.Inputs[0].Type))
                {
                    _proposed.Add(subtype);
                    yield return new Finding
                    {
                        Mod = mod, Kind = FindingKind.NewBlueprint, IsRefinery = true,
                        Message = $"new refinery blueprint {subtype}: {bp.Inputs[0]} -> {bp.Results[0]}",
                        Fix = new[] { subtype, bp.Inputs[0].ToString(), bp.Results[0].ToString() },
                    };
                }
            }
        }

        /// <summary>Removes the characters the data format does not allow.</summary>
        static string Clean(string text)
        {
            foreach (var c in new[] { ',', '=', '*', '"', '\\', '|', '#', '\r', '\n' }) text = text.Replace(c, ' ');
            while (text.Contains("  ")) text = text.Replace("  ", " ");
            return text.Trim();
        }
    }
}
