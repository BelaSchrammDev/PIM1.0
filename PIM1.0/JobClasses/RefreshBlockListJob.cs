using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        /// <summary>
        /// Keeps the list of PIM block objects (e.g. Refinery) in line with the blocks of the grid:
        /// objects of removed blocks are dropped, new blocks get an object. The objects keep their state
        /// between the cycles (tag, errors, manager), so they are not created again every cycle.
        /// </summary>
        public class RefreshBlockListJob<TBlock, TPimBlock> : CountingJob
            where TBlock : class, IMyFunctionalBlock
            where TPimBlock : ManageableBlock
        {
            // filled from the grid every cycle; after the comparison only the new blocks are left in it
            readonly List<TBlock> _blocks;
            readonly List<TPimBlock> _pimBlocks;
            readonly Func<TBlock, TPimBlock> _create;

            public RefreshBlockListJob(List<TBlock> blocks, List<TPimBlock> pimBlocks, Func<TBlock, TPimBlock> create)
            {
                _blocks = blocks;
                _pimBlocks = pimBlocks;
                _create = create;
            }

            protected override void ConfigureCountingBounds(out int startIndex, out int endIndex)
            {
                Tools.GetBlockList(_blocks);

                // block is gone: the accepted blueprints of the remaining blocks must be determined again
                Tools.SyncBlockList(_blocks, _pimBlocks, p => (TBlock)p.FunctionalBlock, p => Properties.Data.AutoCraftingSettingsInvalid = true);

                startIndex = _blocks.Count - 1;
                endIndex = 0;
            }

            protected override void ProcessingIndex(int index)
            {
                _pimBlocks.Add(_create(_blocks[index]));
            }
        }
    }
}
