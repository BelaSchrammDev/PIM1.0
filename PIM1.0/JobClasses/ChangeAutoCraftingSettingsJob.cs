using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        public class ChangeAutoCraftingSettingsJob : Job
        {
            private bool schedule = true;
            private List<string> BluePrintKeyList = new List<string>();
            private int Index = 0;
            public ChangeAutoCraftingSettingsJob(Program program) : base(program, "ChangeAutoCraftingSettings")
            {
            }

            public override void InitJob()
            {
                if (!Propertys.Data.AutoCraftingSettingsInValid)
                {
                    LCD_DebugString += "kein calc_ACDef\n";
                    schedule = false;
                    return;
                }

                LCD_DebugString += "calc_ACDef\n";

                GetBlockList(Lists.Data.Assemblers);
                GetBlockList(Lists.Data.Refinerys);

                BluePrintKeyList = new List<string>(Lists.Data.BluePrints_Active.Keys);

                for (int i = BluePrintKeyList.Count - 1; i >= 0; i--)
                {
                    var b = Lists.Data.BluePrints_Active[BluePrintKeyList[i]];
                    Lists.Data.BluePrints_Inactive.Add(BluePrintKeyList[i], b);
                    Lists.Data.BluePrints_Active.Remove(BluePrintKeyList[i]);
                }

                BluePrintKeyList = new List<string>(Lists.Data.BluePrints_Inactive.Keys);
                Index = BluePrintKeyList.Count - 1;
                Propertys.Data.AutoCraftingSettingsInValid = false;
            }

            public override RunJobResult RunJob()
            {
                if(!schedule) return RunJobResult.Finished;

                var b = Lists.Data.BluePrints_Inactive[BluePrintKeyList[Index]];
                if (BluePrintKeyList[Index] == Ingot.SubFresh || BluePrintKeyList[Index] == (Refinery.BluePrintID_SpentFuelReprocessing))
                {
                    foreach (var r in Lists.Data.Refinerys)
                    {
                        var subTypeName = r.BlockDefinition.SubtypeId;
                        if (r.CustomName.Contains("(sms") && (subTypeName.Contains("Hydroponics") || subTypeName.Contains("Reprocessor")))
                        {
                            Lists.Data.BluePrints_Active.Add(BluePrintKeyList[Index], b);
                            Lists.Data.BluePrints_Inactive.Remove(BluePrintKeyList[Index]);
                            break;
                        }
                    }
                }
                else
                {
                    foreach (var a in Lists.Data.Assemblers)
                    {
                        if (a.CustomName.Contains("(sms") && a.CanUseBlueprint(b.definition_id))
                        {
                            Lists.Data.BluePrints_Active.Add(BluePrintKeyList[Index], b);
                            Lists.Data.BluePrints_Inactive.Remove(BluePrintKeyList[Index]);
                            break;
                        }
                    }
                }

                Index--;

                // check finish
                if (Index >= 0) return RunJobResult.Continue;

                // finish
                Program.InitAutoCraftingTypes();
                return RunJobResult.Finished;
            }
        }
    }
}
