using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        // Weapons
        class Weapon : dyBlockPowerConsumer
        {
            IMyUserControllableGun gun;
            string ammoname;
            int ammonum, ammoamount;
            public Weapon(IMyTerminalBlock b)
            {
                addChangeType = ChangeType.Weapon_add;
                nameChangeType = ChangeType.Weapon_name;
                tb = b;
                gun = (IMyUserControllableGun)b;
                getBlockDefinition();
                sT("@fAo", "", 50);
                sT("@fWt", bs == "" ? bt : bs, 50);
                sD("@fAc", 0, 20);
                sD("@fAa", 0, 10);
                setBlockTypeFields();
                addChange();
            }
            public override void getPCvar(dyDisplayableVariable pv)
            {
                var gi = readInventarList(pv);
                ammoname = "--- empthy ---";
                ammonum = 0;
                ammoamount = 0;
                if (gi.items.Count > 0)
                {
                    var newammo = gi.gIT(0);
                    if (smsItemTypeDefinitions.ContainsKey(newammo))
                    {
                        ammoname = smsItemTypeDefinitions[newammo].name;
                        ammoamount = (int)gi.gA(0);
                        ammonum = (int)gi.gA(newammo) * smsItemTypeDefinitions[newammo].ammocapacity;
                    }
                    else
                    {
                        ammoname = newammo.SubtypeId.ToString();
                        ammonum = (int)gi.gA(newammo);
                    }
                }
                base.getPCvar(pv);
            }
            public override void Display()
            {
                sT("@fAo", ammoname, 50);
                sD("@fAc", ammonum, 20);
                sD("@fAa", ammoamount, 10);
                base.Display();
            }
        }
    }
}
