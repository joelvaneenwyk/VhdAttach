//------------------------------------------------------------------------------
// <copyright file="ServiceInstallerDialog.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.ComponentModel;
using System.Diagnostics;

namespace System.ServiceProcess.Design
{
    /// <summary>Specifies the return value of a <see cref="T:System.ServiceProcess.Design.ServiceInstallerDialog" /> form.</summary>
    public enum ServiceInstallerDialogResult
    {
        /// <summary>The dialog return value is <see langword="OK" />. This value typically indicates that the user confirmed the account properties and pressed the <see langword="OK" /> button to close the dialog.</summary>
        OK,
        /// <summary>Install the service with a system account rather than a user account. This value typically indicates that the dialog was not displayed to the user. For example, the <see cref="P:System.ServiceProcess.ServiceProcessInstaller.Account" /> property is set to something other than <see langword="User" />.</summary>
        UseSystem,
        /// <summary>The dialog return value is <see langword="Canceled" />. This value typically indicates that the user canceled out of the dialog without setting the account fields.</summary>
        Canceled
    }

    /// <summary>Provides a dialog box, which prompts for account information of a Windows Service application.</summary>
    public class ServiceInstallerDialog : Form
    {

        private Button okButton;

        private TextBox passwordEdit;

        private Button cancelButton;

        private TextBox confirmPassword;

        private TextBox usernameEdit;

        private Label label1;

        private Label label2;

        private Label label3;
        private TableLayoutPanel okCancelTableLayoutPanel;
        private TableLayoutPanel overarchingTableLayoutPanel;

        private ServiceInstallerDialogResult result = ServiceInstallerDialogResult.OK;

        /// <summary>Initializes a new instance of the service account form.</summary>
        public ServiceInstallerDialog()
        {
            InitializeComponent();
        }

        /// <summary>Gets or sets the password for the service account form.</summary>
		/// <returns>A string representing the password in the service account form. The default is an empty string ("").</returns>
        public string Password
        {
            get
            {
                return passwordEdit.Text;
            }
            set
            {
                passwordEdit.Text = value;
            }
        }

        /// <summary>Gets the dialog result for the service account form.</summary>
        /// <returns>A <see cref="T:System.ServiceProcess.Design.ServiceInstallerDialogResult" /> indicating the user response to the dialog box. The default is <see langword="OK" />.</returns>
        public ServiceInstallerDialogResult Result
        {
            get
            {
                return result;
            }
        }

        /// <summary>Gets or sets the user name for the service account form.</summary>
		/// <returns>A string representing the user name in the service account form. The default is an empty string ("").</returns>
        public string Username
        {
            get
            {
                return usernameEdit.Text;
            }
            set
            {
                usernameEdit.Text = value;
            }
        }

        /// <summary>Begins running a standard application message loop and displays the service account form.</summary>
        public static void Main()
        {
            Application.Run(new ServiceInstallerDialog());
        }

