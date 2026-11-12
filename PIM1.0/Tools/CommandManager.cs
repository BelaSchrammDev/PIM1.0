namespace IngameScript
{
    partial class Program
    {
        public static class CommandDispatcher
        {
            public static bool Dispatch(string commandLine)
            {
                if (NetWorkingDispatcher.Instance.Dispatch(commandLine))
                {
                    return true;
                }

                return DispatchOtherCommands(commandLine);
            }

            public static bool DispatchOtherCommands(string commandLine)
            {
                switch (NetWorkingParser.ParseCommandName(commandLine))
                {
                    case "flushrefinerys_all":
                        foreach (var o in Lists.Data.RefineryList) o.FlushAllInventorys();
                        SetInfo("all (" + Lists.Data.RefineryList.Count + ") refinerys flushed.");
                        return true;
                }

                return false;
            }
        }
    }
}
