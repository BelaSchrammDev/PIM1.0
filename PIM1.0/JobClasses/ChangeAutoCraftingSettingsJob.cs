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
                if (!Propertys.changeAutoCraftingSettings)
                {
                    debugString += "kein calc_ACDef\n";
                    schedule = false;
                    return;
                }
                debugString += "calc_ACDef\n";
                Program.GridTerminalSystem.GetBlocksOfType<IMyAssembler>(Lists.Assemblers, block => block.CubeGrid == Program.Me.CubeGrid);
                Program.GridTerminalSystem.GetBlocksOfType<IMyRefinery>(Lists.Refinerys, block => block.CubeGrid == Program.Me.CubeGrid);
                BluePrintKeyList = new List<string>(Lists.BluePrints_Active.Keys);
                for (int i = BluePrintKeyList.Count - 1; i >= 0; i--)
                {
                    var b = Lists.BluePrints_Active[BluePrintKeyList[i]];
                    Lists.BluePrints_Inactive.Add(BluePrintKeyList[i], b);
                    Lists.BluePrints_Active.Remove(BluePrintKeyList[i]);
                }
                BluePrintKeyList = new List<string>(Lists.BluePrints_Inactive.Keys);
                Index = BluePrintKeyList.Count - 1;
                Propertys.changeAutoCraftingSettings = false;
            }

            public override RunJobResult RunJob()
            {
                if(!schedule) return RunJobResult.Finished;

                var b = Lists.BluePrints_Inactive[BluePrintKeyList[Index]];
                if (BluePrintKeyList[Index] == Ingot.SubFresh || BluePrintKeyList[Index] == (Refinery.BluePrintID_SpentFuelReprocessing))
                {
                    foreach (var r in Lists.Refinerys)
                    {
                        var subTypeName = r.BlockDefinition.SubtypeId;
                        if (r.CustomName.Contains("(sms") && (subTypeName.Contains("Hydroponics") || subTypeName.Contains("Reprocessor")))
                        {
                            Lists.BluePrints_Active.Add(BluePrintKeyList[Index], b);
                            Lists.BluePrints_Inactive.Remove(BluePrintKeyList[Index]);
                            break;
                        }
                    }
                }
                else
                {
                    foreach (var a in Lists.Assemblers)
                    {
                        if (a.CustomName.Contains("(sms") && a.CanUseBlueprint(b.definition_id))
                        {
                            Lists.BluePrints_Active.Add(BluePrintKeyList[Index], b);
                            Lists.BluePrints_Inactive.Remove(BluePrintKeyList[Index]);
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
