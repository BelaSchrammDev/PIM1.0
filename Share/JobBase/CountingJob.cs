namespace IngameScript
{
    partial class Program
    {
        public abstract class CountingJob : Job
        {
            private int _index, _startIndex, _endIndex, _step;

            protected CountingJob(int cooldownSeconds = 0)
                : base(cooldownSeconds)
            {
            }

            public override void InitJob()
            {
                // Let derived classes define the boundaries
                ConfigureCountingBounds(out _startIndex, out _endIndex);

                // Check if there's nothing to process (empty range)
                if (IsEmptyRange(_startIndex, _endIndex))
                {
                    _step = 0; // Mark as empty/invalid
                    OnCountingStarted();
                    return;
                }

                // Automatically determine the direction
                _step = _startIndex <= _endIndex ? 1 : -1;
                _index = _startIndex;

                OnCountingStarted();
            }

            public override RunJobResult RunJob()
            {
                // If empty range, finish immediately
                if (_step == 0)
                {
                    return RunJobResult.Finished;
                }

                ProcessingIndex(_index);

                // Check if we've reached the end
                bool isFinished = _step > 0
                    ? _index >= _endIndex
                    : _index <= _endIndex;

                if (isFinished)
                {
                    return RunJobResult.Finished;
                }

                _index += _step;
                return RunJobResult.Continue;
            }

            /// <summary>
            /// Empty range: an index below 0, as given by an empty list
            /// (forward: start=0, end=Count-1=-1; backward: start=Count-1=-1, end=0).
            /// </summary>
            private bool IsEmptyRange(int start, int end)
            {
                return start < 0 || end < 0;
            }

            /// <summary>
            /// Defines start and end index. The counting direction is determined automatically.
            /// Example: start=0, end=10 → counts forward
            ///          start=10, end=0 → counts backward
            ///          start=0, end=-1 → empty range, nothing will be processed
            /// </summary>
            protected abstract void ConfigureCountingBounds(out int startIndex, out int endIndex);

            /// <summary>
            /// Called for each index (including start and end index).
            /// </summary>
            protected abstract void ProcessingIndex(int index);

            /// <summary>
            /// Optional: Called after the bounds have been configured.
            /// </summary>
            protected virtual void OnCountingStarted() { }
        }
    }
}
