using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript
{
    partial class Program
    {
        string Debug_RefineryBPs()
        {
            string DebugText = "Accepted BluePrints by Refinerysubtype\n";
            foreach (string refSubType in Refinery.refineryTypesAcceptedBlueprintsList.Keys)
            {
                DebugText += "SubType:" + refSubType + "\n";
                foreach (var refBP in Refinery.refineryTypesAcceptedBlueprintsList[refSubType])
                {
                    DebugText += "bprint -> " + refBP.Name + "\n";
                }
            }
            return DebugText;
        }

        string Debug_AssemblerBPs()
        {
            string DebugText = "Accepted BluePrints by Assemblersubtype\nPool:\n";
            foreach (var bluePrint in Lists.BluePrints_Inactive)
            {
                DebugText += bluePrint.Value.AutoCraftingType + " -> " + bluePrint.Value.AutoCraftingName + " / " + bluePrint.Value.definition_id + "\n";
            }
            DebugText += "Active:\n";
            foreach (var bluePrint in Lists.BluePrints_Active)
            {
                DebugText += bluePrint.Value.AutoCraftingType + " -> " + bluePrint.Value.AutoCraftingName + " / " + bluePrint.Value.definition_id + "\n";
            }
            return DebugText;
        }

        string Debug_AddIPrioLists()
        {
            var DebugText = "";
            foreach (var IPrioListKey in ingotprio.Keys)
            {
                DebugText += "IPrioList:" + IPrioListKey + "\n";
                foreach (var ip in ingotprio[IPrioListKey])
                {
                    DebugText += "\t- " + ip.refineryBP.Definition_id + " % " + ip.prio + "\n";
                }
            }
            return DebugText;
        }

        string Debug_ComponentPrio()
        {
            var DebugText = "";
            foreach (var item in Lists.BluePrints_Active.Values)
            {
                DebugText += " # " + item.AutoCraftingName + " -> " + item.ItemPriority + "\n";
            }
            return DebugText;
        }

        string Debug_RefineryRecipes()
        {
            string DebugText = "Refineryrecipes\n";
            foreach (var refRecipe in RefineryBlueprints)
            {
                DebugText += refRecipe.Name + " : " + refRecipe.InputIDName + " -> " + refRecipe.OutputIDName + "\n";
            }
            return DebugText;
        }

        string Debug_InventoryManagerList()
        {
            var DebugText = "InventoryManagerList:\n";
            foreach (var inventoryKey in InventoryManagerList.Keys)
            {
                DebugText += " - Key: " + inventoryKey + " / " + InventoryManagerList[inventoryKey].Count + " Inventorys\n";
            }
            return DebugText;
        }

        string Debug_Guns()
        {
            var DebugText = "Guns\n";
            foreach (var gun in guns)
            {
                DebugText += " * " + gun.gun.CustomName + " / " + gun.CurrentAmmo + "\n";
                foreach (var item in gun.ammomax)
                {
                    DebugText += "   - " + item.Key + " / " + item.Value + "\n";
                }
            }
            return DebugText;
        }

        void DebugPrint()
        {
            var panel = GridTerminalSystem.GetBlockWithName("PIMXXXDEBUG") as IMyTextPanel;
            if (panel == null) return;
            if (panel.CubeGrid != Me.CubeGrid) return;
            var s = "";
            s += Debug_InventoryManagerList();
            panel.WriteText(s + "\n" + debugString);
            debugString = "";
        }
    }
}
