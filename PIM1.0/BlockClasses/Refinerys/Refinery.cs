using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using VRage;
using VRage.Game.ModAPI.Ingame;
using static IngameScript.Program;

namespace IngameScript
{
    partial class Program
    {
        public partial class Refinery : ProductionBlockWithInventorys
        {
            public enum RefreshType 
            {
                Unknow, 
                VanillaRefinery, 
                WaterRecyclingSystem, 
                HydrophonicsFarm, 
                Reprocessor, 
                Incinerator, 
            }

            public enum RefError 
            { 
                NotFilled, 
                OutputNotEmpty, 
                Damaged, 
                IncineratorNoAutofill 
            }

            #region static

            // TODO: 
            static public Dictionary<string, List<RefineryBlueprint>> refineryTypesAcceptedBlueprintsList = new Dictionary<string, List<RefineryBlueprint>>();

            static public void RemoveUnusedRefinerytypeBlueprintLists()
            {
                foreach (var a in refineryTypesAcceptedBlueprintsList.Keys.ToArray()) 
                {
                    if (!ingotprio.ContainsKey(a))
                    {
                        refineryTypesAcceptedBlueprintsList.Remove(a);
                    }
                }
            }

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
                new TypeDefinitions( "Incinerator", RefreshType.Incinerator),
                new TypeDefinitions( "BitumenExtractor", RefreshType.VanillaRefinery),
                new TypeDefinitions( "Reprocessor", RefreshType.Reprocessor),
                new TypeDefinitions( "OilCracker", RefreshType.VanillaRefinery),
                new TypeDefinitions( "DeuteriumProcessor", RefreshType.VanillaRefinery, "Deuterium Refinery"),
                new TypeDefinitions(), // reserved for unknow types
            };

            #endregion

            public TypeDefinitions typeid { get; set; }
            //public Parameter parameter = new Parameter();
            public IMyRefinery RefineryBlock = null;
            public List<RefError> ErrorList = new List<RefError>();
            public List<RefineryBlueprint> AcceptedBlueprints = null;
            public string BlockSubType = "";
            public int Success;
            public RefineryBlueprint CurrentWorkBluePrint = null, NextWorkBluePrint = null;
            public float CurrentWorkOreAmount = 0, NexWorkOreAmount = 0;

            public Refinery(IMyRefinery refinery)
            {
                RefineryBlock = refinery;
                BlockSubType = refinery.BlockDefinition.SubtypeId;
                typeid = TypeDefs.Find(t => t.CompareTypeName(BlockSubType));
                BlockSubType = typeid.GetAlternativOrDefaultName();
                AcceptedBlueprints = RefineryBlueprints.FindAll(b => RefineryBlock.CanUseBlueprint(b.Definition_id));
                GetScrapBluePrints();
                BlockManagerFactory.GetManager(typeid.GetTypeID(), this);
            }

            public override IMyFunctionalBlock GetFunctionalBlock()
            {
                return RefineryBlock;
            }

            void GetScrapBluePrints()
            {
                var acceptedItems = new List<MyItemType>();
                InputInventory.GetAcceptedItems(acceptedItems, i => i.SubtypeId.ToLower().Contains("scrap") && !RefineryBlueprint.IsKnowScrapType(i));
                foreach (var inventoryItem in acceptedItems)
                {
                    var scrapBlueprint = RefineryBlueprint.GetScrapBlueprintByItemtypeOrCreateNew(inventoryItem);
                    if (!AcceptedBlueprints.Contains(scrapBlueprint)) AcceptedBlueprints.Add(scrapBlueprint);
                }
            }

            void AddRefineryCount()
            {
                if (InputInventory.ItemCount > 0)
                {
                    var ore = InputInventoryItems.First().Key;
                    var bluePrint = AcceptedBlueprints.Find(b => b.InputID == ore);
                    if (bluePrint != null) bluePrint.RefineryCount++;
                }
            }


            public void Refresh()
            {
                if (IsClosed) return;

                RefreshInventoryLists();
                AddInventoryListsToSummary();
                AddRefineryCount();
                ClearInventory(OutputInventory);
                SetErrorByCondition(RefError.OutputNotEmpty, OutputInventory.CurrentVolume > 0);

                if (!Parameter.ParseArgs(RefineryBlock.CustomName, true)) return;

                SetRefineryErrors();

                if (typeid.IsUnknowType() || typeid.GetTypeID() == RefreshType.Incinerator)
                {
                    return;
                }

                if (typeid.IsVanillaManagment())
                {
                    if (!ingotprio.ContainsKey(BlockSubType)) ingotprio.Add(BlockSubType, new List<IPrio>());
                    if (!refineryTypesAcceptedBlueprintsList.ContainsKey(BlockSubType)) refineryTypesAcceptedBlueprintsList.Add(BlockSubType, AcceptedBlueprints);
                }

                CalculateWorkAmount();
                SetRefineryStatus();
            }

