using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript
{
    partial class Program
    {
        public class Loop
        {
            // Index of the currently running job
            public static int CurrentJobIndex = 0;
        }

        public static class Lists 
        {
            public static List<IMyRefinery> Refinerys = new List<IMyRefinery>();
            public static List<IMyAssembler> Assemblers = new List<IMyAssembler>();
            public static List<IMyProgrammableBlock> ProgrammableBlocks = new List<IMyProgrammableBlock>();

            public static Dictionary<string, AssemblerBluePrint> BluePrints_Active = new Dictionary<string, AssemblerBluePrint>();
            public static Dictionary<string, AssemblerBluePrint> BluePrints_Inactive = new Dictionary<string, AssemblerBluePrint>();
        }

        public static class Propertys
        {
            public static double CurrentCycleInSec = 0;
            public static DateTime LastStart = DateTime.Now;
            public static bool changeAutoCraftingSettings = true;
        }

        public static class Constants 
        {
            public const string AssemblerQueueNameSemikolon = "@ASSEMBLERQUEUE;";
            public const string ItemMaxNameSemikolon = "@ITEMMAX;";
        }
    }


}
