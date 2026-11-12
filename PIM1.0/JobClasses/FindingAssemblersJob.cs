namespace IngameScript
{

    partial class Program
    {
        public class FindingAssemblersJob : CountingJob
        {
            protected override void ConfigureCountingBounds(out int startIndex, out int endIndex)
            {
                // the assembler queues are counted again (Assembler.Refresh → AddProductionAmount)
                Lists.Data.ClearAllAssemblerBluePrintAssemblyAmounts();
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
