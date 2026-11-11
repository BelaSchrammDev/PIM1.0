using Sandbox.ModAPI.Ingame;
using VRage.Game;

namespace IngameScript
{
    // Ingame part of AssemblerBluePrint: everything that needs Sandbox/VRage types.
    partial class Program
    {
        public partial class AssemblerBluePrint
        {
            MyDefinitionId _definitionId;
            bool _definitionIdSet;

            // created on first use, so loading the blueprints (BluePrintLoadJob) needs no VRage types
            public MyDefinitionId definition_id
            {
                get
                {
                    if (!_definitionIdSet)
                    {
                        MyDefinitionId.TryParse(BluePrintDefinitionPrefix + DefinitionSubtype, out _definitionId);
                        _definitionIdSet = true;
                    }
                    return _definitionId;
                }
            }
        }

        AssemblerBluePrint AddProductionAmount(MyProductionItem pi)
        {
            var bprint = GetBluePrintByProductionItem(pi);
            if (bprint != null) bprint.AssemblyAmount += pi.Amount.ToIntSafe();
            return bprint;
        }
        AssemblerBluePrint GetBluePrintByItemName(string itemName)
        {
            foreach (var b in Lists.Data.BluePrints_Active.Values) if (b.ItemName == itemName) return b;
            foreach (var b in Lists.Data.BluePrints_Inactive.Values) if (b.ItemName == itemName) return b;
            return null;
        }
        AssemblerBluePrint GetBluePrintByProductionItem(MyProductionItem pi)
        {
            foreach (var b in Lists.Data.BluePrints_Active.Values) if (b.DefinitionSubtype == pi.BlueprintId.SubtypeName) return b;
            foreach (var b in Lists.Data.BluePrints_Inactive.Values) if (b.DefinitionSubtype == pi.BlueprintId.SubtypeName) return b;
            return null;
        }

        public void SetAutocraftingThresholdNew()
        {
            foreach (var bp in Lists.Data.BluePrints_Active.Values) bp.CalcMinimumAmount();
        }
    }
}
