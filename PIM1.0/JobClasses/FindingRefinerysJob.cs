namespace IngameScript
{

    partial class Program
    {
        public class FindingRefinerysJob : CountingJob
        {
            protected override void ConfigureCountingBounds(out int startIndex, out int endIndex)
            {
                foreach (var b in RefineryBlueprints) b.RefineryCount = 0;
                startIndex = 0;
                endIndex = Lists.Data.RefineryList.Count - 1;
            }

            protected override void ProcessingIndex(int index)
            {
                Lists.Data.RefineryList[index].Refresh();
            }
        }
    }
}
