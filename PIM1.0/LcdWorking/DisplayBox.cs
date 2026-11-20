using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript
{
    partial class Program
    {
        static Dictionary<string, DisplayBox> DisplayBoxList = new Dictionary<string, DisplayBox>();

        static DisplayBox GetDisplayBox(string boxID, float width)
        {
            DisplayBox box;
            if (!DisplayBoxList.TryGetValue(boxID, out box))
            {
                // the ids contain the displayed values: start over from time to time instead of growing forever
                if (DisplayBoxList.Count > 2000) DisplayBoxList.Clear();
                box = new DisplayBox(width);
                DisplayBoxList.Add(boxID, box);
            }
            return box;
        }

        static string GetDisplayBoxString(string text, float amount, float width)
        {
            var amountStr = amount == 0 ? "  " : DisplayBox.GetMassString(amount);
            return GetDisplayBox("@@@" + text, width).Get2StringWithSpaces(text, amountStr);
        }

        static string GetDisplayBoxString(int amount, float width, bool left = false)
        {
            var displaytext = amount == 0 ? "  " : amount.ToString();
            return GetDisplayBox(displaytext + width.ToString(), width).GetStringWithSpaces(displaytext, left);
        }

        static string GetDisplayBoxString(float amount, float width, bool left = false)
        {
            var amountStr = amount == 0 ? "  " : DisplayBox.GetMassString(amount);
            return GetDisplayBox(amountStr + width.ToString(), width).GetStringWithSpaces(amountStr, left);
        }

        static string GetDisplayBoxString(string displaytext, float width, bool left = false)
        {
            return GetDisplayBox(displaytext + width.ToString() + left.ToString(), width).GetStringWithSpaces(displaytext, left);
        }

        // add DisplayPool Class
        class DisplayBox
        {
            // Src1/Src2: last arguments (untrimmed), LeftText: Src1 shortened to the width
            string Src1, Src2, LeftText = "", SpaceString = "";
            float Width = 0;

            public DisplayBox(float iwidth)
            {
                Width = iwidth;
            }

            public string Get2StringWithSpaces(string arg1, string arg2)
            {
                if (arg1 != Src1 || arg2 != Src2)
                {
                    Src1 = arg1;
                    Src2 = arg2;
                    var strLength2 = StringWidth(arg2);
                    LeftText = TrimStringByWidth(arg1, Width - 2.6f - strLength2);
                    SpaceString = SpaceFillText(Width - StringWidth(LeftText) - strLength2);
                }
                return LeftText + SpaceString + Src2;
            }

            public string GetStringWithSpaces(string arg, bool leftAlignment = false)
            {
                if (arg != Src1)
                {
                    Src1 = arg;
                    LeftText = TrimStringByWidth(arg, Width - 2.6f);
                    SpaceString = SpaceFillText(Width - StringWidth(LeftText));
                }
                return leftAlignment ? LeftText + SpaceString : SpaceString + LeftText;
            }

            /// <summary>"AVeryLongName" -> "A...ongName": first char, "..." and as much of the end as fits.
            /// Up to about 2 chars too long are tolerated (shortening would replace them by "...").</summary>
            static string TrimStringByWidth(string arg, float cutLength)
            {
                if (StringWidth(arg) <= cutLength + 5f) return arg;
                var w = CharWidth(arg[0]) + 3 * CharWidth('.');
                var i = arg.Length;
                while (i > 1 && w + CharWidth(arg[i - 1]) <= cutLength) w += CharWidth(arg[--i]);
                return arg[0] + "..." + arg.Substring(i);
            }

            public static string GetMassString(double d)
            {
                return GetStringFromDoubleWithSuffixMask(d, WeightFormatStrings);
            }

            private static readonly string[] WeightFormatStrings = new string[] { " 0.0 g  ", " 0.0 kg", " 0.0 T  ", " 0.0 kT" };

            static string GetStringFromDoubleWithSuffixMask(double a, string[] p)
            {
                if (a > 900000.0f)
                {
                    return (a / 1000000).ToString(p[3]);
                }
                else if (a > 900.0f)
                {
                    return (a / 1000).ToString(p[2]);
                }
                else if (a < 1.0f)
                {
                    return (a * 1000).ToString(p[0]);
                }

                return a.ToString(p[1]);
            }
        }
    }
}
