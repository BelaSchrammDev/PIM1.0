namespace IngameScript
{
    partial class Program
    {
        public interface ICommandArgument
        {
            string Name { get; }
            T GetValue<T>();
        }

        public class CommandArgument<T> : ICommandArgument
        {
            public T Value;
            public string Name { get; }

            public CommandArgument(string name, T value)
            {
                Name = name;
                Value = value;
            }

            public TConverted GetValue<TConverted>()
            {
                if (typeof(TConverted) == typeof(T))
                {
                    return (TConverted)(object)Value;
                }

                return default(TConverted);
            }
        }
    }
}
