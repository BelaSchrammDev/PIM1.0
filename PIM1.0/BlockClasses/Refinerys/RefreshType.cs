namespace IngameScript
{
    partial class Program
    {
        public partial class Refinery
        {
            public enum RefreshType
            {
                Unknow,
                VanillaRefinery,
                WaterRecyclingSystem,
                HydrophonicsFarm,
                Reprocessor,
                NoAutofill,   // known block, not filled by PIM (e.g. incinerator, composter): only a hint on the PB display
            }
        }
    }
}
