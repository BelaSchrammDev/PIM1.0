using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public abstract class ManageableBlock : Tools
        {
            public BlockManager Manager { get; internal set; }

            public IMyFunctionalBlock FunctionalBlock => GetFunctionalBlock();

            public bool IsFunctional => FunctionalBlock != null && FunctionalBlock.IsFunctional;

            public bool IsWorking => FunctionalBlock != null && FunctionalBlock.IsWorking;

            public bool IsNotClosed => FunctionalBlock != null && !FunctionalBlock.Closed;

            public virtual bool RunManager()
            {
                if (Manager != null && IsNotClosed)
                {
                    Manager.DoManage();
                    return true;
                }

                return false;
            }

            public abstract IMyFunctionalBlock GetFunctionalBlock();

            public void PushItemToFront(IMyInventory inventory, string itemType)
            {
                var _InvList = new List<MyInventoryItem>();
                inventory.GetItems(_InvList);

                for (int i = 0; i < _InvList.Count; i++)
                {
                    var invItem = _InvList[i];
                    var invItemType = GetPIMItemID(invItem.Type);
                    if (invItemType == itemType)
                    {
                        if (i == 0) return;
                        inventory.TransferItemTo(inventory, i, 0, true, invItem.Amount);
                        break;
                    }
                }
            }
        }

        public abstract class ManageableRefineryBlock : ManageableBlock
        {
            
        }

        public abstract class ManageableAssemblerBlock : ManageableBlock
        {

        }
    }
}
