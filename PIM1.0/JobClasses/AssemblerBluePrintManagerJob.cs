using System.Collections.Generic;

namespace IngameScript
{

    partial class Program
    {
        public class AssemblerBluePrintManagerJob : CountingJob
        {
            private List<string> BluePrintKeyList = new List<string>();

            protected override void ConfigureCountingBounds(out int startIndex, out int endIndex)
            {
                BluePrintKeyList.Clear();
                BluePrintKeyList.AddRange(Lists.Data.BluePrints_Active.Keys);
                startIndex = 0;
                endIndex = BluePrintKeyList.Count - 1;
            }

            protected override void ProcessingIndex(int index)
            {
                var key = BluePrintKeyList[index];
                var b = Lists.Data.BluePrints_Active[key];
                b.SetCurrentAmount((int)Lists.Data.inventar.GetValueOrDefault(b.ItemName, 0));
                b.CalcPriority();
            }
        }
    }
}
