using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript
{
    partial class Program
    {
        public class CargoUse
        {
            public string type = "";
            public double Current = 0, Maximum = 0;
            public CargoUse(string s)
            {
                type = s;
            }

            public void AddCurrentAndMaxCargocapacity(double c, double m)
            {
                Current += c;
                Maximum += m;
            }

            public int GetCarcocapacityUseRatio()
            {
                return (int)(Current * 100 / Maximum);
            }
        }
    }
}
