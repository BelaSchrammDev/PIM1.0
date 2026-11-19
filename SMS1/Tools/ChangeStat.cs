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
        //Update
        public enum ChangeType
        {
            Empty,

            PV_add,

            Cargouse_add,
            Cargouse_change_amount,

            Inventar_add,
            Inventar_change_amount,
            Inventar_change_null,

            Resource_add,
            Resource_change_null,

            Ingotprio_add,
            Ingotprio_change_amount,
            Ingotprio_set_null,

            Weapon_add,
            Weapon_name,

            Battery_add,
            Battery_name,

            Cargo_add,
            Cargo_name,

            Connector_add,
            Connector_name,

            Dockings_add,
            Dockings_name,

            Airvent_add,
            Airvent_name,

            TankHydrogen_add,
            TankHydrogen_name,

            TankOxygen_add,
            TankOxygen_name,

            Jumpdrive_add,
            Jumpdrive_name,

            Gasgenerator_add,
            Gasgenerator_name,

            Reactor_add,
            Reactor_name,

            Refinery_add,
            Refinery_name,

            Hydrogenengine_add,
            Hydrogenengine_name,

            Assembler_add,
            Assembler_name,

            Damage_add,
            Damage_name,

            Display_add,
            Display_name,
        };
        static Dictionary<ChangeType, changeStat> changeStatObjects = new Dictionary<ChangeType, changeStat>();
        class changeStat
        {
            DateTime dt;
            public changeStat(ChangeType type = ChangeType.Empty)
            {
                Update();
            }
            public void Update()
            {
                dt = DateTime.Now;
            }
            public bool ifUpdateNeed(DateTime cdt)
            {
                if (cdt < dt) return true;
                else return false;
            }
        }
        static changeStat getChangeStat(ChangeType type) { if (!changeStatObjects.ContainsKey(type)) changeStatObjects.Add(type, new changeStat(type)); return changeStatObjects[type]; }
        static void changeNeed(ChangeType type) { getChangeStat(type).Update(); }
    }
}
