using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace PimBluePrintTool
{
    /// <summary>Item in PIM notation: "Type Subtype" (without "MyObjectBuilder_").</summary>
    public struct ItemRef
    {
        public string Type, Subtype;
        public override string ToString() => Type + " " + Subtype;
    }

    public sealed class SbcBlueprint
    {
        public string Subtype;
        public string DisplayName;
        public readonly List<ItemRef> Inputs = new List<ItemRef>();
        public readonly List<ItemRef> Results = new List<ItemRef>();
        /// <summary>"Vanilla" or the workshop id.</summary>
        public string Source;
        public string File;
    }

    public sealed class SbcClassEntry
    {
        public string Class, Blueprint;
        public bool Enabled;
    }

    public sealed class SbcBlock
    {
        public string Type, Subtype;
        public readonly List<string> Classes = new List<string>();
    }

    /// <summary>
    /// All blueprint related definitions of one source: the game (Content/Data) or one workshop item.
    /// Only the definitions are collected; merging and overriding is done by <see cref="GameModel"/>.
    /// </summary>
    public sealed class SbcSource
    {
        public string Name;
        public string Directory;
        /// <summary>In file order.</summary>
        public readonly List<SbcBlueprint> Blueprints = new List<SbcBlueprint>();
        public readonly List<SbcClassEntry> ClassEntries = new List<SbcClassEntry>();
        public readonly List<SbcBlock> Blocks = new List<SbcBlock>();
        /// <summary>"Type Subtype" -> DisplayName (text or localization key).</summary>
        public readonly Dictionary<string, string> DisplayNames = new Dictionary<string, string>();
        /// <summary>Localization key -> English text.</summary>
        public readonly Dictionary<string, string> Texts = new Dictionary<string, string>();
        public readonly List<string> Warnings = new List<string>();

        const string ObjectBuilderPrefix = "MyObjectBuilder_";

        public static SbcSource Read(string name, string directory)
        {
            var source = new SbcSource { Name = name, Directory = directory };
            var dataDir = System.IO.Directory.Exists(Path.Combine(directory, "Data")) ? Path.Combine(directory, "Data") : directory;
            if (!System.IO.Directory.Exists(dataDir)) return source;

            foreach (var file in System.IO.Directory.EnumerateFiles(dataDir, "*.sbc", SearchOption.AllDirectories).OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                string text;
                try { text = System.IO.File.ReadAllText(file); }
                catch (IOException e) { source.Warnings.Add($"{file}: {e.Message}"); continue; }

                // skip files without anything of interest (planets, particles, ...)
                if (text.IndexOf("Blueprint", StringComparison.Ordinal) < 0 && text.IndexOf("<DisplayName>", StringComparison.Ordinal) < 0) continue;

                XDocument doc;
                try { doc = XDocument.Parse(text); }
                catch (XmlException e) { source.Warnings.Add($"{file}: invalid XML ({e.Message}), skipped"); continue; }

                source.ReadDefinitions(doc, file);
            }

            var localization = Path.Combine(dataDir, "Localization");
            if (System.IO.Directory.Exists(localization))
            {
                // English: MyTexts.resx (and the .override variant some mods use)
                foreach (var resx in new[] { "MyTexts.resx", "MyTexts.override.resx", "MyTexts.en.resx", "MyTexts.en-US.resx" })
                {
                    var path = Path.Combine(localization, resx);
                    if (System.IO.File.Exists(path)) source.ReadResx(path);
                }
            }
            return source;
        }

        void ReadDefinitions(XDocument doc, string file)
        {
            foreach (var e in doc.Descendants())
            {
                var local = e.Name.LocalName;
                var parent = e.Parent?.Name.LocalName;

                if (local == "Blueprint" && parent == "Blueprints" || local == "Definition" && XsiType(e).EndsWith("BlueprintDefinition"))
                    ReadBlueprint(e, file);
                else if (local == "Entry" && parent == "BlueprintClassEntries")
                    ReadClassEntry(e);
                else if (Child(e, "BlueprintClasses") != null && Child(e, "Id") != null)
                    ReadBlock(e);

                if (Child(e, "DisplayName") != null && Child(e, "Id") != null)
                {
                    var id = ReadId(Child(e, "Id"));
                    if (id.Subtype != null) DisplayNames[id.ToString()] = Child(e, "DisplayName").Value.Trim();
                }
            }
        }

        void ReadBlueprint(XElement e, string file)
        {
            var id = ReadId(Child(e, "Id"));
            if (id.Subtype == null) { Warnings.Add($"{file}: blueprint without SubtypeId"); return; }

            var bp = new SbcBlueprint { Subtype = id.Subtype, Source = Name, File = file, DisplayName = Child(e, "DisplayName")?.Value.Trim() };
            var prerequisites = Child(e, "Prerequisites");
            if (prerequisites != null) bp.Inputs.AddRange(prerequisites.Elements().Select(ItemFromAttributes));
            var result = Child(e, "Result");
            if (result != null) bp.Results.Add(ItemFromAttributes(result));
            var results = Child(e, "Results");
            if (results != null) bp.Results.AddRange(results.Elements().Select(ItemFromAttributes));
            Blueprints.Add(bp);
        }

        void ReadClassEntry(XElement e)
        {
            var cls = (string)e.Attribute("Class");
            var bp = (string)e.Attribute("BlueprintSubtypeId");
            if (cls == null || bp == null) return;
            var type = (string)e.Attribute("BlueprintTypeId");
            if (type != null && StripPrefix(type) != "BlueprintDefinition") return;   // build planner entries etc.
            ClassEntries.Add(new SbcClassEntry
            {
                Class = cls,
                Blueprint = bp,
                Enabled = !string.Equals((string)e.Attribute("Enabled"), "false", StringComparison.OrdinalIgnoreCase),
            });
        }

        void ReadBlock(XElement e)
        {
            var id = ReadId(Child(e, "Id"));
            if (id.Type == null)
            {
                // <Definition xsi:type="MyObjectBuilder_AssemblerDefinition"> without TypeId in Id
                var xsi = XsiType(e);
                if (xsi.EndsWith("Definition")) id.Type = StripPrefix(xsi.Substring(0, xsi.Length - "Definition".Length));
            }
            var block = new SbcBlock { Type = id.Type, Subtype = id.Subtype ?? "" };
            block.Classes.AddRange(Child(e, "BlueprintClasses").Elements().Select(c => c.Value.Trim()));
            Blocks.Add(block);
        }

        void ReadResx(string path)
        {
            try
            {
                foreach (var data in XDocument.Load(path).Descendants("data"))
                {
                    var key = (string)data.Attribute("name");
                    var value = data.Element("value")?.Value;
                    if (key != null && value != null) Texts[key] = value;
                }
            }
            catch (XmlException e) { Warnings.Add($"{path}: invalid XML ({e.Message}), skipped"); }
        }

        // ------------------------------------------------------------ helpers

        static XElement Child(XElement e, string localName) => e.Elements().FirstOrDefault(c => c.Name.LocalName == localName);

        static string XsiType(XElement e) =>
            (string)e.Attributes().FirstOrDefault(a => a.Name.LocalName == "type" && a.Name.NamespaceName.Contains("XMLSchema-instance")) ?? "";

        static string StripPrefix(string type) => type.StartsWith(ObjectBuilderPrefix) ? type.Substring(ObjectBuilderPrefix.Length) : type;

        /// <summary>Both notations: &lt;Id&gt;&lt;TypeId/&gt;&lt;SubtypeId/&gt;&lt;/Id&gt; and &lt;Id Type="" Subtype=""/&gt;.</summary>
        static ItemRef ReadId(XElement id)
        {
            if (id == null) return new ItemRef();
            var type = (string)id.Attribute("Type") ?? Child(id, "TypeId")?.Value;
            var subtype = (string)id.Attribute("Subtype") ?? Child(id, "SubtypeId")?.Value;
            return new ItemRef { Type = type == null ? null : StripPrefix(type.Trim()), Subtype = subtype?.Trim() };
        }

        static ItemRef ItemFromAttributes(XElement e) => new ItemRef
        {
            Type = StripPrefix(((string)e.Attribute("TypeId") ?? "").Trim()),
            Subtype = ((string)e.Attribute("SubtypeId") ?? "").Trim(),
        };
    }
}
