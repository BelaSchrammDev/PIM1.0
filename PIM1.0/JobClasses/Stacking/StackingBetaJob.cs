namespace IngameScript
{
    partial class Program
    {
        public class StackingBetaJob : StackingJobBase
        {
            public StackingBetaJob() : base(StackingMode.Beta)
            {
            }

            public override bool InitStacking()
            {
                StackItem.CurrentStackingType = StackItem.StackingType.VolumeBack;
                StackItemList.Sort();
                return true;
            }

            protected override void ProcessingIndex(int index)
            {
                StackItemList[index].stacking_beta();
            }
        }
    }
}
