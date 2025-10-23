namespace IngameScript
{
    partial class Program
    {
        public class StackingGammaJob : StackingJobBase
        {
            public StackingGammaJob(Program program) : base(program, "Stacking Gamma", StackingMode.Gamma)
            {
            }
            public override bool InitStacking()
            {
                StackItem.CurrentStackingType = StackItem.StackingType.Stack;
                StackItemList.Sort();
                while (StackItemList.Count > 0)
                {
                    var s = StackItemList[0];
                    if (s.check_stacking_gamma()) break;
                    StackItemList.Remove(s);
                }
                return StackItemList.Count > 0;
            }
            protected override void ProcessingIndex(int index)
            {
                StackItemList[0].stacking_gamma();
            }
        }
    }
}
