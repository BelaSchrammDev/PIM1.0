using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        public class HydroponicsManager : RecipeManager
        {
            public HydroponicsManager(ManageableBlock block) : base(block, IngotSubFresh, new Dictionary<string, float>
            {
                { IngotStone, 16.428f },
                { IngotWaterFood, 5f },
                { IngotNutrients, 15f },
            })
            {
            }
        }
    }
}
