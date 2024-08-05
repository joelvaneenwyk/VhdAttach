using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using Medo.Application;
using Medo.Windows.Forms;
using MessageBox = Medo.MessageBox;

namespace VhdAttach
{
    public static class App
    {

        [STAThread]
        public static void Main()
        {
            var mutexSecurity = new MutexSecurity();
            mutexSecurity.AddAccessRule(new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow));
            using (var setupMutex = new Mutex(false, @"Global\JosipMedved_VhdAttach", out bool createdNew))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                UnhandledCatch.ThreadException += UnhandledCatch_ThreadException;
                UnhandledCatch.Attach();

                if (!((Environment.OSVersion.Version.Build < 7000) || (IsRunningOnMono)))
                {
                    var appId = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
                    if (appId.Length > 127) { appId = @"JosipMedved_VhdAttach\" + appId.Substring(appId.Length - 127 - 20); }
                    NativeMethods.SetCurrentProcessExplicitAppUserModelID(appId);
                }
                else
                {
                    MessageBox.ShowError(null, "This program requires Windows 7 or later.");
                    Environment.Exit(1);
                }

                TaskbarProgress.DoNotThrowNotImplementedException = true;


                bool doAttach = Args.Current.ContainsKey("Attach");
                bool doDetach = Args.Current.ContainsKey("Detach") && (!doAttach);
                bool doDetachDrive = Args.Current.ContainsKey("DetachDrive") && (!doAttach) && (!doDetach);
                bool doChangeLetter = Args.Current.ContainsKey("ChangeLetter") && (!doAttach) && (!doDetach) && (!doDetachDrive);

                bool doAnything = doAttach || doDetach || doDetachDrive || doChangeLetter;

                if (doAnything)
                {

                    string[] argfiles = Args.Current.GetValues("");

                    if (doChangeLetter)
                    {
                        CommandLineAddon cla = new CommandLineAddon();
                        int res = cla.ChangeDriveLetter(argfiles);
                        Environment.Exit(res);
                        return;
                    }

                    var files = new List<FileInfo>();
                    foreach (var iFile in argfiles)
                    {
                        files.Add(new FileInfo(iFile.TrimEnd(new[] { '\"' })));
                    }

                    if (files.Count == 0)
                    {
                        Environment.Exit(1);
                        return;
                    }

                    Form appForm = null;
                    if (doAttach)
                    {
                        appForm = new AttachForm(files, Args.Current.ContainsKey("readonly"), false);
                    }
                    else if (doDetach)
                    {
                        appForm = new DetachForm(files);
                    }
                    else if (doDetachDrive)
                    {
                        appForm = new DetachDriveForm(files);
                    }

                    if (appForm != null)
                    {
                        TaskbarProgress.DefaultOwner = appForm;
                        Application.Run(appForm);
                        Environment.Exit(Environment.ExitCode);
                    }
                    else
                    {
                        Environment.Exit(1);
                    }

                }
                else
                { //open localy

                    Application.Run(new MainForm());

                }
            }
        }



        private static void UnhandledCatch_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
#if !DEBUG
            Medo.Diagnostics.ErrorReport.ShowDialog(null, e.Exception, new Uri("https://medo64.com/feedback/"));
#else
            throw e.Exception;
#endif
        }


        private static class NativeMethods
        {

            [DllImport("Shell32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern UInt32 SetCurrentProcessExplicitAppUserModelID(String AppID);

        }

        private static bool IsRunningOnMono
        {
            get
            {
                return (Type.GetType("Mono.Runtime") != null);
            }
        }

    }
}
