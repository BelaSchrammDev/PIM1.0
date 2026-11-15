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
        static Dictionary<string, bool> usemods = new Dictionary<string, bool>();
        public void loadConfig()
        {
            // the mods of the item data (Vanilla is always used)
            for (var i = 1; i < ItemDataMods.Length; i++) usemods.Add(ItemDataMods[i], false);
            var configteil = false;
            var mainpageteil = false;
            var itemdefsteil = false;
            Dictionary<MyItemType, int> curModItemDef = null;
            foreach (var cline in Me.CustomData.Split('\n'))
            {
                var cstr = cline.Trim();
                if (cstr.Length == 0 || cstr[0] == '/') continue;
                else if (cstr[0] == '@') continue;
                else if (cstr.Contains(X_Config)) { configteil = true; continue; }
                else if (cstr.Contains(X_Config_end)) { configteil = false; continue; }
                else if (cstr.Contains(X_tItemDefs)) { itemdefsteil = true; continue; }
                else if (cstr.Contains(X_tItemdefsEnd)) { itemdefsteil = false; continue; }
                else if (itemdefsteil)
                {
                    var cs = new List<string>(cstr.Split('=', '(', ')'));
                    if (cs.Count < 2) continue;
                    if (cs[0] == X_tNewMod)
                    {
                        if (!modItemDefsList.ContainsKey(cs[1]))
                        {
                            modItemDefsList.Add(cs[1], new Dictionary<MyItemType, int>());
                        }
                        curModItemDef = modItemDefsList[cs[1]];
                        continue;
                    }
                    if (curModItemDef != null)
                    {
                        var types = cs[0].Split('/');
                        if (types.Length > 1)
                        {
                            var t = new MyItemType(types[0], types[1]);
                            int amount = 0;
                            int.TryParse(cs[1], out amount);
                            if (!curModItemDef.ContainsKey(t)) curModItemDef.Add(t, amount);
                            if (cs.Count > 2 && cs[2] != "" && !itemAlterNameList.ContainsKey(t)) itemAlterNameList.Add(t, cs[2]);
                        }
                        continue;
                    }
                }
                else if (configteil)
                {
                    if (mainpageteil)
                    {
                        tMainPage += cstr + "\n";
                        continue;
                    }
                    var cs = cstr.Split('=');
                    if (cs.Length < 2) continue;
                    switch (cs[0])
                    {
                        case X_ShowInfoPBLcd: ShowInfoPBLcd = if_true(cs[1]); break;
                        case X_ShowDamageBlockHUD: ShowDamageBlockOnHUD = if_true(cs[1]); break;
                        case X_KeepMenu: int.TryParse(cs[1], out KEEPMENU); break;
                        case X_TargetCycleTime: double.TryParse(cs[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out targetCycleTime); break;
                        case X_tHeadLine: tHeadLine = cs[1]; break;
                        case X_tMainPage: mainpageteil = true; break;
                        case X_tMainPageEnd: mainpageteil = false; break;
                        case X_tItemDefs: itemdefsteil = true; break;
                        case X_tItemdefsEnd: itemdefsteil = false; break;
                        case X_Scriptmode:
                            var mode = cs[1].ToUpper();
                            if (mode == "SHIP") s_mode = ScriptMode.SHIP;
                            else if (mode == "SHIP_PLUS") s_mode = ScriptMode.SHIP_PLUS;
                            else if (mode == "STATION") s_mode = ScriptMode.STATION;
                            else s_mode = ScriptMode.AUTO;
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
            var customstr = X_Config + "\n\n" + X_Line + "  / to activate changes, please restart script\n" + X_Line + "\n  / target cycle time in seconds: how often the displays are updated\n  / smaller = more up to date, larger = less load on the server\n  / 0 = automatic: ship 1 s, station 3 s\n"
                + X_TargetCycleTime + "=" + targetCycleTime.ToString(System.Globalization.CultureInfo.InvariantCulture) + "\n\n"
                + "  / Scriptmode: AUTO, SHIP, SHIP_PLUS, STATION\n"
                + X_Scriptmode + "=" + s_mode.ToString() + "\n\n"
                + "  / show info on the lcd of the programmable block\n"
                + X_ShowInfoPBLcd + "=" + ShowInfoPBLcd.ToString() + "\n\n"
                + "  / show damaged blocks on HUD (need antenna on the grid)\n"
                + X_ShowDamageBlockHUD + "=" + ShowDamageBlockOnHUD + "\n\n"
                + "  / time to return terminals to the mainmenu (global) in sec.\n  / 0 = never return\n"
                + X_KeepMenu + "=" + KEEPMENU.ToString() + "\n\n"
                + "  / terminal headline\n"
                + X_tHeadLine + "=" + tHeadLine + "\n\n"
                + "  / terminal mainpage body\n"
                + X_tMainPage + "\n" + tMainPage + "\n" + X_tMainPageEnd + "\n\n"
            + X_Line + "  / mods that can be used.\n  /     is there a mod missing? \n  /           write it in the comments of SMS or PIM\n\n";
            foreach (var mod in usemods.Keys) customstr += mod + "=" + usemods[mod].ToString() + "\n";
            customstr += "\n" + X_Line + X_tItemDefs + "\n  / maximumamount for the fillbar in the inventory command\n  / it is possible to specify an alternative name of the item\n  / write it in brackets after the amount, like\n  / MyObjectBuilder_Component/Explosives=500 (Kawumm)\n";
            foreach (var mod in modItemDefsList.Keys)
            {
                if (mod == X_M_Vanilla || (usemods.ContainsKey(mod) && usemods[mod]))
                {
                    customstr += "\n" + X_Line + X_tNewMod + "=" + mod + "\n" + X_Line2;
                    foreach (var i in modItemDefsList[mod].Keys)
                    {
                        customstr += i + "=" + modItemDefsList[mod][i] + (itemAlterNameList.ContainsKey(i) ? " (" + itemAlterNameList[i] + ")" : "") + "\n";
                    }
                }
            }
            customstr += "\n" + X_tItemdefsEnd + "\n";
            Me.CustomData = customstr + X_Line + "\n" + X_Config_end + "\n\n";
        }
    }
}
