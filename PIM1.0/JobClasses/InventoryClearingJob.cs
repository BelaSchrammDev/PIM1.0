using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{

    partial class Program
    {
        public class InventoryClearingJob : CountingJob
        {
            private List<IMyInventory> _invList;
            private List<string> _collectList;
            public InventoryClearingJob(Program program, string name, List<IMyInventory> invList, List<string> collect = null) : base(program, name)
            {
                _invList = invList;
                _collectList = collect;
            }

            protected override void ConfigureCountingBounds(out int startIndex, out int endIndex)
            {
                startIndex = 0;
                endIndex = _invList.Count - 1;
            }

            protected override void ProcessingIndex(int index)
            {
                ClearInventory(_invList[index], _collectList);
            }
        }
    }
}
