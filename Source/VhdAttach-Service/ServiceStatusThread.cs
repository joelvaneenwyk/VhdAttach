using System;
using System.Diagnostics;
using System.Globalization;
using System.ServiceProcess;
using System.Threading;

namespace VhdAttachService
{
    internal static class ServiceStatusThread
    {

        private static Thread Thread;
        private static ManualResetEvent CancelEvent;

        public static void Start()
        {
            if (Thread != null) { return; }

            CancelEvent = new ManualResetEvent(false);
            Thread = new Thread(Run);
            Thread.Name = "Service status";
            Thread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.Start();
        }

        public static void Stop()
        {
            try
            {
                CancelEvent.Set();
                while (Thread.IsAlive) { Thread.Sleep(10); }
                Thread = null;
            }
            catch { }
        }


        private static void Run()
        {
            try
            {
                var sw = new Stopwatch();
                using (var service = new ServiceController(AppService.Instance.ServiceName))
                {
                    bool? lastIsRunning = null;
                    Tray.SetStatusToUnknown();
                    while (!CancelEvent.WaitOne(0, false))
                    {
                        if ((sw.IsRunning == false) || (sw.ElapsedMilliseconds > 1000))
                        {
                            bool? currIsRunning;
                            try
                            {
                                service.Refresh();
                                currIsRunning = (service.Status == ServiceControllerStatus.Running);
                            }
                            catch (InvalidOperationException)
                            {
                                currIsRunning = null;
                            }
                            if (lastIsRunning != currIsRunning)
                            {
                                if (currIsRunning == null)
                                {
                                    Tray.SetStatusToUnknown();
                                }
                                else if (currIsRunning == true)
                                {
                                    Tray.SetStatusToRunning();
                                }
                                else
                                {
                                    Tray.SetStatusToStopped();
                                }
                            }
                            lastIsRunning = currIsRunning;
                            sw.Reset();
                            sw.Start();
                        }
                        Thread.Sleep(100);
                    }
                }
            }
            catch (ThreadAbortException) { }
        }

    }
}
