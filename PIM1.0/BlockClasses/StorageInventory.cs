using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public abstract class StorageInventory
        {
            #region private members

            protected List<MyInventoryItem> _InvList { get; private set; } = new List<MyInventoryItem>();
            private Dictionary<string, float> _InsertList = new Dictionary<string, float>();

            #endregion

            #region protected members

            protected IMyInventory inv = null;
            protected Dictionary<string, float> items = new Dictionary<string, float>();

            #endregion

            #region public methods

            public void RefreshInvList()
            {
                _InvList.Clear();
                inv.GetItems(_InvList);
            }

            public void ReloadItems()
            {
                RefreshInvList();

                if (!ItemsAmountInvalid()) return;

                _InsertList.Clear();

                if (StackingInventoryItems(_InvList))
                {
                    RefreshInvList();
                }

                DisposeOfSurplusItems(_InvList, _InsertList);
                InsertMissingItems(_InsertList);
            }

            #endregion

            #region abstract methods

            abstract public bool ItemsAmountInvalid();

            #endregion

            #region private methods

            public bool IfItemsExists()
            {
                RefreshInvList();

                foreach (var item in items.Keys)
                {
                    var found = false;
                    foreach (var invItem in _InvList)
                    {
                        var pimID = ItemTypesConverter.ToPimId(invItem.Type);
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

            private bool StackingInventoryItems(List<MyInventoryItem> invList)
            {
                var stackingSuccess = false;
                for (int i = invList.Count - 1; i >= 0; i--)
                {
                    var iList = new List<MyInventoryItem>();
                    inv.GetItems(iList, v => v.Type == invList[i].Type);
                    if (iList.Count > 1)
                    {
                        inv.TransferItemTo(inv, iList[iList.Count - 1]);
                        stackingSuccess = true;
                    }
                }

                return stackingSuccess;
            }

            /// <param name="einleiten">amount still missing per item, filled by DisposeOfSurplusItems
            /// (no entry: the item is not in the inventory at all)</param>
            private void InsertMissingItems(Dictionary<string, float> einleiten)
            {
                foreach (var i in items.Keys)
                {
                    float missing;
                    if (!einleiten.TryGetValue(i, out missing)) missing = items[i];
                    if (missing <= 0) continue;

                    if (!SendItemByType(i, missing, inv))
                    {
                        // TODO: errormessage
                    }
                }
            }

            /// <summary>
            /// Removes what is not wanted and notes per item what is still missing. The amounts are added
            /// up per item, not per stack: an item can be in several stacks that cannot be merged (bottles).
            /// </summary>
            private void DisposeOfSurplusItems(List<MyInventoryItem> invList, Dictionary<string, float> einleiten)
            {
                for (int i = invList.Count - 1; i >= 0; i--)
                {
                    var iItem = invList[i];
                    var iType = ItemTypesConverter.ToPimId(iItem.Type);
                    if (!items.ContainsKey(iType)) clearItemByType(inv, iType, iItem);
                    else
                    {
                        float missing;
                        if (!einleiten.TryGetValue(iType, out missing)) missing = items[iType];
                        var surplus = SurplusOfStack((float)iItem.Amount, ref missing);
                        if (surplus > 0) clearItemByType(inv, iType, iItem, surplus);
                        einleiten[iType] = missing;
                    }
                }
            }

            #endregion
        }
    }
}
