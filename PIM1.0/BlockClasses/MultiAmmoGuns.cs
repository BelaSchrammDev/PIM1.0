using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript
{
    partial class Program
    {
        class MultiAmmoGuns
        {
            public string DisplayName = "";
            public string MultiAmmoGuntype = "";
            List<Gun> MultiAmmoGunList = new List<Gun>();
            public List<AmmoDefs> ammoDefs = new List<AmmoDefs>();
            public MultiAmmoGuns(string type, string dName)
            {
                MultiAmmoGuntype = type;
                DisplayName = dName;
            }
            public void addAmmoDef(AmmoDefs aDef)
            {
                if (!ammoDefs.Contains(aDef))
                {
                    ammoDefs.Add(aDef);
                    aDef.SetAmmoPriority(MultiAmmoGuntype, ammoDefs.Count);
                }
            }
            public void addGun(Gun mGun)
            {
                if (!MultiAmmoGunList.Contains(mGun)) MultiAmmoGunList.Add(mGun);
            }
            public void removeGun(Gun mGun)
            {
                if (MultiAmmoGunList.Contains(mGun)) MultiAmmoGunList.Remove(mGun);
            }
            public bool if_GunListEmpty()
            {
                return MultiAmmoGunList.Count == 0;
            }
            public AmmoDefs GetAmmoDefs(string _type)
            {
                return ammoDefs.Find(a => a.type == _type);
            }
        }
        static Dictionary<string, MultiAmmoGuns> multiAmmoGuns = new Dictionary<string, MultiAmmoGuns>();
        static MultiAmmoGuns getNewMultiAmmoGun(Gun mGun)
        {
            if (!multiAmmoGuns.ContainsKey(mGun.gunType))
            {
                multiAmmoGuns.Add(mGun.gunType, new MultiAmmoGuns(mGun.gunType, mGun.gun.DefinitionDisplayNameText));
                multiAmmoGuns[mGun.gunType].addGun(mGun);
                return multiAmmoGuns[mGun.gunType];
            }
            multiAmmoGuns[mGun.gunType].addGun(mGun);
            return null;
        }
        static void clearMultiAmmoGunsList() // ToDo: wird das noch gebraucht oder kann das weg???
        {
            var keyList = multiAmmoGuns.Keys.ToArray();
            for (int i = keyList.Length - 1; i >= 0; i--)
            {
                if (multiAmmoGuns[keyList[i]].if_GunListEmpty()) multiAmmoGuns.Remove(keyList[i]);
            }
        }
        static AmmoDefs getAmmoDefs(string name) { if (!ammoDefs.ContainsKey(name)) ammoDefs.Add(name, new AmmoDefs(name)); return ammoDefs[name]; }
    }
}
