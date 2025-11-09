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
                var group = GridTerminalSystem.GetBlockGroupWithName(Propertys.Data.CurrentGunGroupName);
                if (group == null)
                {
                    ClearGunList();
                    startIndex = -1;
                }
                else
                {
                    group.GetBlocksOfType(_GroupGunList, block => BlockConstructMember(block));
                    RefreshGunBlockList();
                    startIndex = _GroupGunList.Count - 1;
                }
                endIndex = 0;
            }

            protected override void ProcessingIndex(int index)
            {
                Lists.Data.guns.Add(new Gun(_GroupGunList[index]));
            }

            private void RefreshGunBlockList()
            {
                for (int i = Lists.Data.guns.Count - 1; i >= 0; i--)
                {
                    if (_GroupGunList.Contains(Lists.Data.guns[i].gun))
                    {
                        _GroupGunList.Remove(Lists.Data.guns[i].gun);
                    }
                    else
                    {
                        var gun = Lists.Data.guns[i];
                        gun.Remove();
                        Lists.Data.guns.Remove(gun);
                    }
                }
            }

            private void ClearGunList()
            {
                for (int i = Lists.Data.guns.Count - 1; i >= 0; i--)
                {
                    var gun = Lists.Data.guns[i];
                    gun.Remove();
                    Lists.Data.guns.Remove(gun);
                }
            }
        }
    }
}
