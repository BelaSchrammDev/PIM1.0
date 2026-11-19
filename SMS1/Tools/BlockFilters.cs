using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        // filters for GetBlocksOfType, and their parameters (filterGrid, BlockSubtypeFilter)

        // BlockGridCheck: the grid of the blocks
        static IMyCubeGrid filterGrid;
        // BlockSutypeCheck: part of the type id
        static string BlockSubtypeFilter = "";
        // the PB and the docked connectors: in SHIP mode the blocks of their constructs belong to the ship
        static List<IMyTerminalBlock> shipConstructBlocks = new List<IMyTerminalBlock>();
        List<IMyTerminalBlock> listTerminalBlock = new List<IMyTerminalBlock>();

        int gn(IMyCubeGrid g) { filterGrid = g; GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(listTerminalBlock, BlockGridCheck); return listTerminalBlock.Count; }
        bool BlockGridCheck(IMyTerminalBlock t) { if (filterGrid == t.CubeGrid) return true; return false; }
        bool BlockConstructCheck(IMyTerminalBlock t) { if (currentMode == ScriptMode.SHIP && !t.IsSameConstructAs(Me)) return false; return true; }
        bool BlockConstructDockingCheck(IMyTerminalBlock t) { if (currentMode != ScriptMode.SHIP) return true; foreach (var x in shipConstructBlocks) if (t.IsSameConstructAs(x)) return true; return false; }
        bool BlockSutypeCheck(IMyTerminalBlock t) { if (BlockConstructCheck(t)) return t.BlockDefinition.TypeIdString.Contains(BlockSubtypeFilter); return false; }
        bool BlockDamageCheck(IMyTerminalBlock t) { if (BlockConstructCheck(t)) return !t.CubeGrid.GetCubeBlock(t.Position).IsFullIntegrity; return false; }
        bool BlockWeaponsCheck(IMyTerminalBlock t) { if (BlockConstructCheck(t)) return (t is IMyUserControllableGun); return false; }
    }
}
