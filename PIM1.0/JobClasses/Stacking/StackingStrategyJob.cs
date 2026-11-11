using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public abstract class StackingStrategyJob : CountingJob
        {
            protected List<ItemStacks> ItemStacksList = new List<ItemStacks>();

            public enum StackingMode
            {
                Single,
                Alpha,
                Beta,
                Delta,
                Gamma
            }

            protected StackingStrategyJob(StackingMode mode) : base()
            {
            }

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
                endIndex = InitStacking() ? ItemStacksList.Count - 1 : -1;
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
                        GetItemStacks(i.Type).AddStack(quelle, i.Amount);
                    }
                }
            }

            ItemStacks GetItemStacks(MyItemType t) { foreach (var s in ItemStacksList) if (s.type == t) return s; var nt = new ItemStacks(t); ItemStacksList.Add(nt); return nt; }

        }
    }
}
