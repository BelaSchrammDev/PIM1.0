using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        /// <summary>
        /// Keeps the dyBlock list of one BlockType in sync with the grid, one block per RunJob:
        /// 1. remove the dyBlocks whose block is gone (the found blocks that already have one are taken out of the found list)
        /// 2. add a dyBlock for every remaining found block
        /// 3. InitRefresh all dyBlocks
        /// </summary>
        class RefreshDyBlockListJob : Job
        {
            readonly Program _program;
            readonly BlockType _type;
            readonly string _subtype;
            readonly Func<IMyTerminalBlock, bool> _check;

            readonly List<IMyTerminalBlock> _found = new List<IMyTerminalBlock>();
            // all dyBlocks of the type, flat, with the grid they are listed under
            readonly List<dyBlock> _blocks = new List<dyBlock>();
            readonly List<IMyCubeGrid> _grids = new List<IMyCubeGrid>();
            Dictionary<IMyCubeGrid, List<dyBlock>> _dyBlocks;
            int _phase, _index;

            public RefreshDyBlockListJob(Program program, BlockType type, string subtype, Func<IMyTerminalBlock, bool> check)
            {
                _program = program;
                _type = type;
                _subtype = subtype;
                _check = check ?? program.BlockSutypeCheck;
            }

            public override void InitJob()
            {
                _dyBlocks = getDyBlockList(_type);
                // read by BlockSutypeCheck
                BlockSubtypeFilter = _subtype;
                gts.GetBlocksOfType(_found, _check);
                CollectBlocks();
                _phase = 0;
            }

            void CollectBlocks()
            {
                _blocks.Clear();
                _grids.Clear();
                foreach (var g in _dyBlocks)
                {
                    foreach (var b in g.Value)
                    {
                        _blocks.Add(b);
                        _grids.Add(g.Key);
                    }
                }
                _index = 0;
            }

            public override RunJobResult RunJob()
            {
                switch (_phase)
                {
                    case 0:
                        if (_index < _blocks.Count)
                        {
                            var b = _blocks[_index];
                            if (!_found.Remove(b.tb))
                            {
                                b.remove();
                                _dyBlocks[_grids[_index]].Remove(b);
                            }
                            _index++;
                            return RunJobResult.Continue;
                        }
                        break;

                    case 1:
                        if (_index < _found.Count)
                        {
                            AddBlock(_found[_index++]);
                            return RunJobResult.Continue;
                        }
                        // now with the new ones
                        CollectBlocks();
                        break;

                    default:
                        if (_index < _blocks.Count)
                        {
                            _blocks[_index++].InitRefresh();
                            return RunJobResult.Continue;
                        }
                        return RunJobResult.Finished;
                }

                _phase++;
                _index = 0;
                return RunJobResult.Continue;
            }

            void AddBlock(IMyTerminalBlock b)
            {
                if (!_program.BlockConstructDockingCheck(b)) return;
                if (!_dyBlocks.ContainsKey(b.CubeGrid)) _dyBlocks.Add(b.CubeGrid, new List<dyBlock>());
                _dyBlocks[b.CubeGrid].Add(_program.newDyBlock(b, _type));
            }
        }
    }
}
