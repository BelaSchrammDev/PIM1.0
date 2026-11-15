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
        string defMenu = "\n" +
            X_Line + "@command(ctrl_ref,Control_all Refinerys,#ctrl_ref#)\n@command(ctrl_ass,Control_all Assemblers,#ctrl_ass#)\n@command(ctrl_all,Control_all Production,#ctrl_all#)\n@command(noctrl_ref,Control_no Refinerys,#noctrl_ref#)\n@command(noctrl_ass,Control_no Assemblers,#noctrl_ass#)\n\n" +
            X_Line + "@titleline(" + tHeadLine + ")\n\n" +
            X_LineNewSite + "main,MainMenu)\n@titleline(@maingridname,,< Terminal Mainpage >,,@time,@date)\n@menu(inv_dock,production,energy,hoxy)\n@body\n" + tMainPage + "\n  @endsite\n\n" +
            X_LineNewSite + "inv_comp,Components)\n@alternative(inv_dock,Inventory Dockings)\n@menu(inv_ore_ingot,inv_tool,docking,main)\n@body\n\n    inventory(component,sort,zero,fillbar,noline,style 2)\n  @endsite\n\n" +
            X_LineNewSite + "damage,Damage)\n@menu(,prod_ref,prod_ass,main)\n@body\n    damage_b(style 3)\n  @endsite\n\n" +
            X_LineNewSite + "production,Production Damage)\n  @menu(damage,prod_ref,prod_ass,main)\n  @body\n\n    refinerys\n\n    assemblers\n  @endsite\n\n" +
            X_LineNewSite + "prod_ref,Refinerys)\n  @menu(ctrl_ref,noctrl_ref,resprio,production)\n  @body\n\n    refinerys\n\n    oreprio(sort amount,col 3)\n  @endsite\n\n" +
            X_LineNewSite + "prod_ass,Assemblers)\n  @menu(ctrl_ass,noctrl_ass,,production)\n  @body\n\n    assemblers\n  @endsite\n\n" +
            X_LineNewSite + "resprio,Resources Oreprio)\n  @menu(prod_ref,,production,main)\n  @body\n    resources(style 3)\n\n    oreprio(sort amount,col 3)\n  @endsite\n\n" +
            X_LineNewSite + "inv_ore_ingot,Ores Ingots)\n  @menu(inv_comp,inv_tool,docking,main)\n  @body\n\n    inventory(sort,Ore,Ingot)\n  @endsite\n\n" +
            X_LineNewSite + "inv_tool,Tools Utils)\n  @menu(inv_comp,inv_ore_ingot,docking,main)\n  @body\n\n    inventory(sort,cash,utils,ammo)\n  @endsite\n\n" +
            X_LineNewSite + "docking,Dockings)\n  @menu(inv_comp,inv_ore_ingot,inv_tool,main)\n  @body\n\n    dockings(sort)\n  @endsite\n\n" +
            X_LineNewSite + "energy,Energy)\n  @menu(batterys,reactors,solarwind,main)\n  @body\n    energy_p\n    energy(consumption)\n  @endsite\n\n" +
            X_LineNewSite + "batterys,Batterys)\n  @menu(energy,reactors,solarwind,main)\n  @body\n    batterys(sort)\n  @endsite\n\n" +
            X_LineNewSite + "reactors,Reactors H.Engines)\n  @menu(energy,batterys,solarwind,main)\n  @body\n\n    reactors(sort,style 3)\n\n    hydroengines(style 3)\n  @endsite\n\n" +
            X_LineNewSite + "solarwind,Solar Wind)\n  @menu(energy,batterys,reactors,main)\n  @body\n    solar(gridname,style 4)\n    wind(gridname,style 4)\n  @endsite\n\n" +
            X_LineNewSite + "luefter,Airvents)\n  @alternative(hoxy,Hydrogen Oxygen)\n  @menu(h_tanks,o_tanks,gasgen,main)\n  @body\n    airvents(sort)\n  @endsite\n\n" +
            X_LineNewSite + "h_tanks,Hydrogen Tanks)\n  @menu(luefter,o_tanks,gasgen,main)\n  @body\n\n    hydrogen\n\n    tanks(sort,hydrogen)\n  @endsite\n\n" +
            X_LineNewSite + "o_tanks,Oxygen Tanks)\n  @menu(luefter,h_tanks,gasgen,main)\n  @body\n\n    oxygen\n\n    tanks(sort,oxygen)\n  @endsite\n\n" +
            X_LineNewSite + "gasgen,H2/O2 Generators)\n  @menu(luefter,h_tanks,o_tanks,main)\n  @body\n\n    hydrogen(noline)\n    oxygen\n\n    writex(Oxygengenerators:,,Ice:(17 left),Full(18%),(13))\n    line\n    gasgenerators(notitle,sort){@fN,,@fE,@fIa,@fF0_B(18%),@fF0_R}\n  @endsite\n\n";
    }
}
