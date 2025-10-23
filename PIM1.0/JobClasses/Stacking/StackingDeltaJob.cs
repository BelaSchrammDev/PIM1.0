namespace IngameScript
{
    partial class Program
    {
        public class StackingDeltaJob : StackingJobBase
        {
            public StackingDeltaJob(Program program) : base(program, "Stacking Delta", StackingMode.Delta)
            {
            }
            public override bool InitStacking()
            {
                StackItem.CurrentStackingType = StackItem.StackingType.Volume;
                StackItemList.Sort();
                return true;
            }
            protected override void ProcessingIndex(int index)
            {
                StackItemList[index].stacking_delta();
            }
        }
    }
}
