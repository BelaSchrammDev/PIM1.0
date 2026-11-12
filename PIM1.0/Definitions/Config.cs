using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript
{
    partial class Program
    {
        public class Config : Tools
        {
            private static Config _instance;
            public static Config Instance 
            {
                get
                {
                    if(_instance == null)
                    {
                        _instance = new Config();
                    }
                    return _instance;
                }
            }

            public bool ShowInfoPBLcd = true;
            public bool delete_queueItem_if_max = true;
            public bool always_recycle_greywater = true;
            public bool assemblers_off = true;
            public bool refinerys_off = true;
            public bool collect_all_Ore = true;
            public bool collect_all_Ingot = true;
            public bool collect_all_Component = true;
            public int stacking_cycle = 10;
            public Dictionary<string, bool> usedMods = new Dictionary<string, bool>();

            // an invalid value (typo) keeps the current setting instead of stopping the script
            bool ParseBoolean(string str, bool current)
            {
                bool value;
                return bool.TryParse(str, out value) ? value : current;
            }

            int ParseInt(string str, int current)
            {
                int value;
                return int.TryParse(str, out value) ? value : current;
            }

            void WriteConfig()
            {
                var configstr =
                      "  / attention!!!\n"
                    + "  / autocraftingconfig now via LCD Display,\n"
                    + "  / place a LCD and add '..(sms,autocrafting) to the name.\n"
                    + "  / follow the instructions, multiple lcds are possible'\n\n"
                    + Strings.X_Config
                    + "\n\n"
                    + Strings.X_Line
                    + "  / options set to 'True' or 'False'.\n"
                    + "  / to activate changes, please restart script\n"
                    + Strings.X_Line + "\n  / show info on programmable blocks LCD\n"
                    + "ShowInfoPBLcd=" + ShowInfoPBLcd.ToString() + "\n\n"
                    + "  / delete item from the production list (Assemblers)\n  / when the maximum value is reached.\n"
                    + "delete_queueItem_if_max=" + delete_queueItem_if_max.ToString() + "\n\n"
                    + "  / always recycle grey water on the Water Recycling System Block\n  / (only Daily Needs Survival Mod)\n"
                    + "always_recycle_greywater=" + always_recycle_greywater.ToString() + "\n\n"
                    + "  / turn all assemblers off when production queue is empty\n"
                    + "assemblers_off=" + assemblers_off.ToString() + "\n\n"
                    + "  / turn all refinerys off when inbound inventory is empty\n"
                    + "refinerys_off=" + refinerys_off.ToString() + "\n\n"
                    + "  / collect all ore\n"
                    + "collect_all_Ore=" + collect_all_Ore.ToString() + "\n\n"
                    + "  / collect all ingot\n"
                    + "collect_all_Ingot=" + collect_all_Ingot.ToString() + "\n\n"
                    + "  / collect all component\n"
                    + "collect_all_Component=" + collect_all_Component.ToString() + "\n\n"
                    + "  / stackingcycle in seconds, 0 = stacking off\n"
                    + "stacking_cycle=" + stacking_cycle.ToString() + "\n\n"
                    + "  / group of PIM controlled Weapons\n"
                    + "  / Control of WeaponCore Turrets is not necessary\n"
                    + "  / and should remain switched off.\n"
                    + "PIM_controlled_Weapons=" + Properties.Data.CurrentGunGroupName + "\n\n"
                    + Strings.X_Line
                    + "  / mods that can be used.\n"
                    + "  /     is there a mod missing? \n"
                    + "  /           write it in the comments of SMS or PIM\n\n";

                foreach (var mod in usedMods.Keys)
                {
                    configstr += mod + "=" + usedMods[mod].ToString() + "\n";
                }

                configstr += "\n" + Strings.X_Config_end + "\n";
                MyProgram.Me.CustomData = configstr;
            }

            public void LoadConfig()
            {
                // all mods with blueprint data (BluePrintData/Mods.txt), Vanilla is always on
                foreach (var m in BluePrintMods)
                {
                    if (m != Strings.M_Vanilla) usedMods.Add(m, false);
                }

                bool config = false;
                foreach (var s1 in MyProgram.Me.CustomData.Split('\n'))
                {
                    var s = s1.Trim();

                    if (s.Length == 0 || s[0] == '/')
                    {
                        continue;
                    }
                    else if (s == Strings.X_Config)
                    {
                        config = true;
                        continue;
                    }
                    else if (s == Strings.X_Config_end)
                    {
                        break;
                    }

                    if (config)
                    {
                        var cs = s.Split('=');
                        if (cs.Length < 2)
                        {
                            continue;
                        }

                        switch (cs[0])
                        {
                            case "ShowInfoPBLcd": ShowInfoPBLcd = ParseBoolean(cs[1], ShowInfoPBLcd); break;
                            case "delete_queueItem_if_max": delete_queueItem_if_max = ParseBoolean(cs[1], delete_queueItem_if_max); break;
                            case "always_recycle_greywater": always_recycle_greywater = ParseBoolean(cs[1], always_recycle_greywater); break;
                            case "assemblers_off": assemblers_off = ParseBoolean(cs[1], assemblers_off); break;
                            case "refinerys_off": refinerys_off = ParseBoolean(cs[1], refinerys_off); break;
                            case "collect_all_Ore": collect_all_Ore = ParseBoolean(cs[1], collect_all_Ore); break;
                            case "collect_all_Ingot": collect_all_Ingot = ParseBoolean(cs[1], collect_all_Ingot); break;
                            case "collect_all_Component": collect_all_Component = ParseBoolean(cs[1], collect_all_Component); break;
                            case "stacking_cycle": stacking_cycle = ParseInt(cs[1], stacking_cycle); break;
                            case "PIM_controlled_Weapons": Properties.Data.CurrentGunGroupName = cs[1]; break;
                            default: if (usedMods.ContainsKey(cs[0])) usedMods[cs[0]] = ParseBoolean(cs[1], usedMods[cs[0]]); break;
                        }

                        continue;
                    }
                }

                WriteConfig();
            }
        }
    }
}
