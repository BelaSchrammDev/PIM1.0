namespace IngameScript
{
    partial class Program
    {
        public class IngameStrings
        {
            public static string GetPositionString(int value, string append, int? suffix = null)
            {
                Program.Instance.Echo($"IngameStrings.GetPositionString '{$"{RPosition}{value:D4}{(suffix.HasValue ? suffix.ToString() : string.Empty)}_{append}"}'\n");
                return $"{RPosition}{value:D4}{(suffix.HasValue ? suffix.ToString() : string.Empty)}_{append}";
            }

            public const string
                RComponent = "Component",
                RPosition = "Position",
                RReprocessing = "Reprocessing",
                RSpentFuelReprocessing = "SpentFuel" + RReprocessing,
                RSeeds = "Seeds",
                RGun = "Gun",
                RIce = "Ice",
                RNATO_25 = "NATO_25",
                RMagazine = "Magazine",
                RAutomaticRifleGunMag = "AutomaticRifleGun_Mag_",
                RGunMagazine = RGun + RMagazine,
                RAutoRifleGunMagazine = "AutoRifle" + RGunMagazine,
                RRifleGunMagazine = "Rifle" + RGunMagazine,
                ROrganic = "Organic",
                RWaterFood = "WaterFood",
                RNutrients = "Nutrients",
                RSubFresh = "SubFresh",
                RGreyWater = "GreyWater",
                RCleanWater = "CleanWater",
                RSpentFuel = "SpentFuel",
                RContainer = "Container",
                RMagnetron = "Magnetron",
                RPowder = "powder",
                RMagnesium = "Magnesium",
                RStone = "Stone",
                RIron = "Iron",
                RNickel = "Nickel",
                RSilicon = "Silicon",
                RCobalt = "Cobalt",
                RPlatinum = "Platinum",
                RUranium = "Uranium",
                RScrap = "Scrap",
                RCarbon = "Carbon",
                RPotassium = "Potassium",
                RPhosphorus = "Phosphorus",
                RNaquadah = "Naquadah",
                RTrinium = "Trinium",
                RNeutronium = "Neutronium",
                RCopper = "Copper",
                RLithium = "Lithium",
                RBauxite = "Bauxite",
                RTitanium = "Titanium",
                RTantalum = "Tantalum",
                RSulfur = "Sulfur",
                RNiter = "Niter",
                RCoal = "Coal",
                RDeuterium = "Deuterium",
                RAluminium = "Aluminium",
                RSilver = "Silver",
                RGold = "Gold";
            // public const string R = "";
        }

        public class Component : IngameStrings 
        {
            public const string
                AngleGrinder = "AngleGrinder",
                HandDrill = "HandDrill",
                Welder = "Welder",
                Magnetron = RMagnetron + "_" + RComponent;
            // public const string  = prefix + "";
        }

        public class Ingot : IngameStrings
        {
            const string prefix = "Ingot ";
            public const string
                SpentFuelReprocessing = prefix + RSpentFuelReprocessing,
                Scrap = prefix + RScrap,
                Magnesium = prefix + RMagnesium,
                Magnesiumpowder = RMagnesium + RPowder,
                Gunpowder = RGun + RPowder,
                Stone = prefix + RStone,
                Iron = prefix + RIron,
                Nickel = prefix + RNickel,
                Silicon = prefix + RSilicon,
                Cobalt = prefix + RCobalt,
                Platinum = prefix + RPlatinum,
                Uranium = prefix + RUranium,
                WaterFood = prefix + RWaterFood,
                Nutrients = prefix + RNutrients,
                SubFresh = prefix + RSubFresh,
                GreyWater = prefix + RGreyWater,
                CleanWater = prefix + RCleanWater,
                SpentFuel = prefix + RSpentFuel,
                Niter = prefix + RNiter,
                DeuteriumContainer = prefix + RDeuterium + RContainer,
                Carbon = prefix + RCarbon,
                Potassium = prefix + RPotassium,
                Phosphorus = prefix + RPhosphorus,
                Naquadah = prefix + RNaquadah,
                Trinium = prefix + RTrinium,
                Neutronium = prefix + RNeutronium,
                Copper = prefix + RCopper,
                Lithium = prefix + RLithium,
                Titanium = prefix + RTitanium,
                Tantalum = prefix + RTantalum,
                Sulfur = prefix + RSulfur,
                Silver = prefix + RSilver,
                Gold = prefix + RGold,
                Aluminium = prefix + RAluminium;
            // public const string  = prefix + "";
        }


        public class Ore : IngameStrings
        {
            const string prefix = "Ore ";
            public const string
                Scrap = prefix + RScrap,
                Magnesium = prefix + RMagnesium,
                Stone = prefix + RStone,
                Iron = prefix + RIron,
                Nickel = prefix + RNickel,
                Silicon = prefix + RSilicon,
                Cobalt = prefix + RCobalt,
                Platinum = prefix + RPlatinum,
                Uranium = prefix + RUranium,
                Organic = prefix + ROrganic,
                Ice = prefix + RIce,
                Deuterium = prefix + RDeuterium,
                Carbon = prefix + RCarbon,
                Potassium = prefix + RPotassium,
                Phosphorus = prefix + RPhosphorus,
                Naquadah = prefix + RNaquadah,
                Trinium = prefix + RTrinium,
                Neutronium = prefix + RNeutronium,
                Niter = prefix + RNiter,
                Copper = prefix + RCopper,
                Lithium = prefix + RLithium,
                Bauxite = prefix + RBauxite,
                Titanium = prefix + RTitanium,
                Tantalum = prefix + RTantalum,
                Sulfur = prefix + RSulfur,
                Silver = prefix + RSilver,
                Gold = prefix + RGold,
                Coal = prefix + RCoal;
            // public const string  = prefix + "";
        }
    }
}
