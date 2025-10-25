using System;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript
{
    partial class Program
    {
        public class Propertys
        {
            private static Propertys _data = null;
            public static Propertys Data
            {
                get
                {
                    if (_data == null)
                    {
                        _data = new Propertys();
                    }
                    return _data;
                }
            }
            
            public double CurrentCycleInSec = 0;
            public DateTime LastStart = DateTime.Now;
            public bool AutoCraftingSettingsInValid = true;
            public string CurrentGunGroupName = Strings.DefaultGunGroupName;
        }
    }
}
