using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public class WaterRecyclingSystemManager : RefineryBlockManager
        {
            public WaterRecyclingSystemManager(ManageableBlock block) : base(block)
            {
            }

            public override void Refresh()
            {
                CalculateRefineryAmount(IngotWaterFood);
            }

            public override void DoManage()
            {
                if (Config.Instance.always_recycle_greywater && ProcessingGreyWater())
                {
                    return;
                }

                AssemblerBluePrint waterBluePrint;

                if (TryGetActiveAssemblerBluePrint(IngotWaterFood, out waterBluePrint))
                {
                    if (!waterBluePrint.IfMax() && Refinery.Success > 90)
                    {
                        if(IsInventoryItemExists(IngotCleanWater))
                        {
                            ProcessingCleanWater();
                        }
                        else
                        {
                            ProcessingIce();
                        }
                    }
                    else if (Refinery.Success < 10 || waterBluePrint.IfMax())
                    {
                        ClearInventory(Refinery.InputInventory);
                    }
                }
            }

            private void ProcessingIce()
            {
                SendItemByType(OreIce, 1000, Refinery.InputInventory);
            }

            private void ProcessingCleanWater()
            {
                SendItemByType(IngotCleanWater, 1000, Refinery.InputInventory, (!Refinery.InputInventoryItems.ContainsKey(IngotCleanWater) || Refinery.InputInventoryItems[IngotCleanWater] == 0 ? 0 : 1));
            }

            private bool ProcessingGreyWater()
            {
                if (IsInventoryItemExists(IngotGreyWater))
                {
                    if (Refinery.Success > 90 || IsInventoryItemEmpty(IngotGreyWater, Refinery.InputInventoryItems))
                    {
                        SendItemByType(IngotGreyWater, 1000, Refinery.InputInventory, 0);
                    }

                    Refinery.PushItemToFront(Refinery.InputInventory, IngotGreyWater);
                    return true;
                }

                return false;
            }
        }
    }
}
