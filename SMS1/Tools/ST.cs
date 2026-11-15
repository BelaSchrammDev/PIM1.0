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
        //ST
        public class ST
        {
            DateTime ls = DateTime.Now;
            public int sec;
            public ST(int isec = 5)
            {
                sec = isec;
            }
            public void UD()
            {
                ls = DateTime.Now;
            }
            public bool ifS(bool rs = true)
            {
                if (sec == 0) return false;
                if ((DateTime.Now - ls).TotalSeconds > sec)
                {
                    if (rs) UD();
                    return true;
                }
                return false;
            }
        }
    }
}
