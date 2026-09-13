using System.Collections.Generic;

namespace IngameScript
{

    partial class Program
    {
        public class RefineryManagerJob : CountingJob
        {
            protected override void ConfigureCountingBounds(out int startIndex, out int endIndex)
            {
                ProgramInstance.CalcIngotPrio();
                ProgramInstance.RenderResourceProccesingLCD();
                startIndex = 0;
                endIndex = Lists.Data.RefineryList.Count - 1;
            }

            protected override void ProcessingIndex(int index)
            {
                var refinery = Lists.Data.RefineryList[index];

                if (refinery.IsPimControlled)
                {
                    refinery.RunManager();
                }
            }
        }

        void CalcIngotPrio()
        {
            foreach (var pl in ingotprio.Values)
            {
                foreach (IPrio ip in pl)
                {
                    ip.setPrio(0);
                }
            }

            foreach (var refSubType in Refinery.refineryTypesAcceptedBlueprintsList.Keys)
            {
                foreach (var refBluePrint in Refinery.refineryTypesAcceptedBlueprintsList[refSubType])
                {
                    var inputOre = refBluePrint.InputID;

                    if (Lists.Data.Inventory.ContainsKey(inputOre) && Lists.Data.Inventory[inputOre] > 0)
                    {
                        if (refBluePrint.IsScrap)
                        {
                            addPrio(refSubType, refBluePrint, 9999);
                        }
                        else
                        {
                            var oreamount = Lists.Data.Inventory[refBluePrint.InputID];
                            var ingotamount = Lists.Data.Inventory.GetValueOrDefault(refBluePrint.OutputID, 0);
                            if (ingotamount == 0) addPrio(refSubType, refBluePrint, 200);
                            else if (ingotamount < 500) addPrio(refSubType, refBluePrint, 150);
                            else if (ingotamount < oreamount) addPrio(refSubType, refBluePrint, 100 - (int)(ingotamount / (oreamount / 97.0f)));
                            else addPrio(refSubType, refBluePrint, 1);
                        }
                    }
                }
            }

            LoadAndRenderOrePrioDefs();
        }
    }
}
