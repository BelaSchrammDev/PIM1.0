namespace IngameScript
{
    partial class Program
    {
        // The item types of the game (TypeId of MyItemType, without "MyObjectBuilder_"), shared by PIM and SMS,
        // and the prefixes of the item names ("TypeId SubtypeId", e.g. "Component SteelPlate").
        // Constants of Program, not of a class: every access 'Class.Name' costs chars in the packed script.

        const string

        // MyObjectBuilder_ + TypeId = the full type (MyItemType.Parse)
        TypeObjectBuilder = "MyObjectBuilder_",

        // TypeIds
        TypeComponent = "Component",
        TypeIngot = "Ingot",
        TypeOre = "Ore",
        TypeAmmo = "AmmoMagazine",
        TypeSeed = "SeedItem",
        TypeDatapad = "Datapad",
        // kits, food (ConsumableItem), cash etc. (PhysicalObject), tools and guns (PhysicalGunObject)
        TypeKit = "ConsumableItem",
        TypePhysicalObject = "PhysicalObject",
        TypeTool = "PhysicalGunObject",
        // hydrogen and oxygen bottles
        TypeHBottle = "GasContainerObject",
        TypeOBottle = "OxygenContainerObject",

        // prefixes of the item names: TypeId + " " + SubtypeId
        ComponentPrefix = TypeComponent + " ",
        IngotPrefix = TypeIngot + " ",
        OrePrefix = TypeOre + " ",
        KitPrefix = TypeKit + " ",
        PhysicalObjectPrefix = TypePhysicalObject + " ",
        SeedPrefix = TypeSeed + " ";
    }
}
