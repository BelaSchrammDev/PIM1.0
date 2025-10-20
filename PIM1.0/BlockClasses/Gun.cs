using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        //TODO: storage inventory list as static in StorageInventory
        public class Gun : StorageInventory
        {

            public IMyUserControllableGun gun = null;
            public string gunType = "";
            public string CurrentAmmo = "";
            public Dictionary<string, int> ammomax = new Dictionary<string, int>();
            public Gun(IMyUserControllableGun g)
            {
                gun = g;
                gunType = gun.BlockDefinition.SubtypeId;
                inv = g.GetInventory();
                List<MyItemType> ammotypes = new List<MyItemType>();
                inv.GetAcceptedItems(ammotypes);

                foreach (var a in ammotypes)
                {
                    if (a.TypeId.EndsWith(IG_Ammo) && a.SubtypeId != "Energy")
                    {
                        var mtype = IG_Ammo + ' ' + a.SubtypeId;
                        var newadef = AmmoDefs.GetAmmoDefs(mtype);
                        newadef.guns.Add(this);
                        var amax = (int)((float)inv.MaxVolume / a.GetItemInfo().Volume);
                        newadef.maxOfVolume += amax;
                        ammomax.Add(mtype, amax);
                    }
                }

                storageinvs.Add(this);
            }
            public override bool CheckItems()
            {
                if (gun is IMyLargeInteriorTurret || CurrentAmmo == "") return false;

                int aamount = (int)(ammomax[CurrentAmmo] * ammoDefs[CurrentAmmo].ratio);
                if (aamount < 1) aamount = 1;
                if (items.Count == 0) items.Add(CurrentAmmo, aamount);
                else if (!items.ContainsKey(CurrentAmmo))
                {
                    items.Clear();
                    items.Add(CurrentAmmo, aamount);
                }
                else items[CurrentAmmo] = aamount;
                return true;
            }

            public void Refresh()
            {
                var propertyUseConveyor = gun.GetProperty(X_UseConveyor);
                if (propertyUseConveyor != null && gun.GetValue<bool>(X_UseConveyor))
                {
                    gun.ApplyAction(X_UseConveyor);
                }

                CurrentAmmo = GetCurrentAmmo();
            }

            string GetCurrentAmmo()
            {
                if (ammomax.Count == 0) return "";
                else if (ammomax.Count == 1) return ammomax.Keys.First();
                var currentAmmunition = "";
                var currentAmmunitionPrio = 0;
                foreach (var a in ammomax)
                {
                    var prio = AmmoDefs.GetAmmoDefs(a.Key).GetAmmoPriority(gunType);
                    if (prio > currentAmmunitionPrio && inventar.ContainsKey(a.Key) && inventar[a.Key] > 0)
                    {
                        currentAmmunition = a.Key;
                        currentAmmunitionPrio = prio;
                    }
                }
                return currentAmmunition;
            }

            public void Remove()
            {
                var keyList = ammoDefs.Keys.ToArray();
                for (int i = ammoDefs.Count - 1; i >= 0; i--)
                {
                    if (ammoDefs[keyList[i]].guns.Contains(this))
                    {
                        ammoDefs[keyList[i]].guns.Remove(this);
                        if (ammoDefs[keyList[i]].guns.Count == 0) ammoDefs.Remove(keyList[i]);
                        else ammoDefs[keyList[i]].maxOfVolume -= ammomax[keyList[i]];
                        break;
                    }
                }
                if (storageinvs.Contains(this)) storageinvs.Remove(this);
                var p = gun.GetProperty(X_UseConveyor);
                if (p != null && !gun.GetValue<bool>(X_UseConveyor))
                {
                    gun.ApplyAction(X_UseConveyor);
                }
            }
        }
    }
}
