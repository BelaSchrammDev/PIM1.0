using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using System;
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
            // accepted ammo in the order of the game: the default priority (ChooseAmmo)
            public List<string> ammoTypes = new List<string>();

            static readonly Func<string, float> AmmoStock = a => Lists.Data.Inventory.GetValueOrDefault(a, 0);
            static readonly Func<string, int> AmmoGunCount = a => AmmoDefs.GetAmmoDefs(a).guns.Count;
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
                        ammoTypes.Add(mtype);
                    }
                }

                Lists.Data.storageinvs.Add(this);
            }
            public override bool ItemsAmountInvalid()
            {
                if (gun is IMyLargeInteriorTurret || CurrentAmmo == "") return false;

                int aamount = (int)(ammomax[CurrentAmmo] * Lists.Data.AmmoDefinitions[CurrentAmmo].ratio);
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
                var propertyUseConveyor = gun.GetProperty(Strings.X_UseConveyor);
                if (propertyUseConveyor != null && gun.GetValue<bool>(Strings.X_UseConveyor))
                {
                    gun.ApplyAction(Strings.X_UseConveyor);
                }

                CurrentAmmo = ChooseAmmo(gunType, ammoTypes, AmmoStock, AmmoGunCount);
            }

            public void Remove()
            {
                // every ammo definition of the gun (a gun with several ammo types is in several of them)
                foreach (var a in ammomax)
                {
                    AmmoDefs def;
                    if (!Lists.Data.AmmoDefinitions.TryGetValue(a.Key, out def)) continue;
                    def.guns.Remove(this);
                    def.maxOfVolume -= a.Value;
                    if (def.guns.Count == 0) Lists.Data.AmmoDefinitions.Remove(a.Key);
                }
                if (Lists.Data.storageinvs.Contains(this)) Lists.Data.storageinvs.Remove(this);
                var p = gun.GetProperty(Strings.X_UseConveyor);
                if (p != null && !gun.GetValue<bool>(Strings.X_UseConveyor))
                {
                    gun.ApplyAction(Strings.X_UseConveyor);
                }
            }
        }
    }
}
