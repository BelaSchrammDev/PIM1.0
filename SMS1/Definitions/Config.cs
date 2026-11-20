using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        bool if_true(string s) { return (s == "True" || s == "true"); }
        static Dictionary<string, bool> usemods = new Dictionary<string, bool>();
        void loadConfig()
        {
            // the mods of the item data (Vanilla is always used)
            for (var i = 1; i < ItemDataMods.Length; i++) usemods.Add(ItemDataMods[i], false);
            // set by the first "### MOD=..." line: from there on the lines are item definitions
            Dictionary<MyItemType, int> curModItemDef = null;
            foreach (var cline in Me.CustomData.Split('\n'))
            {
                var cstr = cline.Trim();
                // comments, and the section markers of older versions ("### Config ###" ..., no '=')
                if (cstr.Length == 0 || cstr[0] == '/' || cstr[0] == '@') continue;
                if (cstr.StartsWith(X_tNewMod) || curModItemDef != null)
                {
                    var cs = cstr.Split('=', '(', ')');
                    if (cs.Length < 2) continue;
                    if (cs[0] == X_tNewMod)
                    {
                        if (!modItemDefsList.ContainsKey(cs[1])) modItemDefsList.Add(cs[1], new Dictionary<MyItemType, int>());
                        curModItemDef = modItemDefsList[cs[1]];
                        continue;
                    }
                    var types = cs[0].Split('/');
                    if (types.Length > 1)
                    {
                        var t = new MyItemType(types[0], types[1]);
                        int amount;
                        int.TryParse(cs[1], out amount);
                        if (!curModItemDef.ContainsKey(t)) curModItemDef.Add(t, amount);
                        if (cs.Length > 2 && cs[2] != "" && !itemAlterNameList.ContainsKey(t)) itemAlterNameList.Add(t, cs[2]);
                    }
                }
                else
                {
                    var cs = cstr.Split('=');
                    if (cs.Length < 2) continue;
                    switch (cs[0])
                    {
                        case X_ShowInfoPBLcd: ShowInfoPBLcd = if_true(cs[1]); break;
                        case X_ShowDamageBlockHUD: ShowDamageBlockOnHUD = if_true(cs[1]); break;
                        case X_KeepMenu: int.TryParse(cs[1], out KEEPMENU); break;
                        case X_TargetCycleTime: double.TryParse(cs[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out configuredCycleSeconds); break;
                        case X_tHeadLine: tHeadLine = cs[1]; break;
                        case X_Scriptmode:
                            var mode = cs[1].ToUpper();
                            if (mode == "SHIP") configuredMode = ScriptMode.SHIP;
                            else if (mode == "SHIP_PLUS") configuredMode = ScriptMode.SHIP_PLUS;
                            else if (mode == "STATION") configuredMode = ScriptMode.STATION;
                            else configuredMode = ScriptMode.AUTO;
                            break;
                        default:
                            if (if_true(cs[1]) && usemods.ContainsKey(cs[0])) usemods[cs[0]] = true;
                            break;
                    }
                }
            }
            writeconfig();
        }
        void writeconfig()
        {
            // one short comment per setting, the details are in the Steam guide
            var customstr = "  / SMS config (details: Steam guide), restart the script after changes\n\n"
                + "  / display update in s, 0 = auto (ship 1, station 3)\n"
                + X_TargetCycleTime + "=" + configuredCycleSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture) + "\n"
                + "  / AUTO, SHIP, SHIP_PLUS, STATION\n"
                + X_Scriptmode + "=" + configuredMode + "\n"
                + X_ShowInfoPBLcd + "=" + ShowInfoPBLcd + "\n"
                + "  / needs an antenna\n"
                + X_ShowDamageBlockHUD + "=" + ShowDamageBlockOnHUD + "\n"
                + "  / s until terminals return to the main menu, 0 = never\n"
                + X_KeepMenu + "=" + KEEPMENU + "\n"
                + X_tHeadLine + "=" + tHeadLine + "\n\n"
                + "  / mods\n";
            foreach (var mod in usemods.Keys) customstr += mod + "=" + usemods[mod] + "\n";
            customstr += "\n  / items: type=max amount of the fillbar (alternative name)\n";
            foreach (var mod in modItemDefsList.Keys)
            {
                if (mod == X_M_Vanilla || (usemods.ContainsKey(mod) && usemods[mod]))
                {
                    customstr += "\n" + X_tNewMod + "=" + mod + "\n";
                    foreach (var i in modItemDefsList[mod].Keys)
                    {
                        customstr += i + "=" + modItemDefsList[mod][i] + (itemAlterNameList.ContainsKey(i) ? " (" + itemAlterNameList[i] + ")" : "") + "\n";
                    }
                }
            }
            Me.CustomData = customstr;
        }
    }
}
