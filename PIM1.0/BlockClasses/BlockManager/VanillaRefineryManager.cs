using System.Collections.Generic;
using VRage;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        /// <summary>
        /// Refinerys that refine ore by the ore priority (ingotprio) of their type.
        /// The refinery works on the first item of its input inventory only.
        /// </summary>
        public class VanillaRefineryManager : RefineryBlockManagerer
        {
            // DoManage alternates between ClearInput and OreSwap
            bool swapOresNext = false;

            // the ore that is refined right now (first input item) and the next one (second input item)
            RefineryBlueprint CurrentWorkBluePrint, NextWorkBluePrint;
            float CurrentWorkOreAmount, NextWorkOreAmount;

            public VanillaRefineryManager(ManageableBlock block) : base(block)
            {
            }

            public override void Refresh()
            {
                // registers the refinery type, CalcIngotPrio then calculates the priorities of its blueprints
                Refinery.TypesInUse.Add(Refinery.BlockSubType);
                if (!ingotprio.ContainsKey(Refinery.BlockSubType)) ingotprio.Add(Refinery.BlockSubType, new List<IPrio>());
                if (!Refinery.refineryTypesAcceptedBlueprintsList.ContainsKey(Refinery.BlockSubType)) Refinery.refineryTypesAcceptedBlueprintsList.Add(Refinery.BlockSubType, Refinery.AcceptedBlueprints);
                GetWorkItems();
            }

            void GetWorkItems()
            {
                var items = new List<MyInventoryItem>();
                Refinery.InputInventory.GetItems(items);

                CurrentWorkBluePrint = items.Count > 0 ? FindBlueprint(items[0]) : null;
                CurrentWorkOreAmount = items.Count > 0 ? (float)items[0].Amount : 0f;
                NextWorkBluePrint = items.Count > 1 ? FindBlueprint(items[1]) : null;
                NextWorkOreAmount = items.Count > 1 ? (float)items[1].Amount : 0f;
            }

            RefineryBlueprint FindBlueprint(MyInventoryItem item)
            {
                var ore = ItemTypesConverter.ToPimId(item.Type);
                return Refinery.AcceptedBlueprints.Find(b => b.InputID == ore);
            }

            public override void DoManage()
            {
                List<IPrio> prios;
                if (!ingotprio.TryGetValue(Refinery.BlockSubType, out prios)) return;

                if (Refinery.Success > 80 || HigherPrioOreWaiting(prios))
                {
                    RefineryFilling(prios);
                }

                if (swapOresNext) OreSwap(prios);
                else ClearInput(prios);
                swapOresNext = !swapOresNext;
            }

            /// <summary>Moves the ores with priority 0 (ignored) out of the input inventory.</summary>
            void ClearInput(List<IPrio> prios)
            {
                var items = new List<MyInventoryItem>();
                Refinery.InputInventory.GetItems(items);
                for (int i = items.Count - 1; i >= 0; i--)
                {
                    var blueprint = FindBlueprint(items[i]);
                    if (blueprint == null) continue;
                    var prio = IPrio.GetBlueprintPrio(prios, blueprint);
                    if (prio != null && prio.initp == 0)
                    {
                        clearItemByType(Refinery.InputInventory, ItemTypesConverter.ToPimId(items[i].Type), items[i]);
                    }
                }
            }

            bool Accept(RefineryBlueprint blueprint)
            {
                return !Refinery.InputInventory.IsFull && Refinery.AcceptedBlueprints.Contains(blueprint);
            }

            /// <summary>
            /// Orders the input inventory: scrap to the front, otherwise the ore with the higher priority
            /// of the first two items to the front. A third item is moved out.
            /// </summary>
            void OreSwap(List<IPrio> prios)
            {
                var inventory = Refinery.InputInventory;
                var items = new List<MyInventoryItem>();
                inventory.GetItems(items);
                if (items.Count < 2) return;

                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].Type.SubtypeId.ToLower().Contains("scrap"))
                    {
                        if (i > 0) inventory.TransferItemTo(inventory, i, 0, true, items[i].Amount);
                        return;
                    }
                }

                int firstPrio = 0, secondPrio = 0;
                var firstOre = ItemTypesConverter.ToPimId(items[0].Type);
                var secondOre = ItemTypesConverter.ToPimId(items[1].Type);
                foreach (IPrio p in prios)
                {
                    if (firstOre == p.refineryBP.InputID) firstPrio = p.initp;
                    else if (secondOre == p.refineryBP.InputID) secondPrio = p.initp;
                }
                if (firstPrio < secondPrio)
                {
                    inventory.TransferItemTo(inventory, 0, 1, true, items[0].Amount);
                }

                // only the current and the next ore stay
                for (int i = items.Count - 1; i >= 2; i--)
                {
                    clearItemByType(inventory, ItemTypesConverter.ToPimId(items[i].Type), items[i]);
                }
            }

            /// <summary>
            /// Fills the input inventory with one ore, from the cargo or from another refinery: the first one
            /// of the priority list (sorted, highest first) that can be filled in. Called if the refinery is
            /// nearly empty (the next ore is ready behind the current one) or a higher prioritised ore is waiting.
            /// </summary>
            void RefineryFilling(List<IPrio> prios)
            {
                bool tried = false;

                foreach (IPrio prio in prios)
                {
                    var blueprint = prio.refineryBP;
                    if (prio.prio == 0 || !Accept(blueprint) || !Lists.Data.Inventory.ContainsKey(blueprint.InputID))
                    {
                        continue;
                    }

                    // a higher prioritised ore is waiting and the refinery is more than half full: empty it first
                    // (not for an ore it works on already, it is only topped up)
                    if (Refinery.Success < 50 && blueprint != CurrentWorkBluePrint && blueprint != NextWorkBluePrint)
                    {
                        ClearInventory(Refinery.InputInventory);
                    }

                    if (SendItemByType(blueprint.InputID, Lists.Data.Inventory[blueprint.InputID], Refinery.InputInventory)
                        || OreStealing(blueprint))
                    {
                        Refinery.DeleteRefineryError(Refinery.RefError.NotFilled);
                        // the list is not enumerated any more after the sort
                        if (prio.LowerAfterFilling(Refinery.FunctionalCount)) prios.Sort();
                        return;
                    }
                    tried = true;
                }

                if (tried) Refinery.SetErrorByCondition(Refinery.RefError.NotFilled, Refinery.InputInventory.CurrentVolume == 0);
            }

            /// <summary>
            /// Empty refinery: takes the ore from another refinery that has more than 100 of it
            /// (half of the stack it is working on, the whole stack otherwise).
            /// </summary>
            bool OreStealing(RefineryBlueprint blueprint)
            {
                if (Refinery.InputInventory.CurrentVolume.RawValue >= 100) return false;

                var ownAmount = 0f;
                if (blueprint == CurrentWorkBluePrint) ownAmount = CurrentWorkOreAmount;
                else if (blueprint == NextWorkBluePrint) ownAmount = NextWorkOreAmount;

                var items = new List<MyInventoryItem>();
                foreach (Refinery refinery in Lists.Data.RefineryList)
                {
                    // only from refinerys PIM fills itself, the others belong to the player
                    if (refinery == Refinery || refinery.IsClosed || !refinery.IsPimControlled || !refinery.Manager.IsFilledByPim) continue;
                    items.Clear();
                    refinery.InputInventory.GetItems(items);
                    for (int i = 0; i < items.Count; i++)
                    {
                        var amount = (float)items[i].Amount;
                        if (amount > 100 && amount > ownAmount && FindBlueprint(items[i]) == blueprint)
                        {
                            var stealAmount = MyFixedPoint.MultiplySafe(items[i].Amount, i == 0 ? 0.5f : 1f);
                            if (refinery.InputInventory.TransferItemTo(Refinery.InputInventory, i, null, true, stealAmount)) return true;
                        }
                    }
                }
                return false;
            }

            /// <summary>
            /// true if an ore that is not in the refinery has a higher priority than the ores it works on
            /// (their priority counts one and a half times).
            /// </summary>
            bool HigherPrioOreWaiting(List<IPrio> prios)
            {
                int otherPrio = 0;
                int workPrio = 0;
                foreach (IPrio p in prios)
                {
                    if (p.refineryBP == CurrentWorkBluePrint || p.refineryBP == NextWorkBluePrint)
                    {
                        var weighted = (int)(p.prio * 1.5);
                        if (weighted > workPrio) workPrio = weighted;
                    }
                    // the list is sorted: the first other ore has the highest priority
                    else if (otherPrio == 0 && Accept(p.refineryBP)) otherPrio = p.prio;
                }
                return workPrio < otherPrio;
            }
        }
    }
}
