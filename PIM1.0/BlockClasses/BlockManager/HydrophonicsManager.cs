using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        public class HydrophonicsManager : RecipeManager
        {
            public HydrophonicsManager(ManageableBlock block) : base(block, Ingot.SubFresh, new Dictionary<string, float>
            {
                { Ingot.Stone, 16.428f },
                { Ingot.WaterFood, 5f },
                { Ingot.Nutrients, 15f },
            })
            {
            }
        }
    }
}
