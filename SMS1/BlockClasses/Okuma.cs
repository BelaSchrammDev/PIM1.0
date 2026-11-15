using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        //Okuma
        public class Okuma : dyBlockPowerConsumer
        {
            public IMyAssembler AssemblerBlock;
            public Okuma(IMyTerminalBlock b)
            {
                addChangeType = ChangeType.Assembler_add;
                nameChangeType = ChangeType.Assembler_name;
                tb = b;
                AssemblerBlock = (IMyAssembler)b;
                getBlockDefinition();
                setBlockTypeFields();
                addChange();
                sT("@fWM", "");
                sT("@fW", "", 55);
                sT("@fWa", "", 20);
                sT("@fNW", "", 55);
                sT("@fNWa", "", 20);
            }
            public override void Display()
            {
                sT("@fWM", AssemblerBlock.Mode == MyAssemblerMode.Disassembly ? "[DisA]" : ("[Ass" + HSS + "]"));
                string firstQueueItemName = "----", firstQueueItemAmount = "0", secondQueueItemName = "----", secondQueueItemAmount = "0";
                if (!AssemblerBlock.IsQueueEmpty)
                {
                    List<MyProductionItem> pitem = new List<MyProductionItem>();
                    AssemblerBlock.GetQueue(pitem);
                    firstQueueItemName = getCoNa(pitem[0].BlueprintId.SubtypeName);
                    firstQueueItemAmount = pitem[0].Amount.ToString();
                    if (pitem.Count > 1)
                    {
                        secondQueueItemName = getCoNa(pitem[1].BlueprintId.SubtypeName);
                        secondQueueItemAmount = pitem[1].Amount.ToString();
                    }
                }
                sT("@fW", firstQueueItemName);
                sT("@fWa", firstQueueItemAmount);
                sT("@fNW", secondQueueItemName);
                sT("@fNWa", secondQueueItemAmount);
                base.Display();
            }
            public override void getPCvar(dyDisplayableVariable pv)
            {
                readInventarList(pv, 0, '0');
                readInventarList(pv, 1, '1');
                base.getPCvar(pv);
            }
            Dictionary<string, string> bluePrintNameCastList = new Dictionary<string, string>
            {
                { "Azimuth", "Supercharger" },
                { "RailGunAm", "RailGun Spike" },
                { "NATO_5p", "Ammo5.56x45mm" },
                { "NATO_25", "Ammo25x184mm" },
                { "Radio", "Radioparts" },
                { "StoneOreToIngotBasic", "Base Ingots" }
            };
            string getCoNa(string s)
            {
                foreach (var x in bluePrintNameCastList.Keys) if (s.StartsWith(x)) return bluePrintNameCastList[x];
                if (s.StartsWith("Blueprint_")) return s.Substring(10);
                if (s.EndsWith("Component")) return s.Substring(0, s.Length - 9);
                return smsItemTypeDefinitions.FirstOrDefault(x => x.Value.blueprintName == s).Value.name;
                // return get_BluePrint_By_smsItemtype(s);
            }
        }
    }
}
