using System.ComponentModel;
using System.Reflection;

namespace VhdAttach
{
    internal partial class ServiceWaitForm : Form
    {

        public ServiceWaitForm(string title, Action action)
        {
            InitializeComponent();
            Font = SystemFonts.MessageBoxFont;
            ControlBox = false;

            Text = title;
            bw.RunWorkerAsync(action);
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            var exceptions = new List<Exception>();
            var action = (Action)e.Argument;
            try
            {
                action();
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                {
                    throw ex.InnerException;
                }

                throw;
            }
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                DialogResult = DialogResult.OK;
            }
            else
            {
                Messages.ShowServiceIOException(this, e.Error);
                DialogResult = DialogResult.Cancel;
            }
        }

    }
}
