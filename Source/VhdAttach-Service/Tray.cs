using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using Medo.Reflection;

namespace VhdAttachService
{
    internal static class Tray
    {

        private static NotifyIcon Notify;

        internal static void Show()
        {
            Notify = new NotifyIcon();
            // TODO ContextMenu is no longer supported. Use ContextMenuStrip instead. For more details see https://docs.microsoft.com/en-us/dotnet/core/compatibility/winforms#removed-controls
            Notify.ContextMenuStrip = new ContextMenuStrip();
            // TODO MenuItem is no longer supported. Use ToolStripMenuItem instead. For more details see https://docs.microsoft.com/en-us/dotnet/core/compatibility/winforms#removed-controls
            Notify.ContextMenuStrip.Items.Add(new ToolStripMenuItem("Exit", null, Tray_Exit_OnClick));
            Notify.Icon = GetApplicationIcon();
            Notify.Text = CallingAssembly.Title;
            Notify.Visible = true;
        }

        internal static void SetStatusToRunningInteractive()
        {
            Notify.Icon = GetAnnotatedIcon(Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(CallingAssembly.Name + ".Resources.Service_RunningInteractive_12.png")));
            Notify.Text = CallingAssembly.Title + " (PID=" + Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture) + ")";
        }

        internal static void SetStatusToUnknown()
        {
            Notify.Icon = GetAnnotatedIcon(Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(CallingAssembly.Name + ".Resources.Service_Unknown_12.png")));
            Notify.Text = CallingAssembly.Title + " - Unknown state.";
        }

        internal static void SetStatusToRunning()
        {
            Notify.Icon = GetAnnotatedIcon(Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(CallingAssembly.Name + ".Resources.Service_Running_12.png")));
            Notify.Text = CallingAssembly.Title + " - Running.";
        }

        internal static void SetStatusToStopped()
        {
            Notify.Icon = GetAnnotatedIcon(Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(CallingAssembly.Name + ".Resources.Service_Stopped_12.png")));
            Notify.Text = CallingAssembly.Title + " - Stopped.";
        }

        internal static void Hide()
        {
            Notify.Visible = false;
        }


        private static void Tray_Exit_OnClick(object sender, EventArgs e)
        {
            Application.Exit();
        }



        #region Helpers

        private static Icon GetAnnotatedIcon(Image annotation)
        {
            var icon = GetApplicationIcon();

            if (icon != null)
            {
                var image = icon.ToBitmap();
                if (icon != null)
                {
                    using (var g = Graphics.FromImage(image))
                    {
                        g.DrawImage(annotation, (int)g.VisibleClipBounds.Width - annotation.Width - 2, (int)g.VisibleClipBounds.Height - annotation.Height - 2);
                        g.Flush();
                    }
                }
                return Icon.FromHandle(image.GetHicon());
            }
            return null;
        }

        private static Icon GetApplicationIcon()
        {
            IntPtr hLibrary = NativeMethods.LoadLibrary(Assembly.GetEntryAssembly().Location);
            if (!hLibrary.Equals(IntPtr.Zero))
            {
                IntPtr hIcon = NativeMethods.LoadImage(hLibrary, "#32512", NativeMethods.IMAGE_ICON, 20, 20, 0);
                if (!hIcon.Equals(IntPtr.Zero))
                {
                    Icon icon = Icon.FromHandle(hIcon);
                    if (icon != null) { return icon; }
                }
            }
            return null;
        }

        private static class NativeMethods
        {

            public const UInt32 IMAGE_ICON = 1;


            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            static extern internal IntPtr LoadImage(IntPtr hInstance, String lpIconName, UInt32 uType, Int32 cxDesired, Int32 cyDesired, UInt32 fuLoad);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            static extern internal IntPtr LoadLibrary(string lpFileName);

        }

        #endregion

    }
}
