using System;
using System.Collections.Generic;

namespace IngameScript
{
    // Which ammo a gun is loaded with, if it accepts more than one type.
    // Pure (no SE types), tested in AmmoPriorityTests.
    partial class Program
    {
        /// <summary>
        /// Configured priorities: gun subtype -> ammo item id ("AmmoMagazine NATO_25x184mm") -> priority,
        /// the highest one wins, 0 = do not use. Meant to be filled by an ammoprio LCD; without an entry
        /// the default priority applies (GetAmmoPriority).
        /// </summary>
        public static Dictionary<string, Dictionary<string, int>> AmmoPrioConfig = new Dictionary<string, Dictionary<string, int>>();

        /// <summary>
        /// Priority of the ammo number <paramref name="index"/> of <paramref name="count"/> accepted types.
        /// Default: the order in which the game lists the accepted types, the first one is the highest.
        /// </summary>
        public static int GetAmmoPriority(string gunType, string ammo, int index, int count)
        {
            Dictionary<string, int> config;
            int prio;
            if (AmmoPrioConfig.TryGetValue(gunType, out config) && config.TryGetValue(ammo, out prio)) return prio;
            return count - index;
        }

        /// <summary>
        /// The ammo a gun of this type is loaded with:
        /// - only one accepted type: that one (also if it is out of stock)
        /// - the type with the highest priority that is in stock at least once per gun accepting it,
        ///   so a few magazines of a preferred type do not unload all guns
        /// - if no type has that much: the type with the highest priority that is in stock at all
        /// - nothing in stock: "" (the gun is left as it is)
        /// </summary>
        /// <param name="ammoTypes">accepted ammo item ids in the order of the game</param>
        /// <param name="stock">amount of an ammo item on the grid (including the guns)</param>
        /// <param name="gunCount">number of controlled guns that accept an ammo item</param>
        public static string ChooseAmmo(string gunType, List<string> ammoTypes, Func<string, float> stock, Func<string, int> gunCount)
        {
            if (ammoTypes.Count == 0) return "";
            if (ammoTypes.Count == 1) return ammoTypes[0];

            string enough = "", some = "";
            int enoughPrio = 0, somePrio = 0;
            for (int i = 0; i < ammoTypes.Count; i++)
            {
                var ammo = ammoTypes[i];
                var prio = GetAmmoPriority(gunType, ammo, i, ammoTypes.Count);
                if (prio <= 0) continue;
                var amount = stock(ammo);
                if (amount <= 0) continue;
                if (prio > somePrio) { some = ammo; somePrio = prio; }
                if (prio > enoughPrio && amount >= gunCount(ammo)) { enough = ammo; enoughPrio = prio; }
            }
            return enough != "" ? enough : some;
        }
    }
}
