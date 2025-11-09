using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        static string LCD_DebugString = "";

        //string Debug_RefineryBPs()
        //{
        //    string DebugText = "Accepted BluePrints by Refinerysubtype\n";
        //    foreach (string refSubType in Refinery.refineryTypesAcceptedBlueprintsList.Keys)
        //    {
        //        DebugText += "SubType:" + refSubType + "\n";
        //        foreach (var refBP in Refinery.refineryTypesAcceptedBlueprintsList[refSubType])
        //        {
        //            DebugText += "bprint -> " + refBP.Name + "\n";
        //        }
        //    }
        //    return DebugText;
        //}

        //string Debug_AssemblerBluePrintsDictionary()
        //{
        //    string DebugText = "Accepted BluePrints by Assemblersubtype\n";
        //    foreach (var bluePrint in Assembler.AssemblerTypesAcceptedBluePrints)
        //    {
        //        DebugText += bluePrint.Key + "\n";
        //        foreach (var item in bluePrint.Value)
        //        {
        //            DebugText += item.AutoCraftingType + " -> " + item.AutoCraftingName + " / " + item.definition_id + "\n";
        //        }
        //    }
        //    return DebugText;
        //}

        //string Debug_AssemblerOwnBluePrintList()
        //{
        //    string debugText = "OwnBluePrintList\n\n";
        //    foreach (var item in Lists.Data.AssemblerList)
        //    {
        //        debugText += $"{item.AssemblerBlock.Name}\n---------------------------------------------\n";
        //        foreach (var bp in item.OwnBlueprintList)
        //        {
        //            debugText += bp.definition_id + "\n";
        //        }
        //    }
        //    return debugText;
        //}

        //string Debug_AssemblerBPs()
        //{
        //    string DebugText = "Accepted BluePrints by Assemblersubtype\nPool:\n";
        //    foreach (var bluePrint in Lists.Data.BluePrints_Inactive)
        //    {
        //        DebugText += bluePrint.Value.AutoCraftingType + " -> " + bluePrint.Value.AutoCraftingName + " / " + bluePrint.Value.definition_id + "\n";
        //    }
        //    DebugText += "Active:\n";
        //    foreach (var bluePrint in Lists.Data.BluePrints_Active)
        //    {
        //        DebugText += bluePrint.Value.AutoCraftingType + " -> " + bluePrint.Value.AutoCraftingName + " / " + bluePrint.Value.definition_id + "\n";
        //    }
        //    return DebugText;
        //}

        //string Debug_AddIPrioLists()
        //{
        //    var DebugText = "";
        //    foreach (var IPrioListKey in ingotprio.Keys)
        //    {
        //        DebugText += "IPrioList:" + IPrioListKey + "\n";
        //        foreach (var ip in ingotprio[IPrioListKey])
        //        {
        //            DebugText += "\t- " + ip.refineryBP.Definition_id + " % " + ip.prio + "\n";
        //        }
        //    }
        //    return DebugText;
        //}

        //string Debug_ComponentPrio()
        //{
        //    var DebugText = "";
        //    foreach (var item in Lists.Data.BluePrints_Active.Values)
        //    {
        //        DebugText += " # " + item.AutoCraftingName + " -> " + item.ItemPriority + "\n";
        //    }
        //    return DebugText;
        //}

        //string Debug_RefineryRecipes()
        //{
        //    string DebugText = "Refineryrecipes\n";
        //    foreach (var refRecipe in RefineryBlueprints)
        //    {
        //        DebugText += refRecipe.Name + " : " + refRecipe.InputIDName + " -> " + refRecipe.OutputIDName + "\n";
        //    }
        //    return DebugText;
        //}

        string Debug_InventoryManagerList()
        {
            var DebugText = "InventoryManagerList:\n";
            foreach (var inventoryKey in Lists.Data.InventoryManagerList.Keys)
            {
                DebugText += " - Key: " + inventoryKey + " / " + Lists.Data.InventoryManagerList[inventoryKey].Count + " Inventorys\n";
            }
            return DebugText;
        }

        //string Debug_Guns()
        //{
        //    var DebugText = "Guns - >\n" + Lists.Data.storageinvs.Count + "\n";
        //    foreach (var gun in Lists.Data.guns)
        //    {
        //        DebugText += " * " + gun.gun.CustomName + " / " + gun.CurrentAmmo + "\n";
        //        foreach (var item in gun.ammomax)
        //        {
        //            DebugText += "   - " + item.Key + " / " + item.Value + "\n";
        //        }
        //    }
        //    return DebugText;
        //}

        //string Debug_AmmoDefs() 
        //{
        //    var DebugText = "AmmoDefs - >\n";
        //    foreach (var ad in Lists.Data.AmmoDefinitions)
        //    {
        //        DebugText += " * " + ad.Value.type + " / " + ad.Value.maxOfVolume + " / " + ad.Value.guns.Count + " / " + ad.Value.ratio + "\n";
        //    }
        //    return DebugText;
        //}

        //string Debug_ListBlockTypes<T>()
        //    where T : class
        //{
        //    var DebugText = $"Blocks{nameof(T)}\n";
        //    var list = new List<T>();
        //    GridTerminalSystem.GetBlocksOfType(list, block => block as T != null);
        //    foreach (var t in list)
        //    {
        //        var pBlock = t as IMyTerminalBlock;
        //        DebugText += " * " + pBlock.CustomName + " / " + pBlock.BlockDefinition.ToString() + "\n";
        //    }
        //    return DebugText;
        //}

        void LCD_DebugPrint()
        {
            var panel = GridTerminalSystem.GetBlockWithName("PIMXXXDEBUG") as IMyTextPanel;
            if (panel == null) return;
            if (panel.CubeGrid != Me.CubeGrid) return;
            var s = "";
            s += Debug_InventoryManagerList();
            panel.WriteText(s + "\n" + LCD_DebugString);
            LCD_DebugString = "";
        }
    }
}
