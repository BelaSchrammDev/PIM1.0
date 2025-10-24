using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public class Loop
        {
            private static Loop _data = null;
            public static Loop Data
            {
                get
                {
                    if (_data == null)
                    {
                        _data = new Loop();
                    }
                    return _data;
                }
            }

            // Index of the currently running job
            public int CurrentJobIndex = 0;
        }

        public class Lists
        {
            private static Lists _data = null;
            public static Lists Data
            {
                get { 
                    if (_data == null) 
                    { 
                        _data = new Lists(); 
                    }

                    return _data;
                }
            }

            public void ClearAllRefineryBlueprintAssemblyAmounts()
            {
                foreach (var bp in BluePrints_Active.Values)
                {
                    bp.AssemblyAmount = 0;
                }
                foreach (var bp in BluePrints_Inactive.Values)
                {
                    bp.AssemblyAmount = 0;
                }
            }

            public List<IMyRefinery> Refinerys = new List<IMyRefinery>();
            public List<Refinery> RefineryList = new List<Refinery>();
            public List<IMyAssembler> Assemblers = new List<IMyAssembler>();
            public List<Assembler> AssemblerList = new List<Assembler>();

            public List<IMyInventory> NonSmsFlagedInventoryList = new List<IMyInventory>();
            public List<IMyInventory> SmsFlagedInventoryList = new List<IMyInventory>();
            public List<string> collectAll_List = new List<string>();


            public List<IMyProgrammableBlock> ProgrammableBlocks = new List<IMyProgrammableBlock>();
            public List<StorageCargo> StorageCargos = new List<StorageCargo>();
            public List<Gun> guns = new List<Gun>();

            public Dictionary<string, AssemblerBluePrint> BluePrints_Active = new Dictionary<string, AssemblerBluePrint>();
            public Dictionary<string, AssemblerBluePrint> BluePrints_Inactive = new Dictionary<string, AssemblerBluePrint>();
        }

        public class Propertys
        {
            private static Propertys _data = null;
            public static Propertys Data
            {
                get
                {
                    if (_data == null)
                    {
                        _data = new Propertys();
                    }
                    return _data;
                }
            }
            
            public double CurrentCycleInSec = 0;
            public DateTime LastStart = DateTime.Now;
            public bool changeAutoCraftingSettings = true;
            public string CurrentGunGroupName = Constants.DefaultGunGroupName;
        }

        public class Constants 
        {
            public const string AssemblerQueueNameSemikolon = "@ASSEMBLERQUEUE;";
            public const string ItemMaxNameSemikolon = "@ITEMMAX;";
            public const string DefaultGunGroupName = "PIM controlled Guns";

        }
    }


}
