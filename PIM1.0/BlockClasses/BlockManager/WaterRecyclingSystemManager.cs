using Sandbox.Game;
using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public class WaterRecyclingSystemManager : RefineryBlockManagerer
        {
            public WaterRecyclingSystemManager(ManageableBlock block) : base(block)
            {
            }

            public override void DoManage()
            {
                LCD_DebugString += "\nWRSManager: Starting management.\n";

                if (Config.Instance.always_recycle_greywater && ProccessingGreyWater())
                {
                    LCD_DebugString += "\nWRSManager: Recycling GreyWater.\n";
                    return;
                }

                AssemblerBluePrint waterBluePrint;

                if (TryGetActiveAssemblerBluePrint(Ingot.WaterFood, out waterBluePrint))
                {
                    if (!waterBluePrint.IfMax() && Refinery.Success > 90)
                    {
                        if(IsInventoryItemExists(Ingot.CleanWater))
                        {
                            LCD_DebugString += $"WRSManager: Processing cleanwater to WaterFood {Lists.Data.BluePrints_Active[Ingot.WaterFood].MaximumItemAmount} / {Lists.Data.BluePrints_Active[Ingot.WaterFood].CurrentItemAmount}\n";
                            ProccessingCleanWater();
                        }
                        else
                        {
                            LCD_DebugString += $"WRSManager: Processing ice to WaterFood {Lists.Data.BluePrints_Active[Ingot.WaterFood].MaximumItemAmount} / {Lists.Data.BluePrints_Active[Ingot.WaterFood].CurrentItemAmount}\n";
                            ProccessingIce();
                        }
                    }
                    else if (Refinery.Success < 10 || waterBluePrint.IfMax())
                    {
                        ClearInventory(Refinery.InputInventory);
                    }
                }
            }

            private void ProccessingIce()
            {
                SendItemByType(Ore.Ice, 1000, Refinery.InputInventory);
            }

            private void ProccessingCleanWater()
            {
                SendItemByType(Ingot.CleanWater, 1000, Refinery.InputInventory, (!Refinery.InputInventoryItems.ContainsKey(Ingot.CleanWater) || Refinery.InputInventoryItems[Ingot.CleanWater] == 0 ? 0 : 1));
            }

            private bool ProccessingGreyWater()
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
