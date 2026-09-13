using Sandbox.ModAPI.Ingame;
using System.Collections;
using System.Reflection;
using VRage;

namespace IngameScript
{
    partial class Program
    {
        public abstract class ManageableBlock : Tools
        {
            public Parameter Parameter = new Parameter();

            public BlockManager Manager { get; internal set; }

            public IMyFunctionalBlock FunctionalBlock => GetFunctionalBlock();

            public bool IsFunctional => FunctionalBlock != null && FunctionalBlock.IsFunctional;

            public bool IsWorking => FunctionalBlock != null && FunctionalBlock.IsWorking;

            public bool IsClosed => FunctionalBlock != null && FunctionalBlock.Closed;

            public bool IsPimControlled => Parameter.PIMcontrolled;

            public virtual bool RunManager()
            {
                if (Manager != null && !IsClosed)
                {
                    Manager.DoManage();
                    return true;
                }

                return false;
            }

            public abstract IMyFunctionalBlock GetFunctionalBlock();
        }
    }
}
