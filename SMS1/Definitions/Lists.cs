using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        // the lists and dictionaries of SMS, by topic (filled by the jobs, read by the displays)

        // displays: the text surfaces, their render engines, the terminals (menus) and the info LCDs
        static List<TextSurfaceDisplay> sms_sf = new List<TextSurfaceDisplay>(); static List<TextRenderEngine> sms_disp = new List<TextRenderEngine>(); static List<LCD_Terminal> LCDts = new List<LCD_Terminal>(); static List<LCD_Infolcd> infolcds = new List<LCD_Infolcd>();
        // display variables (@...) of the texts, and the variables of the blocks
        static Dictionary<string, MDI> allMDIs = new Dictionary<string, MDI>();
        static List<dyDisplayableVariable> pvl = new List<dyDisplayableVariable>();
        static MDIs pt = new MDIs(55), sMb = new MDIs(20), sMt = new MDIs(20), sMc = new MDIs(20), grn = new MDIs(20), gra = new MDIs(20), mdi_time = new MDIs(), mdi_date = new MDIs(), mdi_dateGER = new MDIs(), mdi_mgrid = new MDIs();
        // text layout: the widths of the font characters (FontMetrics), the fill bars and the space bars
        static Dictionary<char, float> fontCharWidths = new Dictionary<char, float>();
        static Dictionary<float, List<BarRatio>> GPL = new Dictionary<float, List<BarRatio>>();
        static List<SP> SPL = new List<SP>();
        // the sort column of the list being rendered (sort option of the LCD command)
        static string sortColumn = "", castTypeName = "";

        // inventory: amounts in total and per grid, the inventory items, the ore/ingot resources
        static Dictionary<MyItemType, float> inventar = new Dictionary<MyItemType, float>();
        static Dictionary<IMyCubeGrid, Dictionary<MyItemType, float>> dict_inv = new Dictionary<IMyCubeGrid, Dictionary<MyItemType, float>>();
        static Dictionary<MyItemType, dyInventory> invitemsg = new Dictionary<MyItemType, dyInventory>();
        static Dictionary<IMyCubeGrid, Dictionary<MyItemType, dyInventory>> gridInventoryItems = new Dictionary<IMyCubeGrid, Dictionary<MyItemType, dyInventory>>();
        static Dictionary<string, dyResourceOreIngot> oiil = new Dictionary<string, dyResourceOreIngot>();

        // the connectors with a dock port (",DOCK:")
        static List<dyConnector> docks = new List<dyConnector>();
        static double rsp;
        // the last PIM data loaded (PimDataParser)
        static string lastPimData = "";
    }
}
