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
            if (!DisplayBoxList.ContainsKey(boxID))
            {
                var newDisplayBox = new DisplayBox(width);
                DisplayBoxList.Add(boxID, newDisplayBox);
            }
            return DisplayBoxList[boxID];
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

        static string GetDisplayBoxStringDisplayNull(int amount, float width, bool left = false)
        {
            var displaytext = amount.ToString();
            return GetDisplayBox("###" + displaytext + width.ToString() + left.ToString(), width).GetStringWithSpaces(displaytext, left);
        }

        static string GetDisplayBoxString(string displaytext, float width, bool left = false)
        {
            return GetDisplayBox(displaytext + width.ToString() + left.ToString(), width).GetStringWithSpaces(displaytext, left);
        }

        // add DisplayPool Class
        class DisplayBox
        {
            const char HSS = '\u00AD';

            static List<SP> SpacePoolList = new List<SP>();
            static Dictionary<char, float> CharWidthList = new Dictionary<char, float>();

            static DisplayBox()
            {
                InitCharWidthList();
            }

            string LeftText = "", RightText = "", BoxText = "", SpaceString = "";
            float Width = 0;

            public DisplayBox(float iwidth)
            {
                Width = iwidth;
            }

            class SP
            {
                static int SpacePoolLiveCycle = 700;
                int LiveCycle = 5;
                public float Width = 0;
                string FillString = "";
                public SP(float iw)
                {
                    Width = iw;
                    if (Width < 0.6f) return;
                    int spnum = (int)(Width / 1.29166f);
                    int hsnum = (int)((Width - (spnum * 1.29166f)) / 0.287035f);
                    if (hsnum >= spnum)
                    {
                        spnum += 1;
                        hsnum = 0;
                    }
                    else spnum -= hsnum;
                    FillString += new String(' ', spnum);
                    FillString += new String(HSS, hsnum);
                }
                public string GetFillString()
                {
                    LiveCycle = 5;
                    RefreshSpacePoolList();
                    return FillString;
                }
                static void RefreshSpacePoolList()
                {
                    if (--SpacePoolLiveCycle < 1)
                    {
                        for (int i = SpacePoolList.Count - 1; i > 0; i--)
                        {
                            SP osp = SpacePoolList[i];
                            if (--osp.LiveCycle < 0) SpacePoolList.Remove(osp);
                        }
                        SpacePoolLiveCycle = 500;
                    }
                }
            }

            public string Get2StringWithSpaces(string arg1, string arg2)
            {
                if (arg1 != LeftText || arg2 != RightText)
                {
                    var strLength2 = GetStringWidth(arg2);
                    arg1 = TrimStringByWidth(arg1, Width - 2.6f - strLength2);
                    SpaceString = GetSpaceStringByWidth(Width - GetStringWidth(arg1) - strLength2);
                    LeftText = arg1;
                    RightText = arg2;
                }
                BoxText = LeftText + SpaceString + RightText;
                return BoxText;
            }

            public string GetStringWithSpaces(string arg, bool leftAlignment = false)
            {
                if (arg != LeftText)
                {
                    arg = TrimStringByWidth(arg, Width - 2.6f);
                    SpaceString = GetSpaceStringByWidth(Width - GetStringWidth(arg));
                    LeftText = arg;
                }
                if (leftAlignment) BoxText = LeftText + SpaceString;
                else BoxText = SpaceString + LeftText;
                return BoxText;
            }

            string TrimStringByWidth(string arg, float cutLength)
            {
                var strLength = GetStringWidth(arg);
                if (strLength > cutLength)
                {
                    var charDiff = (int)((strLength - cutLength - 5f) / 1.5f);
                    if (charDiff > 0 && charDiff < arg.Length - 1)
                    {
                        var lastString = arg.Substring(charDiff);
                        return arg[0] + "..." + lastString;
                    }
                }
                return arg;
            }

            static SP GetSpaceStringObject(float with) 
            {
                SP nsp = SpacePoolList.Find(sp => sp.Width == with);

                if (nsp == null)
                {
                    nsp = new SP(with);
                    SpacePoolList.Add(nsp);
                }

                return nsp;
            }

            static string GetSpaceStringByWidth(float with) 
            {
                return GetSpaceStringObject(with).GetFillString(); 
            }

            static void InitCharWidthList()
            {
                SetCharWidth("\n", 0f);
                SetCharWidth("'|ÎÏ", 1f);
                SetCharWidth(" !`Iiîïjl", 1.29166f);
                SetCharWidth("(),.:;[]{}1ft", 1.43076f);
                SetCharWidth("\"-r", 1.57627f);
                SetCharWidth("*", 1.72222f);
                SetCharWidth("\\", 1.86f);
                SetCharWidth("/", 2.16279f);
                SetCharWidth("«»Lvx_ƒ", 2.325f);
                SetCharWidth("?7Jcçz", 2.44736f); 
                SetCharWidth("3FKTaäàâbdeèéêëghknoöôpqsuüùûßyÿ", 2.58333f); 
                SetCharWidth("+<>=^~EÈÉÊË", 2.73529f); 
                SetCharWidth("#0245689CÇXZ", 2.90625f); 
                SetCharWidth("$&GHPUÜÙÛVYŸ", 3f);
                SetCharWidth("AÄÀÂBDNOÖÔQRS", 3.20689f);
                SetCharWidth("%", 3.57692f);
                SetCharWidth("@", 3.72f);
                SetCharWidth("M", 3.875f);
                SetCharWidth("æœmw", 4.04347f);
                SetCharWidth("WÆŒ", 4.65f);
                CharWidthList.Add(HSS, 1.578695f);
            }

            static void SetCharWidth(string s, float z)
            {
                foreach (var c in s)
                {
                    CharWidthList.Add(c, z);
                }
            }

            static float GetStringWidth(string strData)
            {
                float fltTotal = 0;
                foreach (var c in strData) fltTotal += CharWidthList.ContainsKey(c) ? CharWidthList[c] : 2f;
                return fltTotal;
            }

            public static string GetMassString(double d)
            {
                return GetStringFromDoubleWithSuffixMask(d, WeightFormatStrings);
            }

            public static string GetIntString(double d)
            {
                return GetStringFromDoubleWithSuffixMask(d, DigitalFormatString);
            }

            private static readonly string[] DigitalFormatString = new string[] { " 0.# m ", " 0.#   ", " 0.# k", " 0.# M" };
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
                    (a * 1000).ToString(p[0]);
                }

                return a.ToString(p[1]);
            }
        }
    }
}
