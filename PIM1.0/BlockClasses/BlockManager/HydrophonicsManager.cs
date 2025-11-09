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
        // TODO: gravel not pushed when spezial gravel container is exists
        public class HydrophonicsInputInventory : StorageInventory
        {
            public HydrophonicsInputInventory(IMyInventory hydrophonicsInputInventory)
            {
                this.inv = hydrophonicsInputInventory;
                FillHydroponicsItemList(1);
            }

            /*     Algae Recipe:
                   Volumen
                    1.0     <Item Amount="0.0075" TypeId="Ingot" SubtypeId="Nutrients" />
                    1.0     <Item Amount="0.005" TypeId="Ingot" SubtypeId="WaterFood" />
                    0.37    <Item Amount="0.12" TypeId="Ingot" SubtypeId="Stone" />
            */
            public void FillHydroponicsItemList(float factor = 1)
            {
                items.Clear();
                items.Add(Ingot.Stone, 16.428f * factor);
                items.Add(Ingot.WaterFood, 5f * factor);
                items.Add(Ingot.Nutrients, 15f * factor);
            }

            public bool IfIngredientsExists()
            {
                RefreshInvList();

                foreach (var item in items.Keys)
                {
                    var found = false;
                    foreach (var invItem in _InvList)
                    {
                        var pimID = GetPIMItemID(invItem.Type);
                        if (pimID == item && invItem.Amount > 0)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found) return false;
                }
                return true;
            }

            public override bool ItemsAmountInvalid()
            {
                return true;
            }
        }

        // TODO: add errormessages when ingreadients not exists
        public class HydrophonicsManager : BlockManager
        {
            private Refinery Refinery { get { return (Refinery)Block; } }
            private HydrophonicsInputInventory _InputInventory;

            public HydrophonicsManager(ManageableBlock block) : base(block)
            {
                _InputInventory = new HydrophonicsInputInventory(Refinery.InputInventory);
            }

            public override void DoManage()
            {
                if (Lists.Data.BluePrints_Active.ContainsKey(Ingot.SubFresh) && !Lists.Data.BluePrints_Active[Ingot.SubFresh].IfMax())
                {
                    if (!_InputInventory.IfIngredientsExists() && Refinery.fertig > 90)
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
