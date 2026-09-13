using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public abstract class StackingJobBase : CountingJob
        {
            protected List<StackItem> StackItemList = new List<StackItem>();

            public enum StackingMode
            {
                Single,
                Alpha,
                Beta,
                Delta,
                Gamma
            }

            protected StackingJobBase(StackingMode mode) : base()
            {
            }

            protected override void ConfigureCountingBounds(out int startIndex, out int endIndex)
            {
                StackItemList.Clear();
                StackItem.ClearStackInventory();
                if (Lists.Data.InventoryManagerList.ContainsKey(StackingJob.StackType))
                {
                    foreach (var i in Lists.Data.InventoryManagerList[StackingJob.StackType])
                    {
                        NewStackCount(i, StackingJob.StackType);
                        StackItem.CalculateFreeInventory(i);
                    }
                }

                startIndex = 0;
                endIndex = InitStacking() ? StackItemList.Count - 1 : -1;
            }

            public abstract bool InitStacking();

            void NewStackCount(IMyInventory quelle, string ti)
            {
                var von = new List<MyInventoryItem>();
                quelle.GetItems(von);
                foreach (var i in von)
                {
                    if (i.Type.TypeId.Contains(ti) && !Lists.Data.InventoryManagerList.ContainsKey(GetPIMItemID(i.Type)))
                    {
                        GetStackItem(i.Type).AddStack(quelle, i.Amount);
                    }
                }
            }

            StackItem GetStackItem(MyItemType t) { foreach (var s in StackItemList) if (s.type == t) return s; var nt = new StackItem(t); StackItemList.Add(nt); return nt; }

        }
    }
}
