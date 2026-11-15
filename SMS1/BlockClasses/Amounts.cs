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
        //A
        public abstract class dyAmount : dyBlockDisplayable
        {
            public VC amountVC;
            public void addAmountVC(Func<double, string> f, string field = "@fAm") { amountVC = new VC('A', field, f); amountVC.addm(MDIs); vcl.Add(amountVC); }
            public override int CompareTo(dyBlockDisplayable other) { if (iss.StartsWith("AMOUNT")) { dyAmount oi = other as dyAmount; if (oi.amountVC.C == amountVC.C) return 0; if (iss.EndsWith("BACK")) return oi.amountVC.C > amountVC.C ? -1 : 1; else return oi.amountVC.C > amountVC.C ? 1 : -1; } return base.CompareTo(other); }
        }
        static List<dyCargouse> cargouseList = new List<dyCargouse>();
        public class dyCargouse : dyAmount
        {
            public dyCargouse(String name)
            {
                setName(name);
                sT("@fG", name);
                changeNeed(ChangeType.Cargouse_add);
                addAmountVC(Fv, "@fVC");
            }
            public void setUse(float current, float max)
            {
                if (current != amountVC.C || max != amountVC.M)
                {
                    changeNeed(ChangeType.Cargouse_change_amount);
                    amountVC.C = current;
                    amountVC.M = max;
                    Display();
                }
            }
        }
        static void setCargouse(string name, float current, float max)
        {
            dyCargouse cargouse = null;
            foreach (dyCargouse cuse in cargouseList)
            {
                if (cuse.Name == name)
                {
                    cargouse = cuse;
                    break;
                }
            }
            if (cargouse == null)
            {
                cargouse = new dyCargouse(name);
                cargouseList.Add(cargouse);
            }
            else cargouse.del = false;
            cargouse.setUse(current, max);
        }
        //IIT
        public class dyInventory : dyAmount
        {
            Func<double, string> aF;
            double am, av;
            MDIs q = new MDIs(pW, true);
            public IMyCubeGrid ig = null;
            public smsItemType sy;
            public dyInventory(MyItemType tstr, IMyCubeGrid iig = null, int xsd = 0)
            {
                sy = GetSMSItemType(tstr);
                setName(sy.name);
                if (sy.IfItemtypeOreOrIngot())
                {
                    if (sy.IsItemVolumetype()) aF = Fv;
                    else if (sy.IsItemMasstype()) aF = Fm;
                    else aF = Fdp;
                }
                else aF = Fd;
                addAmountVC(aF);
                amountVC.C = -1;
                changeNeed(ChangeType.Inventar_add);
                ig = iig;
                MDIs.Add("@fQ", q);
                sM("@fAmM", 0, pW);
                sM("@fAmV", 0, pW);
            }
            public double getAmount() { return amountVC.C; }
            public override void Display()
            {
                var qu = sy.ass;
                q.sT(qu == 0 ? (sy.pmax != 0 ? "--- idle ---" : " ") : aF(qu));
                double ma = amountVC.M, oa = amountVC.C; amountVC.M = sy.max;
                amountVC.C = sy.GetAmount(ig);
                if (oa == amountVC.C && ma == amountVC.M) return;
                if (amountVC.C == 0 ^ oa == 0)
                {
                    changeNeed(ChangeType.Inventar_change_null);
                }
                string oas = amountVC.mC.text;
                amountVC.calc();
                if (oas != amountVC.mC.text)
                {
                    av = sy.GetVolume(amountVC.C);
                    am = sy.GetMass(amountVC.C);
                    sT("@fAmV", Fv(av));
                    sM("@fAmM", am);
                    changeNeed(ChangeType.Inventar_change_amount);
                }
            }
            public override int CompareTo(dyBlockDisplayable other) { if (iss.StartsWith("VOLUME") || iss.StartsWith("MASS")) { dyInventory oi = other as dyInventory; double x = 0, xo = 0; if (iss.StartsWith("VOLUME")) { x = av; xo = oi.av; } else { x = am; xo = oi.am; } if (xo == x) return 0; if (iss.EndsWith("BACK")) return xo > x ? -1 : 1; else return xo > x ? 1 : -1; } return base.CompareTo(other); }
        }
        //Ip
        public class IPrio : dyAmount
        {
            public string type;
            public IPrio(string tstr, int pr = 0) { setName(tstr); type = tstr; addAmountVC(Fd); amountVC.C = pr; amountVC.M = 100; changeNeed(ChangeType.Ingotprio_add); }
            public void setPrio(int newIngotprio)
            {
                del = false;
                if (newIngotprio == 0 || amountVC.C != newIngotprio)
                {
                    amountVC.C = newIngotprio;
                    changeNeed(newIngotprio == 0 ? ChangeType.Ingotprio_set_null : ChangeType.Ingotprio_change_amount);
                }
            }
            public static IPrio getPrio(List<IPrio> pl, string pt) { foreach (IPrio ip in pl) { if (ip.type == pt) return ip; } IPrio np = new IPrio(pt); pl.Add(np); return np; }
        }
        //OII
        public class dyResourceOreIngot : dyBlockDisplayable
        {
            smsItemType sto, sti; public int of = 0; public double oa = -1, ia = -1;
            public dyResourceOreIngot(string ist)
            {
                sto = GetSMSItemType(gITo(ist));
                sti = GetSMSItemType(gITi(ist + (ist == "Deuterium" ? "Container" : "")));
                setName(ist);
                changeNeed(ChangeType.Resource_add);
                sD("@fRr", 0, 8, true);
                sM("@fRi", 0, 27, true);
                sM("@fRo", 0, 27, true);
            }
            public override void Display()
            {
                sD("@fRr", of);
                double ooa = oa, oia = ia;
                oa = sto.GetMass(sto.GetAmount());
                ia = sti.GetMass(sti.GetAmount());
                if ((ooa == 0 ^ oa == 0) || (oia == 0 ^ ia == 0)) changeNeed(ChangeType.Resource_change_null);
                sM("@fRo", oa);
                sM("@fRi", ia);
            }
        }
    }
}
