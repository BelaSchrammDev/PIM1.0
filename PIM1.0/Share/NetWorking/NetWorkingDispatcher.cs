namespace IngameScript
{
    partial class Program
    {
        public class NetWorkingDispatcher
        {
            public const string NW_SayHello = "say_hello";

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
                switch (NetWorkingParser.ParseCommandName(commandLine))
                {
                    // say_hello(emitterid=1233445565)
                    // hello_response(emitterid=5534224254;message=SMS.inventorymanager 2.0)
                    case NW_SayHello:
                        return true;
                }

                return false;
            }
        }
    }
}
