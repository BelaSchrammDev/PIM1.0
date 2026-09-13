namespace IngameScript
{
    partial class Program
    {
        public class RefreshControllingGunsJob : CountingJob
        {
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
