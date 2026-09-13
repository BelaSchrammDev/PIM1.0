namespace IngameScript
{
    partial class Program
    {
        public class Loop
        {
            private static Loop _data = null;
            public static Loop Data
            {
                get
                {
                    if (_data == null)
                    {
                        _data = new Loop();
                    }
                    return _data;
                }
            }

            // Index of the currently running job
            public int CurrentJobIndex = 0;
        }
    }
}
