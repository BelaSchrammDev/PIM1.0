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
                Program.LCD_DebugPrint();

                ClearInventoryList(inventar);
                Lists.Data.NoneSmsFlagedInventoryList.Clear();
                Lists.Data.SmsFlagedInventoryList.Clear();
                Lists.Data.CargoUseList.Clear();

                // TODO: must be do that
                foreach (var ivl in Lists.Data.InventoryManagerList.Values) ivl.Clear();
                Lists.Data.InventoryManagerList.Clear();

                return RunJobResult.Finished;
            }
        }
    }
}
