namespace IngameScript
{


    partial class Program
    {
        public class SendInfosToSmsJob : Job
        {
            public override RunJobResult RunJob()
            {
                MyProgram.SendInfosToSMS();
                return base.RunJob();
            }
        }
    }
}
