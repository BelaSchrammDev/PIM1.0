namespace IngameScript
{
    // Manager infos and the text of the PB display. The infos and hints themselves are in StatusViews.cs.
    partial class Program
    {
        class AmmoManagerInfo : View
        {
            public override StringBuilderExtended GetInfoText()
            {
                if (Lists.Data.guns.Count == 0) return null;
                InfoString.SetText("AmmonitionManager: ", Lists.Data.guns.Count.ToString(), " weapons.");
                return InfoString;
            }
        }


        class StorageManagerInfo : View
        {
            public override StringBuilderExtended GetInfoText()
            {
                if (Lists.Data.StorageCargos.Count == 0) return null;
                InfoString.SetText("StorageManager: ", Lists.Data.StorageCargos.Count.ToString(), " containers.");
                return InfoString;
            }
        }


        class RefineryManagerInfo : View
        {
            public override StringBuilderExtended GetInfoText()
            {
                InfoString.Clear();
                foreach (var refinery in Lists.Data.RefineryList) refinery.GetErrorInfo(InfoString);
                if (InfoString.IsEmpty()) return null;
                InfoString.Insert(0, "--------------- RefineryManager ---------------\n");
                return InfoString;
            }
        }


        class AssemblerManagerInfo : View
        {
            public override StringBuilderExtended GetInfoText()
            {
                InfoString.Clear();
                foreach (var assembler in Lists.Data.AssemblerList) assembler.GetErrorInfo(InfoString);
                if (InfoString.IsEmpty()) return null;
                InfoString.Insert(0, "--------------- AssemblerManager ----------------\n");
                return InfoString;
            }
        }


        StringBuilderExtended infoString = new StringBuilderExtended(2000);
        StringBuilderExtended warnungstring = new StringBuilderExtended(1000);

        // PB info after the running sign, built once per cycle (WritePimRunInfo runs every tick)
        string PimStatus = " starting...\n";


        void CalculateInfos()
        {
            infoString.SetText("\n");
            warnungstring.Clear();
            foreach (var v in viewList)
            {
                switch (v.type)
                {
                    case View.ViewType.INFO: infoString.AppendLFifNotEmpty(v.GetInfoText()); break;
                    case View.ViewType.WARNING: warnungstring.AppendLF(v.GetInfoText()); break;
                }
            }
            if (!warnungstring.IsEmpty())
            {
                infoString.Append("\n--------------------- Hints ---------------------\n");
                infoString.Append(warnungstring);
            }
            PimStatus = LoopManager.IsMaster
                ? " Running / " + LoopManager.CurrentInstructionAmount + " inst. per run\ncurrent cycle: " + Properties.Data.CurrentCycleInSec.ToString("0.0") + " sec.\n" + infoString
                : "Standby\nMaster: " + Me.CustomName;
        }
    }
}
