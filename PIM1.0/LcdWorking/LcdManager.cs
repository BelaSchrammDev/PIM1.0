using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage.Game.GUI.TextPanel;

namespace IngameScript
{
    // Ingame part of the LCD pages: finding, reading and writing the panels, one job per page.
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

        /// <summary>
        /// One LCD page: per panel one step to read (if changed by the player) and render the first part,
        /// one step per further part, the last one writes the panel (if the content changed).
        /// </summary>
        public abstract class LcdPage : Job
        {
            readonly string _tag;
            List<LcdPanel> _panels = new List<LcdPanel>();
            int _panel, _part;
            string _current;
            protected readonly Filter Filter = new Filter();

            protected LcdPage(string tag) { _tag = tag; }

            public override void InitJob()
            {
                _panels = MyProgram.GetLcds(_tag, _panels);
                _panel = _part = 0;
                if (_panels.Count > 0) Start();
            }

            public override RunJobResult RunJob()
            {
                if (_panel >= _panels.Count) return RunJobResult.Finished;
                var p = _panels[_panel];
                if (_part == 0)
                {
                    _current = p.Panel.GetText();
                    if (_current != p.Text) Read(p, _current);
                    LcdText.Clear();
                }
                if (Render(p, _part++))
                {
                    Write(p);
                    _panel++;
                    _part = 0;
                }
                return _panel < _panels.Count ? RunJobResult.Continue : RunJobResult.Finished;
            }

            void Write(LcdPanel p)
            {
                var text = LcdText.ToString();
                var lcd = p.Panel;
                if (text != _current)
                {
                    // changed by the player while the page was rendered: keep it, it is read in the next cycle
                    if (lcd.GetText() != _current) return;
                    LCD_DebugString.Append("LCD written: " + lcd.CustomName + "\n");
                    lcd.Alignment = TextAlignment.LEFT;
                    lcd.ContentType = ContentType.TEXT_AND_IMAGE;
                    lcd.WriteText(text);
                }
                p.Text = text;
            }

            /// <summary>Once per cycle before the first panel (only if there are panels).</summary>
            protected virtual void Start() { }

            /// <summary>The panel was changed by the player (or is new): take over its settings.</summary>
            protected virtual void Read(LcdPanel p, string text) { }

            /// <summary>Renders part <paramref name="part"/> (0, 1, ...) into LcdText, returns true if it was the last part.</summary>
            protected abstract bool Render(LcdPanel p, int part);
        }

        /// <summary>(sms,autocrafting): max amounts of the active blueprints, header + one part per autocrafting type.</summary>
        public class AutocraftingPage : LcdPage
        {
            string[] _types;

            public AutocraftingPage() : base(Autocrafting) { }

            protected override void Read(LcdPanel p, string text)
            {
                p.Types = IG_Component + "," + IG_Ammo;
                p.Filters.Clear();
                if (ParseAutocrafting(text, Lists.Data.BluePrints_Active, ref p.Types, p.Filters)) MyProgram.SetAutocraftingThresholdNew();
            }

            protected override bool Render(LcdPanel p, int part)
            {
                if (part == 0)
                {
                    _types = AutocraftingTypeList(p.Types);
                    RenderAutocraftingHeader(LcdText, p.Types, Lists.Data.autocrafting_Types);
                }
                else RenderAutocraftingType(LcdText, _types[part - 1], p.Filters, Lists.Data.BluePrints_Active.Values, Filter);
                return part >= _types.Length;
            }
        }

        /// <summary>(sms,oreprio): ore priorities per refinery type, header + one part per refinery type.
        /// The values are applied by ApplyOrePrioConfig after each priority calculation (RefineryManagerJob).</summary>
        public class OrePrioPage : LcdPage
        {
            string[] _keys;

            public OrePrioPage() : base(OrePrioDefString) { }

            protected override void Start()
            {
                Refinery.RemoveUnusedRefinerytypeBlueprintLists();
                _keys = Refinery.refineryTypesAcceptedBlueprintsList.Keys.ToArray();
            }

            protected override void Read(LcdPanel p, string text)
            {
                p.Filter = ParseOrePrio(text, Refinery.refineryTypesAcceptedBlueprintsList, RefineryBlueprints);
            }

            protected override bool Render(LcdPanel p, int part)
            {
                if (part == 0) RenderOrePrioHeader(LcdText, p.Filter, Filter);
                else RenderOrePrioType(LcdText, _keys[part - 1], Refinery.refineryTypesAcceptedBlueprintsList[_keys[part - 1]], Filter);
                return part >= _keys.Length;
            }
        }

        /// <summary>(sms,refining): refining progress, the same page on all panels.</summary>
        public class RefiningPage : LcdPage
        {
            public RefiningPage() : base(ResourcenOverview) { }

            protected override void Start() { RefineryBlueprint.FillInputOutputAmountAndETA(); }

            protected override bool Render(LcdPanel p, int part)
            {
                RenderRefining(LcdText, RefineryBlueprints);
                return true;
            }
        }
    }
}
