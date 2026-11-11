namespace IngameScript
{
    partial class Program
    {
        /// <summary>
        /// One stacking step per cooldown (config stacking_cycle): one strategy on one item type.
        /// All strategies run on a type one after the other, then the next type is used:
        /// MergeInContainer/Component, Gather/Component, ... Swap/Component, MergeInContainer/Ore, ...
        /// Pure (no SE types), tested in StackingJobTests.
        /// </summary>
        public class StackingJob : Job
        {
            private static readonly string[] StackTypes = new string[] { "Component", "Ore", "Ingot" };

            private readonly Job[] _strategies;
            private int _strategy, _type;

            // item type of the current step, read by the strategies (StackingStrategyJob)
            public static string StackType { get; private set; }

            /// <param name="cooldownSeconds">pause between two steps, 0 = stacking off</param>
            public StackingJob(int cooldownSeconds, params Job[] strategies) : base(cooldownSeconds)
            {
                _strategies = strategies;
                Active = cooldownSeconds > 0 && strategies.Length > 0;
            }

            public override void InitJob()
            {
                StackType = StackTypes[_type];
            }

            public override RunJobResult RunJob()
            {
                if (_strategies[_strategy].Schedule() == ScheduleResult.InProgress) return RunJobResult.Continue;

                // strategy done: the next step uses the next strategy, after the last one the next type
                if (++_strategy >= _strategies.Length)
                {
                    _strategy = 0;
                    if (++_type >= StackTypes.Length) _type = 0;
                }
                return RunJobResult.Finished;
            }
        }
    }
}
