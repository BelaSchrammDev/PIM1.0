using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{

    partial class Program
    {
        public class InventoryClearingJob : CountingJob
        {
            private List<IMyInventory> _invList;
            public InventoryClearingJob(List<IMyInventory> invList)
            {
                _invList = invList;
            }

            protected override void ConfigureCountingBounds(out int startIndex, out int endIndex)
            {
                startIndex = 0;
                endIndex = _invList.Count - 1;
            }

            protected override void ProcessingIndex(int index)
            {
                ClearInventory(_invList[index]);
            }
        }
    }
}
