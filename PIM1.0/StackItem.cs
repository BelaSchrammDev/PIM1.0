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
        StackItem GetStackItem(MyItemType t) { foreach (var s in StackItemList) if (s.type == t) return s; var nt = new StackItem(t); StackItemList.Add(nt); return nt; }
        static string GetPIMItemID(MyItemType type) { return type.TypeId.Substring(type.TypeId.IndexOf('_') + 1) + " " + type.SubtypeId; }

        class StackItem : IComparable<StackItem>
        {
            public enum StackingType { Stack, Volume, VolumeBack, }
            static public StackingType CurrentStackingType = StackingType.Stack;
            public enum StackingSort { Stack, Amount, AmountBack, Delta, ItemsBack, VolumeFree, VolumeFreeBack, }
            static public StackingSort CurrentStackingSorttype = StackingSort.Stack;
            static IMyInventory big = null;
            static IMyInventory free = null;
            static public void ClearStackInventory() { big = null; free = null; }
            static public void CalculateFreeInventory(IMyInventory inv) { if (big == null || (big.MaxVolume < inv.MaxVolume)) big = inv; if (free == null || (free.MaxVolume - free.CurrentVolume < inv.MaxVolume - inv.CurrentVolume)) free = inv; }
            class Stack : IComparable<Stack>
            {
                public int items = 0;
                public float amount = 0;
                public float volume_free = 0;
                public IMyInventory inv = null;
                public Stack(IMyInventory i, MyFixedPoint a) { amount = (float)a; inv = i; refresh(); }
                public void refresh() { items = inv.ItemCount; volume_free = (float)(inv.MaxVolume - inv.CurrentVolume); }
                public float GetMaxVolume() { return (float)inv.MaxVolume; }
                public int CompareTo(Stack other)
                {
                    if (CurrentStackingSorttype == StackingSort.Amount) { if (other.amount == amount) return 0; return other.amount > amount ? 1 : -1; }
                    else if (CurrentStackingSorttype == StackingSort.AmountBack) { if (other.amount == amount) return 0; return other.amount < amount ? 1 : -1; }
                    else if (CurrentStackingSorttype == StackingSort.Delta) { if (items == 1 && other.items == 1) return 0; else if (items == 1) return 1; if (other.amount == amount) return 0; return other.amount < amount ? 1 : -1; }
                    else if (CurrentStackingSorttype == StackingSort.ItemsBack) { if (other.items == items) return 0; return other.items < items ? 1 : -1; }
                    else if (CurrentStackingSorttype == StackingSort.VolumeFree) { if (other.volume_free == volume_free) return 0; return other.volume_free > volume_free ? 1 : -1; }
                    else if (CurrentStackingSorttype == StackingSort.VolumeFreeBack) { if (other.volume_free == volume_free) return 0; return other.volume_free < volume_free ? 1 : -1; }
                    return 0;
                }
            }

            public MyItemType type;
            float typevolume = 1;
            int stacks = 0;
            public int Stackcount { get { return stacks; } }
            float amount = 0;
            float volume = 0;
            List<Stack> invs = new List<Stack>();
            IMyInventory quelle = null, ziel = null;
            public StackItem(MyItemType itype) { type = itype; typevolume = type.GetItemInfo().Volume; }
            void refreshInvs() { foreach (var i in invs) i.refresh(); }
            public void AddStack(IMyInventory inv, MyFixedPoint am) { stacks++; amount += (float)am; volume = amount * typevolume; invs.Add(new Stack(inv, am)); }
            public int CompareTo(StackItem other)
            {
                if (CurrentStackingType == StackingType.Stack) { if (other.stacks == stacks) { if (other.amount == amount) return 0; return other.amount > amount ? 1 : -1; } return other.stacks > stacks ? 1 : -1; }
                else if (CurrentStackingType == StackingType.Volume) { if (other.volume == volume) return 0; return other.volume > volume ? 1 : -1; }
                else if (CurrentStackingType == StackingType.VolumeBack) { if (other.volume == volume) return 0; return other.volume < volume ? 1 : -1; }
                return 0;
            }
            public bool check_stacking_gamma()
            {
                if (stacks == 2)
                {
                    if (invs[0].GetMaxVolume() > volume)
                    {
                        quelle = invs[1].inv;
                        ziel = invs[0].inv;
                        return true;
                    }
                    else if (invs[1].GetMaxVolume() > volume)
                    {
                        quelle = invs[0].inv;
                        ziel = invs[1].inv;
                        return true;
                    }
                }
                return false;
            }
            public bool stacking_gamma()
            {
                var von = new List<MyInventoryItem>();
                ziel.GetItems(von);
                bool zielleer = true;
                for (int x = von.Count - 1; x >= 0; x--)
                {
                    var i = von[x];
                    if (i.Type != type)
                    {
                        if (!quelle.TransferItemFrom(ziel, i, null)) zielleer = false;
                    }
                }
                var item = quelle.FindItem(type);
                if (item != null) quelle.TransferItemTo(ziel, (MyInventoryItem)item, null);
                return (zielleer && item == null);
            }
            public bool stacking_beta()
            {
                if (stacks < 2) return true;
                if (((float)(free.MaxVolume - free.CurrentVolume)) < volume) return false;
                foreach (var i in invs)
                {
                    if (i.inv != free)
                    {
                        var item = i.inv.FindItem(type);
                        if (item != null)
                        {
                            free.TransferItemFrom(i.inv, (MyInventoryItem)item, null);
                        }
                    }
                }
                return true;
            }
            public void stacking_delta()
            {
                if (stacks > 1)
                {
                    refreshInvs();
                    CurrentStackingSorttype = StackingSort.Delta;
                    invs.Sort();
                    int x = 0;
                    int y = invs.Count - 1;
                    for (; x < y; y--)
                    {
                        var item = invs[y].inv.FindItem(type);
                        if (item != null)
                        {
                            if (invs[x].inv.TransferItemFrom(invs[y].inv, (MyInventoryItem)item, null)) x++;
                        }
                    }
                }
            }
            public void stacking_alpha()
            {
                if (stacks < 2) return;
                refreshInvs();
                CurrentStackingSorttype = StackingSort.VolumeFree;
                invs.Sort();
                if (invs[0].volume_free < volume)
                {
                    var ziel = invs[0].inv;
                    CurrentStackingSorttype = StackingSort.AmountBack;
                    invs.Sort();
                    foreach (var st in invs)
                    {
                        if (st.inv != ziel)
                        {
                            var item = st.inv.FindItem(type);
                            if (item != null)
                            {
                                ziel.TransferItemFrom(st.inv, (MyInventoryItem)item, null);
                            }
                        }
                    }
                }
                else
                {
                    for (int x = 1; x < invs.Count - 1; x++)
                    {
                        var i = invs[x].inv;
                        var item = i.FindItem(type);
                        if (item != null)
                        {
                            invs[0].inv.TransferItemFrom(i, (MyInventoryItem)item, null);
                        }
                    }
                }
            }
            public void stacking_single()
            {
                if (stacks < 2) return;
                for (int x = invs.Count - 1; x > 0; x--)
                {
                    var i = invs[x].inv;
                    for (int y = x - 1; y >= 0; y--)
                    {
                        if (i == invs[y].inv)
                        {
                            var vv = new List<MyInventoryItem>();
                            i.GetItems(vv, ooo => ooo.Type == type);
                            if (vv.Count > 1)
                            {
                                i.TransferItemFrom(i, vv[vv.Count - 1], vv[vv.Count - 1].Amount);
                            }
                        }
                    }
                }
            }
        }
    }
}
