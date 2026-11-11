using System.Collections.Generic;
using System.Linq;

namespace PimBluePrintTool
{
    /// <summary>
    /// Several sources merged like the game does it: later sources override definitions with the
    /// same id, class entries can be added or disabled (Enabled="false").
    /// </summary>
    public sealed class GameModel
    {
        /// <summary>Block types whose blueprints PIM treats as assembler blueprints.</summary>
        public static readonly string[] AssemblerBlockTypes = { "Assembler", "SurvivalKit" };
        public static readonly string[] RefineryBlockTypes = { "Refinery" };

        public readonly Dictionary<string, SbcBlueprint> Blueprints = new Dictionary<string, SbcBlueprint>();
        readonly Dictionary<string, HashSet<string>> _classesOfBlueprint = new Dictionary<string, HashSet<string>>();
        readonly HashSet<string> _assemblerClasses = new HashSet<string>();
        readonly HashSet<string> _refineryClasses = new HashSet<string>();
        readonly Dictionary<string, string> _displayNames = new Dictionary<string, string>();
        readonly Dictionary<string, string> _texts = new Dictionary<string, string>();

        public GameModel(IEnumerable<SbcSource> sources)
        {
            var classMembers = new Dictionary<string, HashSet<string>>();
            var blocks = new Dictionary<string, SbcBlock>();

            foreach (var source in sources)
            {
                foreach (var bp in source.Blueprints) Blueprints[bp.Subtype] = bp;
                foreach (var entry in source.ClassEntries)
                {
                    HashSet<string> members;
                    if (!classMembers.TryGetValue(entry.Class, out members)) classMembers[entry.Class] = members = new HashSet<string>();
                    if (entry.Enabled) members.Add(entry.Blueprint);
                    else members.Remove(entry.Blueprint);
                }
                foreach (var block in source.Blocks) blocks[block.Type + "/" + block.Subtype] = block;
                foreach (var kv in source.DisplayNames) _displayNames[kv.Key] = kv.Value;
                foreach (var kv in source.Texts) _texts[kv.Key] = kv.Value;
            }

            foreach (var kv in classMembers)
                foreach (var bp in kv.Value)
                {
                    HashSet<string> classes;
                    if (!_classesOfBlueprint.TryGetValue(bp, out classes)) _classesOfBlueprint[bp] = classes = new HashSet<string>();
                    classes.Add(kv.Key);
                }

            foreach (var block in blocks.Values)
            {
                if (AssemblerBlockTypes.Contains(block.Type)) _assemblerClasses.UnionWith(block.Classes);
                if (RefineryBlockTypes.Contains(block.Type)) _refineryClasses.UnionWith(block.Classes);
            }
        }

        public SbcBlueprint Find(string subtype)
        {
            SbcBlueprint bp;
            return Blueprints.TryGetValue(subtype, out bp) ? bp : null;
        }

        IEnumerable<string> ClassesOf(string subtype)
        {
            HashSet<string> classes;
            return _classesOfBlueprint.TryGetValue(subtype, out classes) ? classes : Enumerable.Empty<string>();
        }

        public bool InAssembler(string subtype) => ClassesOf(subtype).Any(_assemblerClasses.Contains);
        public bool InRefinery(string subtype) => ClassesOf(subtype).Any(_refineryClasses.Contains);

        /// <summary>English display name of an item, null if unknown or not resolvable.</summary>
        public string DisplayNameOf(ItemRef item)
        {
            string name;
            if (!_displayNames.TryGetValue(item.ToString(), out name) || string.IsNullOrWhiteSpace(name)) return null;
            string text;
            if (_texts.TryGetValue(name, out text)) return text.Trim();
            // unresolved localization key
            if (name.StartsWith("DisplayName_") || name.StartsWith("Description_")) return null;
            return name;
        }
    }
}
