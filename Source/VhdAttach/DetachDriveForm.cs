using System.ComponentModel;
using Medo.Windows.Forms;
using MessageBox = Medo.MessageBox;

namespace VhdAttach
{
    internal partial class DetachDriveForm : Form
    {

        private IList<FileInfo> _files;
        private List<Exception> _exceptions;


        public DetachDriveForm(IList<FileInfo> file)
        {
            InitializeComponent();
            Font = SystemFonts.MessageBoxFont;

            _files = file;
        }

        private void Form_Load(object sender, EventArgs e)
        {
            bw.RunWorkerAsync();
        }

        private void Form_Shown(object sender, EventArgs e)
        {
            TaskbarProgress.SetState(TaskbarProgressState.Indeterminate);
        }

        private void Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            TaskbarProgress.SetState(TaskbarProgressState.NoProgress);
        }


        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            _exceptions = new List<Exception>();
            FileSystemInfo iDirectory = null;
            try
            {
                for (var i = 0; i < _files.Count; ++i)
                {
                    iDirectory = new DirectoryInfo(_files[i].FullName);
                    bw.ReportProgress(-1, iDirectory.Name);

                    Utility.FixServiceErrorsIfNeeded();
                    var res = PipeClient.DetachDrive(iDirectory.FullName);
                    if (res.IsError)
                    {
                        _exceptions.Add(new InvalidOperationException(iDirectory.Name, new Exception(res.Message)));
                    }
                }
            }
            catch (TimeoutException)
            {
                _exceptions.Add(new InvalidOperationException(iDirectory.Name, new Exception("Cannot access VHD Attach service.")));
            }
            catch (Exception ex)
            {
                _exceptions.Add(new InvalidOperationException(iDirectory.Name, ex));
            }
            if (_exceptions.Count > 0) { throw new InvalidOperationException(); }
        }

        private static int IndexOfAny(string text, int startingIndex, params string[] fragment)
        {
            if ((fragment == null) || (fragment.Length == 0)) { return -1; }
            int minValue = text.IndexOf(fragment[0], startingIndex, StringComparison.InvariantCultureIgnoreCase);
            for (int i = 1; i < fragment.Length; ++i)
            {
                int iCurrMinValue = text.IndexOf(fragment[i], startingIndex);
                if ((iCurrMinValue >= 0) && (iCurrMinValue < minValue))
                {
                    minValue = iCurrMinValue;
                }
            }
            return minValue;
        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState != null)
            {
                StatusLabel.Text = "Detaching drive" + Environment.NewLine + e.UserState;
            }
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (IsDisposed) { return; }

            progress.Value = 100;
            TaskbarProgress.SetPercentage(100);
            if (e.Error == null)
            {
                TaskbarProgress.SetState(TaskbarProgressState.Normal);
            }
            else
            {
                TaskbarProgress.SetState(TaskbarProgressState.Error);
                Environment.ExitCode = 1;
                foreach (var iException in _exceptions)
                {
                    MessageBox.ShowError(this, string.Format("Drive \"{0}\" cannot be detached.\n\n{1}", iException.Message, iException.InnerException.Message));
                }
            }
            Close();
        }

    }
}
