using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        public class FindStorageContainersJob : CountingJob
        {
            private List<IMyCargoContainer> _CargoContainerBlocks = new List<IMyCargoContainer>();

            protected override void ConfigureCountingBounds(out int startIndex, out int endIndex)
            {
                MyProgram.GridTerminalSystem.GetBlocksOfType(_CargoContainerBlocks, cargo => IsValidStorageContainer(cargo));

                Tools.SyncBlockList(_CargoContainerBlocks, Lists.Data.StorageCargos, s => s.container, s => s.Remove());

                startIndex = _CargoContainerBlocks.Count - 1;
                endIndex = 0;
            }

            protected override void ProcessingIndex(int index)
            {
                Lists.Data.StorageCargos.Add(new StorageCargo(_CargoContainerBlocks[index]));
            }

            private bool IsValidStorageContainer(IMyCargoContainer cargo)
            {
                return cargo.IsMainConstruct() && cargo.CustomName.Contains(Strings.SmsStorageTag);
            }
        }
    }
}
