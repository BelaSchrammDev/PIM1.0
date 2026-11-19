using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        // the thrusters of the main grid: thrust per direction and type (Ion, Atmo, Hydro, Azimuth), for the displays
        List<IMyThrust> th = new List<IMyThrust>();
        List<IMyThrust> CollectThrusters()
        {
            filterGrid = Mgrid;
            GridTerminalSystem.GetBlocksOfType<IMyThrust>(th, BlockGridCheck);
            foreach (VC v in dyDisplayableVariable.getPV(null, "WtoT").vcl) v.sF(Fm);
            return th;
        }
        void ProcessThruster(IMyThrust thrustBlock)
        {
            var typAzimuth = "Azimuth";
            var typHydro = "Hydro";
            var maxThrust = thrustBlock.MaxEffectiveThrust;
            var curThrust = thrustBlock.CurrentThrust;
            var blockDef = thrustBlock.BlockDefinition.ToString();
            var typThrust = "Ion";
            var a = blockDef.Contains(typAzimuth) ? typAzimuth : "";
            if (blockDef.Contains("Atmo")) typThrust = "Atmo";
            else if (blockDef.Contains(typHydro)) typThrust = typHydro;
            dyDisplayableVariable p = dyDisplayableVariable.getPV(null, "Thrust Summary", true, "xXyYzZ");
            dyDisplayableVariable pv = dyDisplayableVariable.getPV(null, a + typThrust + "Thrust", true, "xXyYzZ");
            dyDisplayableVariable pvw = dyDisplayableVariable.getPV(null, a + typThrust + "Thrusters");
            char o = ' ';
            var O = thrustBlock.GridThrustDirection;
            if (O.X != 0) o = O.X == 1 ? 'X' : 'x';
            else if (O.Y != 0) o = O.Y == 1 ? 'Y' : 'y';
            else if (O.Z != 0) o = O.Z == 1 ? 'Z' : 'z';
            pv.aV(o, maxThrust, curThrust);
            p.aV(o, maxThrust, curThrust);
            pvw.aV('t', thrustBlock.MaxThrust, maxThrust);
            if (typThrust != typHydro)
            {
                var maxPowerusage = gP(thrustBlock);
                var curPowerusage = maxThrust == 0 ? 0.000002 : (maxPowerusage * (curThrust / maxThrust));
                pvw.aV('I', maxPowerusage, thrustBlock.Enabled ? curPowerusage : 0);
            }
        }
    }
}
