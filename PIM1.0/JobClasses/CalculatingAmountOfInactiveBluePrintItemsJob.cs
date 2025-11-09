using System.Collections.Generic;

namespace IngameScript
{

    partial class Program
    {
        public class CalculatingAmountOfInactiveBluePrintItemsJob : CountingJob
        {
            private List<string> _blueprintKeys = new List<string>();

            protected override void ConfigureCountingBounds(out int startIndex, out int endIndex)
            {
                _blueprintKeys.Clear();
                _blueprintKeys.AddRange(Lists.Data.BluePrints_Inactive.Keys);
                startIndex = 0;
                endIndex = _blueprintKeys.Count - 1;
            }

            protected override void ProcessingIndex(int index)
            {
                var key = _blueprintKeys[index];
                var b = Lists.Data.BluePrints_Inactive[key];

                if (Lists.Data.inventar.ContainsKey(b.ItemName))
                {
                    b.SetCurrentAmount((int)Lists.Data.inventar[b.ItemName]);
                }
            }
        }
    }
}
