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
            private List<string> _SuccessList = new List<string>();
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

                var invstring = "Inventory before ReloadItems:\n";
                foreach (var ii in _InvList)
                {
                    invstring += $" - {GetPIMItemID(ii.Type)} : {ii.Amount}\n";
                }

                Program.LCD_DebugString += $"StorageInventory_ReloadItems for '{invstring}'\n";

                _SuccessList.Clear();
                _InsertList.Clear();

                if (StackingInventoryItems(_InvList))
                {
                    RefreshInvList();
                }

                DisposeOfSurplusItems(_InvList, _SuccessList, _InsertList);
                InsertMissingItems(_SuccessList, _InsertList);
            }

            #endregion

            #region abstract methods

            abstract public bool ItemsAmountInvalid();

            #endregion

            #region private methods

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

            private void InsertMissingItems(List<string> succlist, Dictionary<string, float> einleiten)
            {
                foreach (var i in items.Keys)
                {
                    if (succlist.Contains(i)) continue;

                    if (!SendItemByType(i, (einleiten.ContainsKey(i) ? einleiten[i] : items[i]), inv))
                    {
                        // TODO: errormessage
                    }
                }
            }

            private void DisposeOfSurplusItems(List<MyInventoryItem> invList, List<string> succlist, Dictionary<string, float> einleiten)
            {
                for (int i = invList.Count - 1; i >= 0; i--)
                {
                    var iItem = invList[i];
                    var iType = GetPIMItemID(iItem.Type);
                    if (!items.ContainsKey(iType)) clearItemByType(inv, iType, iItem);
                    else
                    {
                        var adiff = items[iType] - (float)iItem.Amount;
                        if (adiff < 0)
                        {
                            clearItemByType(inv, iType, iItem, Math.Abs(adiff));
                            succlist.Add(iType);
                        }
                        else if (adiff == 0) succlist.Add(iType);
                        else einleiten.Add(iType, adiff);
                    }
                }
            }

            #endregion
        }
    }
}
