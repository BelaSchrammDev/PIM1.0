namespace IngameScript
{
    partial class Program
    {
        public class StackingGatherJob : StackingStrategyJob
        {
            public override bool InitStacking()
            {
                return SortByStackCount();
            }

            protected override void ProcessingIndex(int index)
            {
                ItemStacksList[index].Gather();
            }
        }
    }
}
