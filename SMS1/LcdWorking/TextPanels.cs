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
        bool getTextPanels()
        {
            P pm = new P();
            do
            {
                switch (m1)
                {
                    case 0:
                        if (!main.ifS() && sms_sf.Count > 0)
                        {
                            m1 = 0;
                            return false;
                        }
                        GridTerminalSystem.GetBlocksOfType<IMyTextSurfaceProvider>(listTerminalBlock, b => b.IsSameConstructAs(Me));
                        m1++;
                        break;
                    case 1:
                        nomaster = true;
                        foreach (var t in sms_sf) t.obs = true;
                        m2 = 0;
                        m1++;
                        break;
                    case 2:
                        for (int i = m2; i < listTerminalBlock.Count; i++, m2++)
                        {
                            if (cI()) return true;
                            var ts = listTerminalBlock[i];
                            if (ts.BlockDefinition.ToString().Contains("_TextPanel/"))
                            {
                                var t = (IMyTextPanel)ts;
                                if (pm.ParseArgs(t.CustomName))
                                {
                                    if (pm.isPM("Infolcd"))
                                    {
                                        LCD_Infolcd il = null;
                                        foreach (var l in infolcds) if (t == l.lcd)
                                            {
                                                il = l;
                                                break;
                                            }
                                        if (il == null)
                                        {
                                            il = new LCD_Infolcd(t);
                                            infolcds.Add(il);
                                            sms_disp.Add(il);
                                        }
                                        il.Calc(pm);
                                    }
                                    else if (pm.isPM("Terminal"))
                                    {
                                        LCD_Terminal newTerminal = null;
                                        foreach (var l in LCDts)
                                            if (t == l.lcd)
                                            {
                                                newTerminal = l;
                                                break;
                                            }
                                        if (newTerminal == null)
                                        {
                                            newTerminal = new LCD_Terminal(t);
                                            LCDts.Add(newTerminal);
                                            sms_disp.Add(newTerminal);
                                        }
                                        newTerminal.Calc(pm);
                                    }
                                    else if (pm.isPM("Linklcd"))
                                    {
                                        nomaster = true;
                                        foreach (var llcd in infolcds)
                                        {
                                            if (llcd.extlcds.Contains(t)) nomaster = false;
                                            else if ((llcd.linkpos == t.Position) && (llcd.lcd.Orientation == t.Orientation))
                                            {
                                                llcd.AddPanel(t);
                                                nomaster = false;
                                                break;
                                            }
                                        }
                                        if (nomaster)
                                        {
                                            w2lcd(t, "No InfoLCD to link....", 1);
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (var iflcd in infolcds)
                                    {
                                        if (iflcd.lcd == t)
                                        {
                                            iflcd.obs = true;
                                            break;
                                        }
                                        else iflcd.RemoveELCD(t);
                                    }
                                    foreach (LCD_Terminal tlcd in LCDts)
                                        if (tlcd.lcd == t)
                                        {
                                            tlcd.obs = true;
                                            break;
                                        }
                                }
                            }
                            else
                            {
                                var p = (IMyTextSurfaceProvider)ts;
                                if (p.SurfaceCount == 1 && pm.ParseArgs(ts.CustomName))
                                {
                                    TextSurfaceDisplay nsf = null;
                                    var sf = p.GetSurface(0);
                                    foreach (var t in sms_sf)
                                        if (t.ts == sf)
                                        {
                                            nsf = t;
                                            break;
                                        }
                                    if (nsf == null)
                                    {
                                        nsf = new TextSurfaceDisplay(sf, ts);
                                        sms_sf.Add(nsf);
                                        sms_disp.Add(nsf);
                                    }
                                    nsf.Calc(pm, 0, ts.CustomData);
                                }
                                else
                                {
                                    var ds = "";
                                    var sfn = -1;
                                    var cdl = ts.CustomData.Split('\n');
                                    foreach (var s in cdl)
                                    {
                                        var su = s.Trim().ToUpper();
                                        if (su.Contains("@DISPLAY"))
                                        {
                                            if (pm.ParseArgs(s)) sfn = pm.isPM("Infolcd") ? getIntegerWithBounds(pm.getPM("Infolcd"), 0, 0, p.SurfaceCount - 1) : -1;
                                        }
                                        else if (su.Contains("@ENDDISPLAY"))
                                        {
                                            if (sfn >= 0 && sfn < p.SurfaceCount)
                                            {
                                                TextSurfaceDisplay nsf = null;
                                                var sf = p.GetSurface(sfn);
                                                foreach (var t in sms_sf)
                                                    if (t.ts == sf)
                                                    {
                                                        nsf = t;
                                                        break;
                                                    }
                                                if (nsf == null)
                                                {
                                                    nsf = new TextSurfaceDisplay(sf, ts);
                                                    sms_sf.Add(nsf);
                                                    sms_disp.Add(nsf);
                                                }
                                                nsf.Calc(pm, 0, ds);
                                            }
                                            sfn = -1;
                                            ds = "";
                                        }
                                        else ds += (ds == "" ? "" : "\n") + s;
                                    }
                                }
                            }
                        }
                        for (int i = LCDts.Count - 1; i >= 0; i--)
                        {
                            LCD_Terminal tlcd = LCDts[i];
                            if (!listTerminalBlock.Contains(tlcd.lcd) | tlcd.obs)
                            {
                                tlcd.addChange();
                                sms_disp.Remove(tlcd);
                                LCDts.Remove(tlcd);
                            }
                        }
                        for (int i = infolcds.Count - 1; i >= 0; i--)
                        {
                            var ilcd = infolcds[i];
                            if (!listTerminalBlock.Contains(ilcd.lcd) | ilcd.obs)
                            {
                                ilcd.addChange();
                                sms_disp.Remove(ilcd);
                                infolcds.Remove(ilcd);
                            }
                            foreach (IMyTextPanel xlcd in ilcd.extlcds)
                            {
                                if (!listTerminalBlock.Contains(xlcd))
                                {
                                    ilcd.ClearELCDs();
                                    break;
                                }
                            }
                        }
                        m1++;
                        break;
                    case 3:
                        for (int i = sms_sf.Count - 1; i >= 0; i--)
                        {
                            TextSurfaceDisplay sf = sms_sf[i];
                            if (sf.obs)
                            {
                                sf.addChange();
                                sms_disp.Remove(sf);
                                sms_sf.Remove(sf);
                            }
                        }
                        m1++;
                        break;
                    default:
                        m1 = 0;
                        return false;
                }
            }
            while (!cI()); return true;
        }
    }
}
