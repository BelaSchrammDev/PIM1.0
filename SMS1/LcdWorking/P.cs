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
        //PM
        public class P
        {
            Dictionary<string, string> pml = new Dictionary<string, string>(); string bcn = ""; IMyTerminalBlock block = null; public string Name = ""; public bool smscontrol;
            public bool ParseArgs(IMyTerminalBlock tb) { if (tb == null) return false; block = tb; return ParseArgs(tb.CustomName); }
            public bool ParseArgs(string newa)
            {
                if (newa == bcn) return smscontrol;
                else bcn = newa;
                pml.Clear();
                var args = bcn.ToLower();
                if (args.Contains("(sms"))
                {
                    Name = bcn.Substring(0, args.IndexOf("(sms"));
                    foreach (var y in args.Split('('))
                    {
                        if (y.Contains("sms") && y.Contains(")"))
                        {
                            foreach (var s in ToArgStr(y).Split(',', ')'))
                            {
                                if (s != "")
                                {
                                    var x = s.IndexOf(':');
                                    if (x > -1) addPM(s.Substring(0, x), s.Substring(x + 1));
                                    else addPM(s);
                                }
                            }
                            smscontrol = true;
                            return true;
                        }
                    }
                }
                Name = newa.Trim();
                smscontrol = false;
                return false;
            }
            public string UpdatePMString()
            {
                if (bcn == null) return "";
                string smsstr = Name;
                if (smscontrol)
                {
                    smsstr += " (sms";
                    foreach (string kstr in pml.Keys)
                    {
                        smsstr += "," + kstr;
                        if (pml[kstr] != "") smsstr += ":" + pml[kstr];
                    }
                    smsstr += ")";
                }
                if (block != null) block.CustomName = smsstr;
                return smsstr;
            }
            public void TakeControl() { smscontrol = true; UpdatePMString(); }
            public void EraseControl() { smscontrol = false; UpdatePMString(); }
            public void addPM(string key, string value = "") { if (!isPM(key)) pml.Add(key, value); }
            public bool isPM(string tag) { return pml.ContainsKey(tag); }
            public string getPM(string tag) { if (pml.ContainsKey(tag)) return pml[tag]; else return ""; }
        }
    }
}
