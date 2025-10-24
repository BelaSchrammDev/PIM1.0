using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript
{
    partial class Program
    {
        public static class CommandDispatcher
        {
            public static void Dispatch(string commandLine)
            {
                switch (commandLine.ToLower())
                {
                    case "flushrefinerys_all":
                        foreach (var o in Lists.Data.RefineryList) o.FlushAllInventorys();
                        SetInfo("all (" + Lists.Data.RefineryList.Count + ") refinerys flushed.");
                        break;

                    default:
                        SetInfo("unknow command: \"" + commandLine + "\"");
                        break;

                }
            }
        }
    }
}
