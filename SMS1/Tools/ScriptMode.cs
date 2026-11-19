using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        // Script mode and standby, determined at the start of every cycle (part of Program, see Cycle.cs):
        // - the mode: STATION, SHIP or SHIP_PLUS from the config, or AUTO by the grid
        //   (static grid: STATION, connectors named ",DOCK:": SHIP_PLUS, else SHIP);
        // - standby: another SMS on the construct is the master (networking), or a ship (SHIP) without pilot.
        // Also reads the ship controller (speed, mass, gravity for the displays).

        // what SMS does in this cycle (PB info and update frequency)
        enum SmsState { Starting, Standby, NoPilot, Running }

        // ScriptMode of the config, and the mode of this cycle (AUTO resolved)
        ScriptMode configuredMode = ScriptMode.AUTO, currentMode;
        SmsState state = SmsState.Starting;
        // TargetCycleTime of the config, 0 = automatic (station 3 s, ship 1 s)
        double configuredCycleSeconds;
        bool shipControlled;
        // values of the ship controller for the displays (ReadShipController)
        static double shipTotalMass = 0, naturalGravity = 0, shipSpeed = 0;

        double TargetCycleSeconds()
        {
            return configuredCycleSeconds > 0 ? configuredCycleSeconds : (currentMode == ScriptMode.STATION ? 3 : 1);
        }

        string ModeName()
        {
            return currentMode == ScriptMode.STATION ? "Stationmode" : currentMode == ScriptMode.SHIP ? "Shipmode" : "Shipmode (plus)";
        }

        // at the start of every cycle: the mode, the ship controller, the state and the update frequency
        bool IsStandby()
        {
            if (configuredMode == ScriptMode.AUTO)
            {
                if (Mgrid.IsStatic) currentMode = ScriptMode.STATION;
                else currentMode = HasDockPorts() ? ScriptMode.SHIP_PLUS : ScriptMode.SHIP;
            }
            else currentMode = configuredMode;
            ReadShipController();

            // at start, while the other SMS answer: no standby yet (Update10, so the answers come in fast)
            if (Net.IsDiscovering) state = SmsState.Starting;
            // the master is decided by the networking
            else if (!Net.IsMaster) state = SmsState.Standby;
            else if (currentMode == ScriptMode.SHIP && !shipControlled) state = SmsState.NoPilot;
            else state = SmsState.Running;

            if (state == SmsState.Standby || state == SmsState.NoPilot)
            {
                rti.UpdateFrequency = UpdateFrequency.Update100;
                instructionLimit = InstructionBudget.INSTRUCTION_MIN;
            }
            // the budget is adjusted at the end of every cycle (EndCycleMeasuring)
            else if (state == SmsState.Running) rti.UpdateFrequency = UpdateFrequency.Update10;
            return state != SmsState.Running;
        }

        bool HasDockPorts()
        {
            if (docks.Count > 0) return true;
            GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(listTerminalBlock, block => block.CubeGrid == Mgrid);
            foreach (var d in listTerminalBlock)
            {
                if (d.CustomName.ToUpper().Contains(",DOCK:")) return true;
            }
            return false;
        }

        // the controller under control, else the main cockpit: its values for the displays
        void ReadShipController()
        {
            var controllers = new List<IMyShipController>();
            GridTerminalSystem.GetBlocksOfType(controllers, block => block.CubeGrid == Mgrid);
            IMyShipController controller = null;
            shipSpeed = 0;
            naturalGravity = 0;
            shipControlled = false;
            foreach (var c in controllers)
            {
                if (c.IsUnderControl)
                {
                    shipControlled = true;
                    if (controller == null) controller = c;
                }
                else if (c.IsMainCockpit)
                {
                    if (controller == null) controller = c;
                }
            }
            if (controller != null)
            {
                shipSpeed = controller.GetShipSpeed();
                var mass = controller.CalculateShipMass();
                shipTotalMass = mass.TotalMass;
                sMb.sM(mass.BaseMass);
                sMt.sM(mass.TotalMass);
                sMc.sM(mass.TotalMass - mass.BaseMass);
                naturalGravity = controller.GetNaturalGravity().Length();
                grn.sG(controller.GetNaturalGravity().Length());
                gra.sG(controller.GetArtificialGravity().Length());
            }
        }
    }
}
