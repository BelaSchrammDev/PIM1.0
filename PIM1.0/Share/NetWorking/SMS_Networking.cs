using Sandbox.ModAPI.Ingame;
using System;

namespace IngameScript
{
    partial class Program
    {
        public enum NetWorkingClientType
        {
            Unknown,
            InventoryManager,
            DisplayManager,
            CommandManager
        }

        public class NetWorkingClient
        {
            public NetWorkingClientType ClientType = NetWorkingClientType.Unknown;
            public long EmitterId = 0;
            public IMyProgrammableBlock Block = null;

            public NetWorkingClient(NetWorkingClientType clientType, long emitterId)
            {
                ClientType = clientType;
                EmitterId = emitterId;
            }
        }

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
