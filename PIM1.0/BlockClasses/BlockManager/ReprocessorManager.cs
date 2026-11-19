using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public class ReprocessorManager : RecipeManager
        {
            public ReprocessorManager(ManageableBlock block) : base(block, IngotSpentFuelReprocessing, Recipe(((Refinery)block).InputInventory))
            {
            }

            static Dictionary<string, float> Recipe(IMyInventory inputInventory)
            {
                var factor = ((inputInventory.MaxVolume.RawValue / 1000) / 56.9f) * 50.5f;
                return new Dictionary<string, float>
                {
                    { IngotSpentFuel, 1f * factor },
                    { OreIce, 0.75f * factor },
                    { IngotSulfur, 0.2f * factor },
                    { IngotNiter, 0.3f * factor },
                };
            }
        }
    }
}
