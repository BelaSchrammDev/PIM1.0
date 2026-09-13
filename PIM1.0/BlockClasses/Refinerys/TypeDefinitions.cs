namespace IngameScript
{
    partial class Program
    {

        public partial class Refinery
        {
            public class TypeDefinitions
            {
                string TypeIDName, AlternativName;
                bool ComplettName;
                RefreshType TypeID;


                public TypeDefinitions()
                {
                    TypeIDName = "";
                    ComplettName = true;
                    TypeID = RefreshType.Unknow;
                }


                public TypeDefinitions(string iTypeName, RefreshType iTypeID, string iAlternativName = "")
                {
                    TypeIDName = iTypeName;
                    ComplettName = true;
                    TypeID = iTypeID;
                    AlternativName = iAlternativName;
                }


                public TypeDefinitions(bool iComplettName, string iTypeName, RefreshType iTypeID, string iAlternativName = "")
                {
                    TypeIDName = iTypeName;
                    ComplettName = iComplettName;
                    TypeID = iTypeID;
                    AlternativName = iAlternativName;
                }

                public RefreshType GetTypeID() { return TypeID; }

                public string GetAlternativOrDefaultName() { return AlternativName == "" ? TypeIDName : AlternativName; }

                public bool IsVanillaManagment() { return TypeID == RefreshType.VanillaRefinery; }

                public bool IsUnknowType() { return TypeID == RefreshType.Unknow; }

                public bool CompareTypeName(string compareString)
                {
                    return
                        (TypeID == RefreshType.Unknow) || 
                        (ComplettName && TypeIDName == compareString) || 
                        (!ComplettName && compareString.StartsWith(TypeIDName));
                }
            }
        }
    }
}
