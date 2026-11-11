namespace IngameScript
{
    partial class Program
    {
        public class StackingSwapJob : StackingStrategyJob
        {
            public StackingSwapJob() : base(StackingMode.Gamma)
            {
            }

            public override bool InitStacking()
            {
                ItemStacks.CurrentStackingType = ItemStacks.StackingType.Stack;
                ItemStacksList.Sort();
                while (ItemStacksList.Count > 0)
                {
                    var s = ItemStacksList[0];
                    if (s.PrepareSwap()) break;
                    ItemStacksList.Remove(s);
                }
                return ItemStacksList.Count > 0;
            }

            protected override void ProcessingIndex(int index)
            {
                ItemStacksList[0].Swap();
            }
        }
    }
}
