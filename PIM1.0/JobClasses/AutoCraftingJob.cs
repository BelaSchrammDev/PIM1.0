using System.Collections.Generic;
using System.Linq;
using VRage;

namespace IngameScript
{


    partial class Program
    {
        public class AutoCraftingJob : CountingJob
        {
            private AssemblerBluePrint[] _bluePrints;

            protected override void ConfigureCountingBounds(out int startIndex, out int endIndex)
            {
                foreach (var assembler in Lists.Data.AssemblerList)
                {
                    assembler.OwnBlueprintList = Assembler.AssemblerTypesAcceptedBluePrints.ContainsKey(assembler.SubTypeName)
                        ? Assembler.AssemblerTypesAcceptedBluePrints[assembler.SubTypeName]
                        : new List<AssemblerBluePrint>();
                }
                _bluePrints = Lists.Data.BluePrints_Active.Values.ToArray();
                startIndex = 0;
                endIndex = Lists.Data.BluePrints_Active.Count - 1;
            }

            protected override void ProcessingIndex(int index)
            {
                var bluePrint = _bluePrints[index];
                if (bluePrint.NeedsAssembling())
                {
                    var bluePrintNeededAmount = (bluePrint.MaximumItemAmount - bluePrint.CurrentItemAmount - bluePrint.AssemblyAmount);

                    if(bluePrint.IfRefineryBluePrint())
                    {
                        return;
                    }
                    else
                    {
                        var validAssemblers = Lists.Data.AssemblerList.FindAll(x => x.OwnBlueprintList.Contains(bluePrint));
                        if (validAssemblers.Count > 0)
                        {
                            MyFixedPoint amount = bluePrintNeededAmount / validAssemblers.Count;
                            foreach (var assembler in validAssemblers)
                            {
                                assembler.AddQueueItemSave(bluePrint, amount);
                            }
                        }
                    }
                }
            }
        }
    }
}
