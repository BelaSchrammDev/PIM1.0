namespace IngameScript
{
    partial class Program
    {
        // runningsign
        public class RunningSign : Tools
        {
            private int r = 0,
                        rc = 1,
                        mr = 7;

            public StringBuilderExtended getRunningSign()
            {
                runningSign.SetText('|');
                r += rc;
                if (r < 0)
                {
                    r = 1;
                    rc = 1;
                }
                else if (r > mr)
                {
                    r = mr - 1;
                    rc = -1;
                }
                for (int i = 0; i <= mr; i++) runningSign.Append(i == r ? (rc < 0 ? '<' : '>') : ' ');
                runningSign.Append("| ");
                return runningSign;
            }

            StringBuilderExtended runningSign = new StringBuilderExtended(10);
        }
    }
}
