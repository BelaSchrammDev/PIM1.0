using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript
{
    partial class Program
    {
        public class AmmoDefs
        {
            public static AmmoDefs GetAmmoDefs(string name)
            {
                if (!Lists.Data.AmmoDefinitions.ContainsKey(name))
                {
                    Lists.Data.AmmoDefinitions.Add(name, new AmmoDefs(name));
                }

                return Lists.Data.AmmoDefinitions[name];
            }

            private readonly string Name = "";
            public string type = "";
            private readonly AssemblerBluePrint ammoBluePrint;

            public float ratio = 1
                , maxOfVolume = 0;

            public List<Gun> guns = new List<Gun>();

            public AmmoDefs(string iname)
            {
                Name = iname;
                ammoBluePrint = Program.MyProgram.GetBluePrintByItemName(iname);
                type = ItemTypesConverter.SubtypeOf(Name);
            }

            public void CalcAmmoInventoryRatio()
            {
                ratio = Lists.Data.Inventory.ContainsKey(Name)
                    ? Math.Min(Lists.Data.Inventory[Name] / maxOfVolume, 1)
                    : 1;
            }
        }
    }
}
