using System.Collections.Generic;
using System.Linq;

namespace IngameScript
{
    // Creates the assembler and refinery blueprints from the generated data
    // (BluePrintsClasses/Generated/BluePrintData.g.cs, source: BluePrintData/*.txt).
    // Pure: no Sandbox/VRage types, linked into the test project.
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
        static readonly string[] BpTypeNames = { IG_Component, IG_Ammo, IG_Tools, IG_Kits, IG_OBottles, IG_HBottles, IG_Datas, IG_Seeds, IG_I, "Ore" };

        static bool IsModEnabled(Dictionary<string, bool> usedMods, string mod)
        {
            return mod == Strings.M_Vanilla || (usedMods.ContainsKey(mod) && usedMods[mod]);
        }

        /// <summary>
        /// Creates the blueprints of all enabled mods in load order. Existing keys are kept
        /// (the first definition wins). The refinery list is sorted by input name; blueprints
        /// with the same input keep their definition order.
        /// </summary>
        public static void DefineBluePrints(Dictionary<string, bool> usedMods, Dictionary<string, AssemblerBluePrint> assembler, List<RefineryBlueprint> refinery)
        {
            var io = IsModEnabled(usedMods, Strings.M_IndustrialOverhaulMod);
            var refineryNames = new HashSet<string>(refinery.Select(r => r.Name));

            for (int m = 0; m < BluePrintMods.Length; m++)
            {
                var mod = BluePrintMods[m];
                if (!IsModEnabled(usedMods, mod)) continue;

                bool active = false, isRefinery = false;
                string type = "", template = "";
                foreach (var entry in BluePrintData[m].Split(','))
                {
                    if (entry[0] == '[')
                    {
                        isRefinery = entry[1] == 'R';
                        active = entry.Length < 3 || (entry[2] == '+') == IsModEnabled(usedMods, entry.Substring(3));
                        template = BpDefaultTemplate;
                    }
                    else if (!active) { }
                    else if (isRefinery)
                    {
                        if (entry.Contains('*')) template = entry;
                        else if (entry.Contains('=')) AddRefineryBlueprint(refinery, refineryNames, entry, io);
                        else foreach (var resource in entry.Split(' ')) AddRefineryBlueprint(refinery, refineryNames, template.Replace("*", resource), io);
                    }
                    else if (entry.Length == 1) type = BpTypeNames[BpTypeCodes.IndexOf(entry[0])];
                    else AddAssemblerBluePrint(assembler, mod, type, entry.Split('='));
                }
            }

            var sorted = refinery.OrderBy(r => r.InputIDName).ToList();
            refinery.Clear();
            refinery.AddRange(sorted);
        }

        static void AddAssemblerBluePrint(Dictionary<string, AssemblerBluePrint> assembler, string mod, string type, string[] fields)
        {
            var definitionSubtype = fields[0][0] == '#' ? "Position0" + fields[0].Substring(1) : fields[0];
            var subtype = definitionSubtype.StartsWith("Position0") ? definitionSubtype.Substring(definitionSubtype.IndexOf('_') + 1) : definitionSubtype;
            var item = fields.Length > 1 ? fields[1] : "";
            int strip;
            if (int.TryParse(item, out strip)) item = AssemblerBluePrint.BluePrintNameToItemName(type, strip > 0 ? subtype.Substring(strip) : subtype.Substring(0, subtype.Length + strip));
            else if (item != "") item = type + " " + item;

            var key = type + " " + subtype;
            if (!assembler.ContainsKey(key)) assembler.Add(key, new AssemblerBluePrint(type, subtype, definitionSubtype, mod, item, fields.Length > 2 ? fields[2] : ""));
        }

        static void AddRefineryBlueprint(List<RefineryBlueprint> refinery, HashSet<string> names, string entry, bool io)
        {
            var f = entry.Split('=');
            if (names.Add(f[0])) refinery.Add(new RefineryBlueprint(f[0], f[1], f[2], io));
        }
    }
}
