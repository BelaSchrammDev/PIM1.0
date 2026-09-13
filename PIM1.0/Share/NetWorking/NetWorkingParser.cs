using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        public static class NetWorkingParser 
        {
            public const string ARG_Value = "value";
            public const string ARG_ValueName = "valuename";
            public const string ARG_ModuleType = "moduletype";
            public const string ARG_Version = "version";
            public const string ARG_Patch = "patch";
            public const string ARG_EmitterId = "emitter_id";

            public static void ParseNetWorkingArguments(string argumentLine, IList<ICommandArgument> argList)
            {
                var args = argumentLine.Split(';');
                foreach (var arg in args)
                {
                    var argParts = arg.Split('=');
                    var aName = argParts[0].ToLower();
                    var aValue = argParts.Length >= 2 ? argParts[1] : null;

                    ICommandArgument commandArgument = null;

                    switch (aName)
                    {
                        case ARG_Value:
                            if (aValue != null)
                            {
                                CommandArgumentFactory.TryCreate<double>(aName, aValue, out commandArgument);
                            }

                            break;

                        case ARG_ValueName:
                        case ARG_ModuleType:
                            if (aValue != null)
                            {
                                CommandArgumentFactory.TryCreate<string>(aName, aValue, out commandArgument);
                            }

                            break;

                        case ARG_Version:
                        case ARG_Patch:
                            if (aValue != null)
                            {
                                CommandArgumentFactory.TryCreate<int>(aName, aValue, out commandArgument);
                            }

                            break;

                        case ARG_EmitterId:
                            if (aValue != null)
                            {
                                CommandArgumentFactory.TryCreate<long>(aName, aValue, out commandArgument);
                            }

                            break;

                    }

                    if (commandArgument != null)
                    {
                        argList.Add(commandArgument);
                    }
                }
            }
        }
    }
}
