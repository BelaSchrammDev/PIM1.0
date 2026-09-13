namespace IngameScript
{


    partial class Program
    {
        public class SendInfosToSmsJob : Job
        {
            public override RunJobResult RunJob()
            {
                ProgramInstance.SendInfosToSMS();
                return base.RunJob();
            }
        }
    }
}
