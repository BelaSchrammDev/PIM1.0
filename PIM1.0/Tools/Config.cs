using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript
{
    partial class Program
    {
        void WriteConfig()
        {
            var configstr = "  / attention!!!\n  / autocraftingconfig now via LCD Display,\n  / place a LCD and add '..(sms,autocrafting) to the name.\n  / follow the instructions, multiple lcds are possible'\n\n" + X_Config + "\n\n" + X_Line + "  / options set to 'True' or 'False'.\n  / to activate changes, please restart script\n"
            + X_Line + "\n  / show info on programmable blocks LCD\n"
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
            + "  / group of PIM controlled Weapons\n  / Control of WeaponCore Turrets is not necessary\n  / and should remain switched off.\n"
            + "PIM_controlled_Weapons=" + gungroupName + "\n\n"
            + X_Line + "  / mods that can be used.\n  /     is there a mod missing? \n  /           write it in the comments of SMS or PIM\n\n";

            foreach (var mod in usedMods.Keys)
            {
                configstr += mod + "=" + usedMods[mod].ToString() + "\n";
            }

            configstr += "\n" + X_Config_end + "\n";
            Me.CustomData = configstr;
        }

        void LoadConfig()
        {

            string[] modInitList =
            {
                M_DailyNeedsSurvival,
                M_AzimuthThruster,
                M_SG_Gates,
                M_SG_Ores,
                M_PaintGun,
                M_DeuteriumReactor,
                M_Shield,
                M_RailGun,
                M_HomingWeaponry,
                M_IndustrialOverhaulMod,
                M_IndustrialOverhaulLLMod,
                M_IndustrialOverhaulWaterMod,
                M_EatDrinkSleep,
                M_PlantCook,
                M_AryxEpsteinDrive,
                M_NorthWindWeapons,
                M_HSR,
                M_SigmaDraconisCore,
            };

            foreach (var m in modInitList)
            {
                usedMods.Add(m, false);
            }

            bool config = false;
            foreach (var s1 in Me.CustomData.Split('\n'))
            {
                var s = s1.Trim();

                if (s.Length == 0 || s[0] == '/')
                {
                    continue;
                }
                else if (s == X_Config)
                {
                    config = true;
                    continue;
                }
                else if (s == X_Config_end)
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
                        case "ShowInfoPBLcd": ShowInfoPBLcd = if_true(cs[1]); break;
                        case "delete_queueItem_if_max": delete_queueItem_if_max = if_true(cs[1]); break;
                        case "always_recycle_greywater": always_recycle_greywater = if_true(cs[1]); break;
                        case "assemblers_off": assemblers_off = if_true(cs[1]); break;
                        case "refinerys_off": refinerys_off = if_true(cs[1]); break;
                        case "collect_all_Ore": collect_all_Ore = if_true(cs[1]); break;
                        case "collect_all_Ingot": collect_all_Ingot = if_true(cs[1]); break;
                        case "collect_all_Component": collect_all_Component = if_true(cs[1]); break;
                        case "stacking_cycle": int.TryParse(cs[1], out stacking_cycle); break;
                        case "PIM_controlled_Weapons": gungroupName = cs[1]; break;
                        default: if (usedMods.ContainsKey(cs[0])) usedMods[cs[0]] = if_true(cs[1]); break;
                    }

                    continue;
                }
            }

            WriteConfig();
        }
    }
}
