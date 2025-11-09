using System.Collections.Generic;

namespace IngameScript
{


    partial class Program
    {
        public class ViewManagerJob : Job
        {
            public override RunJobResult RunJob()
            {
                for (int i = viewList.Count - 1; i >= 0; i--) { if (viewList[i].IsOver()) viewList.Remove(viewList[i]); }

                Dictionary<string, string> recommendedItems = new Dictionary<string, string>
                        {
                            { Ore.Stone, Resources.RStone },
                            { Ingot.Stone, "Gravel" },
                            { Ore.Ice, "Ice" },
                            { Ingot.WaterFood, "Water" },
                            { Ingot.GreyWater, "Greywater" },
                            { Ingot.DeuteriumContainer, "Deuterium" },
                            { Ore.Organic, "Organic" },
                        };

                foreach (var item in recommendedItems)
                {
                    var condition = (Lists.Data.inventar.ContainsKey(item.Key) && Lists.Data.inventar[item.Key] > 0 && !Lists.Data.InventoryManagerList.ContainsKey(item.Key));
                    SetWarningByCondition(condition, Warning.ID.CARGORECOMMENDED, item.Value);
                }

                foreach (var c in Lists.Data.CargoUseList.Keys)
                {
                    var cargoUseRatio = Lists.Data.CargoUseList[c].GetCarcoCapacityUseRatio();
                    if (cargoUseRatio >= 90)
                    {
                        SetWarning(Warning.ID.CARGOUSEHEAVY, c);
                        ClearWarning(Warning.ID.CARGOUSEFULL, c);
                    }
                    else if (cargoUseRatio >= 99)
                    {
                        SetWarning(Warning.ID.CARGOUSEFULL, c);
                        ClearWarning(Warning.ID.CARGOUSEHEAVY, c);
                    }
                    else
                    {
                        ClearWarning(Warning.ID.CARGOUSEHEAVY, c);
                        ClearWarning(Warning.ID.CARGOUSEFULL, c);
                    }
                }

                ProgramInstance.CalcutateInfos();

                return base.RunJob();
            }
        }
    }
}
