namespace IngameScript
{
    partial class Program
    {
        // commands of the player (run argument); the scripts talk via IGC (NetWorkingIgc)
        public static class CommandDispatcher
        {
            public static bool Dispatch(string commandLine)
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
