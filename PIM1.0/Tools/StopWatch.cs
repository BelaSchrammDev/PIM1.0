using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript
{
    partial class Program
    {
        public class StopWatch
        {
            DateTime ls = DateTime.Now;
            int sec;
            public StopWatch(int isec = 5) { sec = isec; }
            public bool IfDone(bool rs = true)
            {
                if (sec == 0) return false;
                if ((DateTime.Now - ls).TotalSeconds > sec)
                {
                    if (rs) ls = DateTime.Now;
                    return true;
                }
                return false;
            }
        }
    }
}
