using System;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        static string ToArgStr(string qstr)
        {
            var zstr = "";
            var startOfWord = true;

            foreach (var ctr in qstr)
            {
                // Separators are checked first: otherwise a separator directly following
                // another one consumes the "next character is upper case" flag and gets
                // lower-cased into the key itself.
                if (ctr == ' ' || ctr == ',' || ctr == '-' || ctr == '_' || ctr == '&' || ctr == ':')
                {
                    startOfWord = true;
                    zstr += ctr;
                    continue;
                }

                if (startOfWord)
                {
                    startOfWord = false;
                    zstr += char.ToUpper(ctr);
                    continue;
                }

                zstr += char.ToLower(ctr);
            }

            return zstr;
        }

        /// <summary>
        /// The block name contains a PIM tag "(sms...)", upper/lower case does not matter.
        /// Same rule as Parameter.ParseArgs (checked in ParameterTests), for places that only have the name.
        /// </summary>
        public static bool HasSmsTag(string customName)
        {
            var nameLower = customName.ToLower();
            if (!nameLower.Contains("(sms")) return false;
            foreach (var tag in nameLower.Split('('))
            {
                if (tag.Contains("sms") && tag.Contains(")")) return true;
            }
            return false;
        }

        public class Parameter
        {
            // TODO: parameter as boolean

            private StringBuilderExtended LastCustomName = new StringBuilderExtended(150);

            public Dictionary<string, string> ParameterList = new Dictionary<string, string>();
            public StringBuilderExtended Name = new StringBuilderExtended(150);
            public bool PIMcontrolled { get; private set; }

            public bool ParseArgs(string newCustomName, bool canChangeAutocraftingStatus = false)
            {
                if (LastCustomName.Equal(newCustomName)) return PIMcontrolled;
                LastCustomName.Clear();
                LastCustomName.Append(newCustomName);
                ParameterList.Clear();
                var nameLower = LastCustomName.ToString().ToLower();
                if (nameLower.Contains("(sms"))
                {
                    LastCustomName.Substring(Name, 0, nameLower.IndexOf("(sms"));
                    Name.Trim();
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
                            if (canChangeAutocraftingStatus && PIMcontrolled == false) Properties.Data.AutoCraftingSettingsInvalid = true;
                            PIMcontrolled = true;
                            return true;
                        }
                    }
                }
                Name.Clear();
                Name.Append(newCustomName.Trim());
                if (canChangeAutocraftingStatus && PIMcontrolled == true) Properties.Data.AutoCraftingSettingsInvalid = true;
                PIMcontrolled = false;
                return false;
            }

            void AddParameter(string key, string value = "") { if (!IsParameter(key)) ParameterList.Add(key, value); }

            public bool IsParameter(string key) { return ParameterList.ContainsKey(key); }
        }
    }
}
