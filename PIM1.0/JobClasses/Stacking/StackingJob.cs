namespace IngameScript
{
    partial class Program
    {
        public class StackingJob : SequentialJob
        {
            private static readonly string[] StackTypes = new string[] { "Component", "Ore", "Ingot" };
            
            private int cur_stack_type;
            public static string StackType { get; private set; }

            public StackingJob(params Job[] jobs) : base(jobs)
            {
                cur_stack_type = -1;
                CooldownSeconds = Config.Instance.stacking_cycle;
                Active = CooldownSeconds > 0;
            }

            public override void InitJob()
            {
                cur_stack_type++;
                if (cur_stack_type >= StackTypes.Length)
                {
                    cur_stack_type = 0;
                    NextJob();
                }
                StackType = StackTypes[cur_stack_type];
            }
        }
    }
}
