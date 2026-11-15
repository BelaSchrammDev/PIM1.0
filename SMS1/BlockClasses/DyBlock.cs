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
        //B
        public abstract class dyBlock : dyBlockDisplayable
        {
            public P pms = new P(); public IMyCubeGrid mcg = null; string sdi = ""; public IMyTerminalBlock tb; public string bt = "xxx"; public string bs = "xxx"; public List<inventarList> invs = new List<inventarList>();
            public inventarList readInventarList(dyDisplayableVariable p, int i = 0, char c = 'F') { inventarList iv = getInventarList(i); if (iv == null) { var niv = tb.GetInventory(i); if (niv == null) return null; iv = new inventarList(i, niv, new VC(c, "@fF" + i, Fv)); invs.Add(iv); add_to_VC(iv.volume); } iv.RefreshInventoryItems(mcg, p); return iv; }
            public inventarList getInventarList(int i = 0) { foreach (var x in invs) if (x.id == i) return x; return null; }
            public string[] getDetailedInfo() { if (sdi != tb.DetailedInfo) { return tb.DetailedInfo.Split('\n'); } return new string[] { }; }
            public void getBlockDefinition() { bs = tb.BlockDefinition.SubtypeName; bt = tb.BlockDefinition.TypeIdString; bt = bt.Substring(bt.IndexOf("_") + 1); }
            public void setBlockTypeFields() { sT("@fBt", bt); sT("@fBs", bs); bt += "s"; }
            public void InitRefresh() { pms.ParseArgs(tb); setName(pms.Name); Refresh(); }
            public virtual void getPCvar(dyDisplayableVariable pv) { }
            public virtual void remove() { addChange(); }
            public override void Refresh(bool f = false) { if (mcg == null) { mcg = tb.CubeGrid; sT("@fG", mcg.CustomName); }; getPCvar(dyDisplayableVariable.getPV(mcg, bt)); base.Refresh(); }
        }
        // Cargo
        public class dyCargoContainer : dyBlock
        {
            public dyCargoContainer(IMyTerminalBlock t) { addChangeType = ChangeType.Cargo_add; nameChangeType = ChangeType.Cargo_name; tb = t; getBlockDefinition(); bt = "Container"; setBlockTypeFields(); addChange(); }
            public override void getPCvar(dyDisplayableVariable pv) { readInventarList(pv); }
        }
        //Damageblocks
        public class dyDamageBlock : dyBlock
        {
            VC h;
            public dyDamageBlock(IMyTerminalBlock t) { addChangeType = ChangeType.Damage_add; nameChangeType = ChangeType.Damage_name; if (ShowDamageBlockOnHUD) t.ShowOnHUD = true; tb = t; getBlockDefinition(); setBlockTypeFields(); bt = "Damageblocks"; addChange(); h = getVC('h'); h.M = 1f; }
            public override void remove() { addChange(); tb.ShowOnHUD = false; }
            float gH() { IMySlimBlock s = tb.CubeGrid.GetCubeBlock(tb.Position); return (s.BuildIntegrity - s.CurrentDamage) / s.MaxIntegrity; }
            public override void getPCvar(dyDisplayableVariable pv) { h.C = gH(); sV('h', 1, h.C, pv); }
        }
        //Py
        public abstract class dyBlockFunctional : dyBlock
        {
            public bool iEF() { return tb.IsWorking; }
            public override void Refresh(bool f = false) { sT("@fC", pms.smscontrol ? "[c]" : "[  ]"); sT("@fE", (tb as IMyFunctionalBlock).Enabled ? "On" : "Off", 7.1f); base.Refresh(); }
        }
        //Mi
        public abstract class dyBlockPowerConsumer : dyBlockFunctional
        {
            public VC vci; public bool IsW = true;
            public dyBlockPowerConsumer() { vci = getVC('I'); }
            public void sI(dyDisplayableVariable pv, float f = 0.001f) { var u = getPstr(getDetailedInfo()[1]); sV('I', u, iEF() ? (IsW ? u : f) : 0, pv); }
            public override void getPCvar(dyDisplayableVariable pv)
            {
                var dilist = getDetailedInfo();
                if (dilist.Length < 2) return;
                var M = getPstr(dilist[1]);
                var C = iEF() ? getPstr(dilist[2]) : 0;
                pv.aV('I', M, C);
                vci.M = M;
                vci.C = C;
            }
        }
        //Ei
        public abstract class dyBlockPowerProducer : dyBlockFunctional
        {
            VC vo; VC vm;
            public dyBlockPowerProducer() { vm = getVC('M'); vo = getVC('E'); }
            public override void getPCvar(dyDisplayableVariable pv) { var p = (IMyPowerProducer)tb; sV('M', p.MaxOutput, iEF() ? p.MaxOutput : 0, pv); sV('E', iEF() ? p.MaxOutput : 0, p.CurrentOutput, pv); }
        }
        //Es
        public abstract class dyBlockPowerStorage : dyBlockPowerProducer
        {
            public VC vs;
            public dyBlockPowerStorage() { vs = getVC('S'); }
            public void aRS(double t) { if (iEF()) rsp += t; }
        }
        //Dock
        public class dyConnector : dyBlockFunctional
        {
            bool ifdock = false;
            public IMyShipConnector connector;
            public IMyShipConnector docked_connector = null;
            public string dockName;
            public dyConnector(IMyTerminalBlock t)
            {
                addChangeType = ChangeType.Connector_add;
                nameChangeType = ChangeType.Connector_name;
                tb = t;
                connector = t as IMyShipConnector;
                getBlockDefinition();
                bt = "Connector";
                setBlockTypeFields();
                sT("@fDN", " ");
                addChange();
            }
            public override void remove() { docks.Remove(this); changeNeed(ChangeType.Dockings_add); addChange(); }
            public override void getPCvar(dyDisplayableVariable pv)
            {
                readInventarList(pv);
                sT("@fSct", gCT());
                var ocon = docked_connector;
                if (connector.Status == MyShipConnectorStatus.Connected)
                {
                    if (connector.OtherConnector.CubeGrid.IsStatic)
                    {
                        docked_connector = null;
                        sT("@fSt", "Connected to Station", 30);
                        sT("@fSh", connector.OtherConnector.CubeGrid.CustomName);
                    }
                    else
                    {
                        docked_connector = connector.OtherConnector;
                        sT("@fSt", "Connected", 30);
                        sT("@fSh", docked_connector.CubeGrid.CustomName);
                    }
                }
                else
                {
                    docked_connector = null;
                    sT("@fSt", connector.Status == MyShipConnectorStatus.Connectable ? "ready to connect" : "Clear", 30);
                    sT("@fSh", "---");
                }
                if (pms.isPM("Dock") && runPB.IsSameConstructAs(connector))
                {
                    if (!ifdock)
                    {
                        ifdock = true;
                        docks.Add(this);
                        changeNeed(ChangeType.Dockings_add);
                    }
                    var newDockName = pms.getPM("Dock");
                    if (dockName != newDockName)
                    {
                        dockName = newDockName;
                        changeNeed(ChangeType.Dockings_name);
                        sT("@fDN", dockName);
                    }
                    if (ocon != docked_connector)
                    {
                        if (òb.Contains(ocon)) òb.Remove(ocon);
                        else if (docked_connector != null) òb.Add(docked_connector);
                        changeNeed(ChangeType.Dockings_add); // get noch besser
                    }
                }
                else
                {
                    if (ifdock)
                    {
                        ifdock = false;
                        docks.Remove(this);
                        dockName = " ";
                        sT("@fDN", dockName);
                        changeNeed(ChangeType.Dockings_add);
                    }
                }
            }
            string gCT(int i = 0)
            {
                var r = " ";
                var di = getDetailedInfo();
                if (di.Length > i)
                {
                    var dis = di[i].Substring(di[i].IndexOf(':') + 1).Split(':');
                    if (dis.Length > 2)
                    {
                        float tm = getIntegerWithBounds(dis[1]);
                        float ts = getIntegerWithBounds(dis[2]);
                        float tg = (ts / 3600) + (tm / 60) + getIntegerWithBounds(dis[0]);
                        r = fT(tg);
                    }
                }
                return r;
            }
        }
    }
}
