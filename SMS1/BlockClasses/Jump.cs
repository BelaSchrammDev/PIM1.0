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
        //Jump
        public class Jump : dyBlockPowerConsumer
        {
            IMyJumpDrive jumpDrive;
            public Jump(IMyTerminalBlock b) { addChangeType = ChangeType.Jumpdrive_add; nameChangeType = ChangeType.Jumpdrive_name; tb = b; jumpDrive = (IMyJumpDrive)b; getBlockDefinition(); bt = "JumpDrive"; setBlockTypeFields(); addChange(); getVC('S'); }
            public override void getPCvar(dyDisplayableVariable pv)
            {
                var mjd = 0f;
                var mi = 0.0;
                var ci = 0.0;
                var d = getDetailedInfo();
                if (d.Length > 3)
                {
                    mi = getPstr(d[1]);
                    ci = getPstr(d[3]);
                }
                if (d.Length > 6)
                {
                    mjd = gINT(d[6]);
                    pv.sV('J', mjd, mjd);
                }
                sD("@fMJ", mjd, 20);
                sV('I', iEF() ? mi : 0, ci, pv);
                sV('S', jumpDrive.MaxStoredPower, jumpDrive.CurrentStoredPower, pv);
                sT("@fChT", ci > 0 ? fT((jumpDrive.MaxStoredPower - jumpDrive.CurrentStoredPower) / ci) : " ", 20);
                sT("@fSt", iEF() ? jumpDrive.Status.ToString() : "Offline", 20);
            }
        }
    }
}
