using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        // word separators of ToArgStr; the soft hyphen can come with texts copied from an LCD
        const string ArgSeparators = " ,-_&:" + SoftHyphen;

        // Every word starts with an upper case letter, the rest is lower case ("infolcd:lines" -> "Infolcd:Lines").
        // Separators are checked first: otherwise a separator directly following another one consumes the
        // "next character is upper case" flag and gets lower-cased into the key itself.
        static string ToArgStr(string text)
        {
            var result = "";
            var startOfWord = true;
            foreach (var c in text)
            {
                if (ArgSeparators.IndexOf(c) >= 0)
                {
                    startOfWord = true;
                    result += c;
                }
                else
                {
                    result += startOfWord ? char.ToUpper(c) : char.ToLower(c);
                    startOfWord = false;
                }
            }
            return result;
        }

        // The tag "(sms...)" in a block name (any case) and its parameters, shared by PIM and SMS:
        // "Box (sms,Infolcd:2, lines:10)" -> Name "Box", parameters Infolcd = "2", Lines = "10".
        public class Parameter
        {
            string lastCustomName;
            public Dictionary<string, string> ParameterList = new Dictionary<string, string>();
            // the name without the tag (trimmed)
            public string Name = "";
            // the name has the tag
            public bool Tagged;

            // parses the name, an unchanged name keeps the last result; returns Tagged
            public bool ParseArgs(string customName)
            {
                if (customName == lastCustomName) return Tagged;
                lastCustomName = customName;
                ParameterList.Clear();
                var nameLower = customName.ToLower();
                if (nameLower.Contains("(sms"))
                {
                    Name = customName.Substring(0, nameLower.IndexOf("(sms")).Trim();
                    foreach (var tag in nameLower.Split('('))
                    {
                        if (tag.Contains("sms") && tag.Contains(")"))
                        {
                            foreach (var raw in ToArgStr(tag).Split(',', ')'))
                            {
                                var s = raw.Trim();
                                if (s != "" && s != "Sms")
                                {
                                    var x = s.IndexOf(':');
                                    if (x > -1) AddParameter(s.Substring(0, x), s.Substring(x + 1));
                                    else AddParameter(s);
                                }
                            }
                            return Tagged = true;
                        }
                    }
                }
                Name = customName.Trim();
                return Tagged = false;
            }

            void AddParameter(string key, string value = "") { if (!IsParameter(key)) ParameterList.Add(key, value); }

            public bool IsParameter(string key) { return ParameterList.ContainsKey(key); }

            // the value of the parameter, "" if it is missing or has none
            public string GetParameter(string key) { string value; return ParameterList.TryGetValue(key, out value) ? value : ""; }
        }
    }
}
