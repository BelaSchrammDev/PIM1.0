using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public abstract class StorageInventory
        {
            public IMyInventory inv = null;
            public Dictionary<string, float> items = new Dictionary<string, float>();
            abstract public bool checkItems();
            public void reloadItems()
            {
                if (!checkItems()) return;
                var invList = new List<MyInventoryItem>();
                var succlist = new List<string>();
                var einleiten = new Dictionary<string, float>();
                inv.GetItems(invList);
                for (int i = invList.Count - 1; i >= 0; i--)
                {
                    var iList = new List<MyInventoryItem>();
                    inv.GetItems(iList, v => v.Type == invList[i].Type);
                    if (iList.Count > 1)
                    {
                        inv.TransferItemTo(inv, iList[iList.Count - 1]);
                    }
                }
                invList.Clear();
                inv.GetItems(invList);
                for (int i = invList.Count - 1; i >= 0; i--)
                {
                    var iItem = invList[i];
                    var iType = GetPIMItemID(iItem.Type);
                    if (!items.ContainsKey(iType)) clearItemByType(inv, iType, iItem);
                    else
                    {
                        var adiff = items[iType] - (float)iItem.Amount;
                        if (adiff < 0)
                        {
                            clearItemByType(inv, iType, iItem, Math.Abs(adiff));
                            succlist.Add(iType);
                        }
                        else if (adiff == 0) succlist.Add(iType);
                        else einleiten.Add(iType, adiff);
                    }
                }
                foreach (var i in items.Keys)
                {
                    if (succlist.Contains(i)) continue;
                    SendItemByType(i, (einleiten.ContainsKey(i) ? einleiten[i] : items[i]), inv);
                }
            }
        }

        class StorageCargo : StorageInventory
        {
            public IMyCargoContainer container = null;
            string oldCustomdata = "";
            public StorageCargo(IMyTerminalBlock cargoContainer)
            {
                container = cargoContainer as IMyCargoContainer;
                inv = container.GetInventory();
                storageinvs.Add(this);
            }
            const string X_ItemDef = "StorageItemDefinition", X_ItemDefBegin = "### " + X_ItemDef + "_begin ###", X_ItemDefEnd = "### " + X_ItemDef + "_end ###", X_AddToList = "add_to_list:";
            public override bool checkItems()
            {
                if (container.CustomData != "" && container.CustomData == oldCustomdata) return true;
                bool itemsdef = false;
                var searchstring = "";
                items.Clear();
                foreach (var s in container.CustomData.Split('\n'))
                {
                    var trims = s.Trim();
                    if (trims.StartsWith("/")) continue;
                    else if (trims.Contains(X_ItemDefBegin)) itemsdef = true;
                    else if (trims.Contains(X_ItemDefEnd)) itemsdef = false;
                    else if (!itemsdef && trims.StartsWith(X_AddToList)) searchstring = trims;
                    else if (itemsdef)
                    {
                        var def = trims.Split(';');
                        if (def.Length > 1)
                        {
                            int amount = 0;
                            if (inventar.ContainsKey(def[1]) && int.TryParse(def[0], out amount))
                            {
                                if (amount != 0) items.Add(def[1], amount);
                            }
                        }
                    }
                }
                if (searchstring != "")
                {
                    var search = searchstring.Split(',', ':');
                    for (int i = 1; i < search.Length; i++)
                    {
                        var setr = search[i].Trim().ToLower();
                        if (setr == "") continue;
                        foreach (var t in inventar.Keys)
                        {
                            if (t.ToLower().Contains(setr) && !items.ContainsKey(t))
                            {
                                items.Add(t, 1);
                            }
                        }
                    }
                }
                var cdata = "  / Itemdefinitionen:\n  / amount and type of items to be stored in the container\n  /\n  / add items to the list:\n  / write search terms after the '" + X_AddToList + "', like 'steel' or 'tube'.\n  / close the window, after a few seconds you will find\n  / relevant items in the list below.\n" + X_AddToList + "\n" + X_Line;
                cdata += "  / List of items, delete the lines that are no longer needed,\n  / or set the value to 0.\n  / please change only the value before the semicolon\n" + X_ItemDefBegin + "\n";
                foreach (var i in items) { cdata += i.Value + ";" + i.Key + "\n"; }
                cdata += X_ItemDefEnd + "\n";
                container.CustomData = cdata;
                oldCustomdata = cdata;
                return true;
            }
            public void Remove()
            {
                storageinvs.Remove(this);
            }
        }
        class Gun : StorageInventory
        {
            public IMyUserControllableGun gun = null;
            public string gunType = "";
            public string CurrentAmmo = "";
            public List<AmmoDefs> ammoTypesDefinition = new List<AmmoDefs>();
            public Dictionary<string, int> ammomax = new Dictionary<string, int>();
            public Gun(IMyUserControllableGun g)
            {
                gun = g;
                gunType = gun.BlockDefinition.SubtypeId;
                inv = g.GetInventory();
                List<MyItemType> ammotypes = new List<MyItemType>();
                inv.GetAcceptedItems(ammotypes);
                var ammoTypesCount = 0;
                MultiAmmoGuns newMultiAmmoGun = null;
                foreach (var a in ammotypes) if (a.TypeId.EndsWith(IG_Ammo) && a.SubtypeId != "Energy") ammoTypesCount++;
                if ((ammoTypesCount > 1) && !(gun is IMyLargeInteriorTurret))
                {
                    newMultiAmmoGun = getNewMultiAmmoGun(this);
                }
                var multiAmmo = (ammoTypesCount > 1) && !(gun is IMyLargeInteriorTurret) ? true : false;
                foreach (var a in ammotypes)
                {
                    if (a.TypeId.EndsWith(IG_Ammo) && a.SubtypeId != "Energy")
                    {
                        var mtype = IG_Ammo + ' ' + a.SubtypeId;
                        var newadef = getAmmoDefs(mtype);
                        newadef.guns.Add(this);
                        ammoTypesDefinition.Add(newadef);
                        var amax = (int)((float)inv.MaxVolume / a.GetItemInfo().Volume);
                        newadef.maxOfVolume += amax;
                        ammomax.Add(mtype, amax);
                        if (newMultiAmmoGun != null) newMultiAmmoGun.addAmmoDef(newadef);
                    }
                }
                storageinvs.Add(this);
            }
            public override bool checkItems()
            {
                if (gun is IMyLargeInteriorTurret) return false;
                // zum testen..................
                if (CurrentAmmo == "") return false;
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
                AddToInventory(inv);
                var propertyUseConveyor = gun.GetProperty(X_UseConveyor);
                if (propertyUseConveyor != null && gun.GetValue<bool>(X_UseConveyor)) gun.ApplyAction(X_UseConveyor);
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
                    var prio = getAmmoDefs(a.Key).GetAmmoPriority(gunType);
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
                if (p != null && !gun.GetValue<bool>(X_UseConveyor)) gun.ApplyAction(X_UseConveyor);
            }
        }

    }
}
