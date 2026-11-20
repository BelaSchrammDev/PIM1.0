using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        // A display on a surface of a block that is no text panel: cockpits, button panels, projectors, the PB ...
        // (text panels are LCD_Infolcd / LCD_Terminal).
        public class TextSurfaceDisplay : TextRenderEngine
        {
            public IMyTextSurface Surface;
            IMyTerminalBlock block;
            // Line limit: lines on a full 512 px height, 36 matches the old per-type values (cockpit 22, strips 5-8);
            // square surfaces are physically small: 20. Square ones (round frames) get some padding, the projector a bit more.
            public TextSurfaceDisplay(IMyTextSurface surface, IMyTerminalBlock block)
            {
                tsg = block.CubeGrid;
                Surface = surface;
                this.block = block;
                var size = surface.SurfaceSize;
                var square = size.X == size.Y;
                SetSurfaceConstants(surface, block.BlockDefinition.ToString().Contains("Projector") ? 4.5f : square ? 2.5f : 0, square ? 20 : 36);
                SetLineLimits();
                setLinesBounds(5);
            }
            void SetLineLimits()
            {
                maxl = readableLines;
                minl = 2;
            }
            // called on every scan that finds the surface; the name is the tag's name or, for @display, the block's name
            public void Configure(P options, string definition)
            {
                Obsolete = false;
                SetLineLimits();
                setName((options.Name.StartsWith("@dis") ? block.CustomName : options.Name) + " (" + Surface.DisplayName + ")");
                ApplyDisplayOptions(options, definition);
            }
            public override void writeToPanel() { WriteSurface(Surface, pages.Count > 0 ? pages[0] : ""); }
        }
    }
}
