using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using VRage;

namespace IngameScript
{
    partial class Program
    {
        public class ChangeAutoCraftingSettingsJob : Job
        {
            private bool schedule = true;
            private List<string> BluePrintKeyList = new List<string>();
            private int Index = 0;

            public override void InitJob()
            {
                if (!Properties.Data.AutoCraftingSettingsInValid)
                {
                    LCD_DebugString.Append("kein calc_ACDef\n");
                    schedule = false;
                    return;
                }

                schedule = true;
                LCD_DebugString.Append("calc_ACDef\n");

                Tools.GetBlockList(Lists.Data.Assemblers);
                Tools.GetBlockList(Lists.Data.Refinerys);

                Assembler.AssemblerTypesAcceptedBluePrints.Clear();

                BluePrintKeyList = new List<string>(Lists.Data.BluePrints_Active.Keys);

                for (int index = BluePrintKeyList.Count - 1; index >= 0; index--)
                {
                    Lists.Data.BluePrints_Inactive.Add(
                        BluePrintKeyList[index],
                        Lists.Data.BluePrints_Active[BluePrintKeyList[index]]);
                    Lists.Data.BluePrints_Active.Remove(BluePrintKeyList[index]);
                }

                BluePrintKeyList = new List<string>(Lists.Data.BluePrints_Inactive.Keys);
                Index = BluePrintKeyList.Count - 1;
                Properties.Data.AutoCraftingSettingsInValid = false;
            }

            public override RunJobResult RunJob()
            {
                if(!schedule) return RunJobResult.Finished;

                // no blueprints at all: nothing to check (Index would be -1)
                if (Index < 0)
                {
                    InitAutoCraftingTypes();
                    return RunJobResult.Finished;
                }

                var bluePrint = Lists.Data.BluePrints_Inactive[BluePrintKeyList[Index]];

                if (bluePrint.IfRefineryBluePrint())
                {
                    foreach (var ingameRefineryBlock in Lists.Data.Refinerys)
                    {
                        var subTypeName = ingameRefineryBlock.BlockDefinition.SubtypeId;
                        if (HasSmsTag(ingameRefineryBlock.CustomName) && (subTypeName.Contains("Hydroponics") || subTypeName.Contains("Reprocessor")))
                        {
                            Lists.Data.BluePrints_Active.Add(BluePrintKeyList[Index], bluePrint);
                            Lists.Data.BluePrints_Inactive.Remove(BluePrintKeyList[Index]);
                            break;
                        }
                    }
                }
                else
                {
                    var bluePrintUseIsAllowed = false;

                    foreach (var a in Lists.Data.Assemblers)
                    {
                        if (HasSmsTag(a.CustomName) && a.CanUseBlueprint(bluePrint.definition_id))
                        {
                            var subTypeId = a.BlockDefinition.SubtypeName;

                            if (!Assembler.AssemblerTypesAcceptedBluePrints.ContainsKey(subTypeId))
                            {
                                Assembler.AssemblerTypesAcceptedBluePrints.Add(subTypeId, new List<AssemblerBluePrint>());
                            }

                            Assembler.AssemblerTypesAcceptedBluePrints[subTypeId].Add(bluePrint);

                            bluePrintUseIsAllowed = true;
                        }
                    }

                    if (bluePrintUseIsAllowed)
                    {
                        Lists.Data.BluePrints_Active.Add(BluePrintKeyList[Index], bluePrint);
                        Lists.Data.BluePrints_Inactive.Remove(BluePrintKeyList[Index]);
                    }
                }

                Index--;

                // check finish
                if (Index >= 0) return RunJobResult.Continue;

                // finish
                InitAutoCraftingTypes();

                return RunJobResult.Finished;
            }

            // built again from the active blueprints: types without a tagged assembler disappear from the autocrafting LCD
            private void InitAutoCraftingTypes()
            {
                Lists.Data.autocrafting_Types.Clear();
                foreach (var bpType in Lists.Data.BluePrints_Active.Values)
                    if (!Lists.Data.autocrafting_Types.Contains(bpType.AutoCraftingType))
                        Lists.Data.autocrafting_Types.Add(bpType.AutoCraftingType);
            }

        }
    }
}
