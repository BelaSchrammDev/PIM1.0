using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        // the constants of SMS (only constants: they cost nothing in the packed script but their uses)

        // version: PB info and networking hello
        const string SmsVersion = "SMS v1.5a";

        // LCD layout: column widths of the tables
        const int zW = 20, pW = 31;

        // LCD command options: the grid of a list
        const string OptMainGrid = "MAINGRID", OptLcdGrid = "LCDGRID", OptGridName = "GRIDNAME";
        // soft hyphen: an invisible character (as padding in short texts)
        const string SoftHyphen = "\u00AD", Cfilled = "@-(filled)";

        // item types (MyObjectBuilder_ + type)
        const string IG_Datas = "Datapad", IG_Kits = "ConsumableItem", IG_Cash = "PhysicalObject", IG_Tools = "PhysicalGunObject", IG_HBottles = "GasContainerObject", IG_OBottles = "OxygenContainerObject", IG_Ammo = "AmmoMagazine", IG_ = "MyObjectBuilder_";

        // CustomData config: section markers, keys and comment lines
        const string X_Config = "### Config ###", X_Config_end = "### Config End ###", X_ShowInfoPBLcd = "ShowInfoOnPBLcd", X_ShowDamageBlockHUD = "ShowDamageBlocksOnHud", X_Line = "  / =================================\n", X_LineNewSite = X_Line + "@newsite(", X_Line2 = "  / ------------\n";
        const string X_KeepMenu = "KeepMenuToReturn", X_TargetCycleTime = "TargetCycleTime", X_tHeadLine = "TerminalHeadLine", X_tMainPage = "TerminalMainPageBody", X_tMainPageEnd = X_tMainPage + "End", X_tItemDefs = "### ItemDefinitions ###", X_tItemdefsEnd = "### ItemDefinitionsEnd ###", X_tNewMod = "### MOD";
        const string X_Scriptmode = "ScriptMode", X_M_Vanilla = "Vanilla";
    }
}
