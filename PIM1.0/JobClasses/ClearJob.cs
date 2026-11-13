namespace IngameScript
{
    partial class Program
    {
        public class ClearJob : Job
        {
            public override RunJobResult RunJob()
            {
                MyProgram.LCD_DebugPrint();

                ClearInventoryList(Lists.Data.Inventory);
                Lists.Data.ClearingInventoryList.Clear();
                Lists.Data.CargoUseList.Clear();

                // TODO: must be do that
                foreach (var ivl in Lists.Data.InventoryManagerList.Values) ivl.Clear();
                Lists.Data.InventoryManagerList.Clear();

                return RunJobResult.Finished;
            }
        }
    }
}
