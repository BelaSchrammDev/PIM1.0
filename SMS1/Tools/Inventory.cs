using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        //Inv
        public class inventarList
        {
            MyItemType leer = new MyItemType();
            const int TPMax = 2;
            public int id;
            IMyInventory inv;
            public VC volume = null;
            public Dictionary<MyItemType, float> items = new Dictionary<MyItemType, float>();
            MyItemType[] tp = new MyItemType[TPMax];
            bool[] tpb = new bool[TPMax];
            public inventarList(int iid, IMyInventory i, VC iv) { id = iid; inv = i; volume = iv; }
            public float gA(MyItemType y) { if (items.ContainsKey(y)) return items[y]; return 0; }
            public float gA(int i) { return (i < TPMax && tpb[i]) ? gA(tp[i]) : 0; }
            public MyItemType gIT(int i) { if (i < TPMax && tpb[i]) return tp[i]; else return leer; }
            public String gISt(int i) { if (i < TPMax && tpb[i]) return tp[i].SubtypeId; return ""; }
            public void RefreshInventoryItems(IMyCubeGrid grid, dyDisplayableVariable p)
            {
                volume.C = inv.CurrentVolume.RawValue / 1000;
                volume.M = inv.MaxVolume.RawValue / 1000;
                p.aV('C', volume.M, volume.C);
                var its = new List<MyInventoryItem>();
                inv.GetItems(its);
                gII(its, items);
                int x = 0;
                for (int i = 0; i < TPMax; i++) tpb[i] = false;
                foreach (var i in its)
                {
                    if (x < TPMax)
                    {
                        tpb[x] = true;
                        tp[x++] = i.Type;
                    }
                    else
                        break;
                }
                AddItemsToInventoryDictionary(grid, items);
            }
        }
        //InvMan
        // all dyInventorys: first per grid, then the totals
        List<dyInventory> CollectInventoryItems()
        {
            var l = new List<dyInventory>();
            foreach (var g in gridInventoryItems.Values) l.AddRange(g.Values);
            l.AddRange(invitemsg.Values);
            return l;
        }
        List<IMyCubeGrid> CollectInventoryGrids()
        {
            if (invitemsg.Count == 0) InitInventoryItemsList(invitemsg);
            return new List<IMyCubeGrid>(dict_inv.Keys);
        }
        void InitGridInventoryItems(IMyCubeGrid g)
        {
            if (!gridInventoryItems.ContainsKey(g))
            {
                gridInventoryItems.Add(g, new Dictionary<MyItemType, dyInventory>());
                InitInventoryItemsList(gridInventoryItems[g], g);
            }
        }
        void InitInventoryItemsList(Dictionary<MyItemType, dyInventory> l, IMyCubeGrid g = null)
        {
            foreach (var s in smsItemTypeDefinitions.Keys) if (!l.ContainsKey(s)) l.Add(s, new dyInventory(s, g));
        }
        static MyItemType gIT(string s) { var i = s.IndexOf(' '); return new MyItemType(TypeObjectBuilder + s.Substring(0, i), s.Substring(i + 1)); }
        static MyItemType gITo(string s) { return new MyItemType(TypeObjectBuilder + TypeOre, s); }
        static MyItemType gITi(string s) { return new MyItemType(TypeObjectBuilder + TypeIngot, s); }
        static void AddItemsToInventoryDictionary(IMyCubeGrid g, Dictionary<MyItemType, float> il)
        {
            var invl = GetInventoryDictionaryByGrid(g);
            var k = new List<MyItemType>(il.Keys);
            for (int i = k.Count - 1; i >= 0; i--)
            {
                var a = il[k[i]];
                if (invl.ContainsKey(k[i])) invl[k[i]] += a;
                else invl.Add(k[i], a);
                if (inventar.ContainsKey(k[i])) inventar[k[i]] += a;
                else inventar.Add(k[i], a);
            }
        }
        static Dictionary<MyItemType, float> gII(List<MyInventoryItem> its, Dictionary<MyItemType, float> il = null)
        {
            var k = il == null ? null : new List<MyItemType>(il.Keys);
            if (il == null) il = new Dictionary<MyItemType, float>();
            else
                for (int i = k.Count - 1; i >= 0; i--) il[k[i]] = 0;
            foreach (var i in its)
            {
                if (!il.ContainsKey(i.Type)) il.Add(i.Type, (float)i.Amount);
                else il[i.Type] += (float)i.Amount;
            }
            k = new List<MyItemType>(il.Keys);
            for (int i = k.Count - 1; i >= 0; i--)
            {
                if (il[k[i]] == 0) il.Remove(k[i]);
            }
            return il;
        }
        static Dictionary<MyItemType, float> gII(IMyInventory v, Dictionary<MyItemType, float> il = null)
        {
            var its = new List<MyInventoryItem>();
            v.GetItems(its);
            return gII(its, il);
        }
        static Dictionary<MyItemType, float> GetInventoryDictionaryByGrid(IMyCubeGrid g)
        {
            if (g == null) return inventar;
            if (!dict_inv.ContainsKey(g)) dict_inv.Add(g, new Dictionary<MyItemType, float>());
            return dict_inv[g];
        }
    }
}
