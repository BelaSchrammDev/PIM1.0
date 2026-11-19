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
        //VC
        public class VC
        {
            Func<double, string> F;
            public char id = '#';
            public string mS = "#";
            public double R, M, C, m = -1, c = -1;
            public MDIs mC = new MDIs(pW);
            MDIs mM = new MDIs(pW, false, "()");
            MDIbar mB = new MDIbar();
            MDIs mR = new MDIs(zW);
            public VC(char iid)
            {
                id = iid; if ("xXyYzZ".Contains(id)) { mS = ("@" + id + (id < 'x' ? "+" : "-") + "THRUST").ToUpper(); F = Fn; } else { F = Fp; }
                switch (id)
                {
                    case '#': mS = "@f#"; F = Fd; break;
                    case 'h': mS = "@fH"; F = Fd; break;
                    case 't': mS = "@fET"; F = Fn; break;
                    case 'G': mS = "@SPEED"; F = Fgm; break;
                    case 'm': mS = "@MPHSPEED"; F = Fmh; break;
                    case 'k': mS = "@KMHSPEED"; F = Fkh; break;
                    case 'M': mS = "@fM"; break;
                    case 'S': mS = "@fS"; F = Fph; break;
                    case 'E': mS = "@fO"; break;
                    case 'I': mS = "@fI"; break;
                    case 'V': mS = "@fV"; F = Fv; break;
                    case 'H': case 'O': case 'C': mS = "@fV" + id; F = Fv; break;
                }
            }
            public VC(char iid, string w, Func<double, string> f) { id = iid; mS = w; F = f; }
            public string gFs(char t = 'C') { string r = mS; switch (t) { case 'M': r += "_M"; break; case 'B': r += "_B"; break; case 'R': r += "_R"; break; } return r; }
            public void addm(Dictionary<string, MDI> mL)
            {
                mL.Add(mS, mC);
                mL.Add(gFs('M'), mM);
                mL.Add(gFs('B'), mB);
                mL.Add(gFs('R'), mR);
            }
            public void calc()
            {
                if (m != M) mM.sT(F(M));
                if (c != C) mC.sT(F(C));
                c = C;
                m = M;
                if (M != 0) R = (C * 100) / M;
                else R = 0;
                mB.sB((int)Math.Round(R, 0));
                mR.sZ(R);
            }
            public void sF(Func<double, string> q) { F = q; }
        }
        //PV
        public class dyDisplayableVariable : dyBlockDisplayable
        {
            static dyDisplayableVariable tp = null;
            static dyDisplayableVariable wp = null;
            char tc = 'g';
            public IMyCubeGrid pcg = null;
            public dyDisplayableVariable(string ts, IMyCubeGrid g)
            {
                setName(ts);
                pcg = g;
                sT("@fG", "");
                if (g == null) tc = 's';
                else sT("@fG", g.CustomName);

                if (ts == "Total") tc = '_';
                else changeNeed(ChangeType.PV_add);
            }
            public bool isID(string i, char t) { if (t != tc) return false; foreach (VC v in vcl) if (i.Contains(v.id)) return true; return false; }
            void AddCopy(dyDisplayableVariable opv) { foreach (VC v in opv.vcl) aV(v.id, v.M, v.C); }
            void AddCopy(dyDisplayableVariable opv, string f) { foreach (VC v in opv.vcl) if (f.Contains(v.id)) aV(v.id, v.M, v.C); }
            public override void Display() { if (pcg != null) sT("fG", pcg.CustomName); base.Display(); }
            public static dyDisplayableVariable getPV(IMyCubeGrid cg, string bt, bool create = true, string v = "")
            {
                foreach (dyDisplayableVariable pv in pvl)
                    if (pv.pcg == cg && pv.Name == bt)
                    {
                        pv.del = false;
                        return pv;
                    }
                if (!create) return null;
                dyDisplayableVariable npv = new dyDisplayableVariable(bt, cg);
                pvl.Add(npv);
                foreach (var y in v) npv.getVC(y);
                return npv;
            }
            public static void RefreshVar()
            {
                if (wp == null) wp = getPV(null, "WtoT");
                else wp.del = false;

                if (tp == null) tp = getPV(null, "Total");
                else tp.del = false;

                for (int i = 0; i < pvl.Count; i++)
                {
                    dyDisplayableVariable pv = pvl[i];
                    if (pv.pcg != null)
                    {
                        getPV(null, pv.Name).AddCopy(pv);
                        if (pv.Name != "JumpDrives") tp.AddCopy(pv, "SMEI");
                    }
                }

                for (int i = pvl.Count - 1; i >= 0; i--)
                    if (pvl[i].del)
                    {
                        changeNeed(ChangeType.PV_add);
                        pvl.Remove(pvl[i]);
                    }

                foreach (dyDisplayableVariable pv in pvl)
                    if (pv.Name.EndsWith("Thrust") && pv.tc == 's')
                        wp.AddCopy(pv);

                foreach (VC v in wp.vcl)
                {
                    v.C = naturalGravity == 0 ? 0 : v.M / naturalGravity;
                    v.M = shipTotalMass == 0 ? 1 : shipTotalMass;
                }

                foreach (dyDisplayableVariable pv in pvl) pv.Refresh();
            }
        }
    }
}
