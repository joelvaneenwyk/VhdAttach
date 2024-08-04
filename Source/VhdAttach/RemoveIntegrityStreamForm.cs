using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Medo.Windows.Forms;
using MessageBox = Medo.MessageBox;

namespace VhdAttach
{
    internal partial class RemoveIntegrityStreamForm : Form
    {
        public RemoveIntegrityStreamForm(FileInfo file)
        {
            InitializeComponent();
            Font = SystemFonts.MessageBoxFont;

            File = file;
        }

        private readonly FileInfo File;


        private void Form_Load(object sender, EventArgs e)
        {
            TaskbarProgress.SetState(TaskbarProgressState.Indeterminate);
            bwAction.RunWorkerAsync();
        }

        private void Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            TaskbarProgress.SetState(TaskbarProgressState.NoProgress);
        }


        private void bwAction_DoWork(object sender, DoWorkEventArgs e)
        {
            ReFS.RemoveIntegrityStream(File);
        }

        private void bwAction_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.ShowError(this, "Cannot remove integrity stream.\n\n" + e.Error.Message);
            }
            Close();
        }

    }
}
