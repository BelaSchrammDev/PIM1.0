namespace IngameScript
{
    partial class Program
    {

        /// <summary>
        /// Type specific handling of a PIM controlled block. The block class itself only does what is the same
        /// for every type (counting, unloading, errors, power).
        /// </summary>
        public abstract class BlockManager : Tools
        {
            public ManageableBlock Block { get; }

            public BlockManager(ManageableBlock block)
            {
                Block = block;
                Block.Manager = this;
            }

            /// <summary>
            /// Phase 1 (FindingRefinerysJob): type specific state. Runs for all blocks before phase 2,
            /// which needs the results (CalcIngotPrio reads refineryTypesAcceptedBlueprintsList).
            /// </summary>
            public virtual void Refresh() { }

            /// <summary>Phase 2 (RefineryManagerJob): fill or clear the block.</summary>
            public abstract void DoManage();

            /// <summary>false: PIM does not fill the block and leaves conveyor and power alone.</summary>
            public virtual bool IsFilledByPim => true;
        }

        public abstract class RefineryBlockManagerer : BlockManager
        {
            protected RefineryBlockManagerer(ManageableBlock block) : base(block)
            {
            }

            protected Refinery Refinery { get { return (Refinery)Block; } }

            /// <summary>Amount still missing up to the autocrafting maximum (shown as "in production").</summary>
            protected static void CalculateRefineryAmount(string bluePrintName)
            {
                AssemblerBluePrint b;
                if (Lists.Data.BluePrints_Active.TryGetValue(bluePrintName, out b))
                {
                    var u = b.MaximumItemAmount - b.CurrentItemAmount;
                    b.RefineryAmount = b.MaximumItemAmount > 0 && u > 0 ? u : 0;
                }
            }
        }

        /// <summary>Known block that PIM does not fill (e.g. incinerator, composter): only a hint on the PB display.</summary>
        public class NoAutofillManager : RefineryBlockManagerer
        {
            public NoAutofillManager(ManageableBlock block) : base(block)
            {
            }

            public override bool IsFilledByPim => false;

            public override void Refresh()
            {
                Refinery.AddRefineryError(Refinery.RefError.NoAutofill);
            }

            public override void DoManage() { }
        }

        /// <summary>Block type PIM does not know: a warning, nothing else.</summary>
        public class UnknownRefineryManager : NoAutofillManager
        {
            public UnknownRefineryManager(ManageableBlock block) : base(block)
            {
            }

            public override void Refresh()
            {
                SetWarning(Warning.ID.REFINERYNOTSUPPORTED, Refinery.BlockSubType);
            }
        }
    }
}
