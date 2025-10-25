using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public class StorageCargo : StorageInventory
        {
            public IMyCargoContainer container = null;
            string oldCustomdata = "";
            public StorageCargo(IMyTerminalBlock cargoContainer)
            {
                container = cargoContainer as IMyCargoContainer;
                inv = container.GetInventory();
                Lists.Data.storageinvs.Add(this);
            }
            const string X_ItemDef = "StorageItemDefinition", X_ItemDefBegin = "### " + X_ItemDef + "_begin ###", X_ItemDefEnd = "### " + X_ItemDef + "_end ###", X_AddToList = "add_to_list:";
            public override bool CheckItems()
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
                var cdata = "  / Itemdefinitionen:\n  / amount and type of items to be stored in the container\n  /\n  / add items to the list:\n  / write search terms after the '" + X_AddToList + "', like 'steel' or 'tube'.\n  / close the window, after a few seconds you will find\n  / relevant items in the list below.\n" + X_AddToList + "\n" + Strings.X_Line;
                cdata += "  / List of items, delete the lines that are no longer needed,\n  / or set the value to 0.\n  / please change only the value before the semicolon\n" + X_ItemDefBegin + "\n";
                foreach (var i in items) { cdata += i.Value + ";" + i.Key + "\n"; }
                cdata += X_ItemDefEnd + "\n";
                container.CustomData = cdata;
                oldCustomdata = cdata;
                return true;
            }
            public void Remove()
            {
                Lists.Data.storageinvs.Remove(this);
            }
        }
    }
}
