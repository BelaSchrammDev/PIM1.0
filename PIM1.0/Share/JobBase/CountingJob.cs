namespace IngameScript
{
    partial class Program
    {
        public abstract class CountingJob : Job
        {
            private int _index;
            private int _startIndex;
            private int _endIndex;
            private int _step;

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
            /// Checks if the range is empty (no valid indices to process).
            /// </summary>
            private bool IsEmptyRange(int start, int end)
            {
                // Forward: start > end is invalid (e.g., start=5, end=3)
                // Backward: end > start is invalid (e.g., start=3, end=5 but meant to go backward)
                // Actually, we need to check if it's logically empty
                // For now, check common empty cases like list.Count=0 → start=0, end=-1
                return (start == 0 && end < 0) || (start < 0) || (end < 0 && start >= 0);
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

            // Helpful properties for derived classes
            protected int CurrentIndex => _index;
            protected int StartIndex => _startIndex;
            protected int EndIndex => _endIndex;
            protected bool IsCountingForward => _step > 0;
            protected bool IsEmpty => _step == 0;
        }
    }
}
