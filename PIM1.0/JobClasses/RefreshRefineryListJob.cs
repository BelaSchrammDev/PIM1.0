using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public class RefreshRefineryListJob : CountingJob
        {
            public RefreshRefineryListJob(Program program) : base(program)
            {
            }

            protected override void ConfigureCountingBounds(out int startIndex, out int endIndex)
            {
                Lists.Data.ClearAllRefineryBlueprintAssemblyAmounts();
                GetBlockList(Lists.Data.Refinerys);

                for (int i = Lists.Data.RefineryList.Count - 1; i >= 0; i--)
                {
                    if (Lists.Data.Refinerys.Contains(Lists.Data.RefineryList[i].RefineryBlock))
                    {
                        Lists.Data.Refinerys.Remove(Lists.Data.RefineryList[i].RefineryBlock);
                    }
                    else
                    {
                        Propertys.Data.AutoCraftingSettingsInValid = true;
                        Lists.Data.RefineryList.Remove(Lists.Data.RefineryList[i]);
                    }
                }

                Refinery.priobt = "";
                startIndex = Lists.Data.Refinerys.Count - 1;
                endIndex = 0;
            }

            protected override void ProcessingIndex(int index)
            {
                Lists.Data.RefineryList.Add(new Refinery(Lists.Data.Refinerys[index]));
            }
        }
    }
}
