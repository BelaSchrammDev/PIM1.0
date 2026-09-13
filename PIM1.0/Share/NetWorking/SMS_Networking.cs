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
    }
}
