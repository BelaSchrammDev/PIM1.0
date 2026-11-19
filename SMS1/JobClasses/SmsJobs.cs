using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        // jumpdrives and thrusters: not in station mode (set at the start of every cycle)
        Job shipOnlyJobs;

        // the cycle, in this order; Main runs it job by job within the instruction budget
        void InitJobs()
        {
            shipOnlyJobs = new SequentialJob(
                RefreshJob(BlockType.Jumpdrive, "JumpDrive"),
                new ForEachJob<IMyThrust>(CollectThrusters, ProcessThruster));

            jobs = new Job[]
            {
                new ActionJob(StartCycle),
                new ActionJob(ResetInventoryAmounts),
                RefreshJob(BlockType.Connector, "ShipConnector"),
                RefreshJob(BlockType.Weapon, "", BlockWeaponsCheck),
                RefreshJob(BlockType.Cargocontainer, "Cargo"),
                new ActionJob(ReadPimData),
                new SequentialJob(
                    new ForEachJob<IMyTerminalBlock>(CollectTextPanels, ProcessTextPanel),
                    new ActionJob(RemoveObsoleteDisplays)),
                RefreshJob(BlockType.Battery, "BatteryBlock"),
                MultiBlockJob(),
                RefreshJob(BlockType.Tank, "OxygenTank"),
                RefreshJob(BlockType.Hydrogenengine, "HydrogenEngine"),
                RefreshJob(BlockType.Reactor, "Reactor"),
                RefreshJob(BlockType.Assembler, "Assembler"),
                new ActionJob(ResetOreIngotAmounts),
                RefreshJob(BlockType.Refinery, "Refinery"),
                RefreshJob(BlockType.Airvent, "AirVent"),
                RefreshJob(BlockType.Gasgenerator, "OxygenGenerator"),
                shipOnlyJobs,
                RefreshJob(BlockType.Damage, "", BlockDamageCheck),
                new ForEachJob<IMyCubeGrid>(CollectInventoryGrids, InitGridInventoryItems),
                new ActionJob(CreateInventoryItems),
                new ForEachJob<dyInventory>(CollectInventoryItems, i => i.Refresh(true)),
                new ActionJob(FinishBlockScan),
                new DisplayJob()
            };
        }

        Job RefreshJob(BlockType type, string subtype, Func<IMyTerminalBlock, bool> check = null)
        {
            return new RefreshDyBlockListJob(this, type, subtype, check);
        }

        void StartCycle()
        {
            shipOnlyJobs.Active = currentMode != ScriptMode.STATION;
            filterGrid = Mgrid;
            rsp = 0;
            mdi_time.sT(DateTime.Now.ToString("HH:mm"));
            mdi_date.sT(DateTime.Now.ToString(" MM-dd-yy"));
            mdi_dateGER.sT(DateTime.Now.ToString("dd.MM.yyyy"));
            mdi_mgrid.sT(Mgrid.CustomName);
            foreach (dyDisplayableVariable pv in pvl) pv.clearAllVCs();
        }

        void ResetInventoryAmounts()
        {
            var items = new List<MyItemType>(inventar.Keys);
            foreach (var i in items) inventar[i] = 0;
            foreach (var grid in dict_inv.Values)
            {
                items = new List<MyItemType>(grid.Keys);
                foreach (var i in items) grid[i] = 0;
            }
        }

        // the data the PIM master sends (NetWorkingScripts.PimData); PIM gone: the PIM values are cleared
        void ReadPimData()
        {
            if (Net.MasterOf(NetWorkingScripts.Pim) == 0) pimData = "";
            LoadPimData(pimData);
        }

        void ResetOreIngotAmounts()
        {
            foreach (var o in oiil.Values) o.of = 0;
        }

        // dyInventorys for the item types found in this cycle, per grid and in total, and the ore/ingot resources
        void CreateInventoryItems()
        {
            foreach (var grid in dict_inv)
            {
                foreach (var key in grid.Value.Keys)
                {
                    if (!gridInventoryItems[grid.Key].ContainsKey(key)) gridInventoryItems[grid.Key].Add(key, new dyInventory(key, grid.Key));
                }
            }
            foreach (var k in inventar.Keys)
                if (!invitemsg.ContainsKey(k)) invitemsg.Add(k, new dyInventory(k));
            foreach (var k in invitemsg.Keys)
            {
                var itemType = GetSMSItemType(k);
                if (itemType.IsItemResource())
                {
                    var resourceName = itemType.GetResourceName();
                    if (oiil.ContainsKey(resourceName)) oiil[resourceName].Refresh();
                    else oiil.Add(resourceName, new dyResourceOreIngot(resourceName));
                }
            }
        }

        void FinishBlockScan()
        {
            foreach (var pv in pvl)
                if (pv.Name == "Damageblocks")
                {
                    VC v = pv.getVC('h');
                    v.C = v.M;
                    v.M = gn(pv.pcg);
                }
            dyDisplayableVariable.RefreshVar();
            PT();
            filterGrid = Mgrid;
        }

        /// <summary>
        /// Renders the displays one after the other; a display can need several steps (c() returns true when done).
        /// </summary>
        class DisplayJob : Job
        {
            int _index;

            public override void InitJob()
            {
                _index = 0;
            }

            public override RunJobResult RunJob()
            {
                if (_index >= sms_disp.Count) return RunJobResult.Finished;
                var display = sms_disp[_index];
                var instructionsBefore = InstructionCount();
                if (display.RenderStep()) _index++;
                display.icount += InstructionCount() - instructionsBefore;
                return RunJobResult.Continue;
            }
        }
    }
}
