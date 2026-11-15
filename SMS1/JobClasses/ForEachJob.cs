using System;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        /// <summary>
        /// Collects a list at the start of the job, then processes one entry per RunJob.
        /// </summary>
        public class ForEachJob<T> : CountingJob
        {
            readonly Func<List<T>> _collect;
            readonly Action<T> _process;
            List<T> _items;

            public ForEachJob(Func<List<T>> collect, Action<T> process)
            {
                _collect = collect;
                _process = process;
            }

            protected override void ConfigureCountingBounds(out int startIndex, out int endIndex)
            {
                _items = _collect();
                startIndex = 0;
                endIndex = _items.Count - 1;
            }

            protected override void ProcessingIndex(int index)
            {
                _process(_items[index]);
            }
        }

        /// <summary>
        /// Runs an action once per cycle.
        /// </summary>
        public class ActionJob : Job
        {
            readonly Action _action;

            public ActionJob(Action action)
            {
                _action = action;
            }

            public override RunJobResult RunJob()
            {
                _action();
                return RunJobResult.Finished;
            }
        }
    }
}
