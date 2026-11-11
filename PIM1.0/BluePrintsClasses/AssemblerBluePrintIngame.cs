using Sandbox.ModAPI.Ingame;
using VRage.Game;

namespace IngameScript
{
    // Ingame part of AssemblerBluePrint: everything that needs Sandbox/VRage types.
    partial class Program
    {
        public partial class AssemblerBluePrint
        {
            public MyDefinitionId definition_id;
        }

        /// <summary>Creates all blueprints of the enabled mods (see BluePrintDefinitions.cs).</summary>
        void InitBluePrints()
        {
            RefineryBlueprint.InitScrapTypeBlueprintTypes();
            DefineBluePrints(Config.Instance.usedMods, Lists.Data.BluePrints_Inactive, RefineryBlueprints);
            foreach (var b in Lists.Data.BluePrints_Inactive.Values)
                MyDefinitionId.TryParse(BluePrintDefinitionPrefix + b.DefinitionSubtype, out b.definition_id);
            foreach (var b in RefineryBlueprints)
                MyDefinitionId.TryParse(BluePrintDefinitionPrefix + b.Name, out b.Definition_id);
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
            foreach (var b in Lists.Data.BluePrints_Active.Values) if (b.definition_id.SubtypeName == pi.BlueprintId.SubtypeName) return b;
            foreach (var b in Lists.Data.BluePrints_Inactive.Values) if (b.definition_id.SubtypeName == pi.BlueprintId.SubtypeName) return b;
            return null;
        }

        public void SetAutocraftingThresholdNew()
        {
            foreach (var bp in Lists.Data.BluePrints_Active.Values) bp.CalcMinimumAmount();
        }
    }
}
