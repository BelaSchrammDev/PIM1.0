using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public abstract class ProductionBlockWithInventorys : ManageableBlock
        {
            private Dictionary<string, float> _InputInventoryItems = new Dictionary<string, float>(), _OutputInventoryItems = new Dictionary<string, float>();
            private const int InputInventoryIndex = 0, OutputInventoryIndex = 1;
            private IMyInventory _InputInventory, _OutputInventory;

            public void PushItemToFront(IMyInventory inventory, string itemType)
            {
                var _InvList = new List<MyInventoryItem>();
                inventory.GetItems(_InvList);

                for (int i = 0; i < _InvList.Count; i++)
                {
                    var invItem = _InvList[i];
                    var invItemType = ItemTypesConverter.ToPimId(invItem.Type);
                    if (invItemType == itemType)
                    {
                        if (i == 0) return;
                        inventory.TransferItemTo(inventory, i, 0, true, invItem.Amount);
                        break;
                    }
                }
            }

            public void ClearInventoryLists()
            {
                ClearInventoryList(_InputInventoryItems);
                ClearInventoryList(_OutputInventoryItems);
            }

            public void RefreshInventoryLists()
            {
                ClearInventoryLists();
                RefreshInventoryItems(InputInventoryIndex, InputInventoryItems);
                RefreshInventoryItems(OutputInventoryIndex, OutputInventoryItems);
            }

            public IMyInventory InputInventory
            {
                get 
                {
                    if (_InputInventory == null)
                    {
                        _InputInventory = FunctionalBlock.GetInventory(InputInventoryIndex);
                    }
                    return _InputInventory;
                }
            }

            public IMyInventory OutputInventory
            {
                get
                {
                    if (_OutputInventory == null)
                    {
                        _OutputInventory = FunctionalBlock.GetInventory(OutputInventoryIndex);
                    }
                    return _OutputInventory;
                }
            }

            public Dictionary<string, float> InputInventoryItems => _InputInventoryItems;

            public Dictionary<string, float> OutputInventoryItems => _OutputInventoryItems;

            #region private methode

            private void ClearInventoryList(Dictionary<string, float> list)
            {
                var keys = new List<string>(list.Keys);

                foreach (var key in keys)
                {
                    list[key] = 0;
                }
            }

            private void RefreshInventoryItems(int type, Dictionary<string, float> ilist)
            {
                var boxl = new List<MyInventoryItem>();
                FunctionalBlock.GetInventory(type).GetItems(boxl);

                foreach (var boxi in boxl)
                {
                    string index = ItemTypesConverter.ToPimId(boxi.Type);
                    var boxia = (float)boxi.Amount;
                    if (ilist.ContainsKey(index)) ilist[index] += boxia;
                    else ilist.Add(index, boxia);
                }
            }

            #endregion
        }
    }
}
