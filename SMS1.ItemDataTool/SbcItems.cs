using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using ItemDataParser = IngameScript.Program.ItemDataParser;

namespace SmsItemDataTool
{
    public sealed class SbcItem
    {
        public string Type, Subtype;
        /// <summary>English display name (localization resolved), "" if none.</summary>
        public string DisplayName = "";
        /// <summary>Rounds per magazine (AmmoMagazine), 0 if none.</summary>
        public int Capacity;
        /// <summary>&lt;Public&gt;false&lt;/Public&gt;: not available to players.</summary>
        public bool Hidden;
        public string File;
        public string Key => Type + "/" + Subtype;
    }

    /// <summary>
    /// The physical item definitions (ore, ingots, components, tools, ammo, ...) of the game data or of one mod:
    /// all *.sbc below Data, display names resolved with Localization/MyTexts*.resx.
    /// Only the item types SMS knows (ItemDataParser.TypeNames) are read.
    /// </summary>
    public sealed class SbcItems
    {
        const string ObjectBuilderPrefix = "MyObjectBuilder_";

        public readonly List<SbcItem> Items = new List<SbcItem>();
        public readonly List<string> Warnings = new List<string>();

        static readonly HashSet<string> ItemTypes = new HashSet<string>(ItemDataParser.TypeNames.Where(t => t != "Resource"));

        public static SbcItems Read(string directory)
        {
            var result = new SbcItems();
            var dataDir = Directory.Exists(Path.Combine(directory, "Data")) ? Path.Combine(directory, "Data") : directory;
            if (!Directory.Exists(dataDir)) return result;

            var texts = new Dictionary<string, string>();
            var localization = Path.Combine(dataDir, "Localization");
            if (Directory.Exists(localization))
                foreach (var resx in new[] { "MyTexts.resx", "MyTexts.override.resx", "MyTexts.en.resx", "MyTexts.en-US.resx" })
                {
                    var path = Path.Combine(localization, resx);
                    if (File.Exists(path)) result.ReadResx(path, texts);
                }

            foreach (var file in Directory.EnumerateFiles(dataDir, "*.sbc", SearchOption.AllDirectories).OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                string text;
                try { text = File.ReadAllText(file); }
                catch (IOException e) { result.Warnings.Add($"{file}: {e.Message}"); continue; }
                if (text.IndexOf("<Id", StringComparison.Ordinal) < 0) continue;

                XDocument doc;
                try { doc = XDocument.Parse(text); }
                catch (XmlException e) { result.Warnings.Add($"{file}: invalid XML ({e.Message}), skipped"); continue; }

                foreach (var e in doc.Descendants())
                {
                    var idElement = Child(e, "Id");
                    if (idElement == null || Child(e, "DisplayName") == null) continue;   // items always have a display name
                    var id = ReadId(idElement);
                    if (id.Type == null || id.Subtype == null || !ItemTypes.Contains(id.Type)) continue;

                    var displayName = Child(e, "DisplayName").Value.Trim();
                    string localized;
                    if (texts.TryGetValue(displayName, out localized)) displayName = localized;
                    int capacity;
                    int.TryParse(Child(e, "Capacity")?.Value.Trim(), out capacity);

                    // a later definition of the same item (e.g. in another file of a mod) replaces the earlier one
                    result.Items.RemoveAll(i => i.Type == id.Type && i.Subtype == id.Subtype);
                    result.Items.Add(new SbcItem
                    {
                        Type = id.Type,
                        Subtype = id.Subtype,
                        DisplayName = displayName,
                        Capacity = capacity,
                        Hidden = string.Equals(Child(e, "Public")?.Value.Trim(), "false", StringComparison.OrdinalIgnoreCase),
                        File = file,
                    });
                }
            }
            return result;
        }

        void ReadResx(string path, Dictionary<string, string> texts)
        {
            try
            {
                foreach (var data in XDocument.Load(path).Descendants("data"))
                {
                    var key = (string)data.Attribute("name");
                    var value = data.Element("value")?.Value;
                    if (key != null && value != null) texts[key] = value;
                }
            }
            catch (XmlException e) { Warnings.Add($"{path}: invalid XML ({e.Message}), skipped"); }
        }

        static XElement Child(XElement e, string localName) => e.Elements().FirstOrDefault(c => c.Name.LocalName == localName);

        static string StripPrefix(string type) => type.StartsWith(ObjectBuilderPrefix) ? type.Substring(ObjectBuilderPrefix.Length) : type;

        /// <summary>Both notations: &lt;Id&gt;&lt;TypeId/&gt;&lt;SubtypeId/&gt;&lt;/Id&gt; and &lt;Id Type="" Subtype=""/&gt;.</summary>
        static (string Type, string Subtype) ReadId(XElement id)
        {
            var type = (string)id.Attribute("Type") ?? Child(id, "TypeId")?.Value;
            var subtype = (string)id.Attribute("Subtype") ?? Child(id, "SubtypeId")?.Value;
            return (type == null ? null : StripPrefix(type.Trim()), subtype?.Trim());
        }
    }
}
