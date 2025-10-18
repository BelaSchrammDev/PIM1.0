namespace IngameScript
{
    partial class Program
    {
        public class RefreshingControllingGunsJob : CountingJob
        {
            public RefreshingControllingGunsJob(Program program) : base(program, "RefreshingControllingGunsJob")
            {
            }

            protected override void ConfigureCountingBounds(out int startIndex, out int endIndex)
            {
                startIndex = 0;
                endIndex = Lists.Data.guns.Count - 1;
            }

            protected override void ProcessingIndex(int index)
            {
                Lists.Data.guns[index].Refresh();
            }
        }
    }
}
