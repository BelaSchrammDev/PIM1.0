using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRage;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public class GridScanningJob : ProcessingBlockListBase
        {
            private bool IsProcessingValid(IMyTerminalBlock t) 
            {
                return t is IMyCargoContainer
                    || t is IMyShipConnector
                    || t is IMyShipDrill
                    || t is IMyShipGrinder
                    || t is IMyShipController
                    || IsValidSpezialWelder(t);
            }

            private bool IsValidSpezialWelder(IMyTerminalBlock t) 
            {
                return t is IMyShipWelder && !(t as IMyFunctionalBlock).Enabled;
            }

            protected override void ProcessingTerminalBlock(IMyTerminalBlock t)
            {
                var inventoryOwner = t as IMyInventoryOwner;

                if(inventoryOwner == null) return;

                Parameter pm = new Parameter();
                bool isStorage = t.CustomName.Contains(Strings.SmsStorageTag)
                    , isProcessingValid = IsProcessingValid(t)
                    , isSmsBlock = false
                    , isNoKeep = false
                    , isContainerOrConnector = t.BlockDefinition.SubtypeId.Contains("Container") || t.BlockDefinition.SubtypeId.Contains("Connector");

                if (pm.ParseArgs(t.CustomName))
                {
                    isSmsBlock = true;
                    isNoKeep = !pm.IsParameter("Keep");
                }

                for (int i = 0; i < inventoryOwner.InventoryCount; i++)
                {
                    var inv = inventoryOwner.GetInventory(i);
                    CountItemsToSummaryDictionary(inv);

                    if (isStorage || !isProcessingValid) continue;

                    if (isSmsBlock)
                    {
                        if (isNoKeep) Lists.Data.SmsFlagedInventoryList.Add(inv);
                        ProgramInstance.AddInventoryToInventoryManagerList(inv, pm.ParameterList);
                    }
                    else if (isContainerOrConnector)
                    {
                        Lists.Data.NoneSmsFlagedInventoryList.Add(inv);
                    }
                }
            }
        }
    }
}
