namespace IngameScript
{
    partial class Program
    {
        public class StackingGatherJob : StackingStrategyJob
        {
            public override bool InitStacking()
            {
                int max_stack = 0;
                foreach (var s in ItemStacksList) if (max_stack < s.Stackcount) max_stack = s.Stackcount;
                if (max_stack > 1)
                {
                    ItemStacks.CurrentStackingType = ItemStacks.StackingType.Stack;
                    ItemStacksList.Sort();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            protected override void ProcessingIndex(int index)
            {
                ItemStacksList[index].Gather();
            }
        }
    }
}
