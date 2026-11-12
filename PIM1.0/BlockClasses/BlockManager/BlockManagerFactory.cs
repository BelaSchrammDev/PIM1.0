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

                    case Refinery.RefreshType.NoAutofill:
                        return new NoAutofillManager(block);

                    default:
                        return new UnknownRefineryManager(block);

                }
            }
        }
    }
}
