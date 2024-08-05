using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;
using Medo.Application;
using Medo.Diagnostics;

namespace VhdAttachService
{

    internal static class App
    {

        [STAThread]
        private static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            if (Args.Current.ContainsKey("Interactive"))
            {

                Tray.Show();
                Service.Start();
                Tray.SetStatusToRunningInteractive();
                Application.Run();
                Service.Stop();
                Tray.Hide();
                Environment.Exit(0);

            }
            else if (Args.Current.ContainsKey("Install"))
            {

                try
                {
                    using (ServiceController sc = new(AppService.Instance.ServiceName))
                    {
                        if (sc.Status != ServiceControllerStatus.Stopped) { sc.Stop(); }
                    }
                }
                catch (Exception) { }

                ManagedInstallerClass.InstallHelper([Assembly.GetExecutingAssembly().Location]);
                Environment.Exit(0);

            }
            else if (Args.Current.ContainsKey("Uninstall"))
            {

                try
                {
                    using (ServiceController sc = new(AppService.Instance.ServiceName))
                    {
                        if (sc.Status != ServiceControllerStatus.Stopped) { sc.Stop(); }
                    }
                }
                catch (Exception) { }
                try
                {
                    ManagedInstallerClass.InstallHelper(["/u", Assembly.GetExecutingAssembly().Location]);
                    Environment.Exit(0);
                }
                catch (InstallException)
                { //no service with that name
                    Environment.Exit(1);
                }

            }
            else if (Args.Current.ContainsKey("Start"))
            {

                try
                {
                    using (var service = new ServiceController("VhdAttach"))
                    {
                        if (service.Status != ServiceControllerStatus.Running)
                        {
                            service.Start();
                            service.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 1));
                        }
                    }
                }
                catch (Exception) { }
                Environment.Exit(0);

            }
            else
            {

                if (Environment.UserInteractive)
                {
                    Tray.Show();
                    ServiceStatusThread.Start();
                    Application.Run();
                    ServiceStatusThread.Stop();
                    Tray.Hide();
                    Environment.Exit(0);
                }
                else
                {
                    ServiceBase.Run([AppService.Instance]);
                }

            }
        }


        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ErrorReport.SaveToTemp(e.ExceptionObject as Exception);
            AppService.Instance.ExitCode = 1064; //ERROR_EXCEPTION_IN_SERVICE
            AppService.Instance.AutoLog = false;
            Thread.Sleep(1000); //just to sort it properly in event log.
            Environment.Exit(AppService.Instance.ExitCode);
        }

    }

}
