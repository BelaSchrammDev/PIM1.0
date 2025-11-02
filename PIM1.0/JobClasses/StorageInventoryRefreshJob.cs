namespace IngameScript
{


    partial class Program
    {
        public class StorageInventoryRefreshJob : CountingJob
        {
            public StorageInventoryRefreshJob(Program program) : base(program)
            {
            }

            protected override void ConfigureCountingBounds(out int startIndex, out int endIndex)
            {
                foreach (var ammoDef in Lists.Data.AmmoDefinitions.Values) ammoDef.CalcAmmoInventoryRatio();
                startIndex = 0;
                endIndex = Lists.Data.storageinvs.Count - 1;
            }

            protected override void ProcessingIndex(int index)
            {
                Lists.Data.storageinvs[index].ReloadItems();
            }
        }
    }
}
