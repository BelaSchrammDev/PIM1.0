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
        //TS
        public abstract class TextRenderEngine : dyBlockDisplayable
        {
            VC ic = new VC('R', "@fIC", Fd);
            MDIs curlines = new MDIs(12);
            public IMyCubeGrid tsg;
            public List<List<dyMultiDisplayLine>> nmdls = new List<List<dyMultiDisplayLine>>();
            public List<DP> dps = new List<DP>();
            DateTime cdsdt = DateTime.Now;
            public string ostr = "", cds = "", lds = "";
            public List<string> ostrl = new List<string>();
            public float with, iwith, dif_wm = 0, dif_wp = 0, fontsize, wconst = 0, fconst = 17.82f;
            public int icount = 0, cnmdl = 0, run_count = 0, DM = -1, maxlines = 1, newlines, pnum = 1, plines = 0, slines = 0, maxl = 40, minl = 5;
            public bool booting = true, lfix = true;
            public TextRenderEngine() { addChangeType = ChangeType.Display_add; nameChangeType = ChangeType.Display_name; curlines.sD(0); MDIs.Add("@fCL", curlines); ic.addm(MDIs); vcl.Add(ic); addChange(); }
            public override void Display()
            {
                // für speed variablen
                foreach (var c in "Gkm")
                {
                    VC v = getVC(c);
                    if (v != null) v.C = mss;
                }
                base.Display();
            }
            bool checkError(DP wl, int lc = -1)
            {
                if (wl.error == 0 && lc != -1 && lc == 0) wl.error = 1;
                if (wl.error == 0) return false;
                writeError(wl);
                return true;
            }
            void writeError(DP wl)
            {
                if ((wl.ifOption("NOERROR") && wl.error < 0) || (wl.ifOption("NOMESSAGE") && wl.error > 0)) wl.error = 0;
                if (wl.cm.Contains("Damag")) wl.error = 40;
                string et = "";
                switch (wl.error)
                {
                    case 0: return;
                    case -1: et = "dock [" + wl.dock + "] not found!"; break;
                    case 1: et = ""; if (wl.ifFilter()) et = " [" + wl.filter + "],"; et += " no items found!"; break;
                    case 2: et = " no valid options"; break;
                    case 3: et = "dock [" + wl.dock + "] no ship is docking!"; break;
                    case 40: et = " no damaged blocks found"; break;
                    default: et = "?"; break;
                }
                if (wl.error != 0)
                {
                    if (wl.error < 40) writeX(wl.cm + (wl.error < 0 ? ": error->," : ": warning->,") + et + ",");
                    else writeX(et + ",");
                    wl.error = 0;
                }
            }
            public virtual void clearWriteValues() { }
            public virtual void checkWriteToPanel() { }
            public abstract void writeToPanel();
            public virtual void addEndOfLineStr() { ostr += "\n"; }
            void setOneCol(DP wl, bool ns = true) { wl.with = with; wl.c = 1; if (ns == true && wl.is_Sort) wl.is_Sort = false; }
            void sort_ifTrue(DP wl) { if (wl.is_Sort) { iss = wl.sortMode; wl.listDy.Sort(); } }
            public void writeX(string arg, float i = 1)
            {
                if (arg == "") return;
                List<dyMultiDisplayLine> nnmdll = new List<dyMultiDisplayLine>();
                for (int x = 0; x < (i <= 4 ? i : 1); x++) nnmdll.Add(getMDLo(this, i <= 4 ? with / i : i, arg));
                nmdls.Add(nnmdll);
            }
            public void setLinesBounds(int l)
            {
                if (l < minl)
                {
                    l = minl;
                    dif_wp = 0;
                }
                if (l > maxl)
                {
                    l = maxl;
                    dif_wm = 0;
                }
                dif_wp = 0;
                dif_wm = 0;
                if (maxlines == l) return;
                maxlines = l;
                fontsize = fconst / l;
                with = wconst / fontsize;
            }
            bool AdjustWith() { int omax = maxlines; int pml = maxlines; if (lfix) if (newlines == maxlines) return false; else { setLinesBounds(newlines); newlines = maxlines; return true; } int nl = 0; if (dif_wm < 0) { nl = 1 + (int)(fconst / (wconst / (with + Math.Abs(dif_wm)))); if (maxlines == nl) nl++; } else if (dif_wp > 30) { nl = 1 + (int)(fconst / (wconst / (with - dif_wp))); } else if ((maxlines * pnum) < slines) nl = (slines + 1) / pnum; if (nl > 0) { if ((nl * pnum) < slines) { nl = (slines / pnum); nl += slines - (nl * pnum); } setLinesBounds(nl); if (omax != maxlines) return true; } return false; }
            public bool c()
            {
                if (DM > 0 && lds != cds) { DM = 0; slines = 0; dif_wp = 0; dif_wm = 0; lds = cds; }
                do
                {
                    switch (DM)
                    {
                        case -1: if (run_count < 1) { ostrl.Clear(); ostrl.Add("Init SMS ..."); run_count++; writeToPanel(); return true; } else DM++; break;
                        case 0: dps.Clear(); foreach (var s in cds.Split('\n')) dps.Add(new DP(with, s, tsg)); cnmdl = 0; nmdls.Clear(); Display(); clearWriteValues(); DM++; break;
                        case 1:
                            for (int c = cnmdl; c < dps.Count; c++, cnmdl++)
                            {
                                if (cI()) return false; DP wl = dps[c]; iss = wl.sortMode; wl.dyn = 0; int y = wl.style;
                                switch (wl.cm)
                                {
                                    case "Displays": wl.addBlockChangeTypes(ChangeType.Display_add, ChangeType.Display_name); foreach (var d in sms_disp) if (wl.ifFilter(d.Name)) wl.listDy.Add(d); writeTitle_ifTrue(wl, "Displays"); sort_ifTrue(wl); wl.headLineString = "Load(15% left),Name,,Lines,Instr.(31)"; wl.FieldTemplates.Add("@fIC_B(15%),@fN,,@fCL,@fIC"); dlLP(wl); break;
                                    case "Debug": DEBUG(wl); break;
                                    case "Connectors": wl.addBlockChangeTypes(ChangeType.Connector_add, ChangeType.Connector_name); MDILP(wl, BlockType.Connector); break;
                                    case "Weapons": wl.addBlockChangeTypes(ChangeType.Weapon_add, ChangeType.Weapon_name); MDILP(wl, BlockType.Weapon); break;
                                    case "Cargocontainers": wl.addBlockChangeTypes(ChangeType.Cargo_add, ChangeType.Cargo_name); MDILP(wl, BlockType.Cargocontainer); break;
                                    case "Energy_R": setOneCol(wl, false); writeX("Energyrange:,,@fChT(left)"); writeLine_ifTrue(wl); break;
                                    case "Speed": SPEED(wl); break;
                                    case "Gravity": var s1 = ",,"; var s2 = ""; if (y == 4) { s1 = "(60%),"; s2 = "(0),"; } if (y < 3 || y == 4) writeX("Natural Gravity:" + s1 + "@fgN" + s2); if (y > 1) writeX("Artificial Gravity:" + s1 + "@fgA" + s2); writeLine_ifTrue(wl); break;
                                    case "Mass": writeX("TotalMass:" + (y > 3 ? "(30)" : "") + ",,@fsMt"); if (y > 1) writeX("CargoMass:" + (y > 3 ? "(30)" : "") + ",,@fsMc"); if (y > 2) writeX("ShipMass:" + (y > 3 ? "(30)" : "") + ",,@fsMb"); writeLine_ifTrue(wl); break;
                                    case "Icload": setOneCol(wl, false); writeX("ICLoad:,,@fIC"); break;
                                    case "Writex": writeX(wl.FieldTemplates[0], wl.with); break;
                                    case "Writex_I":
                                        List<dyMultiDisplayLine> nnmdll = new List<dyMultiDisplayLine>();
                                        nnmdll.Add(getMDLo(this, iwith, wl.FieldTemplates[0]));
                                        nmdls.Add(nnmdll);
                                        break;
                                    case "Line": var s = '-'; if (wl.options.Count > 0 && wl.options[0].Length > 0) s = wl.options[0][0]; writeLine_ifTrue(wl, s, true); break;
                                    case "Resources":
                                        wl.addChangeType(ChangeType.Resource_add);
                                        if (!wl.is_Zero) wl.addChangeType(ChangeType.Resource_change_null);
                                        foreach (dyResourceOreIngot o in oiil.Values) if (wl.is_Zero || (o.oa > 0f || o.ia > 0f) || (o.oa == -1)) wl.listDy.Add(o);
                                        addFieldTemplates_NonBlock(wl, "RES");
                                        writeTitle_ifTrue(wl, "Resources");
                                        dlLP(wl);
                                        break;
                                    case "Inventory":
                                        wl.addChangeType(ChangeType.Inventar_add);
                                        if (!wl.is_Zero) wl.addChangeType(ChangeType.Inventar_change_null);
                                        if (wl.is_Sort && wl.sortMode != "BACK" && wl.sortMode != "") wl.addChangeType(ChangeType.Inventar_change_amount);
                                        addFieldTemplates_NonBlock(wl, "INV");
                                        if (wl.ifOption("GRID"))
                                        {
                                            foreach (IMyCubeGrid g in gridInventoryItems.Keys)
                                                if (wl.ifDisplayGrid(g))
                                                {
                                                    wl.titleString = g.CustomName;
                                                    INVENTORY(wl, gridInventoryItems[g]);
                                                    break;
                                                }
                                            writeError(wl);
                                        }
                                        else
                                            INVENTORY(wl, invitemsg);
                                        break;
                                    case "Gasgenerators": wl.addBlockChangeTypes(ChangeType.Gasgenerator_add, ChangeType.Gasgenerator_name); MDILP(wl, BlockType.Gasgenerator); break;
                                    case "Airvents": wl.addBlockChangeTypes(ChangeType.Airvent_add, ChangeType.Airvent_name); MDILP(wl, BlockType.Airvent); break;
                                    case "Batterys": wl.addBlockChangeTypes(ChangeType.Battery_add, ChangeType.Battery_name); MDILP(wl, BlockType.Battery); break;
                                    case "Jumpdrives": wl.addBlockChangeTypes(ChangeType.Jumpdrive_add, ChangeType.Jumpdrive_name); MDILP(wl, BlockType.Jumpdrive); break;
                                    case "Thrust": wl.addChangeType(ChangeType.PV_add); aTD(wl, "@fN" + (wl.ifGridOption() ? ",@fG" : "")); var x = wl.ifOption("SUMMARY") ? "ary" : "Thrust"; foreach (dyDisplayableVariable pv in pvl) if (wl.ifFilter(pv.Name) && pv.isID("xXyYzZ", 's') && pv.Name.EndsWith(x)) wl.listDy.Add(pv); dlLP(wl, false); break;
                                    case "Thrust_E": wl.addChangeType(ChangeType.PV_add); aYp("Thrusterefficiency total", wl, "@fET"); foreach (dyDisplayableVariable pv in pvl) if (wl.ifFilter(pv.Name) && pv.isID("t", 's')) wl.listDy.Add(pv); dlLP(wl); break;
                                    case "Thrust_W": wl.addChangeType(ChangeType.PV_add); aTD(wl, "Weight to Thrust"); wl.listDy.Add(dyDisplayableVariable.getPV(null, "WtoT")); dlLP(wl, false); break;
                                    case "Reactors": wl.addBlockChangeTypes(ChangeType.Reactor_add, ChangeType.Reactor_name); MDILP(wl, BlockType.Reactor); break;
                                    case "Hydroengines": wl.addBlockChangeTypes(ChangeType.Hydrogenengine_add, ChangeType.Hydrogenengine_name); MDILP(wl, BlockType.Hydrogenengine); break;
                                    case "Energy_P": wl.titleString = "Powerproducing"; wl.addChangeType(ChangeType.PV_add); ENERGYP(wl); break;
                                    case "Energy": wl.titleString = "Poweroverview"; wl.addChangeType(ChangeType.PV_add); ENERGIE(wl); break;
                                    case "Cargo": wl.addChangeType(ChangeType.PV_add); hoc(wl, "C", "Containers"); break;
                                    case "Solar": wl.addChangeType(ChangeType.PV_add); wl.sF("Solar"); GAS(wl, "Solarpanels", "SOLAR", "E"); break;
                                    case "Wind": wl.addChangeType(ChangeType.PV_add); wl.sF("Wind"); GAS(wl, "WindTurbines", "SOLAR", "E"); break;
                                    case "Invman":
                                        wl.addChangeType(ChangeType.Cargouse_add);
                                        if (wl.is_Sort && wl.sortMode != "BACK" && wl.sortMode != "") wl.addChangeType(ChangeType.Cargouse_change_amount);
                                        addFieldTemplates_NonBlock(wl, "HOXY");
                                        foreach (dyCargouse cuse in cargouseList) if (wl.ifFilter(cuse.Name)) wl.listDy.Add(cuse);
                                        writeTitle_ifTrue(wl, "Inventorymanager");
                                        dlLP(wl);
                                        break;
                                    case "Hydrogen": wl.addChangeType(ChangeType.TankHydrogen_add); hoc(wl, "H", "Tanks"); break;
                                    case "Oxygen": wl.addChangeType(ChangeType.TankOxygen_add); hoc(wl, "O", "Tanks"); break;
                                    case "Tanks":
                                        if (!wl.ifOption("HYDROGEN") && !wl.ifOption("OXYGEN")) wl.is_All = true;
                                        if (wl.is_All || wl.ifOption("HYDROGEN")) { wl.addBlockChangeTypes(ChangeType.TankHydrogen_add, ChangeType.TankHydrogen_name); }
                                        if (wl.is_All || wl.ifOption("OXYGEN")) { wl.addBlockChangeTypes(ChangeType.TankOxygen_add, ChangeType.TankOxygen_name); }
                                        MDILP(wl, BlockType.Tank, Ft);
                                        break;
                                    case "Dockings": wl.addChangeType(ChangeType.PV_add); wl.addBlockChangeTypes(ChangeType.Dockings_add, ChangeType.Dockings_name); DOCKINGS(wl); break;
                                    case "Refinerys": wl.addBlockChangeTypes(ChangeType.Refinery_add, ChangeType.Refinery_name); MDILP(wl, BlockType.Refinery); break;
                                    case "Damage_B": wl.addBlockChangeTypes(ChangeType.Damage_add, ChangeType.Damage_name); wl.titleString = "Damaged Blocks"; MDILP(wl, BlockType.Damage); break;
                                    case "Damage": wl.addChangeType(getChangeStat(ChangeType.Damage_add)); aYp(wl, "@fH", "@fG,"); GAS(wl, "Damage", " ", "h"); break;
                                    case "Oreprio":
                                        wl.addChangeType(ChangeType.Ingotprio_add);
                                        wl.addChangeType(ChangeType.Ingotprio_set_null);
                                        if (ingotprio.Count == 0) break;
                                        if (!wl.is_Sort)
                                        {
                                            wl.is_Sort = true;
                                            wl.sortMode = "AMOUNT";
                                        }
                                        if (wl.is_Sort && wl.sortMode != "BACK" && wl.sortMode != "") wl.addChangeType(ChangeType.Ingotprio_change_amount);
                                        addFieldTemplates_NonBlock(wl, "INV");
                                        var opls = ingotprio.Keys.ToArray();
                                        for (int i = 0; i < opls.Length; i++)
                                        {
                                            foreach (IPrio p in ingotprio[opls[i]]) if (p.amountVC.C > 0) wl.listDy.Add(p);
                                            writeTitle_ifTrue(wl, "Orepriority " + opls[i]);
                                            if (wl.listDy.Count > 0) dlLP(wl);
                                            else writeX(",no current prio,");
                                        }
                                        break;
                                    case "Assemblers": wl.addBlockChangeTypes(ChangeType.Assembler_add, ChangeType.Assembler_name); MDILP(wl, BlockType.Assembler); break;
                                    case "": writeX("(filled)"); break;
                                    default: writeX("Command '" + wl.cm + "' unknow,"); break;
                                }
                            }
                            DM++; break;
                        case 2: checkWriteToPanel(); cdsdt = DateTime.Now; DM++; break;
                        case 3: ostr = ""; ostrl.Clear(); cnmdl = 0; slines = 0; plines = 0; DM++; break;
                        case 4:
                            for (int i = cnmdl; i < nmdls.Count; i++, cnmdl++)
                            {
                                if (cI()) return false;
                                float cfx = 10000;
                                foreach (dyMultiDisplayLine ndl in nmdls[i])
                                {
                                    ostr += ndl.getText(this);
                                    if (cfx > ndl.cfx) cfx = ndl.cfx;
                                }
                                cfx *= nmdls[i].Count;
                                if (cfx < 0)
                                {
                                    if (dif_wm > cfx) dif_wm = cfx;
                                }
                                else if (cfx > 0)
                                {
                                    if (dif_wp == 0) dif_wp = cfx;
                                    else
                                    {
                                        if (dif_wp > cfx) dif_wp = cfx;
                                    }
                                }
                                plines++;
                                slines++;
                                addEndOfLineStr();
                                if (plines >= maxlines)
                                {
                                    ostrl.Add(ostr);
                                    ostr = "";
                                    plines = 0;
                                }
                            }
                            if (ostr != "") ostrl.Add(ostr);
                            DM++;
                            break;
                        default: run_count++; if (AdjustWith()) { DM = 0; return true; } writeToPanel(); dif_wp = 0; dif_wm = 0; ic.C = icount; ic.M = MAXIC; icount = 0; curlines.sD(maxlines); cnmdl = 0; DM = 3; Refresh(); foreach (DP w in dps) if (w.checkChangeStats(cdsdt)) { DM = 0; } return true;
                    }
                } while (!cI()); return true;
            }
            void aYp(DP wl, string sC, string s = "@fN,@fG,")
            {
                switch (wl.style)
                {
                    case 1:
                        wl.FieldTemplates.Add(s + "," + sC + "(0)," + sC + "_R");
                        break;
                    default:
                        wl.FieldTemplates.Add(s + "," + sC + "(0)," + sC + "_M," + sC + "_B(20%)," + sC + "_R");
                        break;
                    case 3:
                        wl.FieldTemplates.Add(s + "," + sC + "(0)," + sC + "_M,(19)");
                        wl.FieldTemplates.Add("," + sC + "_B(60%)," + sC + "_R");
                        break;
                    case 4:
                        wl.FieldTemplates.Add(s + "," + sC + "(0)," + sC + "_R");
                        wl.FieldTemplates.Add("," + sC + "_B(70%)");
                        break;
                }
            }
            void aYp(string t, DP wl, string sC, string s = "@fN,@fG,") { if (wl.FieldTemplates.Count > 0) return; writeTitle_ifTrue(wl, t); aYp(wl, sC, s); }
            Dictionary<char, string> Y = new Dictionary<char, string> { { 'F', "Cargo" }, { '0', "Cargo in" }, { '1', "Cargo out" }, { 'S', "Stored" }, { 'M', "Available" }, { 'E', "Output" }, { 'I', "Input" } };
            void aYs(DP wl, dyBlockDisplayable d, char c) { VC v = d.getVC(c, false); if (v != null) { var sv = v.gFs(); wl.FieldTemplates.Add(Y[c] + ":(30)," + sv + "," + sv + "_B(filled)," + sv + "_R"); } }
            void addFieldTemplates_HOXY(DP wl)
            {
                var c = wl.cm[0];
                if (c == 'I') c = 'C';
                switch (wl.style)
                {
                    case (1): wl.FieldTemplates.Add("@fG,,@fV" + c + "(0),@fV" + c + "_M(0),@fV" + c + "_R"); break;
                    case (2): wl.FieldTemplates.Add("@fG,,@fV" + c + ",@fV" + c + "_M,@fV" + c + "_B(28%),@fV" + c + "_R"); break;
                    case (4): wl.FieldTemplates.Add("@fG,,@fV" + c + "(0)"); wl.FieldTemplates.Add("@fV" + c + "_B(filled),@fV" + c + "_R"); break;
                    default: wl.FieldTemplates.Add("@fG,,@fV" + c + "(0),@fV" + c + "_M,(" + zW + ")"); wl.FieldTemplates.Add(",@fV" + c + "_B(60%),@fV" + c + "_R"); break;
                }
            }
            void addFieldTemplates_NonBlock(DP wl, string mode)
            {
                if (mode == "HOXY")
                {
                    addFieldTemplates_HOXY(wl);
                    return;
                }
                if (wl.FieldTemplates.Count > 0) return;
                if (mode == "INV")
                {
                    if (wl.is_Table)
                    {
                        addFieldTemplates_ByArray(wl, wl.options, templateInv, (wl.cm == "Oreprio" ? 15 : -1));
                        return;
                    }
                    if (wl.is_Fillbar)
                    {
                        addFieldTemplates_InvFillbar(wl);
                        return;
                    }
                }
                switch (mode)
                {
                    case "SOLAR": switch (wl.style) { case (1): wl.FieldTemplates.Add("@fG,@fM(0),,@fO(0),@fO_R"); break; case (3): wl.FieldTemplates.Add("@fM,,Max:,@fM_M,@fM_B(30%),@fM_R"); wl.FieldTemplates.Add("@fG,,Out:,@fO,@fO_B(30%),@fO_R"); break; default: wl.FieldTemplates.Add("@fG,@fM(0),,@fO(0),@fO_M,@fO_B(18%),@fO_R"); break; } break;
                    case "PCS": aYp(wl, "@fS"); break;
                    case "PCA": aYp(wl, "@fM"); break;
                    case "PCO": aYp(wl, "@fO"); break;
                    case "PCC": aYp(wl, "@fI"); break;
                    case "INV": switch (wl.style) { case (1): wl.FieldTemplates.Add("@fN,,@fAm(0)"); break; case (2): wl.FieldTemplates.Add("@fAm(40%),@fN,"); break; case (3): wl.FieldTemplates.Add(",@fN,@fAm"); break; default: wl.FieldTemplates.Add("@fAm,,@fN"); break; } break;
                    case "RES": switch (wl.style) { case 1: wl.headLineString = ",Ingot,(21),Ore,(7)"; wl.FieldTemplates.Add(",@fN,:,@fRi,/,@fRo"); break; default: wl.headLineString = ",(40),Ore ,(6),Ref.,(5),Ingot,"; wl.FieldTemplates.Add(",@fN(30),@fRo,@fRr,@fRi,"); break; } break;
                }
            }
            string[] templateInv = new string[]
            {
                "NAME", ",", "@fN,",
                "MAX", "Max(31)", "@fAm_M",
                "AMOUNT", "Current(31)", "@fAm",
                "FILLBAR", "(31)", "@fAm_B(31)",
                "RATIO", "(20)", "@fAm_R",
                "ASSEMBLY", "Assembly(31)", "@fQ(31)",
                "VOLUME", "Volume(31)", "@fAmV",
                "MASS", "Mass(31)", "@fAmM",
            };
            void addFieldTemplates_ByArray(DP wl, List<string> def, string[] templateArray, int maxArrayLength = -1)
            {
                var sx = "";
                foreach (var s in def)
                {
                    for (int i = 0; i < (maxArrayLength == -1 ? templateArray.Length : maxArrayLength); i += 3)
                    {
                        if (s == templateArray[i])
                        {
                            wl.headLineString += (wl.headLineString == "" ? "" : ",") + templateArray[i + 1];
                            sx += (sx == "" ? "" : ",") + templateArray[i + 2];
                        }
                    }
                }
                wl.FieldTemplates.Add(sx);
            }
            void addFieldTemplates_InvFillbar(DP wl)
            {
                List<string> d;
                if (wl.style == 4) d = new List<string> { "NAME", "AMOUNT", "FILLBAR", "MAX" };
                else
                {
                    d = new List<string> { "FILLBAR", "NAME", "AMOUNT" };
                    if (wl.style == 2) d.Add("Max");
                    if (wl.style == 3) d.Add("RATIO");
                }
                if (wl.ifOption("ASSEMBLY")) d.Add("ASSEMBLY");
                addFieldTemplates_ByArray(wl, d, templateInv);
            }
            void addFieldTemplates_Block(DP wl, BlockType type)
            {
                if (wl.ifOption("ONOFF")) wl.FieldTemplates.Add("@fN,,@fE");
                if (wl.FieldTemplates.Count > 0) return;

                switch (type)
                {
                    case BlockType.Weapon: switch (wl.style) { case 1: wl.FieldTemplates.Add("@fN,,@fE,/,@fWt"); break; default: wl.headLineString = ",Type,(20),Shots(15)"; wl.FieldTemplates.Add("@fN,,@fE,@fWt,@fAc"); break; case 3: wl.FieldTemplates.Add("@fN,,@fE,/,@fWt"); wl.FieldTemplates.Add("Ammo:(25 right),@fAo,,Shots:,@fAc"); break; case 4: wl.FieldTemplates.Add("@fN,,@fE,/,@fWt"); wl.FieldTemplates.Add("Inventory:(30 right),@fAa,x,@fAo,,Shots:,@fAc"); wl.FieldTemplates.Add("Cargo:(30 right),@fF0,@fF0_M,@fF0_B(filled),@fF0_R"); break; } break;
                    case BlockType.Damage: switch (wl.style) { case 1: wl.FieldTemplates.Add("@fN,,@fH_R"); break; default: wl.FieldTemplates.Add("@fN,,@fH_B(35%),@fH_R"); break; case 3: wl.FieldTemplates.Add("@fN,,@fBs"); wl.FieldTemplates.Add("@fH_B(filled),@fH_R"); break; } break;
                    case BlockType.Tank: switch (wl.style) { case 1: wl.FieldTemplates.Add("@fN,,@fV_M,@fSP,@fV_R"); break; default: wl.FieldTemplates.Add("@fN,,@fV_M(0),@fE,@fSP,@fV_B(25%),@fV_R"); break; case 3: wl.FieldTemplates.Add("@fN,,@fE,@fV,@fV_M,(18)"); wl.FieldTemplates.Add("@fSP,@fV_B(filled),@fV_R"); break; case 4: wl.FieldTemplates.Add("@fN,@fE, ,@fSP"); wl.FieldTemplates.Add("Stored:,@fV,,Capacity:,@fV_M"); wl.FieldTemplates.Add("@fV_B(filled),@fV_R"); break; } break;
                    case BlockType.Hydrogenengine: switch (wl.style) { case 1: wl.FieldTemplates.Add("@fN,,@fE,@fO"); break; case 4: wl.FieldTemplates.Add("@fN,,@fO_M,/,@fE"); wl.FieldTemplates.Add("Stored:(15),@fS(30),,@fChT"); wl.FieldTemplates.Add("Output:(15),@fO(30),@fO_B(filled),@fO_R"); wl.FieldTemplates.Add("Tank:(15),@fVH(30),@fVH_B(filled),@fVH_R"); break; default: wl.FieldTemplates.Add("@fN,,@fO,@fChT,@fVH_B,@fVH_R"); break; case 3: wl.FieldTemplates.Add("@fN,,@fE,@fO,@fO_B(20%),@fO_R"); wl.FieldTemplates.Add("@fChT,@fVH,@fVH_B(filled),@fVH_R"); break; } break;
                    case BlockType.Gasgenerator: switch (wl.style) { case 1: wl.FieldTemplates.Add("@fN,,Ice:,@fIa,@fE"); break; default: wl.FieldTemplates.Add("@fN,,@fE,/ O2:,@fOB(left),/ H2:,@fHB(left),Ice:,@fIa,@fF0_B(30%),@fF0_R"); break; case 3: wl.FieldTemplates.Add("@fN,,@fE,/ Bottles - O2:,@fOB(left),/ H2:,@fHB(left),Ice:,@fIa,(13)"); wl.FieldTemplates.Add("Cargo:(22),@fF0_M,@fF0_B(filled),@fF0_R"); break; case 4: wl.FieldTemplates.Add("@fN,,@fE"); wl.FieldTemplates.Add("Ice:(15),@fIa(13),Bottles:,@fOB,O2 / ,@fHB,H2,"); wl.FieldTemplates.Add("Cargo:(22),@fF0_M,@fF0_B(filled),@fF0_R"); break; } break;
                    case BlockType.Airvent: switch (wl.style) { case 1: wl.FieldTemplates.Add("@fN,,@fPz,/,@fAt,@fV_R"); break; default: wl.FieldTemplates.Add("@fN,,@fPz,/,@fAt,@fV_B(40%),@fV_R"); break; case 3: wl.FieldTemplates.Add("@fN,,Mode:,@fPz,/,@fAt,(13)"); wl.FieldTemplates.Add(",@fV_B(60%),@fV_R"); break; case 4: wl.FieldTemplates.Add("Air Vent,@fE,,Mode:,@fPz(left)"); wl.FieldTemplates.Add("@fAt,@fV_B(filled),@fV_R"); break; } break;
                    case BlockType.Assembler: switch (wl.style) { case 1: wl.FieldTemplates.Add("@fC,@fWM,@fN, ,@fW,@fWa(left)"); break; default: wl.FieldTemplates.Add("@fC,@fN, ,@fWa(0),@fW(left),@fWM"); break; case 3: wl.FieldTemplates.Add("@fC,@fN, ,@fWa,@fW(left)"); wl.FieldTemplates.Add("@fWM, ,Next:,@fNWa,@fNW(left)"); break; case 4: wl.FieldTemplates.Add("@fN, ,@fWM"); wl.FieldTemplates.Add(",Now:,@fWa,@fW(left)"); wl.FieldTemplates.Add(",Next:,@fNWa,@fNW(left)"); break; } break;
                    case BlockType.Battery: switch (wl.style) { case 1: wl.FieldTemplates.Add("@fN,,@fS_B(30%),@fS_R"); break; default: wl.FieldTemplates.Add("@fN, ,@fChT,@fChM,@fS_B(20%),@fS_R"); break; case 3: wl.FieldTemplates.Add("@fN,,@fChT,@fS,@fS_M,(" + zW + ")"); wl.FieldTemplates.Add("(" + zW + "),@fChM,@fS_B(filled),@fS_R"); break; case 4: wl.FieldTemplates.Add("@fN,@fS_M(0),,@fChT"); wl.FieldTemplates.Add("@fChM,,In:,@fI,Out:,@fO"); wl.FieldTemplates.Add("@fS,@fS_B(filled),@fS_R"); break; } break;
                    case BlockType.Reactor: switch (wl.style) { case 1: wl.FieldTemplates.Add("@fN,,@fE,/,@fO"); break; default: wl.FieldTemplates.Add("@fN, ,@fChT,@fO,@fO_B(20%),@fO_R"); break; case 3: wl.FieldTemplates.Add("@fN, ,Ingots:,@fIG,@fChT,@fE,(13)"); wl.FieldTemplates.Add("@fO,@fO_B(filled),@fO_R"); break; case 4: wl.FieldTemplates.Add("@fN,@fE, ,Time:,@fChT"); wl.FieldTemplates.Add("Stored:,@fS,,Ingots:,@fIG(24)"); wl.FieldTemplates.Add("@fO,@fO_B(filled),@fO_R"); break; } break;
                    case BlockType.Refinery: switch (wl.style) { case 1: wl.FieldTemplates.Add("@fC,@fN, ,@fE,/,@fW(28)"); break; default: wl.FieldTemplates.Add("@fC,@fN, ,@fWa(0),@fW(27 Left),@fV_B(20%),@fV_R"); break; case 3: wl.FieldTemplates.Add("@fC,@fN,,@fW(27),@fWa(23),Next:,@fNW(27),@fNWa(23),(13)"); wl.FieldTemplates.Add(",@fV_B(60%),@fV_R"); break; case 4: wl.FieldTemplates.Add("@fN,,@fW(27),@fWa(23)"); wl.FieldTemplates.Add(",Next:,@fNW(27),@fNWa(23)"); wl.FieldTemplates.Add("@fWM(25),@fV_B(filled),@fV_R"); break; } break;
                    case BlockType.Jumpdrive: switch (wl.style) { case 1: wl.FieldTemplates.Add("@fN,,@fSt,@fS_R"); break; case 3: wl.FieldTemplates.Add("@fN,,@fSt,/,@fChT,(" + zW + ")"); wl.FieldTemplates.Add("Input:(20),@fI,@fI_B(filled),@fI_R"); wl.FieldTemplates.Add("Stored:(20),@fS,@fS_B(filled),@fS_R"); break; default: wl.FieldTemplates.Add("@fN,,Status:,@fSt,/,@fChT,(" + zW + ")"); wl.FieldTemplates.Add("(20),@fS_B(filled),@fS_R"); break; } break;
                    case BlockType.Cargocontainer: switch (wl.style) { case 1: wl.FieldTemplates.Add("@fN,,@fF0_R"); break; case 3: wl.FieldTemplates.Add("@fN,,@fF0_M,(" + zW); wl.FieldTemplates.Add("@fF0,@fF0_B(filled),@fF0_R"); break; default: wl.FieldTemplates.Add("@fN,,@fF0_M(0),@fF0_B(40%),@fF0_R"); break; }; break;
                    case BlockType.Connector: switch (wl.style) { case 1: wl.FieldTemplates.Add("@fN,,@fSh"); break; case 3: wl.FieldTemplates.Add("@fN,,[,@fE,],@fDN(40)"); wl.FieldTemplates.Add("@fSt(50),=>,@fSh,,@fSct"); break; default: wl.FieldTemplates.Add("@fN,@fE,,@fSct,@fSt(0),@fSh"); break; }; break;
                }
            }
            void DEBUG(DP wl)
            {
            }
            void addSingleLine(DP wl, dyBlockDisplayable d, string s) { List<dyMultiDisplayLine> nmol; nmdls.Add(nmol = new List<dyMultiDisplayLine>()); nmol.Add(d.getMDLo(this, wl.with, s)); }
            void writeTitle_ifTrue(DP wl, string tu = "") { if (wl.is_Title) writeX(" ,<<<," + (tu == "" ? wl.titleString : tu) + ",>>>, "); }
            void writeHeadLine_ifTrue(DP wl) { if (wl.headLineString != "" && wl.is_HeadLine) writeX(wl.headLineString, wl.c); }
            void writeLine_ifTrue(DP wl, char c = '-', bool f = false) { if (f || wl.is_Line) writeX("@" + c + "(filled)", 1); }
            void dlLP(DP wl, bool li = true)
            {
                wl.dyn += wl.listDy.Count;
                if (wl.listDy.Count == 0)
                {
                    if (wl.dyn != 0) return;
                    if (checkError(wl, 0)) return;
                }
                sort_ifTrue(wl);
                List<List<dyMultiDisplayLine>> nnmdll = new List<List<dyMultiDisplayLine>>();
                var fos = true;
                int dpr = wl.listDy.Count / wl.c;
                if (dpr < 1) dpr = 1;
                else if ((dpr * wl.c) != wl.listDy.Count) dpr++;
                int di = 0;
                int cos = 0;
                dyMultiDisplayLine cstr;
                for (int c = 0; c < wl.c; c++)
                {
                    for (int r = 0; r < dpr; r++)
                    {
                        for (int f = 0; f < wl.FieldTemplates.Count; f++)
                        {
                            if (di < wl.listDy.Count) cstr = wl.listDy[di].getMDLo(this, wl.with, f >= wl.FieldTemplates.Count ? "@-(" + (wl.with - 2.58332f) + ")" : wl.FieldTemplates[f]);
                            else cstr = getMDLo(this, wl.with, " ");
                            if (fos) nnmdll.Add(new List<dyMultiDisplayLine>());
                            if (cstr != null) nnmdll[cos++].Add(cstr);
                        }
                        di++;
                    }
                    fos = false;
                    cos = 0;
                }
                writeHeadLine_ifTrue(wl);
                foreach (var l1 in nnmdll) nmdls.Add(l1);
                wl.listDy.Clear();
                if (li) writeLine_ifTrue(wl);
            }
            void SPEED(DP wl) { var sp = "@"; var c = 'G'; int y = wl.style; if (wl.ifOption("KMH")) { c = 'k'; sp += "KMH"; } else if (wl.ifOption("MPH")) { c = 'm'; sp += "MPH"; } sp += "SPEED"; add_to_VC(c).M = wl.ms; writeX("Speed:" + (y == 4 ? "(30)" : "") + ",," + sp + "(0)"); if (y == 4) writeX("Maxspeed:(30),," + sp + "_M(0)"); if (y > 1) writeX(sp + "_B(filled)" + (y > 2 ? "," + sp + "_R" : "")); writeLine_ifTrue(wl); }
            bool Ft(DP wl, dyBlock p)
            {
                var g = (GasTank)p;
                if (wl.ifOption(g.medium) || wl.is_All) return true;
                else return false;
            }
            void aMY(DP wl, dyBlockDisplayable d, BlockType type) { if (wl.ifOption("ENERGY") || wl.ifOption("CARGO")) { wl.FieldTemplates.Add("@fN," + (wl.ifOption("ONOFF") ? ",@fE,(" + (zW + 1.6f) + ")" : (wl.is_Line ? Cfilled : ""))); foreach (var c in getPowerCargoOptions(wl)) aYs(wl, d, c); } else addFieldTemplates_Block(wl, type); }
            void MDILP(DP wl, BlockType type, Func<DP, dyBlock, bool> F = null)
            {
                var dll = getDyBlockList(type);
                writeTitle_ifTrue(wl, wl.cm);
                foreach (var g in dll.Keys)
                {
                    if (wl.ifDisplayGrid(g))
                    {
                        foreach (dyBlock dl in dll[g])
                            if (wl.ifFilter(dl.Name) && (F == null ? true : F(wl, dl)))
                                wl.listDy.Add(dl);
                        if (wl.FieldTemplates.Count == 0 && wl.listDy.Count > 0) aMY(wl, wl.listDy[0], type);
                        if (wl.ifOption(oGN))
                        {
                            if (wl.listDy.Count > 0)
                            {
                                if (wl.ifOption(oGN) && wl.is_Title) writeX(g.CustomName + " :, ");
                                wl.dyn += wl.listDy.Count;
                                dlLP(wl);
                            }
                        }
                    }
                }
                dlLP(wl);
            }
            void DOCKINGS(DP wl)
            {
                wl.listDy.Clear();
                setOneCol(wl, false);
                foreach (dyConnector d in docks) if (wl.ifFilter(d.dockName)) wl.listDy.Add(d); if (checkError(wl, wl.listDy.Count)) return; writeTitle_ifTrue(wl, "Dockstations"); sort_ifTrue(wl); foreach (dyConnector dc in wl.listDy) { addSingleLine(wl, dc, "@fDN" + (wl.is_Line ? "," + Cfilled : ",")); addSingleLine(wl, dc, "docked:(24),@fSh,,@fSct,@fSt(0)"); if (dc.docked_connector != null) { var g = dc.docked_connector.CubeGrid; dyDisplayableVariable pv = dyDisplayableVariable.getPV(g, "Damageblocks", false); if (pv != null) addSingleLine(wl, pv, "Damage:(24),@fH,@fH_B(filled),@fH_R"); pv = dyDisplayableVariable.getPV(g, "Batterys", false); if (pv != null) addSingleLine(wl, pv, "Batterys:(24),@fS,@fS_B(filled),@fS_R"); pv = dyDisplayableVariable.getPV(g, "Containers", false); if (pv != null) addSingleLine(wl, pv, "Cargo:(24),@fVC,@fVC_B(filled),@fVC_R"); pv = dyDisplayableVariable.getPV(g, "OxygenTanks", false); if (pv != null) addSingleLine(wl, pv, "Oxygen:(24),@fVO,@fVO_B(filled),@fVO_R"); pv = dyDisplayableVariable.getPV(g, "HydrogenTanks", false); if (pv != null) addSingleLine(wl, pv, "Hydrogen:(24),@fVH,@fVH_B(filled),@fVH_R"); } }
                if (wl.listDy.Count > 0) writeLine_ifTrue(wl);
            }
            void w_cpc(string title, string type, DP wl, string sty = "", char tidx = ' ', string fla = "")
            {
                char tc = wl.ifGridOption() ? 'g' : 's';
                if (tidx != ' ') tc = tidx;
                bool hw = false;
                if (sty != "" && wl.FieldTemplates.Count == 0)
                {
                    addFieldTemplates_NonBlock(wl, sty);
                    wl.FieldTemplates[0] = fla + wl.FieldTemplates[0];
                }
                foreach (dyDisplayableVariable pv in pvl)
                {
                    if (pv.isID("" + type[0], tc) && wl.ifFilter(pv.Name))
                    {
                        if (!hw) writeTitle_ifTrue(wl, title);
                        hw = true;
                        if (wl.ifDisplayGrid(pv.pcg)) wl.listDy.Add(pv);
                    }
                }
                dlLP(wl);
            }
            void hoc(DP wl, string tty, string f)
            {
                var gty = wl.cm;
                var n = wl.filter == "*" && !wl.is_All;
                if (n)
                {
                    wl.sF(f);
                    wl.is_Title = false;
                }
                addFieldTemplates_HOXY(wl);
                if (!n && !wl.FieldTemplates[0].StartsWith("@fN")) wl.FieldTemplates[0] = "@fN," + wl.FieldTemplates[0];
                if (!wl.is_Title && !wl.FieldTemplates[0].StartsWith(gty)) wl.FieldTemplates[0] = gty + "," + wl.FieldTemplates[0];
                w_cpc(gty, tty, wl);
            }
            void GAS(DP wl, string gty, string sty, string tty) { addFieldTemplates_NonBlock(wl, sty); if (!wl.ifOption(oGN)) wl.is_Title = false; if (!wl.is_Title && !wl.FieldTemplates[0].StartsWith(gty)) wl.FieldTemplates[0] = gty + "," + wl.FieldTemplates[0]; w_cpc(gty, tty, wl); }
            void ENERGYP(DP wl)
            {
                var s = "SolarPanels,WindTurbines,HydrogenEngines,Reactors,Batterys,Total";
                if (wl.grid != null) wl.titleString += " " + wl.grid.CustomName;
                if (wl.is_Table)
                {
                    addPowerTable(wl, s);
                    return;
                }
                setOneCol(wl);
                writeTitle_ifTrue(wl);
                foreach (var s1 in s.Split(',')) aPL(wl, dyDisplayableVariable.getPV(wl.grid, s1, false));
                writeLine_ifTrue(wl);
            }
            void ENERGIE(DP wl)
            {
                if (wl.is_Table)
                {
                    addPowerTable(wl);
                    return;
                }
                wl.dyn = 1;
                foreach (char c in getPowerCargoOptions(wl))
                {
                    if (c == 'S') w_cpc("Power stored", "S", wl, "PCS");
                    if (c == 'M') w_cpc("Power available", "M", wl, "PCA");
                    if (c == 'E') w_cpc("Power output", "E", wl, "PCO");
                    if (c == 'I') w_cpc("Power consumption", "I", wl, "PCC");
                    wl.FieldTemplates.Clear();
                }
                if (wl.dyn == 1)
                {
                    wl.error = 1;
                    writeError(wl);
                }
            }
            string getPowerCargoOptions(DP wl)
            {
                string o = "";
                foreach (string s in wl.options)
                {
                    if (s == "STORED") o += 'S';
                    else if (s == "AVAILABLE") o += 'M';
                    else if (s == "OUTPUT") o += 'E';
                    else if (s == "CONSUMPTION") o += 'I';
                    else if (s == "CARGO") o += "F01";
                }
                return o == "" ? "SMEI" : o;
            }
            string atPh(DP wl, string os, bool gn) { var sr = gn ? "Grid," : ""; sr += ",Type"; foreach (char c in os) { sr += ",|,"; if (c == 'S') sr += "Stored"; else if (c == 'M') sr += "Avali."; else if (c == 'E') sr += "Output"; else if (c == 'I') sr += "Input"; switch (wl.style) { case 1: sr += "(" + zW + ")"; break; case 2: sr += "(" + pW + ")"; break; case 3: sr += "(" + pW + "),(" + zW + ")"; break; default: sr += "(" + pW + "),(" + pW + ")"; break; } } return sr; }
            string atPs(DP wl, dyDisplayableVariable p, string os) { var sr = ""; var ls = new List<string> { ",|,(" + zW + ")", ",|,(" + pW + ")", ",|,(" + pW + "),(" + zW + ")", ",|,(" + pW + "),(" + pW + ")" }; foreach (char c in os) { var s2 = ""; if (p.getVC(c, false) != null) { if (c == 'S') s2 = "@fS"; else if (c == 'M') s2 = "@fM"; else if (c == 'E') s2 = "@fO"; else if (c == 'I') s2 = "@fI"; switch (wl.style) { case 1: sr += ",|," + s2 + "_R"; break; case 2: sr += ",|," + s2; break; case 3: sr += ",|," + s2 + "," + s2 + "_R"; break; default: sr += ",|," + s2 + "," + s2 + "_M"; break; } } else sr += ls[wl.style - 1]; } return sr; }
            string gTD(string s) { switch (s) { case "Z+": return "Forward"; case "Z-": return "Backward"; case "X+": return "Right"; case "X-": return "Left"; case "Y+": return "Upward"; case "Y-": return "Downward"; } return "-?-"; }
            void aTD(DP wl, string t = "", string f = "")
            {
                if (wl.FieldTemplates.Count > 0) return;
                if (t != "" && wl.is_Title) wl.FieldTemplates.Add(",<<<," + t + ",>>>,");
                if (wl.sortMode == "" && f == "") f = "X+X-Y+Y-Z+Z-";
                else f = wl.sortMode;
                int i = 0;
                while (i < f.Length)
                {
                    var s = "" + f[i++] + f[i++];
                    aYp(wl, "@" + s + "THRUST", gTD(s) + "(30),[" + s + "],");
                }
                if (wl.is_Line) wl.FieldTemplates.Add(Cfilled);
            }
            void addPowerTable(DP wl, string si = "")
            {
                setOneCol(wl);
                var powerValueList = new List<dyDisplayableVariable>();
                dyDisplayableVariable powerTotal = null;
                char t = 's';
                if (wl.ifGridOption() && si == "") t = 'g';
                var o = getPowerCargoOptions(wl);
                if (si == "")
                {
                    foreach (dyDisplayableVariable p in pvl)
                        if (p.isID(o, t) && wl.ifFilter(p.Name))
                        {
                            if (!wl.ifDisplayGrid(p.pcg)) continue;
                            powerValueList.Add(p);
                        }
                    if (wl.is_Sort) powerValueList.Sort();
                }
                else
                {
                    var si2 = si.Split(',');
                    foreach (var s in si2)
                    {
                        dyDisplayableVariable p = dyDisplayableVariable.getPV(null, s, false);
                        if (s == "Total") powerTotal = p;
                        else if (p != null) powerValueList.Add(p);
                    }
                }
                if (checkError(wl, powerValueList.Count)) return;
                wl.headLineString = atPh(wl, o, t == 'g');
                if (powerValueList.Count < 2 && powerValueList[0].pcg != null) wl.titleString += " " + powerValueList[0].pcg.CustomName;
                writeTitle_ifTrue(wl);
                writeHeadLine_ifTrue(wl);
                writeLine_ifTrue(wl);
                var s1 = ((wl.ifOption(oGN) && powerValueList.Count > 1) ? ("@fG,") : "") + ",@fN";
                foreach (dyDisplayableVariable p in powerValueList)
                {
                    addSingleLine(wl, p, s1 + atPs(wl, p, o));
                }
                writeLine_ifTrue(wl);
                if (powerTotal != null)
                {
                    addSingleLine(wl, powerTotal, s1 + atPs(wl, powerTotal, o));
                    writeLine_ifTrue(wl, '=');
                }
            }
            string aPL1(string s) { return "," + s + ",/," + s + "_M"; }
            string aPL2(string s) { return "," + s + "," + s + "_M," + s + "_R"; }
            string aPL3(string s) { return "," + s + "," + s + "_B(20%)," + s + "_R"; }
            string aPL4(string s) { return "," + s + "," + s + "_M," + s + "_B(20%)," + s + "_R"; }
            void aPL(DP wl, dyDisplayableVariable p)
            {
                if (p == null) return;
                var sp = new List<string>();
                var fi = false;
                var fs = false;
                if (null != p.getVC('I', false)) fi = true;
                if (null != p.getVC('S', false)) fs = true;
                switch (wl.style)
                {
                    case 1:
                        sp.Add("@fN,,out:" + aPL1("@fO"));
                        if (fi) sp.Add(",in:" + aPL1("@fI"));
                        if (fs) sp.Add(",stored:" + aPL1("@fS"));
                        break;
                    case 2:
                        sp.Add("@fN,,Output:(18)" + aPL3("@fO"));
                        if (fi) sp.Add(",Input:(18)" + aPL3("@fI"));
                        if (fs) sp.Add(",Stored:(18)" + aPL3("@fS"));
                        break;
                    case 3:
                        sp.Add("@fN,,Output:(18)" + aPL2("@fO"));
                        if (fi) sp.Add(",Input:(18)" + aPL2("@fI"));
                        if (fs) sp.Add(",Stored:(18)" + aPL2("@fS"));
                        break;
                    default:
                        sp.Add("@fN" + (wl.is_Line ? "," + Cfilled : ","));
                        sp.Add(",Output:(18)" + aPL4("@fO"));
                        if (fi) sp.Add(",Input:(18)" + aPL4("@fI"));
                        if (fs) sp.Add(",Stored:(18)" + aPL4("@fS"));
                        break;
                }
                foreach (string s in sp) addSingleLine(wl, p, s);
            }
            bool cc(DP wl, string s, string s1) { if (wl.options.Contains("-" + s1)) return false; if (wl.is_All || s == s1) return true; return false; }
            void INVENTORY(DP wl, Dictionary<MyItemType, dyInventory> invl)
            {
                List<inventoryDisplayItems> idil = new List<inventoryDisplayItems>();
                inventoryDisplayItems blockItemList = null;
                if (wl.ifOption("BLOCK"))
                {
                    blockItemList = new inventoryDisplayItems("Inventory");
                    idil.Add(blockItemList);
                }
                if (wl.is_All || wl.options.Count == 0)
                {
                    wl.options.Add("ALL");
                    wl.is_All = true;
                }
                foreach (string bs in wl.options)
                {
                    if (bs == "UTILS") { inventoryDisplayItems idix = new inventoryDisplayItems("Utils", IG_Tools); idix.AddTag("Weapons"); idix.AddTag(IG_Datas); idix.AddTag(IG_HBottles); idix.AddTag(IG_OBottles); idix.AddTag(IG_Kits); idil.Add(idix); }
                    if (bs == "BOTTLES") { inventoryDisplayItems idix = new inventoryDisplayItems("Bottles", IG_HBottles); idix.AddTag(IG_OBottles); idil.Add(idix); }
                    if (cc(wl, bs, "ORE")) addidi(blockItemList, "Ore", idil);
                    if (cc(wl, bs, "INGOT")) addidi(blockItemList, "Ingot", idil);
                    if (cc(wl, bs, "CRUSHED")) addidi(blockItemList, "Crushed Materials", "Crushed", idil);
                    if (cc(wl, bs, "PURIFIED")) addidi(blockItemList, "Purified Materials", "Purified", idil);
                    if (cc(wl, bs, "COMPONENT")) addidi(blockItemList, "Components", "Component", idil);
                    if (cc(wl, bs, "TOOLS")) addidi(blockItemList, "Tools", IG_Tools, idil);
                    if (cc(wl, bs, "WEAPONS")) addidi(blockItemList, "Weapons", idil);
                    if (cc(wl, bs, "AMMO")) addidi(blockItemList, "Ammo", IG_Ammo, idil);
                    if (cc(wl, bs, "OIL")) addidi(blockItemList, "Oilproducts", "Oil", idil);
                    if (cc(wl, bs, "DATAPADS")) addidi(blockItemList, "Data Pads", IG_Datas, idil);
                    if (cc(wl, bs, "H-BOTTLES")) addidi(blockItemList, "Hydrogen Bottles", IG_HBottles, idil);
                    if (cc(wl, bs, "O-BOTTLES")) addidi(blockItemList, "Oxygen Bottles", IG_OBottles, idil);
                    if (cc(wl, bs, "KITS")) addidi(blockItemList, "Consumable Kits", IG_Kits, idil);
                    if (cc(wl, bs, "FOOD")) addidi(blockItemList, "Foodproducts", "Food", idil);
                    if (cc(wl, bs, "CASH")) addidi(blockItemList, "Space Credits", IG_Cash, idil);
                    if (bs == "ALL") break;
                }
                addInventarItems(wl, idil, invl);
                foreach (inventoryDisplayItems idix in idil)
                {
                    if (idix.idl.Count > 0)
                    {
                        writeTitle_ifTrue(wl, idix.Name + (wl.titleString == "" ? "" : " " + wl.titleString));
                        wl.listDy = idix.idl;
                        dlLP(wl);
                    }
                }
                if (idil.Count == 0) wl.error = 2;
                else if (wl.dyn == 0) wl.error = 1;
                writeError(wl);
            }
            class inventoryDisplayItems
            {
                public string Name = "";
                public List<string> tags = new List<string>();
                public List<dyBlockDisplayable> idl = new List<dyBlockDisplayable>();
                public inventoryDisplayItems(string iN)
                {
                    Name = iN;
                }
                public inventoryDisplayItems(string iN, string iT)
                {
                    Name = iN;
                    AddTag(iT);
                }
                public void AddTag(string aT)
                {
                    tags.Add(aT);
                }
            }
            static void addidi(inventoryDisplayItems blockItemList, string name, List<inventoryDisplayItems> idil)
            {
                addidi(blockItemList, name, name, idil);
            }
            static void addidi(inventoryDisplayItems blockItemList, string name, string tag, List<inventoryDisplayItems> idil)
            {
                if (blockItemList == null) idil.Add(new inventoryDisplayItems(name, tag));
                else blockItemList.AddTag(tag);
            }
            void addInventarItems(DP wl, List<inventoryDisplayItems> idil, Dictionary<MyItemType, dyInventory> dll)
            {
                var z = wl.is_Zero;
                foreach (dyInventory iit in dll.Values)
                {
                    if (wl.ifFilter(iit.Name))
                    {
                        if (z || iit.getAmount() > 0)
                        {
                            foreach (var idix in idil)
                            {
                                if (idix.tags.Contains(iit.sy.typeString)) idix.idl.Add(iit);
                            }
                        }
                    }
                }
            }
        }
    }
}
