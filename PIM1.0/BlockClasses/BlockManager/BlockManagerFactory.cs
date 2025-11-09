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
                    case Refinery.RefreshType.VanillaRefinery:
                        return new VanillaRefineryManager(block);

                    //case Refinery.RefreshType.WaterRecyclingSystem:
                    //    return new WaterRecyclingManager(block);

                    case Refinery.RefreshType.HydroponicsFarm:
                        return new HydrophonicsManager(block);

                    default:
                        // for creating all BlockManager use return null
                        return new DummyBlockManager(block);
                        // throw new Exception("Unsupported BlockManager type");

                }
            }
        }
    }
}
