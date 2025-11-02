namespace IngameScript
{

    partial class Program
    {
        public class RefineryManagerJob : CountingJob
        {
            public RefineryManagerJob(Program program) : base(program)
            {
            }

            protected override void ConfigureCountingBounds(out int startIndex, out int endIndex)
            {
                Tools.ProgramInstance.CalcIngotPrio();
                Tools.ProgramInstance.RenderResourceProccesingLCD();
                startIndex = 0;
                endIndex = Lists.Data.RefineryList.Count - 1;
            }

            protected override void ProcessingIndex(int index)
            {
                Lists.Data.RefineryList[index].RefineryManager();
            }
        }
    }
}
