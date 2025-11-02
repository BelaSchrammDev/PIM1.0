using System.Collections.Generic;

namespace IngameScript
{

    partial class Program
    {
        public class AssemblerBluePrintManagerJob : CountingJob
        {
            private List<string> BluePrintKeyList;

            public AssemblerBluePrintManagerJob(Program program) : base(program)
            {
                BluePrintKeyList = new List<string>();
            }

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
                b.SetCurrentAmount((int)inventar.GetValueOrDefault(b.ItemName, 0));
                b.CalcPriority();
                if (b.NeedsAssembling())
                {
                    if (key != Ingot.SubFresh)
                    {
                        foreach (var o in Lists.Data.AssemblerList) o.AddValidBlueprint(b);
                    }
                }
            }
        }
    }
}
