namespace IngameScript
{
    partial class Program
    {
        public static class CommandArgumentFactory 
        {
            public static bool TryCreate<T>(string argumentName, string stringValue, out ICommandArgument cArgument)
            {
                cArgument = null;

                if (typeof(T) == typeof(string))
                {
                    cArgument = new CommandArgument<string>(argumentName, stringValue);
                }
                else if (typeof(T) == typeof(int))
                {
                    int intValue;
                    if (int.TryParse(stringValue, out intValue))
                    {
                        cArgument = new CommandArgument<int>(argumentName, intValue);
                    }
                }
                else if (typeof(T) == typeof(long))
                {
                    long longValue;
                    if (long.TryParse(stringValue, out longValue))
                    {
                        cArgument = new CommandArgument<long>(argumentName, longValue);
                    }
                }
                else if (typeof(T) == typeof(double))
                {
                    double doubleValue;
                    if (double.TryParse(stringValue, out doubleValue))
                    {
                        cArgument = new CommandArgument<double>(argumentName, doubleValue);
                    }
                }

                return cArgument != null;
            }
        }
    }
}
