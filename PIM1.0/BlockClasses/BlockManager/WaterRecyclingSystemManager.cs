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
                CalculateRefineryAmount(Ingot.WaterFood);
            }

            public override void DoManage()
            {
                if (Config.Instance.always_recycle_greywater && ProcessingGreyWater())
                {
                    return;
                }

                AssemblerBluePrint waterBluePrint;

                if (TryGetActiveAssemblerBluePrint(Ingot.WaterFood, out waterBluePrint))
                {
                    if (!waterBluePrint.IfMax() && Refinery.Success > 90)
                    {
                        if(IsInventoryItemExists(Ingot.CleanWater))
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
                SendItemByType(Ore.Ice, 1000, Refinery.InputInventory);
            }

            private void ProcessingCleanWater()
            {
                SendItemByType(Ingot.CleanWater, 1000, Refinery.InputInventory, (!Refinery.InputInventoryItems.ContainsKey(Ingot.CleanWater) || Refinery.InputInventoryItems[Ingot.CleanWater] == 0 ? 0 : 1));
            }

            private bool ProcessingGreyWater()
            {
                if (IsInventoryItemExists(Ingot.GreyWater))
                {
                    if (Refinery.Success > 90 || IsInventoryItemEmpty(Ingot.GreyWater, Refinery.InputInventoryItems))
                    {
                        SendItemByType(Ingot.GreyWater, 1000, Refinery.InputInventory, 0);
                    }

                    Refinery.PushItemToFront(Refinery.InputInventory, Ingot.GreyWater);
                    return true;
                }

                return false;
            }
        }
    }
}
