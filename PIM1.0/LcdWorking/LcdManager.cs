using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage.Game.GUI.TextPanel;

namespace IngameScript
{
    // Ingame part of the LCD pages: finding, reading and writing the panels.
    // Parsing and rendering live in LcdPages.cs.
    //
    // A panel is only read again if its text differs from what PIM wrote last (changed by the player),
    // and only written if the new page differs from its current text.
    partial class Program
    {
        public class LcdPanel
        {
            public IMyTextPanel Panel;
            // last text written by PIM, null = not written yet
            public string Text;
            // read from the panel: autocrafting types / oreprio refinery type filter, autocrafting type filters
            public string Types, Filter;
            public Dictionary<string, string> Filters = new Dictionary<string, string>();
        }

        List<LcdPanel> AutocraftingLcds = new List<LcdPanel>(), OrePrioLcds = new List<LcdPanel>(), RefiningLcds = new List<LcdPanel>();

        // render buffer, shared by all pages
        static readonly StringBuilder LcdText = new StringBuilder();

        /// <summary>
        /// Text panels of this construct (incl. subgrids via rotors/pistons, without docked ships) whose name contains the tag.
        /// Panels that are already known keep their state.
        /// </summary>
        List<LcdPanel> GetLcds(string tag, List<LcdPanel> known)
        {
            var blocks = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType(blocks, block => block.CustomName.Contains(tag) && block.IsSameConstructAs(Me));
            return blocks.Select(b => known.Find(p => p.Panel == b) ?? new LcdPanel { Panel = b }).ToList();
        }

        /// <summary>Writes LcdText to the panel if it differs from the current panel text.</summary>
        static void WriteLcd(LcdPanel p, string current)
        {
            p.Text = LcdText.ToString();
            if (p.Text == current) return;
            var lcd = p.Panel;
            LCD_DebugString.Append("LCD written: " + lcd.CustomName + "\n");
            lcd.Alignment = TextAlignment.LEFT;
            lcd.ContentType = ContentType.TEXT_AND_IMAGE;
            lcd.WriteText(p.Text);
        }

        void loadAutocratingDefinitions()
        {
            AutocraftingLcds = GetLcds(Autocrafting, AutocraftingLcds);
            foreach (var p in AutocraftingLcds)
            {
                var current = p.Panel.GetText();
                if (current != p.Text)
                {
                    p.Types = IG_Component + "," + IG_Ammo;
                    p.Filters.Clear();
                    if (ParseAutocrafting(current, Lists.Data.BluePrints_Active, ref p.Types, p.Filters)) SetAutocraftingThresholdNew();
                }
                LcdText.Clear();
                RenderAutocrafting(LcdText, p.Types, p.Filters, Lists.Data.autocrafting_Types, Lists.Data.BluePrints_Active.Values, filter);
                WriteLcd(p, current);
            }
        }

        void LoadAndRenderOrePrioDefs()
        {
            OrePrioLcds = GetLcds(OrePrioDefString, OrePrioLcds);
            if (OrePrioLcds.Count == 0) return;
            Refinery.RemoveUnusedRefinerytypeBlueprintLists();
            var accepted = Refinery.refineryTypesAcceptedBlueprintsList;
            // read all panels first, then write all
            var current = new List<string>();
            foreach (var p in OrePrioLcds)
            {
                var text = p.Panel.GetText();
                if (text != p.Text) p.Filter = ParseOrePrio(text, accepted, RefineryBlueprints);
                current.Add(text);
            }
            ApplyOrePrioConfig();
            for (int i = 0; i < OrePrioLcds.Count; i++)
            {
                LcdText.Clear();
                RenderOrePrio(LcdText, OrePrioLcds[i].Filter, accepted, filter);
                WriteLcd(OrePrioLcds[i], current[i]);
            }
        }

        void RenderResourceProccesingLCD()
        {
            RefiningLcds = GetLcds(ResourcenOverview, RefiningLcds);
            if (RefiningLcds.Count == 0) return;
            RefineryBlueprint.FillInputOutputAmountAndETA();
            LcdText.Clear();
            RenderRefining(LcdText, RefineryBlueprints);
            foreach (var p in RefiningLcds) WriteLcd(p, p.Panel.GetText());
        }
    }
}
