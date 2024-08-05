using System.Diagnostics;
using System.ServiceProcess;

namespace VhdAttachService
{
    internal class AppService : ServiceBase
    {

        private static AppService _instance = new();
        public static AppService Instance => _instance;


        private AppService()
        {
            AutoLog = true;
            CanStop = true;
            ServiceName = "VhdAttach";
        }

        protected override void OnStart(string[] args)
        {
            Debug.WriteLine("AppService : Start requested.");
            Service.Start();
        }

        protected override void OnStop()
        {
            Debug.WriteLine("AppService : Stop requested.");
            Service.Stop();
        }

    }
}