            private void SetRefineryErrors()
            {
                if (typeid.GetTypeID() == RefreshType.Incinerator)
                {
                    AddRefineryError(RefError.IncineratorNoAutofill);
                }

                if (typeid.IsUnknowType())
                {
                    SetWarning(Warning.ID.REFINERYNOTSUPPORTED, BlockSubType);
                }

                if (RefineryBlock.IsFunctional)
                {
                    if (InputInventory.ItemCount == 0 || InputInventory.CurrentVolume > 0)
                    {
                        DeleteRefineryError(RefError.NotFilled);
                    }
                }
                else
                {
                    AddRefineryError(RefError.Damaged);
                }
            }

            private void SetRefineryStatus()
            {
                if (RefineryBlock.IsFunctional)
                {
                    RefineryBlock.UseConveyorSystem = false;
                    if (InputInventory.ItemCount == 0)
                    {
                        RefineryBlock.Enabled = (Config.Instance.refinerys_off && !Parameter.IsParameter("Nooff")) ? false : true;
                    }
                    else
                    {
                        RefineryBlock.Enabled = true;
                    }

                    Success = 100 - (int)((InputInventory.CurrentVolume.RawValue * 100) / InputInventory.MaxVolume.RawValue);
                }
            }

            public void AddRefineryError(RefError error) { if (!ErrorList.Contains(error)) ErrorList.Add(error); }

            public void DeleteRefineryError(RefError error) { if (ErrorList.Contains(error)) ErrorList.Remove(error); }

            public void SetErrorByCondition(RefError error, bool condition) { if (condition) AddRefineryError(error); else DeleteRefineryError(error); }

            public void FlushAllInventorys() { ClearInventory(InputInventory); ClearInventory(OutputInventory); }

            static Dictionary<RefError, string> RefErrors = new Dictionary<RefError, string>
            {
                { RefError.NotFilled, " could not be filled\n"},
                { RefError.OutputNotEmpty, " cannot unload outputitems.\n"},
                { RefError.Damaged, " is damaged.\n"},
                { RefError.IncineratorNoAutofill, " cannot filled by PIM.\n"},
            };


            public void GetErrorInfo(StringBuilderExtended errString)
            {
                if (!IsPimControlled && ErrorList.Count == 0) return;
                foreach (var error in ErrorList)
                {
                    errString.Append(Parameter.Name);
                    errString.Append(RefErrors.GetValueOrDefault(error, ": unknown error\n"));
                }
            }


            void CalculateRefineryAmount(string bluePrintName)
            {
                if (Lists.Data.BluePrints_Active.ContainsKey(bluePrintName))
                {
                    var b = Lists.Data.BluePrints_Active[bluePrintName];
                    var u = b.MaximumItemAmount - b.CurrentItemAmount;
                    if (b.MaximumItemAmount > 0 && u > 0) b.RefineryAmount = u;
                    else b.RefineryAmount = 0;
                }
            }

            private void GetVanillaWorkItems() 
            {
                string ws = "----";
                string nws = ws;
                float waf = 0f;
                float nwaf = 0f;
                var inhalt = new List<MyInventoryItem>();

                InputInventory.GetItems(inhalt);

                if (inhalt.Count() > 0)
                {
                    ws = GetPIMItemID(inhalt[0].Type);
                    waf = (float)inhalt[0].Amount;
                    if (inhalt.Count() > 1)
                    {
                        nws = GetPIMItemID(inhalt[1].Type);
                        nwaf = (float)inhalt[1].Amount;
                    }
                }

                CurrentWorkBluePrint = AcceptedBlueprints.Find(b => b.InputID == ws);
                CurrentWorkOreAmount = waf;
                NextWorkBluePrint = AcceptedBlueprints.Find(b => b.InputID == nws);
                NexWorkOreAmount = nwaf;
            }

            void CalculateWorkAmount()
            {
                switch (typeid.GetTypeID())
                {
                    case RefreshType.HydrophonicsFarm:
                        CalculateRefineryAmount(Ingot.SubFresh);
                        break;

                    case RefreshType.WaterRecyclingSystem:
                        CalculateRefineryAmount(Ingot.WaterFood);
                        break;

                    case RefreshType.Reprocessor:
                        CalculateRefineryAmount(Ingot.SpentFuelReprocessing);
                        break;

                    case RefreshType.VanillaRefinery:
                        GetVanillaWorkItems();
                        break;
    
                }
            }
        }
    }
}
