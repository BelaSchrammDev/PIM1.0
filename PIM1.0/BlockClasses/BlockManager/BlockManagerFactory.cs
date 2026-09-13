using System;

namespace IngameScript
{
    partial class Program
    {
        public static class BlockManagerFactory
        {
            public static BlockManager GetManager(Refinery.RefreshType type, ManageableBlock block)
            {
                switch (type)
                {
                    case Refinery.RefreshType.Reprocessor:
                        return new ReprocessorManager(block);

                    case Refinery.RefreshType.VanillaRefinery:
                        return new VanillaRefineryManager(block);

                    case Refinery.RefreshType.HydrophonicsFarm:
                        return new HydrophonicsManager(block);

                    case Refinery.RefreshType.WaterRecyclingSystem:
                        return new WaterRecyclingSystemManager(block);

                    default:
                        return new DummyBlockManager(block);

                }
            }
        }
    }
}
