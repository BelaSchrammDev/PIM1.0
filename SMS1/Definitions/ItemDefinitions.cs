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
        // the item definitions of Vanilla and the enabled mods, from SMS1/ItemData/*.txt (Generated/ItemData.g.cs)
        void LoadItemDefinitions()
        {
            for (var i = 0; i < ItemDataMods.Length; i++)
            {
                curMod = ItemDataMods[i];
                if (i > 0 && !usemods[curMod]) continue;
                ItemDataParser.Parse(ItemData[i], (type, subtype, max, name, category, ammoCapacity) =>
                {
                    // the category is used for new item types only (as before: castTypeName)
                    castTypeName = category;
                    var itemType = addItemType(type, subtype, max, name);
                    if (itemType != null && ammoCapacity > 0) itemType.ammocapacity = ammoCapacity;
                });
            }
            castTypeName = "";
        }

        static Dictionary<string, Dictionary<MyItemType, int>> modItemDefsList = new Dictionary<string, Dictionary<MyItemType, int>>();
        static Dictionary<MyItemType, string> itemAlterNameList = new Dictionary<MyItemType, string>();
        static bool writeNewItemDefs = false;
        static string curMod = "Vanilla";
        // returns the new smsItemType, null if the item type exists already (defined by an earlier mod)
        smsItemType addItemType(string typeName, string subtypeName, int maximumAmount, string displayName = "")
        {
            var itemType = new MyItemType(IG_ + typeName, subtypeName);

            // the CustomData overrides the item data: the max of the section of the current mod
            // (missing entries are added and written), the name of any section
            if (!modItemDefsList.ContainsKey(curMod)) modItemDefsList.Add(curMod, new Dictionary<MyItemType, int>());
            var modItemDefs = modItemDefsList[curMod];
            if (modItemDefs.ContainsKey(itemType)) maximumAmount = modItemDefs[itemType];
            else
            {
                writeNewItemDefs = true;
                modItemDefs.Add(itemType, maximumAmount);
            }
            string customName;
            if (itemAlterNameList.TryGetValue(itemType, out customName) && customName != "") displayName = customName;

            smsItemType existing;
            if (smsItemTypeDefinitions.TryGetValue(itemType, out existing))
            {
                // defined by an earlier mod: the later mod overrides the max and, if it has one, the name
                existing.InitMaximumAmount(maximumAmount);
                if (displayName != "") existing.SetName(displayName);
                return null;
            }
            var newItem = new smsItemType(itemType, maximumAmount, displayName, castTypeName);
            smsItemTypeDefinitions.Add(itemType, newItem);
            return newItem;
        }
        //SMS ItemType
        public class smsItemType
        {
            public MyItemType type;
            public MyItemInfo info;
            public string name, typeString, subtypeString, blueprintName;
            public int max, imax, pmax = 0, ammocapacity = 0;
            public float ass = 0;
            public smsItemType(MyItemType _type, int amount = -1, string alternativname = "", string s = "")
            {
                type = _type;
                info = type.GetItemInfo();
                subtypeString = type.SubtypeId;
                typeString = s == "" ? type.TypeId.Substring(type.TypeId.IndexOf('_') + 1) : s;
                SetName(alternativname);
                InitMaximumAmount(amount);
            }
            public void InitMaximumAmount(int initMax)
            {
                if (initMax == -1) imax = IfItemtypeOreOrIngot() ? 1000 : 10;
                else imax = initMax;
                max = imax;
            }
            public void SetMaximumAmount(float m)
            {
                pmax = (int)m;
                max = pmax;
            }
            // replaces the name (a later mod overrides it; before, a name was appended to the old one)
            public void SetName(string displayName)
            {
                name = ItemNames.Build(displayName, type.SubtypeId, info.IsOre, info.IsIngot);
            }
            public bool IfItemtypeOreOrIngot() { return info.IsOre || info.IsIngot; }
            public double GetAmount(IMyCubeGrid g = null) { var d = GetInventoryDictionaryByGrid(g); return d.GetValueOrDefault(type, 0); }
            public double GetMass(double a) { return a * info.Mass; }
            public double GetVolume(double a) { return a * info.Volume * 1000; }
            public bool IsItemMasstype() { return info.Mass == 1; }
            public bool IsItemVolumetype() { return info.Volume == 0.001f; }
            public bool IsItemResource() { return (typeString == "Ore" || typeString == "Ingot") && (subtypeString != "Ice" && subtypeString != "Organic" && !subtypeString.StartsWith("K_HSR_")); }
            public string GetResourceName() { return subtypeString.Contains("Deuterium") ? "Deuterium" : subtypeString; }
        }
    }
}