        private void InitializeComponent()
        {
            ComponentResourceManager resources = new ComponentResourceManager(typeof(ServiceInstallerDialog));
            okButton = new Button();
            passwordEdit = new TextBox();
            cancelButton = new Button();
            confirmPassword = new TextBox();
            usernameEdit = new TextBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            okCancelTableLayoutPanel = new TableLayoutPanel();
            overarchingTableLayoutPanel = new TableLayoutPanel();
            okCancelTableLayoutPanel.SuspendLayout();
            overarchingTableLayoutPanel.SuspendLayout();
            SuspendLayout();
            //
            // okButton
            //
            resources.ApplyResources(okButton, "okButton");
            okButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            okButton.DialogResult = DialogResult.OK;
            okButton.Margin = new Padding(0, 0, 3, 0);
            okButton.MinimumSize = new Size(75, 23);
            okButton.Name = "okButton";
            okButton.Padding = new Padding(10, 0, 10, 0);
            okButton.Click += okButton_Click;
            //
            // passwordEdit
            //
            resources.ApplyResources(passwordEdit, "passwordEdit");
            passwordEdit.Margin = new Padding(3, 3, 0, 3);
            passwordEdit.Name = "passwordEdit";
            //
            // cancelButton
            //
            resources.ApplyResources(cancelButton, "cancelButton");
            cancelButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Margin = new Padding(3, 0, 0, 0);
            cancelButton.MinimumSize = new Size(75, 23);
            cancelButton.Name = "cancelButton";
            cancelButton.Padding = new Padding(10, 0, 10, 0);
            cancelButton.Click += cancelButton_Click;
            //
            // confirmPassword
            //
            resources.ApplyResources(confirmPassword, "confirmPassword");
            confirmPassword.Margin = new Padding(3, 3, 0, 3);
            confirmPassword.Name = "confirmPassword";
            //
            // usernameEdit
            //
            resources.ApplyResources(usernameEdit, "usernameEdit");
            usernameEdit.Margin = new Padding(3, 0, 0, 3);
            usernameEdit.Name = "usernameEdit";
            //
            // label1
            //
            resources.ApplyResources(label1, "label1");
            label1.Margin = new Padding(0, 0, 3, 3);
            label1.Name = "label1";
            //
            // label2
            //
            resources.ApplyResources(label2, "label2");
            label2.Margin = new Padding(0, 3, 3, 3);
            label2.Name = "label2";
            //
            // label3
            //
            resources.ApplyResources(label3, "label3");
            label3.Margin = new Padding(0, 3, 3, 3);
            label3.Name = "label3";
            //
            // okCancelTableLayoutPanel
            //
            resources.ApplyResources(okCancelTableLayoutPanel, "okCancelTableLayoutPanel");
            okCancelTableLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            overarchingTableLayoutPanel.SetColumnSpan(okCancelTableLayoutPanel, 2);
            okCancelTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            okCancelTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            okCancelTableLayoutPanel.Controls.Add(okButton, 0, 0);
            okCancelTableLayoutPanel.Controls.Add(cancelButton, 1, 0);
            okCancelTableLayoutPanel.Margin = new Padding(0, 6, 0, 0);
            okCancelTableLayoutPanel.Name = "okCancelTableLayoutPanel";
            okCancelTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            //
            // overarchingTableLayoutPanel
            //
            resources.ApplyResources(overarchingTableLayoutPanel, "overarchingTableLayoutPanel");
            overarchingTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
            overarchingTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            overarchingTableLayoutPanel.Controls.Add(label1, 0, 0);
            overarchingTableLayoutPanel.Controls.Add(okCancelTableLayoutPanel, 0, 3);
            overarchingTableLayoutPanel.Controls.Add(label2, 0, 1);
            overarchingTableLayoutPanel.Controls.Add(confirmPassword, 1, 2);
            overarchingTableLayoutPanel.Controls.Add(label3, 0, 2);
            overarchingTableLayoutPanel.Controls.Add(passwordEdit, 1, 1);
            overarchingTableLayoutPanel.Controls.Add(usernameEdit, 1, 0);
            overarchingTableLayoutPanel.Name = "overarchingTableLayoutPanel";
            overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
            overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
            overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
            overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
            //
            // ServiceInstallerDialog
            //
            AcceptButton = okButton;
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            AutoScaleDimensions = new SizeF(6, 13);
            CancelButton = cancelButton;
            Controls.Add(overarchingTableLayoutPanel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            HelpButton = true;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ServiceInstallerDialog";
            ShowIcon = false;
            ShowInTaskbar = false;
            HelpButtonClicked += ServiceInstallerDialog_HelpButtonClicked;
            okCancelTableLayoutPanel.ResumeLayout(false);
            okCancelTableLayoutPanel.PerformLayout();
            overarchingTableLayoutPanel.ResumeLayout(false);
            overarchingTableLayoutPanel.PerformLayout();
            ResumeLayout(false);

        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            result = ServiceInstallerDialogResult.Canceled;
            DialogResult = DialogResult.Cancel;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            result = ServiceInstallerDialogResult.OK;
            if (passwordEdit.Text == confirmPassword.Text)
                DialogResult = DialogResult.OK;
            else
            {
                MessageBoxOptions options = 0;
                Control current = this;
                while (current.RightToLeft == RightToLeft.Inherit)
                    current = current.Parent;
                if (current.RightToLeft == RightToLeft.Yes)
                    options = MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign;

                DialogResult = DialogResult.None;
                MessageBox.Show(Res.GetString(Res.Label_MissmatchedPasswords), Res.GetString(Res.Label_SetServiceLogin), MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, options);
                passwordEdit.Text = string.Empty;
                confirmPassword.Text = string.Empty;
                passwordEdit.Focus();
            }
            // Consider, V2, jruiz: check to make sure the password is correct for the given account.                
        }

        private void ServiceInstallerDialog_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            Debug.Fail("Undone: Needs a help topic. VSWhidbey 326855");
            e.Cancel = true;
        }
    }
}
