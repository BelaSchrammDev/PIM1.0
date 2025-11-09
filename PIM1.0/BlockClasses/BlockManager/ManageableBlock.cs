using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        public abstract class ManageableBlock 
        {
            public BlockManager Manager { get; internal set; }

            public IMyFunctionalBlock FunctionalBlock => GetFunctionalBlock();

            public bool IsFunctional => FunctionalBlock != null && FunctionalBlock.IsFunctional;

            public bool IsWorking => FunctionalBlock != null && FunctionalBlock.IsWorking;

            public bool IsNotClosed => FunctionalBlock != null && !FunctionalBlock.Closed;

            public virtual bool RunManager()
            {
                if (Manager != null && IsNotClosed)
                {
                    Manager.DoManage();
                    return true;
                }

                return false;
            }

            public abstract IMyFunctionalBlock GetFunctionalBlock();
        }

        public abstract class ManageableRefineryBlock : ManageableBlock
        {
            
        }

        public abstract class ManageableAssemblerBlock : ManageableBlock
        {

        }
    }
}
