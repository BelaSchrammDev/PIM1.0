namespace IngameScript
{
    partial class Program
    {
        public class StackingMergePairsJob : StackingStrategyJob
        {
            public StackingMergePairsJob() : base(StackingMode.Delta)
            {
            }

            public override bool InitStacking()
            {
                ItemStacks.CurrentStackingType = ItemStacks.StackingType.Volume;
                ItemStacksList.Sort();
                return true;
            }

            protected override void ProcessingIndex(int index)
            {
                ItemStacksList[index].MergePairs();
            }
        }
    }
}
