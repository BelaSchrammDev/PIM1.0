namespace IngameScript
{
    partial class Program
    {
        public static class NetWorkingParser
        {
            /// <summary>
            /// Name of a command line: "Say_Hello(emitter_id=1)" and " say_hello " -> "say_hello".
            /// The brackets with the arguments are optional.
            /// </summary>
            public static string ParseCommandName(string commandLine)
            {
                return commandLine.Split('(')[0].Trim().ToLower();
            }
        }
    }
}
