namespace IngameScript
{
    partial class Program
    {
        public class NetWorkingDispatcher
        {
            public const string NW_SayHello = "say_hello";
            public const string NW_HelloResponse = "hello_response";
            public const string NW_RequestCargoUse = "request_cargouse";
            public const string NW_RequestAssemblys = "request_assemblys";

            private static NetWorkingDispatcher _Instance;
            public static NetWorkingDispatcher Instance
            {
                get
                {
                    if (_Instance == null)
                    {
                        _Instance = new NetWorkingDispatcher();
                    }
                    return _Instance;
                }
            }

            public bool Dispatch(string commandLine)
            {
                var commandFound = true;
                var commandParts = commandLine.Split('(', ')');
                if (commandParts.Length >= 2)
                {
                    switch (commandLine.ToLower())
                    {
                        // say_hello(emitterid=1233445565)
                        // hello_response(emitterid=5534224254;message=SMS.inventorymanager 2.0)
                        case NW_SayHello:

                            break;

                        default:
                            commandFound = false;
                            break;

                    }
                }

                return commandFound;
            }
        }
    }
}
