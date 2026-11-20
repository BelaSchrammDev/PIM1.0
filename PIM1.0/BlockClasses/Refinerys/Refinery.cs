using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public partial class Refinery : ProductionBlockWithInventorys
        {
            public enum RefError
            {
                NotFilled,
                OutputNotEmpty,
                Damaged,
                NoAutofill
            }

            #region static

            // TODO: create partial blueprint class
            static public Dictionary<string, List<RefineryBlueprint>> refineryTypesAcceptedBlueprintsList = new Dictionary<string, List<RefineryBlueprint>>();

            // number of functional PIM controlled refinerys, counted in every cycle (FindingRefinerysJob)
            static public int FunctionalCount = 0;

            // types of the PIM controlled refinerys of this cycle: cleared by FindingRefinerysJob,
            // filled by VanillaRefineryManager.Refresh
            static public HashSet<string> TypesInUse = new HashSet<string>();

            /// <summary>
            /// Types without a PIM controlled refinery in this cycle (block removed, tag removed) are dropped:
            /// otherwise they stay on the oreprio LCD and in the data for SMS until the script restarts.
            /// The settings of the oreprio LCD (OrePrioConfig) are kept, in case the type comes back.
            /// </summary>
            static public void RemoveUnusedRefineryTypes()
            {
                foreach (var type in ingotprio.Keys.ToArray())
                {
                    if (!TypesInUse.Contains(type)) ingotprio.Remove(type);
                }
                foreach (var type in refineryTypesAcceptedBlueprintsList.Keys.ToArray())
                {
                    if (!TypesInUse.Contains(type)) refineryTypesAcceptedBlueprintsList.Remove(type);
                }
            }

            #endregion

            public TypeDefinitions RefineryType { get; set; }
            public IMyRefinery RefineryBlock = null;
            public List<RefError> ErrorList = new List<RefError>();
            public List<RefineryBlueprint> AcceptedBlueprints = null;
            public string BlockSubType = "";
            public int Success;

            public Refinery(IMyRefinery refinery)
            {
                RefineryBlock = refinery;
                BlockSubType = refinery.BlockDefinition.SubtypeId;
                RefineryType = GetRefineryDefinition(BlockSubType);
                BlockSubType = RefineryType.GetAlternativeOrDefaultName();
                AcceptedBlueprints = RefineryBlueprints.FindAll(b => RefineryBlock.CanUseBlueprint(b.Definition_id));
                GetScrapBluePrints();
                BlockManagerFactory.GetManager(RefineryType.GetTypeID(), this);
            }

            public override IMyFunctionalBlock GetFunctionalBlock()
            {
                return RefineryBlock;
            }

            void GetScrapBluePrints()
            {
                var acceptedItems = new List<MyItemType>();
                InputInventory.GetAcceptedItems(acceptedItems, i => i.SubtypeId.ToLower().Contains("scrap") && !RefineryBlueprint.IsKnownScrapType(i));
                foreach (var inventoryItem in acceptedItems)
                {
                    var scrapBlueprint = RefineryBlueprint.GetScrapBlueprintByItemtypeOrCreateNew(inventoryItem);
                    if (!AcceptedBlueprints.Contains(scrapBlueprint)) AcceptedBlueprints.Add(scrapBlueprint);
                }
            }

            void AddRefineryCount()
            {
                // the ore that is refined right now is the first item of the input inventory
                // (not the first key of InputInventoryItems: that is the first ore the refinery ever had)
                var firstItem = InputInventory.GetItemAt(0);
                if (firstItem.HasValue)
                {
                    var ore = ItemTypesConverter.ToPimId(firstItem.Value.Type);
                    var bluePrint = AcceptedBlueprints.Find(b => b.InputID == ore);
                    if (bluePrint != null) bluePrint.RefineryCount++;
                }
            }


            /// <summary>
            /// Phase 1 of the refinery handling (FindingRefinerysJob): what is the same for every refinery type.
            /// The type specific part is done by the manager (Manager.Refresh here, Manager.DoManage in RefineryManagerJob).
            /// </summary>
            public void Refresh()
            {
                if (IsClosed) return;

                var wasPimControlled = IsPimControlled;
                var pimControlled = ParseProductionTag(Parameter, RefineryBlock.CustomName);
                if (wasPimControlled && !pimControlled) Release();

                // for every refinery, also the ones without tag:
                // only the own lists: the stock (Lists.Data.Inventory) already contains the refinery
                // inventories, GridScanningJob counts every inventory of the grid
                RefreshInventoryLists();
                AddRefineryCount();
                ClearInventory(OutputInventory);
                SetErrorByCondition(RefError.OutputNotEmpty, OutputInventory.CurrentVolume > 0);

                if (!pimControlled) return;

                SetRefineryErrors();
                Manager.Refresh();
                if (Manager.IsFilledByPim) SetRefineryStatus();
            }

            /// <summary>
            /// The tag was removed: give the block back to the player. PIM switched the conveyor off (and the
            /// block off while idle) for its own filling, and its errors are no longer refreshed.
            /// </summary>
            void Release()
            {
                ErrorList.Clear();
                if (!Manager.IsFilledByPim) return;
                RefineryBlock.UseConveyorSystem = true;
                RefineryBlock.Enabled = true;
            }

            private void SetRefineryErrors()
            {
                SetErrorByCondition(RefError.Damaged, !RefineryBlock.IsFunctional);

                // set again by the filling (phase 2, RefineryFilling) if the refinery is still empty
                if (RefineryBlock.IsFunctional) DeleteRefineryError(RefError.NotFilled);
            }

            private void SetRefineryStatus()
            {
                if (RefineryBlock.IsFunctional)
                {
                    FunctionalCount++;
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
                { RefError.OutputNotEmpty, " cannot unload output items.\n"},
                { RefError.Damaged, " is damaged.\n"},
                { RefError.NoAutofill, " cannot be filled by PIM.\n"},
            };


            public void GetErrorInfo(StringBuilderExtended errString)
            {
                foreach (var error in ErrorList)
                {
                    errString.Append(Parameter.Name);
                    errString.Append(RefErrors.GetValueOrDefault(error, ": unknown error\n"));
                }
            }

        }
    }
}
