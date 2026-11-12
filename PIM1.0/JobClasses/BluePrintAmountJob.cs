using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        /// <summary>
        /// Current amount (stock) of the items of a blueprint list, for the active blueprints also the
        /// assembling priority. One blueprint per step.
        /// </summary>
        public class BluePrintAmountJob : CountingJob
        {
            readonly Dictionary<string, AssemblerBluePrint> _bluePrints;
            readonly bool _calcPriority;
            readonly List<string> _keys = new List<string>();

            public BluePrintAmountJob(Dictionary<string, AssemblerBluePrint> bluePrints, bool calcPriority)
            {
                _bluePrints = bluePrints;
                _calcPriority = calcPriority;
            }

            protected override void ConfigureCountingBounds(out int startIndex, out int endIndex)
            {
                _keys.Clear();
                _keys.AddRange(_bluePrints.Keys);
                startIndex = 0;
                endIndex = _keys.Count - 1;
            }

            protected override void ProcessingIndex(int index)
            {
                var b = _bluePrints[_keys[index]];
                b.SetCurrentAmount((int)Lists.Data.Inventory.GetValueOrDefault(b.ItemName, 0));
                if (_calcPriority) b.CalcPriority();
            }
        }
    }
}
