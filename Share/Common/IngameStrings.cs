namespace IngameScript
{
    partial class Program
    {
        // Names of the game (subtypes of ores, ingots, items), shared by PIM and SMS.
        // Constants of Program, not of a class: every access 'Class.Name' costs chars in the packed script,
        // unused constants are dropped by the packing. Item types and prefixes: ItemTypes.cs.

        const string

        // subtypes (and parts of subtype names)
        SubtypeReprocessing = "Reprocessing",
        SubtypeSpentFuel = "SpentFuel",
        SubtypeSpentFuelReprocessing = SubtypeSpentFuel + SubtypeReprocessing,
        SubtypeSeeds = "Seeds",
        SubtypeIce = "Ice",
        SubtypeOrganic = "Organic",
        SubtypeDeuterium = "Deuterium",
        SubtypeStone = "Stone",
        SubtypeScrap = "Scrap",
        SubtypeMagnesium = "Magnesium",
        SubtypeCarbon = "Carbon",
        SubtypePotassium = "Potassium",
        SubtypeLithium = "Lithium",
        SubtypeBauxite = "Bauxite",
        SubtypeSulfur = "Sulfur",
        SubtypeNiter = "Niter",
        SubtypeCoal = "Coal",
        // food and water (ingots of the food mods)
        SubtypeWaterFood = "WaterFood",
        SubtypeNutrients = "Nutrients",
        SubtypeSubFresh = "SubFresh",
        SubtypeGreyWater = "GreyWater",
        SubtypeCleanWater = "CleanWater",
        // powders (ingots without the "Ingot " prefix in the names)
        SubtypePowder = "powder",
        SubtypeGun = "Gun",
        SubtypeMagnesiumpowder = SubtypeMagnesium + SubtypePowder,
        SubtypeGunpowder = SubtypeGun + SubtypePowder,
        // magazines
        SubtypeNATO_25 = "NATO_25",
        SubtypeMagazine = "Magazine",
        SubtypeAutomaticRifleGunMag = "AutomaticRifleGun_Mag_",
        SubtypeGunMagazine = SubtypeGun + SubtypeMagazine,
        SubtypeAutoRifleGunMagazine = "AutoRifle" + SubtypeGunMagazine,
        SubtypeRifleGunMagazine = "Rifle" + SubtypeGunMagazine,

        // ingots: "Ingot " + subtype
        IngotSpentFuelReprocessing = IngotPrefix + SubtypeSpentFuelReprocessing,
        IngotSpentFuel = IngotPrefix + SubtypeSpentFuel,
        IngotScrap = IngotPrefix + SubtypeScrap,
        IngotMagnesium = IngotPrefix + SubtypeMagnesium,
        IngotStone = IngotPrefix + SubtypeStone,
        IngotNiter = IngotPrefix + SubtypeNiter,
        IngotCarbon = IngotPrefix + SubtypeCarbon,
        IngotLithium = IngotPrefix + SubtypeLithium,
        IngotSulfur = IngotPrefix + SubtypeSulfur,
        IngotWaterFood = IngotPrefix + SubtypeWaterFood,
        IngotNutrients = IngotPrefix + SubtypeNutrients,
        IngotSubFresh = IngotPrefix + SubtypeSubFresh,
        IngotGreyWater = IngotPrefix + SubtypeGreyWater,
        IngotCleanWater = IngotPrefix + SubtypeCleanWater,
        // fuel of the Deuterium Fusion Reactors mod
        IngotDTFuelCapsule = IngotPrefix + "DTFuelCapsule",

        // ores: "Ore " + subtype
        OreScrap = OrePrefix + SubtypeScrap,
        OreMagnesium = OrePrefix + SubtypeMagnesium,
        OreStone = OrePrefix + SubtypeStone,
        OreOrganic = OrePrefix + SubtypeOrganic,
        OreIce = OrePrefix + SubtypeIce,
        OreNiter = OrePrefix + SubtypeNiter,
        OreBauxite = OrePrefix + SubtypeBauxite,
        OreCoal = OrePrefix + SubtypeCoal;
    }
}
