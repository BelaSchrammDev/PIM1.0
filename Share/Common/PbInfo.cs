using Sandbox.ModAPI.Ingame;
using System;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        /// <summary>
        /// Writes the PB info of PIM and SMS (the texts: RunStatus): Echo and, if wanted, the PB's own LCD.
        /// The look of the LCD is the same in all scripts, only the font color is the script's own.
        /// </summary>
        public static class PbInfo
        {
            // the master of my script type, with its grid (the address if its PB is not found)
            public static string Standby(NetWorkingNode net, IMyGridTerminalSystem gts)
            {
                var master = gts.GetBlockWithId(net.MasterAddress);
                return RunStatus.Standby(master?.CustomName ?? net.MasterAddress.ToString(), master?.CubeGrid.CustomName);
            }

            public static void Write(Action<string> echo, IMyTextSurface lcd, bool showOnLcd, Color fontColor, string text)
            {
                echo(text);
                if (!showOnLcd) return;
                // only set up if it shows no text yet: a look the player chose stays
                if (lcd.ContentType != ContentType.TEXT_AND_IMAGE)
                {
                    lcd.ContentType = ContentType.TEXT_AND_IMAGE;
                    lcd.Alignment = TextAlignment.LEFT;
                    lcd.TextPadding = 2f;
                    lcd.FontSize = 0.45f;
                    lcd.FontColor = fontColor;
                }
                lcd.WriteText(text);
            }
        }
    }
}
