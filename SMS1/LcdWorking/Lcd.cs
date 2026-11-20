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
            public IMyTextPanel lcd; public string ly = "";
            // SmallTextPanel gets some padding; LCD panels: up to 40 lines on a full 512 px height
            public void InitLCD(IMyTextPanel p, int nl = 0) { lcd = p; ly = getPanelSubtype(p); tsg = lcd.CubeGrid; SetSurfaceConstants(p, ly.Contains("SmallTextPanel") ? 1.5f : 0, 40); setLinesBounds(nl); }
        }
        //infoLCD
        class LCD_Infolcd : LCD
        {
            public Vector3I linkpos; public List<IMyTextPanel> extlcds = new List<IMyTextPanel>();
            public LCD_Infolcd(IMyTextPanel panel) { linkpos = getUpP(panel); InitLCD(panel); }
            // low surfaces (corner LCDs) may go down to 1 line
            void setMML()
            {
                maxl = readableLines;
                minl = maxl < 10 ? 1 : 5;
            }
            public void Calc(Parameter pm)
            {
                setName(pm.Name);
                setMML();
                ApplyDisplayOptions(pm, lcd.CustomData);
            }
            public bool RemoveELCD(IMyTextPanel elcd) { if (extlcds.Contains(elcd)) { ClearELCDs(); return true; } return false; }
            public void ClearELCDs() { extlcds.Clear(); pnum = 1; linkpos = getUpP(lcd); }
            public void AddPanel(IMyTextPanel np) { if (ly == getPanelSubtype(np) && !extlcds.Contains(np)) { extlcds.Add(np); pnum++; linkpos = getUpP(np); np.WriteText("Init Linklcd....", false); } }
            public override void writeToPanel() { int cos = 0; WriteSurface(lcd, pages.Count > cos ? pages[cos++] : ""); foreach (var t in extlcds) { t.FontColor = lcd.FontColor; t.BackgroundColor = lcd.BackgroundColor; WriteSurface(t, pages.Count > cos ? pages[cos++] : ""); } }
        }
        //terminalLCD
        public class LCD_Terminal : LCD
        {
            const string mDefBegin = "  / Menudefinition for custommenus:";
            public string lcd_id = "";
            // clicked: the number of the click (the smallest waits longest), 0 = not waiting
            public int urgent;
            static int clickNumber;
            ST mkdt = new ST(KEEPMENU);
            MenuMode mmode = MenuMode.Default;
            TerminalMenu menu = null;
            string lcdclonemenu = "";
            TerminalSite curSite = null;
            string oldCustomData = "nix";
            int curml;
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
            public void Calc(Parameter pm)
            {
                if (!ifNewCustomData() && mkdt.ifS()) setNewTerminalSite(menu.mainmenu);
                setName(pm.Name);
                if (pm.IsParameter("Terminal")) lcd_id = pm.GetParameter("Terminal");
                else lcd_id = "xxx";
                if (pm.IsParameter("Keepmenu")) mkdt.sec = getIntegerWithBounds(pm.GetParameter("Keepmenu"));
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
                if (comnum == -1 || comnum >= curSite.commands.Count || curSite.commands[comnum] == null) return;
                var site = curSite.commands[comnum].scheduleCommand();
                if (site == null) return;
                setNewTerminalSite(site);
                // a new site: rendered before the cycle goes on (RenderUrgentTerminals), in the order of the clicks
                urgent = ++clickNumber;
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
            public override void writeToPanel() { WriteSurface(lcd, pages.Count > 0 ? pages[0] : ""); curml = 0; }
            public override void clearWriteValues() { curml = 0; }
            public override void addEndOfLineStr() { if (curSite != null && curml < curSite.mel.Count) pageText += curSite.mel[curml++]; else pageText += "\n"; }
        }
    }
}
