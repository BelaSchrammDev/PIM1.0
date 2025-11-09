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
            public string Type = "";
            public double CurrentCapacity = 0, MaximumCapacity = 0;

            public CargoUse(string s)
            {
                Type = s;
            }

            public void AddCapacityValues(double currentCapacity, double maxCapacity)
            {
                CurrentCapacity += currentCapacity;
                MaximumCapacity += maxCapacity;
            }

            public int GetCarcoCapacityUseRatio()
            {
                return (int)(CurrentCapacity * 100 / MaximumCapacity);
            }
        }
    }
}
