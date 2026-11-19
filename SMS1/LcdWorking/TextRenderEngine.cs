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
            public List<List<dyMultiDisplayLine>> displayLines = new List<List<dyMultiDisplayLine>>();
            public List<DP> displayCommands = new List<DP>();
            DateTime cdsdt = DateTime.Now;
            public string pageText = "", displayDefinition = "", lastDisplayDefinition = "";
            public List<string> pages = new List<string>();
            public float with, iwith, dif_wm, dif_wp, fontsize, wconst, fconst = 17.82f;
            // the phases of RenderStep, in this order (renderPhase++ goes to the next one)
            public enum RenderPhase { Start, ReadCommands, BuildLines, CheckPanel, ResetPages, RenderLines, Finish }
            public RenderPhase renderPhase = RenderPhase.Start;
            public int icount, stepIndex, run_count, maxlines = 1, newlines, pnum = 1, plines, slines, maxl = 40, minl = 5;
            public bool booting = true, lfix = true;
            public TextRenderEngine() { addChangeType = ChangeType.Display_add; nameChangeType = ChangeType.Display_name; curlines.sD(0); MDIs.Add("@fCL", curlines); ic.addm(MDIs); vcl.Add(ic); addChange(); }
            public override void Display()
            {
                // für speed variablen
                foreach (var c in "Gkm")
                {
                    VC v = getVC(c);
                    if (v != null) v.C = shipSpeed;
                }
                base.Display();
            }
            bool checkError(DP command, int lc = -1)
            {
                if (command.error == 0 && lc != -1 && lc == 0) command.error = 1;
                if (command.error == 0) return false;
                writeError(command);
                return true;
            }
            void writeError(DP command)
            {
                if ((command.ifOption("NOERROR") && command.error < 0) || (command.ifOption("NOMESSAGE") && command.error > 0)) command.error = 0;
                if (command.cm.Contains("Damag")) command.error = 40;
                string et = "";
                switch (command.error)
                {
                    case 0: return;
                    case -1: et = "dock [" + command.dock + "] not found!"; break;
                    case 1: et = ""; if (command.ifFilter()) et = " [" + command.filter + "],"; et += " no items found!"; break;
                    case 2: et = " no valid options"; break;
                    case 3: et = "dock [" + command.dock + "] no ship is docking!"; break;
                    case 40: et = " no damaged blocks found"; break;
                    default: et = "?"; break;
                }
                if (command.error != 0)
                {
                    if (command.error < 40) writeX(command.cm + (command.error < 0 ? ": error->," : ": warning->,") + et + ",");
                    else writeX(et + ",");
                    command.error = 0;
                }
            }
            public virtual void clearWriteValues() { }
            public virtual void checkWriteToPanel() { }
            public abstract void writeToPanel();
            public virtual void addEndOfLineStr() { pageText += "\n"; }
            void setOneCol(DP command, bool ns = true) { command.with = with; command.c = 1; if (ns == true && command.is_Sort) command.is_Sort = false; }
            void sort_ifTrue(DP command) { if (command.is_Sort) { sortColumn = command.sortMode; command.listDy.Sort(); } }
            public void writeX(string arg, float i = 1)
            {
                if (arg == "") return;
                List<dyMultiDisplayLine> nnmdll = new List<dyMultiDisplayLine>();
                for (int x = 0; x < (i <= 4 ? i : 1); x++) nnmdll.Add(getMDLo(this, i <= 4 ? with / i : i, arg));
                displayLines.Add(nnmdll);
            }
            public void setLinesBounds(int l)
            {
                if (l < minl) l = minl;
                if (l > maxl) l = maxl;
                dif_wp = 0;
                dif_wm = 0;
                if (maxlines == l) return;
                maxlines = l;
                fontsize = fconst / l;
                with = wconst / fontsize;
            }
            bool AdjustWith() { int omax = maxlines; int pml = maxlines; if (lfix) if (newlines == maxlines) return false; else { setLinesBounds(newlines); newlines = maxlines; return true; } int nl = 0; if (dif_wm < 0) { nl = 1 + (int)(fconst / (wconst / (with + Math.Abs(dif_wm)))); if (maxlines == nl) nl++; } else if (dif_wp > 30) { nl = 1 + (int)(fconst / (wconst / (with - dif_wp))); } else if ((maxlines * pnum) < slines) nl = (slines + 1) / pnum; if (nl > 0) { if ((nl * pnum) < slines) { nl = (slines / pnum); nl += slines - (nl * pnum); } setLinesBounds(nl); if (omax != maxlines) return true; } return false; }
            public bool RenderStep()
            {
                if (renderPhase > RenderPhase.ReadCommands && lastDisplayDefinition != displayDefinition) { renderPhase = RenderPhase.ReadCommands; slines = 0; dif_wp = 0; dif_wm = 0; lastDisplayDefinition = displayDefinition; }
                do
                {
                    switch (renderPhase)
                    {
                        case RenderPhase.Start: if (run_count < 1) { pages.Clear(); pages.Add("Init SMS ..."); run_count++; writeToPanel(); return true; } else renderPhase++; break;
                        case RenderPhase.ReadCommands: displayCommands.Clear(); foreach (var s in displayDefinition.Split('\n')) displayCommands.Add(new DP(with, s, tsg)); stepIndex = 0; displayLines.Clear(); Display(); clearWriteValues(); renderPhase++; break;
                        case RenderPhase.BuildLines:
                            for (int c = stepIndex; c < displayCommands.Count; c++, stepIndex++)
                            {
                                if (IsBudgetUsedUp()) return false; DP command = displayCommands[c]; sortColumn = command.sortMode; command.dyn = 0; int y = command.style;
                                switch (command.cm)
                                {
                                    case "Displays": command.addBlockChangeTypes(ChangeType.Display_add, ChangeType.Display_name); foreach (var d in sms_disp) if (command.ifFilter(d.Name)) command.listDy.Add(d); writeTitle_ifTrue(command, "Displays"); sort_ifTrue(command); command.headLineString = "Load(15% left),Name,,Lines,Instr.(31)"; command.FieldTemplates.Add("@fIC_B(15%),@fN,,@fCL,@fIC"); WriteList(command); break;
                                    case "Energy_R": setOneCol(command, false); writeX("Energyrange:,,@fChT(left)"); writeLine_ifTrue(command); break;
                                    case "Speed": SPEED(command); break;
                                    case "Gravity": var s1 = ",,"; var s2 = ""; if (y == 4) { s1 = "(60%),"; s2 = "(0),"; } if (y < 3 || y == 4) writeX("Natural Gravity:" + s1 + "@fgN" + s2); if (y > 1) writeX("Artificial Gravity:" + s1 + "@fgA" + s2); writeLine_ifTrue(command); break;
                                    case "Mass": writeX("TotalMass:" + (y > 3 ? "(30)" : "") + ",,@fsMt"); if (y > 1) writeX("CargoMass:" + (y > 3 ? "(30)" : "") + ",,@fsMc"); if (y > 2) writeX("ShipMass:" + (y > 3 ? "(30)" : "") + ",,@fsMb"); writeLine_ifTrue(command); break;
                                    case "Icload": setOneCol(command, false); writeX("ICLoad:,,@fIC"); break;
                                    case "Writex": writeX(command.FieldTemplates[0], command.with); break;
                                    case "Writex_I":
                                        List<dyMultiDisplayLine> nnmdll = new List<dyMultiDisplayLine>();
                                        nnmdll.Add(getMDLo(this, iwith, command.FieldTemplates[0]));
                                        displayLines.Add(nnmdll);
                                        break;
                                    case "Line": var s = '-'; if (command.options.Count > 0 && command.options[0].Length > 0) s = command.options[0][0]; writeLine_ifTrue(command, s, true); break;
                                    case "Resources":
                                        command.addChangeType(ChangeType.Resource_add);
                                        if (!command.is_Zero) command.addChangeType(ChangeType.Resource_change_null);
                                        foreach (dyResourceOreIngot o in oiil.Values) if (command.is_Zero || (o.oa > 0f || o.ia > 0f) || (o.oa == -1)) command.listDy.Add(o);
                                        addFieldTemplates_NonBlock(command, "RES");
                                        writeTitle_ifTrue(command, "Resources");
                                        WriteList(command);
                                        break;
                                    case "Inventory":
                                        command.addChangeType(ChangeType.Inventar_add);
                                        if (!command.is_Zero) command.addChangeType(ChangeType.Inventar_change_null);
                                        if (command.is_Sort && command.sortMode != "BACK" && command.sortMode != "") command.addChangeType(ChangeType.Inventar_change_amount);
                                        addFieldTemplates_NonBlock(command, "INV");
                                        if (command.ifOption("GRID"))
                                        {
                                            foreach (IMyCubeGrid g in gridInventoryItems.Keys)
                                                if (command.ifDisplayGrid(g))
                                                {
                                                    command.titleString = g.CustomName;
                                                    INVENTORY(command, gridInventoryItems[g]);
                                                    break;
                                                }
                                            writeError(command);
                                        }
                                        else
                                            INVENTORY(command, invitemsg);
                                        break;
                                    case "Thrust": command.addChangeType(ChangeType.PV_add); aTD(command, "@fN" + (command.ifGridOption() ? ",@fG" : "")); var x = command.ifOption("SUMMARY") ? "ary" : "Thrust"; foreach (dyDisplayableVariable pv in pvl) if (command.ifFilter(pv.Name) && pv.isID("xXyYzZ", 's') && pv.Name.EndsWith(x)) command.listDy.Add(pv); WriteList(command, false); break;
                                    case "Thrust_E": command.addChangeType(ChangeType.PV_add); AddValueTemplates("Thrusterefficiency total", command, "@fET"); foreach (dyDisplayableVariable pv in pvl) if (command.ifFilter(pv.Name) && pv.isID("t", 's')) command.listDy.Add(pv); WriteList(command); break;
                                    case "Thrust_W": command.addChangeType(ChangeType.PV_add); aTD(command, "Weight to Thrust"); command.listDy.Add(dyDisplayableVariable.getPV(null, "WtoT")); WriteList(command, false); break;
                                    case "Energy_P": command.titleString = "Powerproducing"; command.addChangeType(ChangeType.PV_add); ENERGYP(command); break;
                                    case "Energy": command.titleString = "Poweroverview"; command.addChangeType(ChangeType.PV_add); ENERGIE(command); break;
                                    case "Cargo": command.addChangeType(ChangeType.PV_add); WriteVolumeSummary(command, "C", "Containers"); break;
                                    case "Solar": command.addChangeType(ChangeType.PV_add); command.sF("Solar"); WriteVariableGroup(command, "Solarpanels", "SOLAR", "E"); break;
                                    case "Wind": command.addChangeType(ChangeType.PV_add); command.sF("Wind"); WriteVariableGroup(command, "WindTurbines", "SOLAR", "E"); break;
                                    case "Invman":
                                        command.addChangeType(ChangeType.Cargouse_add);
                                        if (command.is_Sort && command.sortMode != "BACK" && command.sortMode != "") command.addChangeType(ChangeType.Cargouse_change_amount);
                                        addFieldTemplates_NonBlock(command, "HOXY");
                                        foreach (dyCargouse cuse in cargouseList) if (command.ifFilter(cuse.Name)) command.listDy.Add(cuse);
                                        writeTitle_ifTrue(command, "Inventorymanager");
                                        WriteList(command);
                                        break;
                                    case "Hydrogen": command.addChangeType(ChangeType.TankHydrogen_add); WriteVolumeSummary(command, "H", "Tanks"); break;
                                    case "Oxygen": command.addChangeType(ChangeType.TankOxygen_add); WriteVolumeSummary(command, "O", "Tanks"); break;
                                    case "Tanks":
                                        if (!command.ifOption("HYDROGEN") && !command.ifOption("OXYGEN")) command.is_All = true;
                                        if (command.is_All || command.ifOption("HYDROGEN")) { command.addBlockChangeTypes(ChangeType.TankHydrogen_add, ChangeType.TankHydrogen_name); }
                                        if (command.is_All || command.ifOption("OXYGEN")) { command.addBlockChangeTypes(ChangeType.TankOxygen_add, ChangeType.TankOxygen_name); }
                                        WriteBlockList(command, BlockType.Tank, Ft);
                                        break;
                                    case "Dockings": command.addChangeType(ChangeType.PV_add); command.addBlockChangeTypes(ChangeType.Dockings_add, ChangeType.Dockings_name); DOCKINGS(command); break;
                                    case "Damage_B": command.addBlockChangeTypes(ChangeType.Damage_add, ChangeType.Damage_name); command.titleString = "Damaged Blocks"; WriteBlockList(command, BlockType.Damage); break;
                                    case "Damage": command.addChangeType(getChangeStat(ChangeType.Damage_add)); AddValueTemplates(command, "@fH", "@fG,"); WriteVariableGroup(command, "Damage", " ", "h"); break;
                                    case "Oreprio":
                                        command.addChangeType(ChangeType.Ingotprio_add);
                                        command.addChangeType(ChangeType.Ingotprio_set_null);
                                        if (ingotprio.Count == 0) break;
                                        if (!command.is_Sort)
                                        {
                                            command.is_Sort = true;
                                            command.sortMode = "AMOUNT";
                                        }
                                        if (command.is_Sort && command.sortMode != "BACK" && command.sortMode != "") command.addChangeType(ChangeType.Ingotprio_change_amount);
                                        addFieldTemplates_NonBlock(command, "INV");
                                        var opls = ingotprio.Keys.ToArray();
                                        for (int i = 0; i < opls.Length; i++)
                                        {
                                            foreach (IPrio p in ingotprio[opls[i]]) if (p.amountVC.C > 0) command.listDy.Add(p);
                                            writeTitle_ifTrue(command, "Orepriority " + opls[i]);
                                            if (command.listDy.Count > 0) WriteList(command);
                                            else writeX(",no current prio,");
                                        }
                                        break;
                                    case "": writeX("(filled)"); break;
                                    default:
                                        // the block lists (connectors, batterys, ...): command -> block type and change type
                                        var blockList = Array.IndexOf(BlockListCommands, command.cm);
                                        if (blockList >= 0)
                                        {
                                            var add = BlockListChangeTypes[blockList];
                                            // the name change type follows the add change type in ChangeType
                                            command.addBlockChangeTypes(add, add + 1);
                                            WriteBlockList(command, BlockListTypes[blockList]);
                                        }
                                        else writeX("Command '" + command.cm + "' unknow,");
                                        break;
                                }
                            }
                            renderPhase++; break;
                        case RenderPhase.CheckPanel: checkWriteToPanel(); cdsdt = DateTime.Now; renderPhase++; break;
                        case RenderPhase.ResetPages: pageText = ""; pages.Clear(); stepIndex = 0; slines = 0; plines = 0; renderPhase++; break;
                        case RenderPhase.RenderLines:
                            for (int i = stepIndex; i < displayLines.Count; i++, stepIndex++)
                            {
                                if (IsBudgetUsedUp()) return false;
                                float cfx = 10000;
                                foreach (dyMultiDisplayLine ndl in displayLines[i])
                                {
                                    pageText += ndl.getText(this);
                                    if (cfx > ndl.cfx) cfx = ndl.cfx;
                                }
                                cfx *= displayLines[i].Count;
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
                                    pages.Add(pageText);
                                    pageText = "";
                                    plines = 0;
                                }
                            }
                            if (pageText != "") pages.Add(pageText);
                            renderPhase++;
                            break;
                        case RenderPhase.Finish: run_count++; if (AdjustWith()) { renderPhase = RenderPhase.ReadCommands; return true; } writeToPanel(); dif_wp = 0; dif_wm = 0; ic.C = icount; ic.M = instructionLimit; icount = 0; curlines.sD(maxlines); stepIndex = 0; renderPhase = RenderPhase.ResetPages; Refresh(); foreach (DP w in displayCommands) if (w.checkChangeStats(cdsdt)) { renderPhase = RenderPhase.ReadCommands; } return true;
                    }
                } while (!IsBudgetUsedUp()); return true;
            }
            void AddValueTemplates(DP command, string sC, string s = "@fN,@fG,")
            {
                switch (command.style)
                {
                    case 1:
                        command.FieldTemplates.Add(s + "," + sC + "(0)," + sC + "_R");
                        break;
                    default:
                        command.FieldTemplates.Add(s + "," + sC + "(0)," + sC + "_M," + sC + "_B(20%)," + sC + "_R");
                        break;
                    case 3:
                        command.FieldTemplates.Add(s + "," + sC + "(0)," + sC + "_M,(19)");
                        command.FieldTemplates.Add("," + sC + "_B(60%)," + sC + "_R");
                        break;
                    case 4:
                        command.FieldTemplates.Add(s + "," + sC + "(0)," + sC + "_R");
                        command.FieldTemplates.Add("," + sC + "_B(70%)");
                        break;
                }
            }
            void AddValueTemplates(string t, DP command, string sC, string s = "@fN,@fG,") { if (command.FieldTemplates.Count > 0) return; writeTitle_ifTrue(command, t); AddValueTemplates(command, sC, s); }
            Dictionary<char, string> Y = new Dictionary<char, string> { { 'F', "Cargo" }, { '0', "Cargo in" }, { '1', "Cargo out" }, { 'S', "Stored" }, { 'M', "Available" }, { 'E', "Output" }, { 'I', "Input" } };
            void AddPowerCargoTemplate(DP command, dyBlockDisplayable d, char c) { VC v = d.getVC(c, false); if (v != null) { var sv = v.gFs(); command.FieldTemplates.Add(Y[c] + ":(30)," + sv + "," + sv + "_B(filled)," + sv + "_R"); } }
            void addFieldTemplates_HOXY(DP command)
            {
                var c = command.cm[0];
                if (c == 'I') c = 'C';
                switch (command.style)
                {
                    case (1): command.FieldTemplates.Add("@fG,,@fV" + c + "(0),@fV" + c + "_M(0),@fV" + c + "_R"); break;
                    case (2): command.FieldTemplates.Add("@fG,,@fV" + c + ",@fV" + c + "_M,@fV" + c + "_B(28%),@fV" + c + "_R"); break;
                    case (4): command.FieldTemplates.Add("@fG,,@fV" + c + "(0)"); command.FieldTemplates.Add("@fV" + c + "_B(filled),@fV" + c + "_R"); break;
                    default: command.FieldTemplates.Add("@fG,,@fV" + c + "(0),@fV" + c + "_M,(" + zW + ")"); command.FieldTemplates.Add(",@fV" + c + "_B(60%),@fV" + c + "_R"); break;
                }
            }
            void addFieldTemplates_NonBlock(DP command, string mode)
            {
                if (mode == "HOXY")
                {
                    addFieldTemplates_HOXY(command);
                    return;
                }
                if (command.FieldTemplates.Count > 0) return;
                if (mode == "INV")
                {
                    if (command.is_Table)
                    {
                        addFieldTemplates_ByArray(command, command.options, templateInv, (command.cm == "Oreprio" ? 15 : -1));
                        return;
                    }
                    if (command.is_Fillbar)
                    {
                        addFieldTemplates_InvFillbar(command);
                        return;
                    }
                }
                switch (mode)
                {
                    case "SOLAR": switch (command.style) { case (1): command.FieldTemplates.Add("@fG,@fM(0),,@fO(0),@fO_R"); break; case (3): command.FieldTemplates.Add("@fM,,Max:,@fM_M,@fM_B(30%),@fM_R"); command.FieldTemplates.Add("@fG,,Out:,@fO,@fO_B(30%),@fO_R"); break; default: command.FieldTemplates.Add("@fG,@fM(0),,@fO(0),@fO_M,@fO_B(18%),@fO_R"); break; } break;
                    case "PCS": AddValueTemplates(command, "@fS"); break;
                    case "PCA": AddValueTemplates(command, "@fM"); break;
                    case "PCO": AddValueTemplates(command, "@fO"); break;
                    case "PCC": AddValueTemplates(command, "@fI"); break;
                    case "INV": switch (command.style) { case (1): command.FieldTemplates.Add("@fN,,@fAm(0)"); break; case (2): command.FieldTemplates.Add("@fAm(40%),@fN,"); break; case (3): command.FieldTemplates.Add(",@fN,@fAm"); break; default: command.FieldTemplates.Add("@fAm,,@fN"); break; } break;
                    case "RES": switch (command.style) { case 1: command.headLineString = ",Ingot,(21),Ore,(7)"; command.FieldTemplates.Add(",@fN,:,@fRi,/,@fRo"); break; default: command.headLineString = ",(40),Ore ,(6),Ref.,(5),Ingot,"; command.FieldTemplates.Add(",@fN(30),@fRo,@fRr,@fRi,"); break; } break;
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
            void addFieldTemplates_ByArray(DP command, List<string> def, string[] templateArray, int maxArrayLength = -1)
            {
                var sx = "";
                foreach (var s in def)
                {
                    for (int i = 0; i < (maxArrayLength == -1 ? templateArray.Length : maxArrayLength); i += 3)
                    {
                        if (s == templateArray[i])
                        {
                            command.headLineString += (command.headLineString == "" ? "" : ",") + templateArray[i + 1];
                            sx += (sx == "" ? "" : ",") + templateArray[i + 2];
                        }
                    }
                }
                command.FieldTemplates.Add(sx);
            }
            void addFieldTemplates_InvFillbar(DP command)
            {
                List<string> d;
                if (command.style == 4) d = new List<string> { "NAME", "AMOUNT", "FILLBAR", "MAX" };
                else
                {
                    d = new List<string> { "FILLBAR", "NAME", "AMOUNT" };
                    // was "Max" before: never found in templateInv, so style 2 had no max column
                    if (command.style == 2) d.Add("MAX");
                    if (command.style == 3) d.Add("RATIO");
                }
                if (command.ifOption("ASSEMBLY")) d.Add("ASSEMBLY");
                addFieldTemplates_ByArray(command, d, templateInv);
            }
            // block list commands (CustomData of the displays), with block type and add change type at the same index
            static readonly string[] BlockListCommands = { "Connectors", "Weapons", "Cargocontainers", "Gasgenerators", "Airvents", "Batterys", "Jumpdrives", "Reactors", "Hydroengines", "Refinerys", "Assemblers" };
            static readonly BlockType[] BlockListTypes = { BlockType.Connector, BlockType.Weapon, BlockType.Cargocontainer, BlockType.Gasgenerator, BlockType.Airvent, BlockType.Battery, BlockType.Jumpdrive, BlockType.Reactor, BlockType.Hydrogenengine, BlockType.Refinery, BlockType.Assembler };
            static readonly ChangeType[] BlockListChangeTypes = { ChangeType.Connector_add, ChangeType.Weapon_add, ChangeType.Cargo_add, ChangeType.Gasgenerator_add, ChangeType.Airvent_add, ChangeType.Battery_add, ChangeType.Jumpdrive_add, ChangeType.Reactor_add, ChangeType.Hydrogenengine_add, ChangeType.Refinery_add, ChangeType.Assembler_add };
            void addFieldTemplates_Block(DP command, BlockType type)
            {
                if (command.ifOption("ONOFF")) command.FieldTemplates.Add("@fN,,@fE");
                if (command.FieldTemplates.Count > 0) return;
                // the default templates of the style, a line starting with '^' is the head line
                foreach (var line in BlockTemplates.For(type, command.style))
                    if (line.StartsWith("^")) command.headLineString = line.Substring(1);
                    else command.FieldTemplates.Add(line);
            }
            void addSingleLine(DP command, dyBlockDisplayable d, string s) { List<dyMultiDisplayLine> nmol; displayLines.Add(nmol = new List<dyMultiDisplayLine>()); nmol.Add(d.getMDLo(this, command.with, s)); }
            void writeTitle_ifTrue(DP command, string tu = "") { if (command.is_Title) writeX(" ,<<<," + (tu == "" ? command.titleString : tu) + ",>>>, "); }
            void writeHeadLine_ifTrue(DP command) { if (command.headLineString != "" && command.is_HeadLine) writeX(command.headLineString, command.c); }
            void writeLine_ifTrue(DP command, char c = '-', bool f = false) { if (f || command.is_Line) writeX("@" + c + "(filled)", 1); }
            void WriteList(DP command, bool li = true)
            {
                command.dyn += command.listDy.Count;
                if (command.listDy.Count == 0)
                {
                    if (command.dyn != 0) return;
                    if (checkError(command, 0)) return;
                }
                sort_ifTrue(command);
                List<List<dyMultiDisplayLine>> nnmdll = new List<List<dyMultiDisplayLine>>();
                var fos = true;
                int dpr = command.listDy.Count / command.c;
                if (dpr < 1) dpr = 1;
                else if ((dpr * command.c) != command.listDy.Count) dpr++;
                int di = 0;
                int cos = 0;
                dyMultiDisplayLine cstr;
                for (int c = 0; c < command.c; c++)
                {
                    for (int r = 0; r < dpr; r++)
                    {
                        for (int f = 0; f < command.FieldTemplates.Count; f++)
                        {
                            if (di < command.listDy.Count) cstr = command.listDy[di].getMDLo(this, command.with, f >= command.FieldTemplates.Count ? "@-(" + (command.with - 2.58332f) + ")" : command.FieldTemplates[f]);
                            else cstr = getMDLo(this, command.with, " ");
                            if (fos) nnmdll.Add(new List<dyMultiDisplayLine>());
                            if (cstr != null) nnmdll[cos++].Add(cstr);
                        }
                        di++;
                    }
                    fos = false;
                    cos = 0;
                }
                writeHeadLine_ifTrue(command);
                foreach (var l1 in nnmdll) displayLines.Add(l1);
                command.listDy.Clear();
                if (li) writeLine_ifTrue(command);
            }
            void SPEED(DP command) { var sp = "@"; var c = 'G'; int y = command.style; if (command.ifOption("KMH")) { c = 'k'; sp += "KMH"; } else if (command.ifOption("MPH")) { c = 'm'; sp += "MPH"; } sp += "SPEED"; add_to_VC(c).M = command.ms; writeX("Speed:" + (y == 4 ? "(30)" : "") + ",," + sp + "(0)"); if (y == 4) writeX("Maxspeed:(30),," + sp + "_M(0)"); if (y > 1) writeX(sp + "_B(filled)" + (y > 2 ? "," + sp + "_R" : "")); writeLine_ifTrue(command); }
            bool Ft(DP command, dyBlock p)
            {
                var g = (GasTank)p;
                if (command.ifOption(g.medium) || command.is_All) return true;
                else return false;
            }
            void AddBlockTemplates(DP command, dyBlockDisplayable d, BlockType type) { if (command.ifOption("ENERGY") || command.ifOption("CARGO")) { command.FieldTemplates.Add("@fN," + (command.ifOption("ONOFF") ? ",@fE,(" + (zW + 1.6f) + ")" : (command.is_Line ? Cfilled : ""))); foreach (var c in getPowerCargoOptions(command)) AddPowerCargoTemplate(command, d, c); } else addFieldTemplates_Block(command, type); }
            void WriteBlockList(DP command, BlockType type, Func<DP, dyBlock, bool> F = null)
            {
                var dll = getDyBlockList(type);
                writeTitle_ifTrue(command, command.cm);
                foreach (var g in dll.Keys)
                {
                    if (command.ifDisplayGrid(g))
                    {
                        foreach (dyBlock dl in dll[g])
                            if (command.ifFilter(dl.Name) && (F == null ? true : F(command, dl)))
                                command.listDy.Add(dl);
                        if (command.FieldTemplates.Count == 0 && command.listDy.Count > 0) AddBlockTemplates(command, command.listDy[0], type);
                        if (command.ifOption(OptGridName))
                        {
                            if (command.listDy.Count > 0)
                            {
                                if (command.ifOption(OptGridName) && command.is_Title) writeX(g.CustomName + " :, ");
                                command.dyn += command.listDy.Count;
                                WriteList(command);
                            }
                        }
                    }
                }
                WriteList(command);
            }
            void DOCKINGS(DP command)
            {
                command.listDy.Clear();
                setOneCol(command, false);
                foreach (dyConnector d in docks) if (command.ifFilter(d.dockName)) command.listDy.Add(d); if (checkError(command, command.listDy.Count)) return; writeTitle_ifTrue(command, "Dockstations"); sort_ifTrue(command); foreach (dyConnector dc in command.listDy) { addSingleLine(command, dc, "@fDN" + (command.is_Line ? "," + Cfilled : ",")); addSingleLine(command, dc, "docked:(24),@fSh,,@fSct,@fSt(0)"); if (dc.docked_connector != null) { var g = dc.docked_connector.CubeGrid; dyDisplayableVariable pv = dyDisplayableVariable.getPV(g, "Damageblocks", false); if (pv != null) addSingleLine(command, pv, "Damage:(24),@fH,@fH_B(filled),@fH_R"); pv = dyDisplayableVariable.getPV(g, "Batterys", false); if (pv != null) addSingleLine(command, pv, "Batterys:(24),@fS,@fS_B(filled),@fS_R"); pv = dyDisplayableVariable.getPV(g, "Containers", false); if (pv != null) addSingleLine(command, pv, "Cargo:(24),@fVC,@fVC_B(filled),@fVC_R"); pv = dyDisplayableVariable.getPV(g, "OxygenTanks", false); if (pv != null) addSingleLine(command, pv, "Oxygen:(24),@fVO,@fVO_B(filled),@fVO_R"); pv = dyDisplayableVariable.getPV(g, "HydrogenTanks", false); if (pv != null) addSingleLine(command, pv, "Hydrogen:(24),@fVH,@fVH_B(filled),@fVH_R"); } }
                if (command.listDy.Count > 0) writeLine_ifTrue(command);
            }
            void WriteVariables(string title, string type, DP command, string sty = "", char tidx = ' ', string fla = "")
            {
                char tc = command.ifGridOption() ? 'g' : 's';
                if (tidx != ' ') tc = tidx;
                bool hw = false;
                if (sty != "" && command.FieldTemplates.Count == 0)
                {
                    addFieldTemplates_NonBlock(command, sty);
                    command.FieldTemplates[0] = fla + command.FieldTemplates[0];
                }
                foreach (dyDisplayableVariable pv in pvl)
                {
                    if (pv.isID("" + type[0], tc) && command.ifFilter(pv.Name))
                    {
                        if (!hw) writeTitle_ifTrue(command, title);
                        hw = true;
                        if (command.ifDisplayGrid(pv.pcg)) command.listDy.Add(pv);
                    }
                }
                WriteList(command);
            }
            void WriteVolumeSummary(DP command, string tty, string f)
            {
                var gty = command.cm;
                var n = command.filter == "*" && !command.is_All;
                if (n)
                {
                    command.sF(f);
                    command.is_Title = false;
                }
                addFieldTemplates_HOXY(command);
                if (!n && !command.FieldTemplates[0].StartsWith("@fN")) command.FieldTemplates[0] = "@fN," + command.FieldTemplates[0];
                if (!command.is_Title && !command.FieldTemplates[0].StartsWith(gty)) command.FieldTemplates[0] = gty + "," + command.FieldTemplates[0];
                WriteVariables(gty, tty, command);
            }
            void WriteVariableGroup(DP command, string gty, string sty, string tty) { addFieldTemplates_NonBlock(command, sty); if (!command.ifOption(OptGridName)) command.is_Title = false; if (!command.is_Title && !command.FieldTemplates[0].StartsWith(gty)) command.FieldTemplates[0] = gty + "," + command.FieldTemplates[0]; WriteVariables(gty, tty, command); }
            void ENERGYP(DP command)
            {
                var s = "SolarPanels,WindTurbines,HydrogenEngines,Reactors,Batterys,Total";
                if (command.grid != null) command.titleString += " " + command.grid.CustomName;
                if (command.is_Table)
                {
                    addPowerTable(command, s);
                    return;
                }
                setOneCol(command);
                writeTitle_ifTrue(command);
                foreach (var s1 in s.Split(',')) AddPowerLines(command, dyDisplayableVariable.getPV(command.grid, s1, false));
                writeLine_ifTrue(command);
            }
            void ENERGIE(DP command)
            {
                if (command.is_Table)
                {
                    addPowerTable(command);
                    return;
                }
                command.dyn = 1;
                foreach (char c in getPowerCargoOptions(command))
                {
                    if (c == 'S') WriteVariables("Power stored", "S", command, "PCS");
                    if (c == 'M') WriteVariables("Power available", "M", command, "PCA");
                    if (c == 'E') WriteVariables("Power output", "E", command, "PCO");
                    if (c == 'I') WriteVariables("Power consumption", "I", command, "PCC");
                    command.FieldTemplates.Clear();
                }
                if (command.dyn == 1)
                {
                    command.error = 1;
                    writeError(command);
                }
            }
            string getPowerCargoOptions(DP command)
            {
                string o = "";
                foreach (string s in command.options)
                {
                    if (s == "STORED") o += 'S';
                    else if (s == "AVAILABLE") o += 'M';
                    else if (s == "OUTPUT") o += 'E';
                    else if (s == "CONSUMPTION") o += 'I';
                    else if (s == "CARGO") o += "F01";
                }
                return o == "" ? "SMEI" : o;
            }
            string PowerTableHeadline(DP command, string os, bool gn) { var sr = gn ? "Grid," : ""; sr += ",Type"; foreach (char c in os) { sr += ",|,"; if (c == 'S') sr += "Stored"; else if (c == 'M') sr += "Avali."; else if (c == 'E') sr += "Output"; else if (c == 'I') sr += "Input"; switch (command.style) { case 1: sr += "(" + zW + ")"; break; case 2: sr += "(" + pW + ")"; break; case 3: sr += "(" + pW + "),(" + zW + ")"; break; default: sr += "(" + pW + "),(" + pW + ")"; break; } } return sr; }
            string PowerTableRow(DP command, dyDisplayableVariable p, string os) { var sr = ""; var ls = new List<string> { ",|,(" + zW + ")", ",|,(" + pW + ")", ",|,(" + pW + "),(" + zW + ")", ",|,(" + pW + "),(" + pW + ")" }; foreach (char c in os) { var s2 = ""; if (p.getVC(c, false) != null) { if (c == 'S') s2 = "@fS"; else if (c == 'M') s2 = "@fM"; else if (c == 'E') s2 = "@fO"; else if (c == 'I') s2 = "@fI"; switch (command.style) { case 1: sr += ",|," + s2 + "_R"; break; case 2: sr += ",|," + s2; break; case 3: sr += ",|," + s2 + "," + s2 + "_R"; break; default: sr += ",|," + s2 + "," + s2 + "_M"; break; } } else sr += ls[command.style - 1]; } return sr; }
            string gTD(string s) { switch (s) { case "Z+": return "Forward"; case "Z-": return "Backward"; case "X+": return "Right"; case "X-": return "Left"; case "Y+": return "Upward"; case "Y-": return "Downward"; } return "-?-"; }
            void aTD(DP command, string t = "", string f = "")
            {
                if (command.FieldTemplates.Count > 0) return;
                if (t != "" && command.is_Title) command.FieldTemplates.Add(",<<<," + t + ",>>>,");
                if (command.sortMode == "" && f == "") f = "X+X-Y+Y-Z+Z-";
                else f = command.sortMode;
                int i = 0;
                while (i < f.Length)
                {
                    var s = "" + f[i++] + f[i++];
                    AddValueTemplates(command, "@" + s + "THRUST", gTD(s) + "(30),[" + s + "],");
                }
                if (command.is_Line) command.FieldTemplates.Add(Cfilled);
            }
            void addPowerTable(DP command, string si = "")
            {
                setOneCol(command);
                var powerValueList = new List<dyDisplayableVariable>();
                dyDisplayableVariable powerTotal = null;
                char t = 's';
                if (command.ifGridOption() && si == "") t = 'g';
                var o = getPowerCargoOptions(command);
                if (si == "")
                {
                    foreach (dyDisplayableVariable p in pvl)
                        if (p.isID(o, t) && command.ifFilter(p.Name))
                        {
                            if (!command.ifDisplayGrid(p.pcg)) continue;
                            powerValueList.Add(p);
                        }
                    if (command.is_Sort) powerValueList.Sort();
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
                if (checkError(command, powerValueList.Count)) return;
                command.headLineString = PowerTableHeadline(command, o, t == 'g');
                if (powerValueList.Count < 2 && powerValueList[0].pcg != null) command.titleString += " " + powerValueList[0].pcg.CustomName;
                writeTitle_ifTrue(command);
                writeHeadLine_ifTrue(command);
                writeLine_ifTrue(command);
                var s1 = ((command.ifOption(OptGridName) && powerValueList.Count > 1) ? ("@fG,") : "") + ",@fN";
                foreach (dyDisplayableVariable p in powerValueList)
                {
                    addSingleLine(command, p, s1 + PowerTableRow(command, p, o));
                }
                writeLine_ifTrue(command);
                if (powerTotal != null)
                {
                    addSingleLine(command, powerTotal, s1 + PowerTableRow(command, powerTotal, o));
                    writeLine_ifTrue(command, '=');
                }
            }
            string aPL1(string s) { return "," + s + ",/," + s + "_M"; }
            string aPL2(string s) { return "," + s + "," + s + "_M," + s + "_R"; }
            string aPL3(string s) { return "," + s + "," + s + "_B(20%)," + s + "_R"; }
            string aPL4(string s) { return "," + s + "," + s + "_M," + s + "_B(20%)," + s + "_R"; }
            void AddPowerLines(DP command, dyDisplayableVariable p)
            {
                if (p == null) return;
                var sp = new List<string>();
                var fi = false;
                var fs = false;
                if (null != p.getVC('I', false)) fi = true;
                if (null != p.getVC('S', false)) fs = true;
                switch (command.style)
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
                        sp.Add("@fN" + (command.is_Line ? "," + Cfilled : ","));
                        sp.Add(",Output:(18)" + aPL4("@fO"));
                        if (fi) sp.Add(",Input:(18)" + aPL4("@fI"));
                        if (fs) sp.Add(",Stored:(18)" + aPL4("@fS"));
                        break;
                }
                foreach (string s in sp) addSingleLine(command, p, s);
            }
            bool IsCategoryShown(DP command, string s, string s1) { if (command.options.Contains("-" + s1)) return false; if (command.is_All || s == s1) return true; return false; }
            // inventory categories: option = title [= tag (typeString of the item, default: the title)], in display order
            const string InventoryCategories = "ORE=Ore;INGOT=Ingot;CRUSHED=Crushed Materials=Crushed;PURIFIED=Purified Materials=Purified;COMPONENT=Components=Component;TOOLS=Tools="
                + TypeTool + ";WEAPONS=Weapons;AMMO=Ammo=" + TypeAmmo + ";OIL=Oilproducts=Oil;DATAPADS=Data Pads=" + TypeDatapad + ";H-BOTTLES=Hydrogen Bottles=" + TypeHBottle
                + ";O-BOTTLES=Oxygen Bottles=" + TypeOBottle + ";KITS=Consumable Kits=" + TypeKit + ";FOOD=Foodproducts=Food;SEEDS=Seeds=SeedItem;CASH=Space Credits=" + TypePhysicalObject;
            void INVENTORY(DP command, Dictionary<MyItemType, dyInventory> invl)
            {
                List<inventoryDisplayItems> idil = new List<inventoryDisplayItems>();
                inventoryDisplayItems blockItemList = null;
                if (command.ifOption("BLOCK"))
                {
                    blockItemList = new inventoryDisplayItems("Inventory");
                    idil.Add(blockItemList);
                }
                if (command.is_All || command.options.Count == 0)
                {
                    command.options.Add("ALL");
                    command.is_All = true;
                }
                foreach (string bs in command.options)
                {
                    if (bs == "UTILS") { inventoryDisplayItems idix = new inventoryDisplayItems("Utils", TypeTool); idix.AddTag("Weapons"); idix.AddTag(TypeDatapad); idix.AddTag(TypeHBottle); idix.AddTag(TypeOBottle); idix.AddTag(TypeKit); idil.Add(idix); }
                    if (bs == "BOTTLES") { inventoryDisplayItems idix = new inventoryDisplayItems("Bottles", TypeHBottle); idix.AddTag(TypeOBottle); idil.Add(idix); }
                    foreach (var category in InventoryCategories.Split(';'))
                    {
                        var fields = category.Split('=');
                        if (IsCategoryShown(command, bs, fields[0])) AddInventoryCategory(blockItemList, fields[1], fields.Length > 2 ? fields[2] : fields[1], idil);
                    }
                    if (bs == "ALL") break;
                }
                addInventarItems(command, idil, invl);
                foreach (inventoryDisplayItems idix in idil)
                {
                    if (idix.idl.Count > 0)
                    {
                        writeTitle_ifTrue(command, idix.Name + (command.titleString == "" ? "" : " " + command.titleString));
                        command.listDy = idix.idl;
                        WriteList(command);
                    }
                }
                if (idil.Count == 0) command.error = 2;
                else if (command.dyn == 0) command.error = 1;
                writeError(command);
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
            // a list of its own, or (option BLOCK) the tag into the one inventory list
            static void AddInventoryCategory(inventoryDisplayItems blockItemList, string name, string tag, List<inventoryDisplayItems> idil)
            {
                if (blockItemList == null) idil.Add(new inventoryDisplayItems(name, tag));
                else blockItemList.AddTag(tag);
            }
            void addInventarItems(DP command, List<inventoryDisplayItems> idil, Dictionary<MyItemType, dyInventory> dll)
            {
                var z = command.is_Zero;
                foreach (dyInventory iit in dll.Values)
                {
                    if (command.ifFilter(iit.Name))
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
