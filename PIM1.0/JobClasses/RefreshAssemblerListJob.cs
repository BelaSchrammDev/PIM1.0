namespace IngameScript
{

    partial class Program
    {
        public class RefreshAssemblerListJob : CountingJob
        {
            public RefreshAssemblerListJob(Program program) : base(program)
            {
            }

            protected override void ConfigureCountingBounds(out int startIndex, out int endIndex)
            {
                Tools.GetBlockList(Lists.Data.Assemblers);

                for (int i = Lists.Data.AssemblerList.Count - 1; i >= 0; i--)
                {
                    if (Lists.Data.Assemblers.Contains(Lists.Data.AssemblerList[i].AssemblerBlock))
                    {
                        Lists.Data.Assemblers.Remove(Lists.Data.AssemblerList[i].AssemblerBlock);
                    }
                    else
                    {
                        Propertys.Data.AutoCraftingSettingsInValid = true;
                        Lists.Data.AssemblerList.Remove(Lists.Data.AssemblerList[i]);
                    }
                }

                startIndex = Lists.Data.Assemblers.Count - 1;
                endIndex = 0;
            }

            protected override void ProcessingIndex(int index)
            {
                Lists.Data.AssemblerList.Add(new Assembler(Lists.Data.Assemblers[index]));
            }
        }
    }
}
