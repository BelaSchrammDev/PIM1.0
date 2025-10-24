using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        public abstract class Tools 
        {
            public static bool BlockConstructMember(IMyTerminalBlock block) 
            {
                return block.IsSameConstructAs(Program.Instance.Me);
            }

            public static void AddToDebugString(string str)
            {
                Program.debugString += str;
            }

            public static int GetBlockList<T>(List<T> blockList)
                where T : class, IMyTerminalBlock
            {
                Program.Instance.GridTerminalSystem.GetBlocksOfType<T>(blockList, block => BlockConstructMember(block));
                return blockList.Count;
            }
        }

        public abstract class Job : Tools
        {
            // Reference to the main Program instance
            protected readonly Program Program;

            // Human-readable name of the job
            public string Name { get; }

            public bool Active { get; set; } = true;

            // Cooldown period in seconds
            public int CooldownSeconds
            {
                get
                {
                    return (int)_cooldown.TotalSeconds;
                }
                set
                {
                    _cooldown = TimeSpan.FromSeconds(value);
                }
            }

            // Cooldown period between job executions
            private TimeSpan _cooldown;

            // Timestamp when the last run ended
            private DateTime _lastRunEnd = DateTime.Now;

            protected Job(Program program, string name) : this(program, name, 0)
            {
            }

            protected Job(Program program, string name, int cooldownSeconds)
            {
                Program = program;
                Name = name;
                _cooldown = TimeSpan.FromSeconds(cooldownSeconds);
            }

            // Internal state machine statuses
            private enum JobStatus
            {
                Init,       // Ready to initialize
                Running,    // Currently running
                Cooling     // Waiting for cooldown to expire
            }

            // Result of scheduling attempt
            public enum ScheduleResult
            {
                InProgress, // Job has more work to do
                Done        // Job has completed this cycle
            }

            // Result of a single RunJob invocation
            public enum RunJobResult
            {
                Continue,   // Continue running next step
                Finished    // This job is finished for now
            }

            private JobStatus _status = JobStatus.Init;

            /// <summary>
            /// Called once when the job is first scheduled.
            /// </summary>
            public virtual void InitJob() { }

            /// <summary>
            /// Performs one unit of work. Return Continue to indicate more work remains,
            /// or Finished when this job has completed its current batch.
            /// </summary>
            public virtual RunJobResult RunJob()
            {
                return RunJobResult.Finished;
            }

            /// <summary>
            /// Advances the job state machine:
            /// - Init → calls InitJob, moves to Running
            /// - Running → calls RunJob until it returns Finished, then starts cooldown
            /// - Cooling → waits until cooldown expires, then resets to Init
            /// </summary>
            public ScheduleResult Schedule()
            {
                if (!Active)
                {
                    return ScheduleResult.Done;
                }

                switch (_status)
                {
                    case JobStatus.Init:
                        InitJob();
                        _status = JobStatus.Running;
                        return ScheduleResult.InProgress;

                    case JobStatus.Running:
                        if (RunJob() == RunJobResult.Continue)
                        {
                            // Job is in progress, continue running
                            return ScheduleResult.InProgress;
                        }
                        else
                        {
                            // Job finished for now, start cooldown
                            _status = JobStatus.Cooling;
                            _lastRunEnd = DateTime.Now;
                            return ScheduleResult.Done;
                        }

                    case JobStatus.Cooling:
                        // Check if cooldown has elapsed
                        if (DateTime.Now - _lastRunEnd > _cooldown)
                        {
                            _status = JobStatus.Init;
                            return ScheduleResult.InProgress;
                        }

                        return ScheduleResult.Done;
                }

                return ScheduleResult.Done;
            }
        }

        public abstract class CountingJob : Job
        {
            private int _index;
            private int _startIndex;
            private int _endIndex;
            private int _step;

            protected CountingJob(Program program, string name, int cooldownSeconds = 0)
                : base(program, name, cooldownSeconds)
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

        public class MultiJob : Job
        {
            // Sub-jobs to be executed in sequence
            private readonly Job[] _subJobs;

            // Index of the currently active sub-job
            private int _currentSubJobIndex;

            public MultiJob(Program program, string name, params Job[] subJobs)
                : base(program, name)
            {
                _subJobs = subJobs;
            }

            public override void InitJob()
            {
                // Start from the first sub-job
                _currentSubJobIndex = 0;
            }

            public override RunJobResult RunJob()
            {
                // Run the current sub-job until it signals Finished
                var result = _subJobs[_currentSubJobIndex].Schedule();
                if (result == ScheduleResult.Done)
                {
                    _currentSubJobIndex++;
                    // All sub-jobs completed?
                    if (_currentSubJobIndex >= _subJobs.Length)
                        return RunJobResult.Finished;
                }
                return RunJobResult.Continue;
            }
        }

        public class SequentialJob : Job
        {
            private readonly Job[] _jobs;
            private int? _currentJobIndex = null;
            public SequentialJob(Program program, string name, params Job[] jobs) : base(program, name)
            {
                _jobs = jobs;
            }

            public bool NextJob()
            {
                if(_currentJobIndex.HasValue)
                {
                    _currentJobIndex++;
                    if (_currentJobIndex >= _jobs.Length)
                    {
                        _currentJobIndex = null;
                        return false;
                    }
                }
                else if (_jobs.Length == 0)
                {
                    return false;
                }
                else
                {
                    _currentJobIndex = 0;
                }

                return true;
            }

            public override void InitJob()
            {
                NextJob();
            }

            public override RunJobResult RunJob()
            {
                if(!_currentJobIndex.HasValue)
                {
                    return RunJobResult.Finished;
                }

                var result = _jobs[_currentJobIndex.Value].Schedule();

                if (result == ScheduleResult.Done && !NextJob())
                {
                    return RunJobResult.Finished;
                }

                return RunJobResult.Continue;
            }
        }
    }
}
