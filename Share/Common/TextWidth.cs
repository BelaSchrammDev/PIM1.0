using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        // Text widths of the default LCD font, shared by PIM and SMS: the width of each character at font size 1
        // (units as SMS has always measured them), unknown characters count as 2.
        // The soft hyphen is invisible but has a width: as fill it allows finer steps than spaces.

        const float SpaceWidth = 1.29166f;
        static Dictionary<char, float> charWidths = CreateCharWidths();

        static Dictionary<char, float> CreateCharWidths()
        {
            var groups = new[] { "\n", "'|ÎÏ", " !`Iiîïjl", "(),.:;[]{}1ft", "\"-r", "*", "\\", "/", "«»Lvx_ƒ", "?7Jcçz",
                "3FKTaäàâbdeèéêëghknoöôpqsuüùûßyÿ", "+<>=^~EÈÉÊË", "#0245689CÇXZ", "$&GHPUÜÙÛVYŸ", "AÄÀÂBDNOÖÔQRS",
                "%", "@", "M", "æœmw", "WÆŒ", SoftHyphen };
            var widths = new[] { 0f, 1f, SpaceWidth, 1.43076f, 1.57627f, 1.72222f, 1.86f, 2.16279f, 2.325f, 2.44736f,
                2.58333f, 2.73529f, 2.90625f, 3f, 3.20689f,
                3.57692f, 3.72f, 3.875f, 4.04347f, 4.65f, 1.578695f };
            var result = new Dictionary<char, float>();
            for (int i = 0; i < groups.Length; i++)
                foreach (var c in groups[i]) result.Add(c, widths[i]);
            return result;
        }

        static float CharWidth(char c)
        {
            float width;
            return charWidths.TryGetValue(c, out width) ? width : 2f;
        }

        static float StringWidth(string text)
        {
            float width = 0;
            foreach (var c in text) width += CharWidth(c);
            return width;
        }

        // A fill text of the width: spaces, some of them replaced by soft hyphens (each one 0.287035 wider).
        // The texts are pooled by width; an entry not used for 5 cleanups is removed.
        class SpaceFill
        {
            public float Width;
            public string Text;
            public int Life;
        }
        static List<SpaceFill> spaceFills = new List<SpaceFill>();
        static int spaceFillCleanup = 700;

        static string SpaceFillText(float width)
        {
            if (--spaceFillCleanup < 1)
            {
                for (int i = spaceFills.Count - 1; i >= 0; i--)
                    if (--spaceFills[i].Life < 0) spaceFills.RemoveAt(i);
                spaceFillCleanup = 500;
            }
            foreach (var fill in spaceFills)
                if (fill.Width == width)
                {
                    fill.Life = 5;
                    return fill.Text;
                }
            var text = "";
            if (width >= 0.6f)
            {
                int spaces = (int)(width / SpaceWidth), hyphens = (int)((width - spaces * SpaceWidth) / 0.287035f);
                if (hyphens >= spaces)
                {
                    spaces++;
                    hyphens = 0;
                }
                else spaces -= hyphens;
                text = new string(' ', spaces) + new string(SoftHyphen[0], hyphens);
            }
            spaceFills.Add(new SpaceFill { Width = width, Text = text, Life = 5 });
            return text;
        }
    }
}
