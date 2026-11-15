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

            /// <summary>
            /// Value of a key in a message body "key=value;key=value": "type=PIM;version=1" and "type" -> "PIM".
            /// The values must not contain ';'. Missing key: "".
            /// </summary>
            public static string GetValue(string body, string key)
            {
                foreach (var pair in body.Split(';'))
                {
                    var i = pair.IndexOf('=');
                    if (i > 0 && pair.Substring(0, i) == key) return pair.Substring(i + 1);
                }
                return "";
            }
        }
    }
}
