namespace IngameScript
{
    partial class Program
    {
        public class StackingGatherInEmptiestJob : StackingStrategyJob
        {
            public override bool InitStacking()
            {
                ItemStacks.CurrentStackingType = ItemStacks.StackingType.VolumeBack;
                ItemStacksList.Sort();
                return true;
            }

            protected override void ProcessingIndex(int index)
            {
                ItemStacksList[index].GatherInEmptiest();
            }
        }
    }
}
