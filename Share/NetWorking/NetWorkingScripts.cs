namespace IngameScript
{
    partial class Program
    {
        /// <summary>
        /// The script types and application message types of my scripts (shared, so both sides use the same strings).
        /// Only constants: minify=full renames type and member names, not string values.
        /// </summary>
        public static class NetWorkingScripts
        {
            public const string

            // script types (master election among the scripts of one type)
            Pim = "PIM",
            Sms = "SMS",

            // PIM master -> SMS master: the PIM data lines (@INGOTPRIOLIST, @INGOTPRIO, @CARGOUSE, @ITEMMAX, @ASSEMBLERQUEUE)
            PimData = "pimdata",

            // SMS standby -> SMS master: a run argument (trigger of a button panel etc.) for the master
            Command = "command";
        }
    }
}
