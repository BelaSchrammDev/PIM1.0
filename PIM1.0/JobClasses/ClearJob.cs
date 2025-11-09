namespace IngameScript
{
    partial class Program
    {
        public class ClearJob : Job
        {
            public override RunJobResult RunJob()
            {
                ProgramInstance.loadAutocratingDefinitions();
                ProgramInstance.LCD_DebugPrint();

                ClearInventoryList(Lists.Data.inventar);
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
