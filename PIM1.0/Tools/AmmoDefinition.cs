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
            private readonly Dictionary<string, int> gunAmmoPrio = new Dictionary<string, int>();
            private readonly AssemblerBluePrint ammoBluePrint;

            public float ratio = 1
                , maxOfVolume = 0;

            public List<Gun> guns = new List<Gun>();

            public AmmoDefs(string iname)
            {
                Name = iname;
                ammoBluePrint = Program.Instance.GetBluePrintByItemName(iname);
                type = Name.Substring(Name.IndexOf(' ') + 1);
            }

            public int GetAmmoPriority(string gunType)
            {
                return gunAmmoPrio.ContainsKey(gunType) ? gunAmmoPrio[gunType] : 0;
            }

            public void CalcAmmoInventoryRatio()
            {
                ratio = Lists.Data.inventar.ContainsKey(Name)
                    ? Math.Min(Lists.Data.inventar[Name] / maxOfVolume, 1)
                    : 1;
            }
        }
    }
}
