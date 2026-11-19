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
        //Dy
        public abstract class dyBlockDisplayable : IComparable<dyBlockDisplayable>
        {
            int refcount = 10; public string Name = ""; public bool del = false; public ChangeType addChangeType = ChangeType.Empty, nameChangeType = ChangeType.Empty; DateTime curdt = DateTime.Now; public List<VC> vcl = new List<VC>(); public Dictionary<string, MDI> MDIs = new Dictionary<string, MDI>(); List<dyMultiDisplayLine> mdls = new List<dyMultiDisplayLine>();
            public virtual int CompareTo(dyBlockDisplayable other) { int ct = string.Compare(Name, other.Name); if (sortColumn.Contains("BACK")) { if (ct < 0) ct = 1; else if (ct > 0) ct = -1; return ct; } return ct; }
            public dyMultiDisplayLine getMDLo(TextRenderEngine sd, float with, string mode)
            {
                dyMultiDisplayLine dls = null;
                foreach (dyMultiDisplayLine xdls in mdls)
                    if (xdls.with == with && xdls.mode == mode)
                    {
                        dls = xdls;
                        break;
                    }
                if (dls == null)
                {
                    var msl = mode.Split(',');
                    var nmdil = new List<MDIbox>();
                    foreach (var msls in msl) nmdil.Add(new MDIbox(with, msls, MDIs));
                    dls = new dyMultiDisplayLine(with, mode, ref nmdil);
                    mdls.Add(dls);
                }
                return dls;
            }
            public void addChange() { changeNeed(addChangeType); }
            public virtual void Display() { foreach (VC v in vcl) v.calc(); }
            public void sV(string k, double aM, double aC, char id = ' ', dyDisplayableVariable p = null) { VC v = getVC(k); if (v != null) { v.M = aM; v.C = aC; } if (p != null) p.aV(id, aM, aC); }
            public void sV(char id, double aM, double aC, dyDisplayableVariable p = null) { VC v = getVC(id); v.M = aM; v.C = aC; if (p != null) p.aV(id, aM, aC); }
            public void aV(char id, double aM, double aC) { VC v = getVC(id); v.M += aM; v.C += aC; }
            public void aM(char id, double am) { getVC(id).M += am; }
            public void aC(char id, double ac) { getVC(id).C += ac; }
            public void setName(string n) { if (Name != n) { Name = n; sT("@fN", n); changeNeed(nameChangeType); } }
            MDIs ns(string k, float z, bool n) { if (!MDIs.ContainsKey(k)) MDIs.Add(k, new MDIs(z, n)); return MDIs[k] as MDIs; }
            public void sT(string k, string s, float z = 0) { ns(k, z, false).sT(s); }
            public void sM(string k, double d, float z = 0, bool n = false) { ns(k, z, n).sM(d); }
            public void sD(string k, double d, float z = 0, bool n = false) { ns(k, z, n).sD(d); }
            public void clearAllVCs() { del = true; foreach (VC v in vcl) { v.C = 0; v.M = 0; } }
            public VC add_to_VC(VC v) { vcl.Add(v); if (!MDIs.ContainsKey(v.gFs())) v.addm(MDIs); return v; }
            public VC add_to_VC(char c) { var v = getVC(c); if (!MDIs.ContainsKey(v.gFs())) v.addm(MDIs); return v; }
            public VC getVC(char c, bool n = true) { foreach (VC x in vcl) if (x.id == c) return x; VC v = null; if (n) { vcl.Add(v = new VC(c)); v.addm(MDIs); } return v; }
            public VC getVC(string k) { foreach (VC x in vcl) if (x.mS == k) return x; return null; }
            public virtual void Refresh(bool f = false) { del = false; if (--refcount < 0) { for (int i = mdls.Count - 1; i >= 0; i--) { var ml = mdls[i]; if (--ml.live < 0) mdls.Remove(ml); } refcount = 10; } if (f || mdls.Count > 0) Display(); }
        }
    }
}
