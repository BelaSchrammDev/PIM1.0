namespace IngameScript
{
    partial class Program
    {
        /// <summary>
        /// The block name contains a PIM tag "(sms...)", upper/lower case does not matter.
        /// Same rule as Parameter.ParseArgs (Share/Common/Parameter.cs, checked in ParameterTests), for places that only have the name.
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

        /// <summary>
        /// The name with the tag "(sms)" (on), or without the tag "(sms...)" (off: its parameters are removed too,
        /// the text after the tag stays). A name that already is as wanted is returned unchanged.
        /// </summary>
        public static string WithSmsTag(string name, bool on)
        {
            if (HasSmsTag(name) == on) return name;
            if (on) return name.Trim() + " (sms)";
            var start = name.ToLower().IndexOf("(sms");
            var end = name.IndexOf(')', start);
            return (name.Substring(0, start).TrimEnd() + " " + (end < 0 ? "" : name.Substring(end + 1).TrimStart())).Trim();
        }

        /// <summary>
        /// Parses the tag of a production block. A changed tag changes the blocks autocrafting may use,
        /// so the autocrafting settings are checked again.
        /// </summary>
        public static bool ParseProductionTag(Parameter parameter, string customName)
        {
            var wasTagged = parameter.Tagged;
            if (parameter.ParseArgs(customName) != wasTagged) Properties.Data.AutoCraftingSettingsInvalid = true;
            return parameter.Tagged;
        }
    }
}
