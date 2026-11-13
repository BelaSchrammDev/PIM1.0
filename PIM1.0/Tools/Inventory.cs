using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        // CARGOMISSING hint for an item type ("AmmoMagazine" -> "Ammo"), with the category of the whole type
        // as alternative ("AmmoMagazine" -> "Armory", but not "PhysicalGunObject": welders are no weapons)
        static void SetCargoMissing(string itemType)
        {
            SetWarning(Warning.ID.CARGOMISSING, ItemTypesConverter.ToTag(itemType) ?? itemType, ItemTypesConverter.Category(itemType));
        }

        static void ClearInventory(IMyInventory quelle)
        {
            var von = new List<MyInventoryItem>();
            quelle.GetItems(von);
            if (von.Count() == 0) return;
            for (int j = von.Count() - 1; j >= 0; j--)
            {
                var id = ItemTypesConverter.ToPimId(von[j].Type);
                var found = false;
                // the next cargo is tried if the item could not be moved into the more specific one
                foreach (var ziele in ItemTypesConverter.CargoFor(Lists.Data.InventoryManagerList, id))
                {
                    found = true;
                    if (SendItemByNum(quelle, j, ziele)) break;
                }
                if (!found) SetCargoMissing(ItemTypesConverter.TypeOf(id));
            }
        }

        static bool clearItemByType(IMyInventory quelle, string id, MyInventoryItem item, float amount = 0)
        {
            if (amount == 0) amount = (float)item.Amount;
            var found = false;
            // like ClearInventory: the next cargo is tried if the item could not be moved into the more specific one
            foreach (var ziele in ItemTypesConverter.CargoFor(Lists.Data.InventoryManagerList, id))
            {
                found = true;
                if (SendItemByIItem(quelle, item, amount, ziele)) return true;
            }
            if (!found) SetCargoMissing(ItemTypesConverter.TypeOf(id));
            return false;
        }

        static bool SendItemByIItem(IMyInventory quelle, MyInventoryItem item, float amount, List<IMyInventory> ziele)
        {
            var volume = (MyFixedPoint)amount * item.Type.GetItemInfo().Volume;
            if (ziele.Count > 0)
            {
                foreach (var zinv in ziele) if (quelle == zinv) return true;
                for (int i = 0; i < ziele.Count; i++)
                {
                    var ziel_inv = ziele[i];
                    if (volume < (ziel_inv.MaxVolume - ziel_inv.CurrentVolume))
                    {
                        if (quelle.TransferItemTo(ziel_inv, item, (MyFixedPoint)amount)) return true;
                    }
                }
            }
            return false;
        }

        static bool SendItemByNum(IMyInventory quelle, int itemnum, List<IMyInventory> ziele)
        {
            if (ziele.Count != 0)
            {
                foreach (var zinv in ziele) if (quelle == zinv) return true;
                for (int i = 0; i < ziele.Count; i++)
                {
                    var ziel_inv = ziele[i];
                    if (!ziel_inv.IsFull && quelle.TransferItemTo(ziel_inv, itemnum, null, true, null))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>Moves an item ("Ore Ice") from its cargo into <paramref name="ziel"/>.</summary>
        static bool SendItemByType(string id, float itemAmount, IMyInventory ziel, int? p = null)
        {
            var quellen = new List<IMyInventory>();
            // every cargo that takes the item
            foreach (var c in ItemTypesConverter.CargoFor(Lists.Data.InventoryManagerList, id)) quellen.AddRange(c);
            if (quellen.Count == 0)
            {
                SetCargoMissing(ItemTypesConverter.TypeOf(id));
                return false;
            }

            var itemType = ItemTypesConverter.ToItemType(id);
            var von = new List<MyInventoryItem>();
            for (int i = 0; i < quellen.Count; i++)
            {
                von.Clear();
                quellen[i].GetItems(von);
                for (int j = von.Count - 1; j >= 0; j--)
                {
                    if (von[j].Type == itemType && quellen[i].TransferItemTo(ziel, j, p, true, (MyFixedPoint)itemAmount)) return true;
                }
            }
            return false;
        }

        static void ClearInventoryList(Dictionary<string, float> invList)
        {
            var keys = invList.Keys.ToArray();
            for (int i = 0; i < keys.Length; i++) invList[keys[i]] = 0;
        }

        /// <summary>Adds the amounts of all items of <paramref name="box"/> to <paramref name="amounts"/> (key: PIM item id).</summary>
        static void CountItems(IMyInventory box, Dictionary<string, float> amounts)
        {
            var boxl = new List<MyInventoryItem>();
            box.GetItems(boxl);
            foreach (var boxi in boxl)
            {
                string index = ItemTypesConverter.ToPimId(boxi.Type);
                var boxia = (float)boxi.Amount;
                if (amounts.ContainsKey(index)) amounts[index] += boxia;
                else amounts.Add(index, boxia);
            }
        }

        void AddInventoryToInventoryManagerList(IMyInventory inv, Dictionary<string, string> tags)
        {
            foreach (var tag in tags.Keys)
            {
                // "Keep" is an option, no cargo type (ToIngame returns unknown tags unchanged)
                if (tag == Strings.X_Keep) continue;
                var ingame = ItemTypesConverter.ToIngame(tag);
                if (ingame != "")
                {
                    if (!Lists.Data.InventoryManagerList.ContainsKey(ingame)) Lists.Data.InventoryManagerList.Add(ingame, new List<IMyInventory>());
                    if (!Lists.Data.InventoryManagerList[ingame].Contains(inv)) Lists.Data.InventoryManagerList[ingame].Add(inv);
                    if (!Lists.Data.CargoUseList.ContainsKey(tag)) Lists.Data.CargoUseList.Add(tag, new CargoUse(tag));
                    Lists.Data.CargoUseList[tag].AddCapacityValues(inv.CurrentVolume.RawValue / 1000, inv.MaxVolume.RawValue / 1000);
                }
            }
        }
    }
}
