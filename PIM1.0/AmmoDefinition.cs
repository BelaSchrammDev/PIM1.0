using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript
{
    partial class Program
    {
        public class AmmoDefs : IComparable<AmmoDefs>
        {
            static string CurrentSortGuntype = "";
            static public void SetCurrentSortGuntype(string type) { CurrentSortGuntype = type; }

            private readonly string Name = "";
            private readonly string PrioDefName = "";
            public string type = "";
            private readonly Dictionary<string, int> gunAmmoPrio = new Dictionary<string, int>();
            private readonly AssemblerBluePrint ammoBluePrint;
            public float ratio = 1, maxOfVolume = 0;
            public List<Gun> guns = new List<Gun>();

            public int CompareTo(AmmoDefs other)
            {
                var prio = GetAmmoPriority(CurrentSortGuntype);
                var otherprio = other.GetAmmoPriority(CurrentSortGuntype);
                if (prio == otherprio)
                {
                    return 0;
                }

                return otherprio > prio ? 1 : -1;
            }

            public AmmoDefs(string iname)
            {
                Name = iname;
                ammoBluePrint = Program.Instance.GetBluePrintByItemName(iname);
                type = Name.Substring(Name.IndexOf(' ') + 1);
                PrioDefName = ammoBluePrint == null ? type : ammoBluePrint.AutoCraftingName;
            }

            public void SetAmmoPriority(string iType, int iPrio)
            {
                if (iPrio < 0)
                {
                    iPrio = 0;
                }
                else if (iPrio > 10)
                {
                    iPrio = 10;
                }

                if (!gunAmmoPrio.ContainsKey(iType))
                {
                    gunAmmoPrio.Add(iType, iPrio);
                }
                else
                {
                    gunAmmoPrio[iType] = iPrio;
                }
            }

            public int GetAmmoPriority(string gunType)
            {
                return gunAmmoPrio.ContainsKey(gunType) ? gunAmmoPrio[gunType] : 0;
            }

            public string GetAmmoBluePrintAutocraftingName()
            {
                return PrioDefName;
            }

            public void CalcAmmoInventoryRatio()
            {
                if (inventar.ContainsKey(Name))
                {
                    ratio = inventar[Name] / maxOfVolume;

                    if (ratio > 1)
                    {
                        ratio = 1;
                    }
                }
                else ratio = 1;
            }
        }
    }
}
