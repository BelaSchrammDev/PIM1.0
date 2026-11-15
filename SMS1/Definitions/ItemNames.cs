namespace IngameScript
{
    partial class Program
    {
        /// <summary>
        /// Display name of an item from the name of the item data (or the CustomData).
        /// Pure (no SE types), tested in SMS1.Tests (ItemNamesTests).
        /// </summary>
        public static class ItemNames
        {
            // "" = derived from the subtype (Iron -> Ironore / Ironingot), '@' = the subtype, '_' = space
            public static string Build(string displayName, string subtype, bool isOre, bool isIngot)
            {
                if (displayName == "") return subtype + (isOre ? "ore" : isIngot ? "ingot" : "");
                var name = "";
                foreach (var c in displayName)
                    name += c == '@' ? subtype : c == '_' ? " " : c.ToString();
                return name;
            }
        }
    }
}
