namespace IngameScript
{
    partial class Program
    {
        const string IG_Food = "Food", IG_Component = "Component", IG_I = "Ingot", IG_Ingot = IG_I + " ", IG_Com = IG_Component + " ", IG_Datas = "Datapad", IG_Kits = "ConsumableItem", IG_K = IG_Kits + " ", IG_Phys = "PhysicalObject", IG_P = IG_Phys + " ", IG_Tools = "PhysicalGunObject", IG_HBottles = "GasContainerObject", IG_OBottles = "OxygenContainerObject", IG_Ammo = "AmmoMagazine", IG_ = "MyObjectBuilder_", IG_Seeds = "SeedItem", IG_S = IG_Seeds + " ";

        public class Strings 
        {
            public const string
                // PIM Strings
                PimVersion = "PIM v1.2a", // also sent to the other scripts (networking hello)
                // SMS before the networking (it read the PIM data from its CustomData): only for a hint
                OldSmsVersion = "SMS v1.4",

                // General
                SmsStorageTag = "(sms,storage)",
                X_UseConveyor = "UseConveyor",
                X_Keep = "Keep", // (sms,keep): the container is not emptied
                AC_ToolsAndGuns = "Tools&Guns",
                X_Autocrafting_threshold = "Autocrafting_threshold",
                AutoCraftingTypeStringName = "AutocraftingTypes",

                // configstrings
                X_Line = "  / =================================\n",

                // PIM data for SMS (NetWorkingScripts.PimData)
                AssemblerQueueNameSemicolon = "@ASSEMBLERQUEUE;",
                ItemMaxNameSemicolon = "@ITEMMAX;",

                // Gun Control
                DefaultGunGroupName = "PIM controlled Guns",

                // mod names used in code (the full mod list is generated: BluePrintMods)
                M_Vanilla = "Vanilla",
                M_IndustrialOverhaulMod = "IndustrialOverhaulMod";
        }
    }
}
