namespace IngameScript
{
    partial class Program
    {
        public abstract class BlockManager : Tools
        {
            public ManageableBlock Block { get; }

            public BlockManager(ManageableBlock block)
            {
                Block = block;
                Block.Manager = this;
            }

            public abstract void DoManage();
        }

        public class DummyBlockManager : BlockManager
        {
            public DummyBlockManager(ManageableBlock block) : base(block)
            {
            }

            public override void DoManage()
            {
                Program.LCD_DebugString += $"DummyBlockManager_DoManage '{Block.FunctionalBlock.BlockDefinition}'\n";
                // Do nothing
            }
        }
    }
}
