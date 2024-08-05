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
            get => passwordEdit.Text;
            set => passwordEdit.Text = value;
        }

        /// <summary>Gets the dialog result for the service account form.</summary>
        /// <returns>A <see cref="T:System.ServiceProcess.Design.ServiceInstallerDialogResult" /> indicating the user response to the dialog box. The default is <see langword="OK" />.</returns>
        public ServiceInstallerDialogResult Result => result;

        /// <summary>Gets or sets the user name for the service account form.</summary>
		/// <returns>A string representing the user name in the service account form. The default is an empty string ("").</returns>
        public string Username
        {
            get => usernameEdit.Text;
            set => usernameEdit.Text = value;
        }

        /// <summary>Begins running a standard application message loop and displays the service account form.</summary>
        public static void Main()
        {
            Application.Run(new ServiceInstallerDialog());
        }

        private void InitializeComponent()
        {
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
            okButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            okButton.DialogResult = DialogResult.OK;
            okButton.Location = new Point(0, 0);
            okButton.Margin = new Padding(0, 0, 5, 0);
            okButton.MinimumSize = new Size(125, 44);
            okButton.Name = "okButton";
            okButton.Padding = new Padding(17, 0, 17, 0);
            okButton.Size = new Size(125, 44);
            okButton.TabIndex = 0;
            okButton.Click += okButton_Click;
            // 
            // passwordEdit
            // 
            passwordEdit.Location = new Point(177, 56);
            passwordEdit.Margin = new Padding(5, 6, 0, 6);
            passwordEdit.Name = "passwordEdit";
            passwordEdit.Size = new Size(154, 31);
            passwordEdit.TabIndex = 5;
            // 
            // cancelButton
            // 
            cancelButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Location = new Point(171, 0);
            cancelButton.Margin = new Padding(5, 0, 0, 0);
            cancelButton.MinimumSize = new Size(125, 44);
            cancelButton.Name = "cancelButton";
            cancelButton.Padding = new Padding(17, 0, 17, 0);
            cancelButton.Size = new Size(125, 44);
            cancelButton.TabIndex = 1;
            cancelButton.Click += cancelButton_Click;
            // 
            // confirmPassword
            // 
            confirmPassword.Location = new Point(177, 112);
            confirmPassword.Margin = new Padding(5, 6, 0, 6);
            confirmPassword.Name = "confirmPassword";
            confirmPassword.Size = new Size(154, 31);
            confirmPassword.TabIndex = 3;
            // 
            // usernameEdit
            // 
            usernameEdit.Location = new Point(177, 0);
            usernameEdit.Margin = new Padding(5, 0, 0, 6);
            usernameEdit.Name = "usernameEdit";
            usernameEdit.Size = new Size(154, 31);
            usernameEdit.TabIndex = 6;
            // 
            // label1
            // 
            label1.Location = new Point(0, 0);
            label1.Margin = new Padding(0, 0, 5, 6);
            label1.Name = "label1";
            label1.Size = new Size(167, 44);
            label1.TabIndex = 0;
            // 
            // label2
            // 
            label2.Location = new Point(0, 56);
            label2.Margin = new Padding(0, 6, 5, 6);
            label2.Name = "label2";
            label2.Size = new Size(167, 44);
            label2.TabIndex = 2;
            // 
            // label3
            // 
            label3.Location = new Point(0, 112);
            label3.Margin = new Padding(0, 6, 5, 6);
            label3.Name = "label3";
            label3.Size = new Size(167, 44);
            label3.TabIndex = 4;
            // 
            // okCancelTableLayoutPanel
            // 
            okCancelTableLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            overarchingTableLayoutPanel.SetColumnSpan(okCancelTableLayoutPanel, 2);
            okCancelTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            okCancelTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            okCancelTableLayoutPanel.Controls.Add(okButton, 0, 0);
            okCancelTableLayoutPanel.Controls.Add(cancelButton, 1, 0);
            okCancelTableLayoutPanel.Location = new Point(0, 174);
            okCancelTableLayoutPanel.Margin = new Padding(0, 12, 0, 0);
            okCancelTableLayoutPanel.Name = "okCancelTableLayoutPanel";
            okCancelTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            okCancelTableLayoutPanel.Size = new Size(333, 192);
            okCancelTableLayoutPanel.TabIndex = 1;
            // 
            // overarchingTableLayoutPanel
            // 
            overarchingTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
            overarchingTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            overarchingTableLayoutPanel.Controls.Add(label1, 0, 0);
            overarchingTableLayoutPanel.Controls.Add(okCancelTableLayoutPanel, 0, 3);
            overarchingTableLayoutPanel.Controls.Add(label2, 0, 1);
            overarchingTableLayoutPanel.Controls.Add(confirmPassword, 1, 2);
            overarchingTableLayoutPanel.Controls.Add(label3, 0, 2);
            overarchingTableLayoutPanel.Controls.Add(passwordEdit, 1, 1);
            overarchingTableLayoutPanel.Controls.Add(usernameEdit, 1, 0);
            overarchingTableLayoutPanel.Location = new Point(14, 15);
            overarchingTableLayoutPanel.Margin = new Padding(5, 6, 5, 6);
            overarchingTableLayoutPanel.Name = "overarchingTableLayoutPanel";
            overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
            overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
            overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
            overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
            overarchingTableLayoutPanel.Size = new Size(435, 439);
            overarchingTableLayoutPanel.TabIndex = 0;
            // 
            // ServiceInstallerDialog
            // 
            AcceptButton = okButton;
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = cancelButton;
            ClientSize = new Size(463, 469);
            Controls.Add(overarchingTableLayoutPanel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            HelpButton = true;
            Margin = new Padding(5, 6, 5, 6);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ServiceInstallerDialog";
            ShowIcon = false;
            ShowInTaskbar = false;
            HelpButtonClicked += ServiceInstallerDialog_HelpButtonClicked;
            okCancelTableLayoutPanel.ResumeLayout(false);
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
