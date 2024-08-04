using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Medo.Windows.Forms;
using MessageBox = Medo.MessageBox;

namespace VhdAttach
{
    internal partial class DetachForm : Form
    {

        private IList<FileInfo> _files;
        private List<Exception> _exceptions;


        public DetachForm(IList<FileInfo> file)
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
            FileInfo iFile = null;
            try
            {
                for (var i = 0; i < _files.Count; ++i)
                {
                    iFile = _files[i];
                    bw.ReportProgress(-1, iFile.Name);

                    Utility.FixServiceErrorsIfNeeded();
                    var res = PipeClient.Detach(iFile.FullName);
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
            StatusLabel.Text = "Detaching" + Environment.NewLine + e.UserState;
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
                    MessageBox.ShowError(this, string.Format("Virtual disk file \"{0}\" cannot be detached.\n\n{1}", iException.Message, iException.InnerException.Message));
                }
            }
            Close();
        }

    }
}
