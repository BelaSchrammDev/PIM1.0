namespace IngameScript
{

    partial class Program
    {
        public class FindingAssemblersJob : CountingJob
        {
            public FindingAssemblersJob(Program program) : base(program)
            {
            }

            protected override void ConfigureCountingBounds(out int startIndex, out int endIndex)
            {
                startIndex = 0;
                endIndex = Lists.Data.AssemblerList.Count - 1;
            }

            protected override void ProcessingIndex(int index)
            {
                Lists.Data.AssemblerList[index].Refresh();
            }
        }
    }
}
