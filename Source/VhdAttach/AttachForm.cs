using System.ComponentModel;
using Medo.Windows.Forms;
using MessageBox = Medo.MessageBox;

namespace VhdAttach
{
    internal partial class AttachForm : Form
    {

        private readonly IList<FileInfo> Files;
        private readonly bool MountReadOnly;
        private readonly bool InitializeDisk;
        private List<Exception> _exceptions;

        private AttachForm()
        {
            InitializeComponent();
            Font = SystemFonts.MessageBoxFont;
        }

        public AttachForm(IList<FileInfo> files, bool mountReadOnly, bool initializeDisk)
            : this()
        {
            Files = files;
            MountReadOnly = mountReadOnly;
            InitializeDisk = initializeDisk;
        }

        public AttachForm(FileInfo file, bool mountReadOnly, bool initializeDisk)
            : this(new[] { file }, mountReadOnly, initializeDisk)
        {
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
            FileInfo iFile = null;
            try
            {
                for (var i = 0; i < Files.Count; ++i)
                {
                    iFile = Files[i];
                    bw.ReportProgress(-1, iFile.Name);

                    Utility.FixServiceErrorsIfNeeded();
                    var res = PipeClient.Attach(iFile.FullName, MountReadOnly, InitializeDisk);
                    if (res.IsError)
                    {
                        _exceptions.Add(new InvalidOperationException(iFile.Name, new Exception(res.Message)));
                    }
                }
            }
            catch (IOException)
            {
                _exceptions.Add(new InvalidOperationException(iFile.Name, new Exception(Messages.ServiceIOException)));
            }
            catch (Exception ex)
            {
                _exceptions.Add(new InvalidOperationException(iFile.Name, ex));
            }
            if (_exceptions.Count > 0) { throw new InvalidOperationException(); }
        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            StatusLabel.Text = "Attaching" + Environment.NewLine + e.UserState;
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
                    MessageBox.ShowError(this, string.Format("Virtual disk file \"{0}\" cannot be attached.\n\n{1}", iException.Message, iException.InnerException.Message));
                }
            }
            Close();
        }

    }
}
