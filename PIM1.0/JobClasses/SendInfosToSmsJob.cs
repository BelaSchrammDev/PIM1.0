namespace IngameScript
{


    partial class Program
    {
        public class SendInfosToSmsJob : Job
        {
            public override RunJobResult RunJob()
            {
                Tools.ProgramInstance.SendInfosToSMS();
                return base.RunJob();
            }
        }
    }
}
