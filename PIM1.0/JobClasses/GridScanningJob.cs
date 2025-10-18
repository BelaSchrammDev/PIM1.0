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
            public GridScanningJob(Program program) : base(program, "GridScanningJob")
            {
            }

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
                return !(t as IMyFunctionalBlock).Enabled;
            }

            protected override void ProcessingTerminalBlock(IMyTerminalBlock t)
            {
                var inventoryOwner = t as IMyInventoryOwner;

                if(inventoryOwner == null) return;

                Parameter pm = new Parameter();
                bool isStorage = t.CustomName.Contains(X_StorageTag)
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
                    CountItemsToDictionary(inv);

                    if (isStorage || !isProcessingValid) continue;

                    if (isSmsBlock)
                    {
                        if (isNoKeep) NonSmsFlagedInventoryList.Add(inv);
                        Program.AddInventoryToInventoryManagerList(inv, pm.ParameterList);
                    }
                    else if (isContainerOrConnector) NonSmsFlagedInventoryList.Add(inv);
                    else SmsFlagedInventoryList.Add(inv);
                }
            }
        }
    }
}
