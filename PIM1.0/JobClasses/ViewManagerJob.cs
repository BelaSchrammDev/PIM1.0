using System.Collections.Generic;

namespace IngameScript
{


    partial class Program
    {
        public class ViewManagerJob : Job
        {
            static readonly Dictionary<string, string> recommendedItems = new Dictionary<string, string>
            {
                { Ore.Stone, IngameStrings.RStone },
                { Ingot.Stone, "Gravel" },
                { Ore.Ice, "Ice" },
                { Ingot.WaterFood, "Water" },
                { Ingot.GreyWater, "Greywater" },
                { Ingot.DTFuelCapsule, "Deuterium" },
                { Ore.Organic, "Organic" },
            };

            // a recommendation is only a hint: shown for a while, then hidden until the script restarts
            const int RecommendationSeconds = 300;
            static readonly HashSet<string> hiddenRecommendations = new HashSet<string>();

            // the hints of this job; the other jobs reported theirs earlier in the cycle (see Warning)
            public override RunJobResult RunJob()
            {
                foreach (var item in recommendedItems)
                {
                    if (hiddenRecommendations.Contains(item.Value)) continue;
                    if (!Lists.Data.Inventory.ContainsKey(item.Key) || Lists.Data.Inventory[item.Key] <= 0 || Lists.Data.InventoryManagerList.ContainsKey(item.Key)) continue;
                    var shown = GetWarning(Warning.ID.CARGORECOMMENDED, item.Value);
                    if (shown != null && shown.AgeInSec > RecommendationSeconds) hiddenRecommendations.Add(item.Value);
                    else SetWarning(Warning.ID.CARGORECOMMENDED, item.Value);
                }

                foreach (var c in Lists.Data.CargoUseList.Keys)
                {
                    var cargoUseRatio = Lists.Data.CargoUseList[c].GetCargoCapacityUseRatio();
                    if (cargoUseRatio >= 99) SetWarning(Warning.ID.CARGOUSEFULL, c);
                    else if (cargoUseRatio >= 90) SetWarning(Warning.ID.CARGOUSEHEAVY, c);
                }

                SweepViews();
                MyProgram.CalculateInfos();

                return base.RunJob();
            }
        }
    }
}
