using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
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

            public Dictionary<string, List<IMyInventory>> InventoryManagerList = new Dictionary<string, List<IMyInventory>>();
            public Dictionary<string, CargoUse> CargoUseList = new Dictionary<string, CargoUse>();

            public List<IMyInventory> NoneSmsFlagedInventoryList = new List<IMyInventory>();
            public List<IMyInventory> SmsFlagedInventoryList = new List<IMyInventory>();
            public List<string> collectAll_List = new List<string>();


            public List<IMyProgrammableBlock> ProgrammableBlocks = new List<IMyProgrammableBlock>();

            public List<StorageCargo> StorageCargos = new List<StorageCargo>();
            public List<StorageInventory> storageinvs = new List<StorageInventory>();

            public List<Gun> guns = new List<Gun>();
            public Dictionary<string, AmmoDefs> AmmoDefinitions = new Dictionary<string, AmmoDefs>();

            public Dictionary<string, AssemblerBluePrint> BluePrints_Active = new Dictionary<string, AssemblerBluePrint>();
            public Dictionary<string, AssemblerBluePrint> BluePrints_Inactive = new Dictionary<string, AssemblerBluePrint>();

            public List<string> autocrafting_Types = new List<string>();

        }
    }
}
