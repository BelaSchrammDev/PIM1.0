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
        // the PIM data lines (see PIM SendInfosToSMS); "" clears the PIM values (IngotPrio, CargoUse, max/queue)
        public static void LoadPimData(string data)
        {
            if (data == lastPimData) return;
            lastPimData = data;
            foreach (var t in ingotprio.Keys) foreach (IPrio ip in ingotprio[t]) ip.del = true;
            foreach (var s in smsItemTypeDefinitions.Values) { s.pmax = 0; s.max = s.imax; s.ass = 0; }
            foreach (dyCargouse cuse in cargouseList) cuse.del = true;
            var nipl = "";
            var sl = data.Split('\n');
            var max = 0;
            List<IPrio> cpl = null;
            foreach (var x in sl)
            {
                var sll = x.Split(';');
                if (sll[0] == "@INGOTPRIOLIST")
                {
                    nipl += "@" + sll[1];
                    if (!ingotprio.ContainsKey(sll[1]))
                    {
                        ingotprio.Add(sll[1], new List<IPrio>());
                        changeNeed(ChangeType.Ingotprio_add);
                    }
                    cpl = ingotprio[sll[1]];
                }
                else if ((sll[0] == "@INGOTPRIO") && sll.Length > 2 && cpl != null)
                {
                    var ci = getIntegerWithBounds(sll[2], 0, 0, 10000);
                    IPrio.getPrio(cpl, sll[1]).setPrio(ci);
                    if (ci > max) max = ci;
                }
                else if ((sll[0] == "@ASSEMBLERQUEUE") && sll.Length > 2)
                {
                    float q;
                    float.TryParse(sll[2], out q);
                    var y = GetSMSItemType(gIT(sll[1]));
                    y.ass = q;
                    if (sll.Length > 3) y.blueprintName = sll[3];
                }
                else if ((sll[0] == "@ITEMMAX") && sll.Length > 2)
                {
                    float q;
                    float.TryParse(sll[2], out q);
                    var y = GetSMSItemType(gIT(sll[1]));
                    y.SetMaximumAmount(q);
                    if (sll.Length > 3) y.blueprintName = sll[3];
                }
                else if ((sll[0] == "@CARGOUSE") && sll.Length > 3)
                {
                    float c;
                    float.TryParse(sll[2], out c);
                    float m;
                    float.TryParse(sll[3], out m);
                    setCargouse(sll[1], c, m);
                }
            }
            var listCount = cargouseList.Count;
            for (int i = cargouseList.Count - 1; i >= 0; i--)
            {
                if (cargouseList[i].del) cargouseList.Remove(cargouseList[i]);
            }
            if (listCount != cargouseList.Count) changeNeed(ChangeType.Cargouse_add);
            foreach (var t in ingotprio.Keys)
            {
                if (!nipl.Contains("@" + t))
                {
                    ingotprio.Remove(t);
                    changeNeed(ChangeType.Ingotprio_add);
                }
                else
                    foreach (IPrio ip in ingotprio[t])
                    {
                        if (ip.del) ip.setPrio(0);
                        ip.amountVC.M = max;
                        ip.Display();
                    }
            }
        }
        public static Dictionary<string, List<IPrio>> ingotprio = new Dictionary<string, List<IPrio>>();
    }
}
