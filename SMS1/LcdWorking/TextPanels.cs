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
        // the text panels and surface providers of the construct, the known displays are marked obsolete until found again
        readonly List<IMyTerminalBlock> textPanelBlocks = new List<IMyTerminalBlock>();
        readonly P textPanelArgs = new P();
        List<IMyTerminalBlock> CollectTextPanels()
        {
            GridTerminalSystem.GetBlocksOfType<IMyTextSurfaceProvider>(textPanelBlocks, b => b.IsSameConstructAs(Me));
            nomaster = true;
            foreach (var t in sms_sf) t.obs = true;
            // also Infolcd/Terminal: a panel whose tag changed (e.g. Infolcd -> Linklcd) is removed, not kept twice
            foreach (var t in infolcds) t.obs = true;
            foreach (var t in LCDts) t.obs = true;
            return textPanelBlocks;
        }
        void ProcessTextPanel(IMyTerminalBlock ts)
        {
            var pm = textPanelArgs;
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
                        il.obs = false;
                        RemoveFromLinkLcds(t);
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
                        newTerminal.obs = false;
                        RemoveFromLinkLcds(t);
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
        // a panel that is no Linklcd (anymore) must not stay linked to an Infolcd
        void RemoveFromLinkLcds(IMyTextPanel t)
        {
            foreach (var l in infolcds) l.RemoveELCD(t);
        }
        // removes the displays that were not found again
        void RemoveObsoleteDisplays()
        {
            for (int i = LCDts.Count - 1; i >= 0; i--)
            {
                LCD_Terminal tlcd = LCDts[i];
                if (!textPanelBlocks.Contains(tlcd.lcd) | tlcd.obs)
                {
                    tlcd.addChange();
                    sms_disp.Remove(tlcd);
                    LCDts.Remove(tlcd);
                }
            }
            for (int i = infolcds.Count - 1; i >= 0; i--)
            {
                var ilcd = infolcds[i];
                if (!textPanelBlocks.Contains(ilcd.lcd) | ilcd.obs)
                {
                    ilcd.addChange();
                    sms_disp.Remove(ilcd);
                    infolcds.Remove(ilcd);
                }
                foreach (IMyTextPanel xlcd in ilcd.extlcds)
                {
                    if (!textPanelBlocks.Contains(xlcd))
                    {
                        ilcd.ClearELCDs();
                        break;
                    }
                }
            }
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
        }
    }
}
