namespace IngameScript
{
    partial class Program
    {
        // the constants of SMS: one declaration per type, grouped by comments
        // (every 'const string' costs chars in the packed script, the comments do not)

        // LCD layout: column widths of the tables
        const int zW = 20, pW = 31;

        const string

            // version: PB info and networking hello
            SmsVersion = "SMS v1.5a",

            // LCD command options: the grid of a list
            OptMainGrid = "MAINGRID",
            OptLcdGrid = "LCDGRID",
            OptGridName = "GRIDNAME",

            // soft hyphen: an invisible character (as padding in short texts)
            SoftHyphen = "­",
            Cfilled = "@-(filled)",
            // (item types: Share/Common/ItemTypes.cs)

            // CustomData config: section markers
            X_Config = "### Config ###",
            X_Config_end = "### Config End ###",
            X_tItemDefs = "### ItemDefinitions ###",
            X_tItemdefsEnd = "### ItemDefinitionsEnd ###",
            X_tNewMod = "### MOD",

            // CustomData config: keys
            X_ShowInfoPBLcd = "ShowInfoOnPBLcd",
            X_ShowDamageBlockHUD = "ShowDamageBlocksOnHud",
            X_KeepMenu = "KeepMenuToReturn",
            X_TargetCycleTime = "TargetCycleTime",
            X_Scriptmode = "ScriptMode",
            X_M_Vanilla = "Vanilla",
            X_tHeadLine = "TerminalHeadLine",
            X_tMainPage = "TerminalMainPageBody",
            X_tMainPageEnd = X_tMainPage + "End",

            // CustomData config: comment lines
            X_Line = "  / =================================\n",
            X_LineNewSite = X_Line + "@newsite(",
            X_Line2 = "  / ------------\n";
    }
}
