namespace IngameScript
{
    partial class Program
    {
        public class IngameStrings
        {
            public const string
                RComponent = "Component",
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
                RPowder = "powder",
                RMagnesium = "Magnesium",
                RStone = "Stone",
                RScrap = "Scrap",
                RCarbon = "Carbon",
                RPotassium = "Potassium",
                RLithium = "Lithium",
                RBauxite = "Bauxite",
                RSulfur = "Sulfur",
                RNiter = "Niter",
                RCoal = "Coal",
                RDeuterium = "Deuterium";
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
                WaterFood = prefix + RWaterFood,
                Nutrients = prefix + RNutrients,
                SubFresh = prefix + RSubFresh,
                GreyWater = prefix + RGreyWater,
                CleanWater = prefix + RCleanWater,
                SpentFuel = prefix + RSpentFuel,
                Niter = prefix + RNiter,
                DTFuelCapsule = prefix + "DTFuelCapsule",   // fuel of the Deuterium Fusion Reactors mod
                Carbon = prefix + RCarbon,
                Lithium = prefix + RLithium,
                Sulfur = prefix + RSulfur;
        }

        public class Ore : IngameStrings
        {
            const string prefix = "Ore ";
            public const string
                Scrap = prefix + RScrap,
                Magnesium = prefix + RMagnesium,
                Stone = prefix + RStone,
                Organic = prefix + ROrganic,
                Ice = prefix + RIce,
                Niter = prefix + RNiter,
                Bauxite = prefix + RBauxite,
                Coal = prefix + RCoal;
        }
    }
}
