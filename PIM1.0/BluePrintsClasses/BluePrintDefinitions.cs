using System.Collections.Generic;
using System.Linq;

namespace IngameScript
{
    // Creates the assembler and refinery blueprints from the generated data
    // (BluePrintsClasses/Generated/BluePrintData.g.cs, source: BluePrintData/*.txt).
    // Pure: no Sandbox/VRage types, linked into the test project.
    //
    // Loading runs as a job: BluePrintLoadJob is a SequentialJob with one BluePrintModJob per mod,
    // every step handles one entry. After the last mod the refinery list is sorted and the job
    // deactivates itself.
    //
    // One string per mod, entries separated by ',':
    //   [A  [R            section start: assembler / refinery blueprints
    //   [A!Mod  [A+Mod    section is only used if Mod is disabled / enabled
    // assembler section:
    //   X                 single char: item type, see BpTypeCodes
    //   sub[=item[=name]] blueprint; '#' at the start of sub stands for "Position0"
    //                     item: empty = derived from sub, number n = derived from sub without the
    //                     first n chars (n < 0: without the last -n chars), else item subtype
    //                     name: autocrafting display name, empty = derived from the item
    // refinery section:
    //   name=input=output one blueprint
    //   text with '*'     template for the following resource lists
    //   res1 res2 ...     one blueprint per resource: '*' of the current template replaced
    //                     (template at section start: BpDefaultTemplate)
    // The encoding is produced by PIM1.0.BluePrintGenerator (BluePrintDataEncoder.cs).
    partial class Program
    {
        const string BpTypeCodes = "CATKOHDSIE", BpDefaultTemplate = "*OreToIngot=Ore *=Ingot *";
        static readonly string[] BpTypeNames = { TypeComponent, TypeAmmo, TypeTool, TypeKit, TypeOBottle, TypeHBottle, TypeDatapad, TypeSeed, TypeIngot, TypeOre };

        static bool IsModEnabled(Dictionary<string, bool> usedMods, string mod)
        {
            return mod == Strings.M_Vanilla || (usedMods.ContainsKey(mod) && usedMods[mod]);
        }

        /// <summary>Loads all blueprints at once (runs the BluePrintLoadJob to the end).</summary>
        public static void DefineBluePrints(Dictionary<string, bool> usedMods, Dictionary<string, AssemblerBluePrint> assembler, List<RefineryBlueprint> refinery)
        {
            var job = new BluePrintLoadJob(new BluePrintSet(usedMods, assembler, refinery));
            while (job.Schedule() == Job.ScheduleResult.InProgress) { }
        }

        /// <summary>
        /// Target of a loading run. Existing keys are kept (the first definition wins).
        /// </summary>
        public class BluePrintSet
        {
            public readonly Dictionary<string, bool> UsedMods;
            readonly Dictionary<string, AssemblerBluePrint> _assembler;
            readonly List<RefineryBlueprint> _refinery;
            readonly HashSet<string> _refineryNames;
            readonly bool _io;

            public BluePrintSet(Dictionary<string, bool> usedMods, Dictionary<string, AssemblerBluePrint> assembler, List<RefineryBlueprint> refinery)
            {
                UsedMods = usedMods;
                _assembler = assembler;
                _refinery = refinery;
                _refineryNames = new HashSet<string>(refinery.Select(r => r.Name));
                _io = IsModEnabled(usedMods, Strings.M_IndustrialOverhaulMod);
            }

            public void AddAssembler(string mod, string type, string[] fields)
            {
                var definitionSubtype = fields[0][0] == '#' ? "Position0" + fields[0].Substring(1) : fields[0];
                var subtype = definitionSubtype.StartsWith("Position0") ? definitionSubtype.Substring(definitionSubtype.IndexOf('_') + 1) : definitionSubtype;
                var item = fields.Length > 1 ? fields[1] : "";
                int strip;
                if (int.TryParse(item, out strip)) item = AssemblerBluePrint.BluePrintNameToItemName(type, strip > 0 ? subtype.Substring(strip) : subtype.Substring(0, subtype.Length + strip));
                else if (item != "") item = type + " " + item;

                var key = type + " " + subtype;
                if (!_assembler.ContainsKey(key)) _assembler.Add(key, new AssemblerBluePrint(type, subtype, definitionSubtype, mod, item, fields.Length > 2 ? fields[2] : ""));
            }

            public void AddRefinery(string entry)
            {
                var f = entry.Split('=');
                if (_refineryNames.Add(f[0])) _refinery.Add(new RefineryBlueprint(f[0], f[1], f[2], _io));
            }

            /// <summary>Sorts by input name; blueprints with the same input keep their definition order.</summary>
            public void SortRefinery()
            {
                var sorted = _refinery.OrderBy(r => r.InputIDName).ToList();
                _refinery.Clear();
                _refinery.AddRange(sorted);
            }
        }

        /// <summary>Loads all mods in load order, runs once.</summary>
        public class BluePrintLoadJob : SequentialJob
        {
            readonly BluePrintSet _set;

            public BluePrintLoadJob(BluePrintSet set)
                : base(BluePrintMods.Select((m, i) => (Job)new BluePrintModJob(set, i)).ToArray())
            {
                _set = set;
            }

            public override RunJobResult RunJob()
            {
                var result = base.RunJob();
                if (result == RunJobResult.Finished)
                {
                    _set.SortRefinery();
                    Active = false;
                }
                return result;
            }
        }

        /// <summary>Loads the blueprints of one mod (nothing if the mod is disabled), one entry per step.</summary>
        public class BluePrintModJob : Job
        {
            readonly BluePrintSet _set;
            readonly int _mod;
            string[] _entries;
            int _index;
            bool _active, _isRefinery;
            string _type, _template;

            public BluePrintModJob(BluePrintSet set, int mod)
            {
                _set = set;
                _mod = mod;
            }

            public override void InitJob()
            {
                _entries = IsModEnabled(_set.UsedMods, BluePrintMods[_mod]) ? BluePrintData[_mod].Split(',') : new string[0];
                _index = 0;
            }

            public override RunJobResult RunJob()
            {
                if (_index >= _entries.Length) return RunJobResult.Finished;

                var entry = _entries[_index++];
                if (entry[0] == '[')
                {
                    _isRefinery = entry[1] == 'R';
                    _active = entry.Length < 3 || (entry[2] == '+') == IsModEnabled(_set.UsedMods, entry.Substring(3));
                    _template = BpDefaultTemplate;
                }
                else if (!_active) { }
                else if (_isRefinery)
                {
                    if (entry.Contains('*')) _template = entry;
                    else if (entry.Contains('=')) _set.AddRefinery(entry);
                    else foreach (var resource in entry.Split(' ')) _set.AddRefinery(_template.Replace("*", resource));
                }
                else if (entry.Length == 1) _type = BpTypeNames[BpTypeCodes.IndexOf(entry[0])];
                else _set.AddAssembler(BluePrintMods[_mod], _type, entry.Split('='));

                return _index < _entries.Length ? RunJobResult.Continue : RunJobResult.Finished;
            }
        }
    }
}
