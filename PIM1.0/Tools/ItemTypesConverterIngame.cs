using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        // Ingame part of ItemTypesConverter (SE types), the pure part is in ItemTypesConverter.cs.
        public static partial class ItemTypesConverter
        {
            /// <summary>PIM item id of an item type: MyObjectBuilder_Component/SteelPlate -> "Component SteelPlate".</summary>
            public static string ToPimId(MyItemType type)
            {
                return type.TypeId.Substring(type.TypeId.IndexOf('_') + 1) + " " + type.SubtypeId;
            }

            /// <summary>Item type of a PIM item id: "Component SteelPlate" -> MyObjectBuilder_Component/SteelPlate.</summary>
            public static MyItemType ToItemType(string id)
            {
                return MyItemType.Parse(TypeObjectBuilder + TypeOf(id) + "/" + SubtypeOf(id));
            }
        }
    }
}
