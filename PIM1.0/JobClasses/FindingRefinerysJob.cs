namespace IngameScript
{

    partial class Program
    {
        public class FindingRefinerysJob : CountingJob
        {
            public FindingRefinerysJob(Program program) : base(program)
            {
            }

            protected override void ConfigureCountingBounds(out int startIndex, out int endIndex)
            {
                Refinery.cn = 0;
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
