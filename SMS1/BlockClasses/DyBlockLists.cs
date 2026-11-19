using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        // the dyBlocks of the block lists, per block type and grid (RefreshDyBlockListJob), and their creation
        static Dictionary<BlockType, Dictionary<IMyCubeGrid, List<dyBlock>>> dyBlockLists = new Dictionary<BlockType, Dictionary<IMyCubeGrid, List<dyBlock>>>();
        static Dictionary<IMyCubeGrid, List<dyBlock>> getDyBlockList(BlockType type) { if (!dyBlockLists.ContainsKey(type)) dyBlockLists.Add(type, new Dictionary<IMyCubeGrid, List<dyBlock>>()); return dyBlockLists[type]; }
        dyBlock newDyBlock(IMyTerminalBlock b, BlockType type)
        {
            switch (type)
            {
                case BlockType.Connector: return new dyConnector(b);
                case BlockType.Cargocontainer: return new dyCargoContainer(b);
                case BlockType.Damage: return new dyDamageBlock(b);
                case BlockType.Jumpdrive: return new Jump(b);
                case BlockType.Weapon: return new Weapon(b);
                case BlockType.Assembler: return new Okuma(b);
                case BlockType.Refinery: return new Ofen(b);
                case BlockType.Reactor: return new Reactor(b);
                case BlockType.Hydrogenengine: return new HydrogenEngine(b);
                case BlockType.Battery: return new Akku(b);
                case BlockType.Tank: return new GasTank(b);
                case BlockType.Gasgenerator: return new dyGasgenerator(b);
                case BlockType.Airvent: return new Luefter(b);
            }
            return null;
        }
    }
}
