using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        public class FindControllingGunJob : CountingJob
        {
            private List<IMyUserControllableGun> _GroupGunList = new List<IMyUserControllableGun>();

            protected override void ConfigureCountingBounds(out int startIndex, out int endIndex)
            {
                // no group: no gun is found, so all known guns are released
                var group = MyProgram.GridTerminalSystem.GetBlockGroupWithName(Properties.Data.CurrentGunGroupName);
                if (group == null) _GroupGunList.Clear();
                else group.GetBlocksOfType(_GroupGunList, block => block.IsMainConstruct());

                Tools.SyncBlockList(_GroupGunList, Lists.Data.guns, g => g.gun, g => g.Remove());
                startIndex = _GroupGunList.Count - 1;
                endIndex = 0;
            }

            protected override void ProcessingIndex(int index)
            {
                Lists.Data.guns.Add(new Gun(_GroupGunList[index]));
            }
        }
    }
}
