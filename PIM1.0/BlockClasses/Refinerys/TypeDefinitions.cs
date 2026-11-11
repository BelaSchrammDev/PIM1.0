using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        public partial class Refinery
        {
            static readonly TypeDefinitions UnknownTypeDefinition = new TypeDefinitions();

            static List<TypeDefinitions> TypeDefs = new List<TypeDefinitions>
            {
                new TypeDefinitions( false, "WRS", RefreshType.WaterRecyclingSystem, "Water Recycling System" ),
                new TypeDefinitions( "Blast Furnace", RefreshType.VanillaRefinery, "Basic Refinery"),
                new TypeDefinitions( "LargeRefineryIndustrial", RefreshType.VanillaRefinery, "Large Industrial Refinery"),
                new TypeDefinitions( "LargeRefinery", RefreshType.VanillaRefinery, "Large Refinery"),
                new TypeDefinitions( "K_HSR_Refinery_A", RefreshType.VanillaRefinery, "HSR Refinery A"),
                new TypeDefinitions( false, "Hydroponics", RefreshType.HydrophonicsFarm, "Hydroponics Farm"),
                new TypeDefinitions( "RockCrusher", RefreshType.VanillaRefinery),
                new TypeDefinitions( "OrePurifier", RefreshType.VanillaRefinery),
                new TypeDefinitions( "ChemicalPlant", RefreshType.VanillaRefinery),
                new TypeDefinitions( "Centrifuge", RefreshType.VanillaRefinery),
                new TypeDefinitions( "Incinerator", RefreshType.NoAutofill),
                new TypeDefinitions( "Composter", RefreshType.NoAutofill),
                new TypeDefinitions( "BitumenExtractor", RefreshType.VanillaRefinery),
                new TypeDefinitions( "Reprocessor", RefreshType.Reprocessor),
                new TypeDefinitions( "OilCracker", RefreshType.VanillaRefinery),
                new TypeDefinitions( "DeuteriumProcessor", RefreshType.VanillaRefinery, "Deuterium Refinery")
            };

            public TypeDefinitions GetRefineryDefinition(string BlockSubType)
            {
                return TypeDefs.Find(t => t.CompareTypeName(BlockSubType)) ?? UnknownTypeDefinition;
            }

            public class TypeDefinitions
            {
                string TypeIDName, AlternativName;
                bool ComplettName;
                RefreshType TypeID;

                public TypeDefinitions()
                    : this(true, "", RefreshType.Unknow)
                {
                }

                public TypeDefinitions(string iTypeName, RefreshType iTypeID, string iAlternativName = "")
                    : this(true, iTypeName, iTypeID, iAlternativName)
                {
                }

                public TypeDefinitions(bool iComplettName, string iTypeName, RefreshType iTypeID, string iAlternativName = "")
                {
                    TypeIDName = iTypeName;
                    ComplettName = iComplettName;
                    TypeID = iTypeID;
                    AlternativName = iAlternativName;
                }

                public RefreshType GetTypeID() => TypeID;

                public bool IsVanillaManagment() => TypeID == RefreshType.VanillaRefinery;

                public bool IsUnknowType() => TypeID == RefreshType.Unknow;

                public bool IsType(RefreshType typeToCheck) => TypeID == typeToCheck;

                public string GetAlternativOrDefaultName() 
                {
                    return string.IsNullOrEmpty(AlternativName) 
                        ? TypeIDName
                        : AlternativName; 
                }

                public bool CompareTypeName(string compareString)
                {
                    return
                        (ComplettName && TypeIDName == compareString) || 
                        (!ComplettName && compareString.StartsWith(TypeIDName));
                }
            }
        }
    }
}
