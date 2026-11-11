using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public abstract class StackingStrategyJob : CountingJob
        {
            protected List<ItemStacks> ItemStacksList = new List<ItemStacks>();

            protected override void ConfigureCountingBounds(out int startIndex, out int endIndex)
            {
                ItemStacksList.Clear();
                ItemStacks.ClearStackInventory();
                if (Lists.Data.InventoryManagerList.ContainsKey(StackingJob.StackType))
                {
                    foreach (var i in Lists.Data.InventoryManagerList[StackingJob.StackType])
                    {
                        NewStackCount(i, StackingJob.StackType);
                        ItemStacks.CalculateFreeInventory(i);
                    }
                }

                startIndex = 0;
                endIndex = InitStacking() ? StepCount - 1 : -1;
            }

            public abstract bool InitStacking();

            /// <summary>Number of ProcessingIndex calls of one run, by default one per item of the list.</summary>
            protected virtual int StepCount => ItemStacksList.Count;

            void NewStackCount(IMyInventory quelle, string ti)
            {
                var von = new List<MyInventoryItem>();
                quelle.GetItems(von);
                foreach (var i in von)
                {
                    if (i.Type.TypeId.Contains(ti) && !Lists.Data.InventoryManagerList.ContainsKey(GetPIMItemID(i.Type)))
                    {
                        GetItemStacks(i.Type).AddStack(quelle, i.Amount);
                    }
                }
            }

            ItemStacks GetItemStacks(MyItemType t) { foreach (var s in ItemStacksList) if (s.type == t) return s; var nt = new ItemStacks(t); ItemStacksList.Add(nt); return nt; }

        }
    }
}
