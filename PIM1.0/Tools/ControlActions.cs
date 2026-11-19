using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        /// <summary>
        /// Menu actions of SMS (IGC message NetWorkingScripts.RunAction): PIM control on/off for the refineries
        /// and assemblers of the construct, by the tag "(sms)" in the block name (the same as setting it by hand).
        /// </summary>
        static class ControlActions
        {
            /// <summary>ctrl_ref, ctrl_ass, ctrl_all, noctrl_ref, noctrl_ass; other actions are ignored.</summary>
            public static void Run(string action)
            {
                var on = action.StartsWith("ctrl_");
                if (!on && !action.StartsWith("noctrl_")) return;
                var what = action.Substring(action.IndexOf('_') + 1);
                if (what == "ref" || what == "all") SetTags(Lists.Data.RefineryList, on);
                if (what == "ass" || what == "all") SetTags(Lists.Data.AssemblerList, on);
            }

            static void SetTags<T>(List<T> blocks, bool on) where T : ManageableBlock
            {
                foreach (var b in blocks)
                {
                    var block = b.FunctionalBlock;
                    if (block != null && !block.Closed) block.CustomName = WithSmsTag(block.CustomName, on);
                }
            }
        }
    }
}
