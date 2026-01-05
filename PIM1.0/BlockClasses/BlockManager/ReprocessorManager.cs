using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public class ReprocessorManager : RefineryBlockManagerer
        {
            private class ReprocessorInputInventory : StorageInventory
            {
                /*
                ReprocessorIngots
                SubTypeID: SpentFuelReprocessing	File: \Blueprints_POW.sbc
                    IN------>
                        Ingot SpentFuel:1
                        Ore Ice:0.75
                        Ingot Sulfur:0.2
                        Ingot Niter:0.3
                    OUT------->
                        Ingot Uranium:0.25
                        Ingot DepletedUranium:0.5
                        Ingot NuclearWaste:0.25
                */

                public ReprocessorInputInventory(IMyInventory inputInventory)
                {
                    this.inv = inputInventory;
                    var factor = ((inputInventory.MaxVolume.RawValue / 1000) / 56.9f) * 50.5f;
                    FillHydroponicsItemList(factor);
                }

                public void FillHydroponicsItemList(float factor = 1)
                {
                    items.Clear();
                    items.Add(Ingot.SpentFuel, 1f * factor);
                    items.Add(Ore.Ice, 0.75f * factor);
                    items.Add(Ingot.Sulfur, 0.2f * factor);
                    items.Add(Ingot.Niter, 0.3f * factor);
                }

                public override bool ItemsAmountInvalid()
                {
                    return true;
                }
            }

            private ReprocessorInputInventory _InputInventory;

            public ReprocessorManager(ManageableBlock block) : base(block)
            {
                _InputInventory = new ReprocessorInputInventory(Refinery.InputInventory);
            }

            public override void DoManage()
            {
                if (Lists.Data.BluePrints_Active.ContainsKey(Ingot.SpentFuelReprocessing) && !Lists.Data.BluePrints_Active[Ingot.SpentFuelReprocessing].IfMax())
                {
                    if (!_InputInventory.IfItemsExists() && Refinery.Success > 90)
                    {
                        _InputInventory.ReloadItems();
                    }
                }
                else
                {
                    Refinery.ClearInputInventoryIfControledByPIM();
                }

            }
        }
    }
}
