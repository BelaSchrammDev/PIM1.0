namespace IngameScript
{
    partial class Program
    {
        // The default terminal menu (also copied into the CustomData of an LCD by menu=COPYDEFAULT).
        // The sites are built by DefaultSite, so the repeated @menu/@body/@endsite parts are in the script only once.

        // a site: head = "name,Title" (and more lines, e.g. "\n  @alternative(..."), menu = the 4 menu entries,
        // body = the body lines, '|' starts an indented line, "\n" before it adds an empty line
        static string DefaultSite(string head, string menu, string body)
        {
            return X_LineNewSite + head + ")\n  @menu(" + menu + ")\n  @body" + body.Replace("|", "\n    ") + "\n  @endsite\n\n";
        }

        string defMenu = "\n" +
            X_Line + "@command(ctrl_ref,Control_all Refinerys,#ctrl_ref#)\n@command(ctrl_ass,Control_all Assemblers,#ctrl_ass#)\n@command(ctrl_all,Control_all Production,#ctrl_all#)\n@command(noctrl_ref,Control_no Refinerys,#noctrl_ref#)\n@command(noctrl_ass,Control_no Assemblers,#noctrl_ass#)\n\n" +
            X_Line + "@titleline(" + tHeadLine + ")\n\n" +
            // the main page: its body is the TerminalMainPageBody of the config (not indented, may contain '|')
            X_LineNewSite + "main,MainMenu)\n  @titleline(@maingridname,,< Terminal Mainpage >,,@time,@date)\n  @menu(inv_dock,production,energy,hoxy)\n  @body\n" + tMainPage + "\n  @endsite\n\n" +
            DefaultSite("inv_comp,Components)\n  @alternative(inv_dock,Inventory Dockings", "inv_ore_ingot,inv_tool,docking,main", "\n|inventory(component,sort,zero,fillbar,noline,style 2)") +
            DefaultSite("damage,Damage", ",prod_ref,prod_ass,main", "|damage_b(style 3)") +
            DefaultSite("production,Production Damage", "damage,prod_ref,prod_ass,main", "\n|refinerys\n|assemblers") +
            DefaultSite("prod_ref,Refinerys", "ctrl_ref,noctrl_ref,resprio,production", "\n|refinerys\n|oreprio(sort amount,col 3)") +
            DefaultSite("prod_ass,Assemblers", "ctrl_ass,noctrl_ass,,production", "\n|assemblers") +
            DefaultSite("resprio,Resources Oreprio", "prod_ref,,production,main", "|resources(style 3)\n|oreprio(sort amount,col 3)") +
            DefaultSite("inv_ore_ingot,Ores Ingots", "inv_comp,inv_tool,docking,main", "\n|inventory(sort,Ore,Ingot)") +
            DefaultSite("inv_tool,Tools Utils", "inv_comp,inv_ore_ingot,docking,main", "\n|inventory(sort,cash,utils,ammo)") +
            DefaultSite("docking,Dockings", "inv_comp,inv_ore_ingot,inv_tool,main", "\n|dockings(sort)") +
            DefaultSite("energy,Energy", "batterys,reactors,solarwind,main", "|energy_p|energy(consumption)") +
            DefaultSite("batterys,Batterys", "energy,reactors,solarwind,main", "|batterys(sort)") +
            DefaultSite("reactors,Reactors H.Engines", "energy,batterys,solarwind,main", "\n|reactors(sort,style 3)\n|hydroengines(style 3)") +
            DefaultSite("solarwind,Solar Wind", "energy,batterys,reactors,main", "|solar(gridname,style 4)|wind(gridname,style 4)") +
            DefaultSite("luefter,Airvents)\n  @alternative(hoxy,Hydrogen Oxygen", "h_tanks,o_tanks,gasgen,main", "|airvents(sort)") +
            DefaultSite("h_tanks,Hydrogen Tanks", "luefter,o_tanks,gasgen,main", "\n|hydrogen\n|tanks(sort,hydrogen)") +
            DefaultSite("o_tanks,Oxygen Tanks", "luefter,h_tanks,gasgen,main", "\n|oxygen\n|tanks(sort,oxygen)") +
            DefaultSite("gasgen,H2/O2 Generators", "luefter,h_tanks,o_tanks,main",
                "\n|hydrogen(noline)|oxygen\n|writex(Oxygengenerators:,,Ice:(17 left),Full(18%),(13))|line|gasgenerators(notitle,sort){@fN,,@fE,@fIa,@fF0_B(18%),@fF0_R}");
    }
}
