using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        /// <summary>Input inventory of a block with one fixed recipe: PIM keeps the ingredients in it.</summary>
        public class RecipeInputInventory : StorageInventory
        {
            public RecipeInputInventory(IMyInventory input, Dictionary<string, float> recipe)
            {
                inv = input;
                foreach (var ingredient in recipe) items.Add(ingredient.Key, ingredient.Value);
            }

            public override bool ItemsAmountInvalid()
            {
                return true;
            }
        }

        /// <summary>
        /// Block with one fixed recipe (Hydroponics, Reprocessor): filled with the ingredients until the
        /// autocrafting maximum of its blueprint is reached, emptied then.
        /// </summary>
        public abstract class RecipeManager : RefineryBlockManagerer
        {
            readonly string _bluePrint;
            readonly RecipeInputInventory _inputInventory;

            protected RecipeManager(ManageableBlock block, string bluePrint, Dictionary<string, float> recipe) : base(block)
            {
                _bluePrint = bluePrint;
                _inputInventory = new RecipeInputInventory(Refinery.InputInventory, recipe);
            }

            public override void Refresh()
            {
                CalculateRefineryAmount(_bluePrint);
            }

            public override void DoManage()
            {
                AssemblerBluePrint bluePrint;
                if (TryGetActiveAssemblerBluePrint(_bluePrint, out bluePrint) && !bluePrint.IfMax())
                {
                    if (!_inputInventory.IfItemsExists() && Refinery.Success > 90)
                    {
                        _inputInventory.ReloadItems();
                    }
                }
                else
                {
                    ClearInventory(Refinery.InputInventory);
                }
            }
        }
    }
}
