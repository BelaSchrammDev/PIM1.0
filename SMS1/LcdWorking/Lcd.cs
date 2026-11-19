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
        //LCD
        public abstract class LCD : TextRenderEngine
        {
            public bool obs = false; public IMyTextPanel lcd = null; public string ly = "";
            public void InitLCD(IMyTextPanel p, int nl = 0) { lcd = p; ly = getPanelSubtype(p); tsg = lcd.CubeGrid; setWFConst(); setLinesBounds(nl); }
            public void setWFConst() { wconst = 94.5f; if (ly.EndsWith("Wide")) wconst = 188.8f; else if (ly.StartsWith("Small")) { wconst = 188.8f; if (ly.Contains("Flat")) fconst = 13.8f; else if (ly.Contains("_LCD")) fconst = 11.0f; else { fconst = 17.2854f; wconst = 91.66f; } } else { if (ly.Contains("_LCD_")) { wconst = 375.8f; fconst = 9.5f; } else if (ly.Contains("Text") || ly.Contains("5x3")) fconst = 10.4f; } }
        }
        //infoLCD
        public class LCD_Infolcd : LCD
        {
            public Vector3I linkpos; public List<IMyTextPanel> extlcds = new List<IMyTextPanel>();
            public LCD_Infolcd(IMyTextPanel panel) { linkpos = getUpP(panel); InitLCD(panel); }
            public void setMML()
            {
                if (ly.Contains("Corner"))
                {
                    maxl = ly.Contains("Small") ? 8 : 5;
                    minl = 1;
                }
                else
                {
                    maxl = 40;
                    minl = 5;
                }
            }
            public void Calc(P pm)
            {
                if (displayDefinition != lcd.CustomData) displayDefinition = lcd.CustomData;
                if (displayDefinition == "") displayDefinition = "writex(write commands to LCDs 'CustomData' ,)\nwritex(for example 'batterys' or 'energy',)";
                setName(pm.Name);
                setMML();
                if (pm.isPM("Maxlines")) maxl = getIntegerWithBounds(pm.getPM("Maxlines"), maxlines);
                if (pm.isPM("Minlines")) minl = getIntegerWithBounds(pm.getPM("Minlines"), maxlines);
                if (pm.isPM("Lines"))
                {
                    newlines = getIntegerWithBounds(pm.getPM("Lines"), maxlines, minl, maxl);
                    lfix = true;
                }
                else
                {
                    lfix = false;
                    newlines = maxlines;
                }
            }
            public bool RemoveELCD(IMyTextPanel elcd) { if (extlcds.Contains(elcd)) { ClearELCDs(); return true; } return false; }
            public void ClearELCDs() { extlcds.Clear(); pnum = 1; linkpos = getUpP(lcd); }
            public void AddPanel(IMyTextPanel np) { if (ly == getPanelSubtype(np) && !extlcds.Contains(np)) { extlcds.Add(np); pnum++; linkpos = getUpP(np); np.WriteText("Init Linklcd....", false); } }
            public override void writeToPanel() { int cos = 0; w2lcd(lcd, pages.Count > cos ? pages[cos++] : "", fontsize); foreach (IMyTextPanel t in extlcds) { t.FontColor = lcd.FontColor; t.BackgroundColor = lcd.BackgroundColor; w2lcd(t, pages.Count > cos ? pages[cos++] : "", fontsize); } }
        }
        //terminalLCD
        public class LCD_Terminal : LCD
        {
            const string mDefBegin = "  / Menudefinition for custommenus:";
            public string lcd_id = "";
            ST mkdt = new ST(KEEPMENU);
            MenuMode mmode = MenuMode.Default;
            public TerminalMenu menu = null;
            string lcdclonemenu = "";
            TerminalSite curSite = null;
            string oldCustomData = "nix";
            public int curml;
            public LCD_Terminal(IMyTextPanel tp)
            {
                lfix = true;
                minl = 35;
                maxl = 35;
                InitLCD(tp, 35);
                iwith = with;
                ifNewCustomData();
            }
            void setMenu(TerminalMenu newmenu = null)
            {
                if (newmenu != null)
                {
                    if (newmenu != menu && menu != null && menu.terminals.Contains(this)) menu.terminals.Remove(this);
                    menu = newmenu;
                    menu.terminals.Add(this);
                }
                with = iwith - menu.maxMenuWith;
                setNewTerminalSite(menu.mainmenu);
            }
            bool ifNewCustomData()
            {
                if (oldCustomData == lcd.CustomData) return false;
                oldCustomData = lcd.CustomData;
                var cdl = oldCustomData.Split('\n');
                if (cdl.Length < 7)
                {
                    mmode = MenuMode.Default;
                    setMenu(defaultMenu);
                    writeCustomData();
                    return false;
                }
                else if (cdl[5].Trim().StartsWith("menu="))
                {
                    var mdef = oldCustomData.Substring(oldCustomData.IndexOf(mDefBegin) + mDefBegin.Length + 1);
                    var modestrings = cdl[5].Split('=');
                    var modestr = modestrings.Length < 2 ? "DEFAULT" : modestrings[1].ToUpper().Trim();
                    if (modestr == "DEFAULT")
                    {
                        mmode = MenuMode.Default;
                        setMenu(defaultMenu);
                        writeCustomData();
                    }
                    else if (modestr == "COPYDEFAULT")
                    {
                        var custommenu = new TerminalMenu();
                        custommenu.parseMenuDefinition(defaultMenu.oldMenuDef);
                        mmode = MenuMode.Custom;
                        setMenu(custommenu);
                        writeCustomData();
                    }
                    else if (modestr == "CUSTOM")
                    {
                        if (menu == defaultMenu || menu == null)
                        {
                            mmode = MenuMode.Custom;
                            var custommenu = new TerminalMenu();
                            if (custommenu.parseMenuDefinition(mdef))
                            {
                                setMenu(custommenu);
                                writeCustomData();
                            }
                        }
                        else
                        {
                            if (menu.parseMenuDefinition(mdef))
                            {
                                foreach (var t in menu.terminals) t.setMenu();
                                writeCustomData();
                            }
                        }
                    }
                    else if (modestr.StartsWith("MENUCLONE"))
                    {
                        mmode = MenuMode.MenuClone;
                        if (menu == null)
                        {
                            getMenu(modestr.Substring(modestr.Length - 2));
                        }
                        return false;
                    }
                    else
                    {
                        var lcdmenuclone = false;
                        if (modestrings.Length > 1)
                        {
                            lcdmenuclone = getMenu(modestrings[1].Trim());
                        }
                        if (!lcdmenuclone)
                        {
                            mmode = MenuMode.Default;
                            setMenu(defaultMenu);
                        }
                        writeCustomData();
                    }
                }
                return true;
            }
            bool getMenu(string menuid)
            {
                lcdclonemenu = "";
                foreach (LCD_Terminal smst in LCDts)
                {
                    if (menuid == smst.lcd_id && smst.menu != null)
                    {
                        lcdclonemenu = smst.lcd_id;
                        mmode = MenuMode.MenuClone;
                        setMenu(smst.menu);
                        return true;
                    }
                }
                return false;
            }
            void writeCustomData()
            {
                var menustr = mmode != MenuMode.Custom ? "\n" : menu == null ? "no menu connected\n" : menu.oldMenuDef;
                // custom menu: the errors of the definition as comment lines before the definition
                lcd.CustomData = "  / Menu:\n  / default - the defaultmenu is used\n  / custom - specify your own menu structure\n  / copydefault - copies the defaultmenu as custom\n  / or the ID from other terminal to clone the menu\nmenu=" + mmode.ToString() + (mmode == MenuMode.MenuClone ? " from terminal " + lcdclonemenu + "\n" : "\n") + (mmode == MenuMode.Custom && menu != null ? menu.def.Errors : "") + mDefBegin + "\n" + menustr;
                oldCustomData = lcd.CustomData;
            }
            public void Calc(P pm)
            {
                if (!ifNewCustomData() && mkdt.ifS()) setNewTerminalSite(menu.mainmenu);
                setName(pm.Name);
                if (pm.isPM("Terminal")) lcd_id = pm.getPM("Terminal");
                else lcd_id = "xxx";
                if (pm.isPM("Keepmenu")) mkdt.sec = getIntegerWithBounds(pm.getPM("Keepmenu"));
            }
            public void Action(string command)
            {
                if (curSite == null) return;
                int comnum = -1;
                switch (command)
                {
                    case "1": comnum = 0; break;
                    case "2": comnum = 1; break;
                    case "3": comnum = 2; break;
                    case "4": comnum = 3; break;
                }
                if (comnum != -1 && comnum < curSite.commands.Count && curSite.commands[comnum] != null) setNewTerminalSite(curSite.commands[comnum].scheduleCommand());
            }
            void setNewTerminalSite(TerminalSite tsite)
            {
                if (tsite == null) return;
                renderPhase = RenderPhase.ReadCommands;
                mkdt.UD();
                curSite = tsite;
                tsite.setMenu(maxlines);
                // errors in the menu definition: a hint in the title line (the errors are in the CustomData)
                displayDefinition = "writex_i(" + (menu == null ? tHeadLine : menu.def.Errors != "" ? "!! Menu error - see CustomData !!" : tsite.Def.TitleLine) + ")\nwritex_i(@*(filled))\n" + tsite.Def.Body;
            }
            public override void checkWriteToPanel() { if (displayLines.Count < maxlines) for (int i = displayLines.Count; i < maxlines; i++) writeX("(filled)"); }
            public override void writeToPanel() { w2lcd(lcd, pages.Count > 0 ? pages[0] : "", fontsize); curml = 0; }
            public override void clearWriteValues() { curml = 0; }
            public override void addEndOfLineStr() { if (curSite != null && curml < curSite.mel.Count) pageText += curSite.mel[curml++]; else pageText += "\n"; }
        }
    }
}
