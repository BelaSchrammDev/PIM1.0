namespace IngameScript
{
    partial class Program
    {
        public class ClearJob : Job
        {
            public ClearJob(Program program)
                : base(program, "ClearJob")
            {
            }

            public override RunJobResult RunJob()
            {
                Program.loadAutocratingDefinitions();
                Program.DebugPrint();
                ClearInventoryList(inventar);
                Lists.Data.NonSmsFlagedInventoryList.Clear();
                Lists.Data.SmsFlagedInventoryList.Clear();
                Program.CargoUseList.Clear();
                foreach (var ivl in InventoryManagerList.Values) ivl.Clear();
                InventoryManagerList.Clear();

                return RunJobResult.Finished;
            }
        }
    }
}
