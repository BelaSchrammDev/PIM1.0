using System;

namespace IngameScript
{
    partial class Program
    {
        public abstract class Job
        {
            // Reference to the main Program instance
            protected readonly Program Program;

            // Human-readable name of the job
            public string Name { get; }

            // Cooldown period between job executions
            private readonly TimeSpan _cooldown;

            // Timestamp when the last run ended
            private DateTime _lastRunEnd = DateTime.Now;

            protected Job(Program program, string name, int cooldownSeconds = 0)
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
                switch (_status)
                {
                    case JobStatus.Init:
                        InitJob();
                        _status = JobStatus.Running;
                        return ScheduleResult.InProgress;

                    case JobStatus.Running:
                        if (RunJob() == RunJobResult.Continue)
                            return ScheduleResult.InProgress;

                        // Job finished for now, start cooldown
                        _status = JobStatus.Cooling;
                        _lastRunEnd = DateTime.Now;
                        return ScheduleResult.Done;

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
    }
}
