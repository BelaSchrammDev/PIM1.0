using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        public class FindStorageContainersJob : CountingJob
        {
            private List<IMyCargoContainer> _CargoContainerBlocks = new List<IMyCargoContainer>();
            public FindStorageContainersJob(Program program) : base(program, "FindStorageContainersJob")
            {
            }

            protected override void ConfigureCountingBounds(out int startIndex, out int endIndex)
            {
                GridTerminalSystem.GetBlocksOfType(_CargoContainerBlocks, cargo => IsValidStorageContainer(cargo));

                for (int i = Lists.Data.StorageCargos.Count - 1; i >= 0; i--)
                {
                    if (_CargoContainerBlocks.Contains(Lists.Data.StorageCargos[i].container))
                    {
                        _CargoContainerBlocks.Remove(Lists.Data.StorageCargos[i].container);
                    }
                    else
                    {
                        var stor = Lists.Data.StorageCargos[i];
                        stor.Remove();
                        Lists.Data.StorageCargos.Remove(stor);
                    }
                }

                startIndex = _CargoContainerBlocks.Count - 1;
                endIndex = 0;
            }

            protected override void ProcessingIndex(int index)
            {
                Lists.Data.StorageCargos.Add(new StorageCargo(_CargoContainerBlocks[index]));
            }

            private bool IsValidStorageContainer(IMyCargoContainer cargo)
            {
                return BlockConstructMember(cargo) && cargo.CustomName.Contains(Strings.SmsStorageTag);
            }
        }
    }
}
