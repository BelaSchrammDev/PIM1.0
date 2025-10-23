namespace IngameScript
{
    partial class Program
    {
        public class StackingAlphaJob : StackingJobBase
        {
            public StackingAlphaJob(Program program) : base(program, "Stacking Alpha", StackingMode.Alpha)
            {
            }

            public override bool InitStacking()
            {
                int max_stack = 0;
                foreach (var s in StackItemList) if (max_stack < s.Stackcount) max_stack = s.Stackcount;
                if (max_stack > 1)
                {
                    StackItem.CurrentStackingType = StackItem.StackingType.Stack;
                    StackItemList.Sort();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            protected override void ProcessingIndex(int index)
            {
                StackItemList[index].stacking_alpha();
            }
        }
    }
}
