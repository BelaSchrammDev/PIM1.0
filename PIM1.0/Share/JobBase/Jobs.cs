using System;

namespace IngameScript
{
    partial class Program
    {

        public abstract class Job
        {
            public static Func<DateTime> Clock = () => DateTime.Now;

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
            private DateTime _lastRunEnd = Clock();

            protected Job(int cooldownSeconds = 0)
            {
                Name = this.GetType().Name;
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

            protected void ResetJob()
            {
                _status = JobStatus.Init;
            }

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
                            _lastRunEnd = Clock();
                            return ScheduleResult.Done;
                        }

                    case JobStatus.Cooling:
                        // Check if cooldown has elapsed
                        if (Clock() - _lastRunEnd > _cooldown)
                        {
                            _status = JobStatus.Init;
                            return ScheduleResult.InProgress;
                        }

                        return ScheduleResult.Done;
                }

                return ScheduleResult.Done;
            }
        }
    }
}
