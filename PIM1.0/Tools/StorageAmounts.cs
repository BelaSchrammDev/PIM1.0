namespace IngameScript
{
    // Amounts of a storage inventory (StorageInventory): wanted amount of an item against its stacks.
    // Pure (no SE types), tested in StorageAmountsTests.
    partial class Program
    {
        /// <summary>
        /// One stack of a wanted item. <paramref name="missing"/> is the amount that is still wanted
        /// before this stack and is reduced by the part of the stack that is kept.
        /// Returns the part of the stack that is too much (0 if the whole stack is kept).
        /// Called for every stack of the item, so several stacks of one item add up.
        /// </summary>
        public static float SurplusOfStack(float stackAmount, ref float missing)
        {
            if (stackAmount > missing)
            {
                var surplus = stackAmount - missing;
                missing = 0;
                return surplus;
            }

            missing -= stackAmount;
            return 0;
        }
    }
}
