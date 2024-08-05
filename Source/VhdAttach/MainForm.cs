using Medo.Application;
using Medo.Configuration;
using Medo.Diagnostics;
using Medo.Extensions;
using Medo.IO;
using Medo.Localization.Croatia;
using Medo.Reflection;
using Medo.Services;
using Medo.Windows.Forms;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using VhdAttachCommon;
using VirtualHardDiskImage;
using MessageBox = Medo.MessageBox;

namespace VhdAttach
{

    internal partial class MainForm : Form
    {

        private string VhdFileName;
        private RecentFiles Recent;
        private static readonly NumberDeclination CylinderSuffix = new NumberDeclination("cylinder", "cylinders", "cylinders");
        private static readonly NumberDeclination HeadSuffix = new NumberDeclination("head", "heads", "heads");
        private static readonly NumberDeclination SectorSuffix = new NumberDeclination("sector", "sectors", "sectors");
        private static readonly ListViewGroup GroupFileSystem = new ListViewGroup("File system");
        private static readonly ListViewGroup GroupDetails = new ListViewGroup("Details");
        private static readonly ListViewGroup GroupInternals = new ListViewGroup("Internals");
        private static readonly ListViewGroup GroupInternalsDynamic = new ListViewGroup("Internals (dynamic disk)");

        public MainForm()
        {
            InitializeComponent();
            Font = SystemFonts.MessageBoxFont;
            TaskbarProgress.DefaultOwner = this;
            TaskbarProgress.DoNotThrowNotImplementedException = true;

            mnu.Renderer = new Helper.ToolStripBorderlessProfessionalRenderer();
            Helper.UpdateToolstripImages(null, mnu);

            using (var g = CreateGraphics())
            {
                var scale = (Settings.ScaleFactor > 1) ? Settings.ScaleFactor : Math.Max(g.DpiX, g.DpiY) / 96.0;
                var newScale = ((int)Math.Floor(scale * 100) / 50 * 50) / 100.0;
                if (newScale > 1)
                {
                    var newWidth = (int)(mnu.ImageScalingSize.Width * newScale);
                    var newHeight = (int)(mnu.ImageScalingSize.Height * newScale);
                    mnu.ImageScalingSize = new Size(newWidth, newHeight);
                    mnu.AutoSize = false; //because sometime it is needed
                }
            }

            Recent = new RecentFiles();
        }


        private bool SuppressMenuKey;

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (((keyData & Keys.Alt) == Keys.Alt) && (keyData != (Keys.Alt | Keys.Menu))) { SuppressMenuKey = true; }

            switch (keyData)
            {

                case Keys.F10:
                    ToggleMenu();
                    return true;

                case Keys.Control | Keys.N:
                case Keys.Alt | Keys.N:
                    mnuNew.PerformClick();
                    return true;

                case Keys.Control | Keys.O:
                    mnuOpen.PerformButtonClick();
                    return true;

                case Keys.Alt | Keys.O:
                    mnuOpen.ShowDropDown();
                    return true;

                case Keys.F6:
                    mnuAttach.PerformButtonClick();
                    return true;

                case Keys.Alt | Keys.A:
                    if (mnuAttach.Enabled)
                    {
                        mnuAttach.ShowDropDown();
                    }
                    return true;

                case Keys.Alt | Keys.D:
                    mnuDetach.PerformClick();
                    return true;

                case Keys.Alt | Keys.M:
                    if (mnuAutomount.Enabled) { mnuAutomount.ShowDropDown(); }
                    return true;

                case Keys.Alt | Keys.L:
                    if (mnuDrive.Enabled) { mnuDrive.ShowDropDown(); }
                    return true;

                case Keys.F1:
                    mnuApp.ShowDropDown();
                    mnuAppAbout.Select();
                    return true;


                case Keys.F5:
                    mnuRefresh.PerformClick();
                    return true;


                case Keys.Control | Keys.C:
                    mnxListCopy.PerformClick();
                    return true;

                case Keys.Control | Keys.A:
                    mnxListSelectAll.PerformClick();
                    return true;

            }

            return base.ProcessDialogKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyData == Keys.Menu)
            {
                if (SuppressMenuKey) { SuppressMenuKey = false; return; }
                ToggleMenu();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else
            {
                base.OnKeyDown(e);
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyData == Keys.Menu)
            {
                if (SuppressMenuKey) { SuppressMenuKey = false; return; }
                ToggleMenu();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else
            {
                base.OnKeyUp(e);
            }
        }


        private void Form_Load(object sender, EventArgs e)
        {
            State.Load(this, list);
            OpenFromCommandLineArgs();
            UpdateRecent();

            CheckErrors();

            Form_Resize(null, null);
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            State.Save(this, list);
            Recent.Save();
            bwCheckForUpgrade.CancelAsync();
        }

        private void Form_Resize(object sender, EventArgs e)
        {
            using (var listGraphics = list.CreateGraphics())
            {
                var x = listGraphics.MeasureString("XxWwAaZz ÈèQqXxWw", list.Font).ToSize();
                list.Columns[0].Width = x.Width;
            }
            list.Columns[1].Width = list.ClientSize.Width - list.Columns[0].Width - SystemInformation.VerticalScrollBarWidth;
        }

        private void Form_Shown(object sender, EventArgs e)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version; //don't auto-check for development builds
            if ((version.Major != 0) || (version.Minor != 0)) { bwCheckForUpgrade.RunWorkerAsync(); }
        }


        private void UpdateData(string vhdFileName)
        {
            if (vhdFileName == null)
            {
                list.Items.Clear();
                list.Groups.Clear();
                mnuRefresh.Enabled = false;
                mnuAttach.Enabled = false;
                mnuDetach.Enabled = false;
                mnuAutomount.Enabled = false;
                mnuDrive.Enabled = false;
                mnuAutomount_DropDownOpening(null, null);
                mnuDrive_DropDownOpening(null, null);
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                mnuRefresh.Enabled = true;

                using (var document = new VirtualDisk(vhdFileName))
                {
                    var items = new List<ListViewItem>();
                    var fileInfo = new FileInfo(document.FileName);
                    items.Add(new ListViewItem(new[] { "File path", fileInfo.Directory.FullName }) { Group = GroupFileSystem });
                    items.Add(new ListViewItem(new[] { "File name", fileInfo.Name }) { Group = GroupFileSystem });

                    try
                    {
                        var fi = new FileInfo(document.FileName);
                        items.Add(new ListViewItem(new[] { "File size", string.Format(CultureInfo.CurrentCulture, "{0} ({1:#,##0} bytes)", BinaryPrefixExtensions.ToBinaryPrefixString(fi.Length, "B", "0"), fi.Length) }) { Group = GroupFileSystem });
                    }
                    catch { }

                    try
                    {
                        var di = new DriveInfo(new FileInfo(document.FileName).Directory.Root.FullName);
                        items.Add(new ListViewItem(new[] { "Free space on " + di.Name, string.Format(CultureInfo.CurrentCulture, "{0} ({1:#,##0} bytes)", BinaryPrefixExtensions.ToBinaryPrefixString(di.AvailableFreeSpace, "B", "0"), di.AvailableFreeSpace) }) { Group = GroupFileSystem });
                    }
                    catch { }


                    mnuAttach.Enabled = false;
                    mnuDetach.Enabled = false;
                    mnuAutomount.Enabled = false;
                    mnuDrive.Enabled = false;

                    try
                    {
                        document.Open(VirtualDiskAccessMask.GetInfo | VirtualDiskAccessMask.Detach); //Workaround: The VirtualDiskAccessMask parameter must include the VIRTUAL_DISK_ACCESS_DETACH (0x00040000) flag.

                        string attachedDevice = null;
                        string[] attachedPaths = null;
                        try
                        {
                            attachedDevice = document.GetAttachedPath();
                            attachedPaths = PathFromDevice.GetPath(attachedDevice);
                        }
                        catch { }
                        if (attachedDevice != null)
                        {
                            items.Add(new ListViewItem(new[] { "Attached device", attachedDevice }) { Group = GroupFileSystem });
                        }
                        if (attachedPaths != null)
                        {
                            for (int i = 0; i < attachedPaths.Length; i++)
                            {
                                items.Add(new ListViewItem(new[] { ((i == 0) ? "Attached path" : ""), attachedPaths[i] }) { Group = GroupFileSystem });
                            }
                        }

                        try
                        {
                            long virtualSize;
                            long physicalSize;
                            int blockSize;
                            int sectorSize;
                            document.GetSize(out virtualSize, out physicalSize, out blockSize, out sectorSize);
                            items.Add(new ListViewItem(new[] { "Virtual size", string.Format(CultureInfo.CurrentCulture, "{0} ({1:#,##0} bytes)", BinaryPrefixExtensions.ToBinaryPrefixString(virtualSize, "B", "0"), virtualSize) }) { Group = GroupDetails });
                            if (fileInfo.Length != physicalSize)
                            {
                                items.Add(new ListViewItem(new[] { "Physical size", string.Format(CultureInfo.CurrentCulture, "{0} ({1:#,##0} bytes)", BinaryPrefixExtensions.ToBinaryPrefixString(physicalSize, "B", "0"), physicalSize) }) { Group = GroupDetails });
                            }
                            if (blockSize != 0)
                            {
                                items.Add(new ListViewItem(new[] { "Block size", string.Format(CultureInfo.CurrentCulture, "{0} ({1:#,##0} bytes)", BinaryPrefixExtensions.ToBinaryPrefixString(blockSize, "B", "0"), blockSize) }) { Group = GroupDetails });
                            }
                            items.Add(new ListViewItem(new[] { "Sector size", string.Format(CultureInfo.CurrentCulture, "{0} ({1} bytes)", BinaryPrefixExtensions.ToBinaryPrefixString(sectorSize, "B", "0"), sectorSize) }) { Group = GroupDetails });
                        }
                        catch { }

                        try
                        {
                            items.Add(new ListViewItem(new[] { "Identifier", document.GetIdentifier().ToString() }) { Group = GroupDetails });
                        }
                        catch { }

                        try
                        {
                            int deviceId;
                            Guid vendorId;
                            document.GetVirtualStorageType(out deviceId, out vendorId);
                            string deviceText = string.Format(CultureInfo.InvariantCulture, "Unknown ({0})", deviceId);
                            switch (deviceId)
                            {
                                case 1: deviceText = "ISO"; break;
                                case 2: deviceText = "VHD"; break;
                                case 3: deviceText = "VHDX"; break;
                            }
                            string vendorText = string.Format(CultureInfo.InvariantCulture, "Unknown ({0})", vendorId);
                            if (vendorId.Equals(new Guid("EC984AEC-A0F9-47e9-901F-71415A66345B"))) { vendorText = "Microsoft"; }
                            items.Add(new ListViewItem(new[] { "Device ID", deviceText }) { Group = GroupDetails });
                            items.Add(new ListViewItem(new[] { "Vendor ID", vendorText }) { Group = GroupDetails });
                        }
                        catch { }

                        try
                        {
                            items.Add(new ListViewItem(new[] { "Provider subtype", string.Format(CultureInfo.CurrentCulture, "{0} (0x{0:x8})", document.GetProviderSubtype()) }) { Group = GroupDetails });
                        }
                        catch { }


                        if (document.DiskType == VirtualDiskType.Vhd)
                        {
                            try
                            {
                                var footerCopyBytes = new byte[512];
                                var headerBytes = new byte[1024];
                                var footerBytes = new byte[512];
                                using (var vhdFile = new FileStream(vhdFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                {
                                    vhdFile.Read(footerCopyBytes, 0, 512);
                                    vhdFile.Read(headerBytes, 0, 1024);
                                    vhdFile.Position = vhdFile.Length - 512;
                                    vhdFile.Read(footerBytes, 0, 512);
                                }

                                var footer = new HardDiskFooter(footerBytes);

                                if (footer.Cookie != "conectix")
                                {
                                    items.Add(new ListViewItem(new[] { "Cookie", footer.Cookie }) { Group = GroupInternals });
                                }

                                items.Add(new ListViewItem(new[] { "Creation time stamp", string.Format(CultureInfo.CurrentCulture, "{0}", footer.TimeStamp.ToLocalTime()) }) { Group = GroupInternals });

                                var creatorApplicationText = string.Format(CultureInfo.InvariantCulture, "Unknown (0x{0:x4})", (int)footer.CreatorApplication);
                                switch (footer.CreatorApplication)
                                {
                                    case VhdCreatorApplication.JosipMedvedVhdAttach: creatorApplicationText = "Josip Medved's VHD Attach"; break;
                                    case VhdCreatorApplication.MicrosoftSysinternalsDisk2Vhd: creatorApplicationText = "Microsoft Sysinternals Disk2vhd"; break;
                                    case VhdCreatorApplication.MicrosoftVirtualPC: creatorApplicationText = "Microsoft Virtual PC"; break;
                                    case VhdCreatorApplication.MicrosoftVirtualServer: creatorApplicationText = "Microsoft Virtual Server"; break;
                                    case VhdCreatorApplication.MicrosoftWindows: creatorApplicationText = "Microsoft Windows"; break;
                                    case VhdCreatorApplication.OracleVirtualBox: creatorApplicationText = "Oracle VirtualBox"; break;
                                }
                                items.Add(new ListViewItem(new[] { "Creator application", string.Format(CultureInfo.InvariantCulture, "{0} {1}.{2}", creatorApplicationText, footer.CreatorVersion.Major, footer.CreatorVersion.Minor) }) { Group = GroupInternals });

                                var creatorHostOsText = string.Format(CultureInfo.InvariantCulture, "Unknown (0x{0:x4})", (int)footer.CreatorHostOs);
                                switch (footer.CreatorHostOs)
                                {
                                    case VhdCreatorHostOs.Windows: creatorHostOsText = "Windows"; break;
                                    case VhdCreatorHostOs.Macintosh: creatorHostOsText = "Macintosh"; break;
                                }
                                items.Add(new ListViewItem(new[] { "Creator host OS", creatorHostOsText }) { Group = GroupInternals });

                                items.Add(new ListViewItem(new[] { "Disk geometry", string.Format(CultureInfo.CurrentCulture, "{0}, {1}, {2}", CylinderSuffix.GetText(footer.DiskGeometryCylinders), HeadSuffix.GetText(footer.DiskGeometryHeads), SectorSuffix.GetText(footer.DiskGeometrySectors)) }) { Group = GroupInternals });

                                var diskTypeText = string.Format(CultureInfo.CurrentCulture, "Unknown ({0}: 0x{0:x4})", (int)footer.DiskType);
                                switch ((int)footer.DiskType)
                                {
                                    case 0: diskTypeText = "None"; break;
                                    case 1: diskTypeText = "Reserved (deprecated: 0x0001)"; break;
                                    case 2: diskTypeText = "Fixed hard disk"; break;
                                    case 3: diskTypeText = "Dynamic hard disk"; break;
                                    case 4: diskTypeText = "Differencing hard disk"; break;
                                    case 5: diskTypeText = "Reserved (deprecated: 0x0005)"; break;
                                    case 6: diskTypeText = "Reserved (deprecated: 0x0006)"; break;
                                }
                                items.Add(new ListViewItem(new[] { "Disk type", diskTypeText }) { Group = GroupInternals });

                                if ((footer.DiskType == VhdDiskType.DynamicHardDisk) || (footer.DiskType == VhdDiskType.DifferencingHardDisk))
                                {

                                    var header = new DynamicDiskHeader(headerBytes);

                                    if (header.Cookie != "cxsparse")
                                    {
                                        items.Add(new ListViewItem(new[] { "Cookie", header.Cookie }) { Group = GroupInternalsDynamic });
                                    }

                                    if (header.DataOffset != ulong.MaxValue)
                                    {
                                        items.Add(new ListViewItem(new[] { "Data offset", header.DataOffset.ToString("#,##0") }) { Group = GroupInternalsDynamic });
                                    }

                                    items.Add(new ListViewItem(new[] { "Max table entries", header.MaxTableEntries.ToString("#,##0") }) { Group = GroupInternalsDynamic });

                                    items.Add(new ListViewItem(new[] { "Block size", string.Format(CultureInfo.CurrentCulture, "{0} ({1:#,##0} bytes)", BinaryPrefixExtensions.ToBinaryPrefixString(header.BlockSize, "B", "0"), header.BlockSize) }) { Group = GroupInternalsDynamic });

                                }

                            }
                            catch { }
                        }

                        mnuAttach.Enabled = string.IsNullOrEmpty(attachedDevice);
                        mnuDetach.Enabled = !mnuAttach.Enabled;
                        mnuAutomount.Enabled = true;
                        mnuDrive.Enabled = true;
                        mnuAutomount_DropDownOpening(null, null);
                        mnuDrive_DropDownOpening(null, null);

                    }
                    catch (InvalidDataException ex)
                    {
                        MessageBox.ShowError(this, "Cannot open virtual disk drive.\n\n" + ex.Message);
                    }


                    list.BeginUpdate();
                    list.Items.Clear();
                    list.Groups.Clear();
                    list.Groups.Add(GroupFileSystem);
                    list.Groups.Add(GroupDetails);
                    list.Groups.Add(GroupInternals);
                    list.Groups.Add(GroupInternalsDynamic);
                    foreach (var iItem in items)
                    {
                        list.Items.Add(iItem);
                    }
                    list.EndUpdate();
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }


            Text = GetFileTitle(vhdFileName) + " - " + EntryAssembly.Title;
        }

        private void UpdateRecent()
        {
            mnuOpen.DropDownItems.Clear();
            var paths = new List<string>();
            foreach (var iRecentFile in Recent.Items)
            {
                var item2 = new ToolStripMenuItem(iRecentFile.Title);
                item2.Tag = iRecentFile;
                item2.Click += recentItem_Click;
                mnuOpen.DropDownItems.Add(item2);
                paths.Add(iRecentFile.FileName.ToUpperInvariant());
            }
            foreach (var fwo in ServiceSettings.AutoAttachVhdList)
            {
                RecentFile iRecentFile;
                iRecentFile = RecentFile.GetRecentFile(fwo.FileName);
                if (iRecentFile != null)
                {
                    if (paths.Contains(iRecentFile.FileName.ToUpperInvariant()) == false)
                    {
                        var item2 = new ToolStripMenuItem(iRecentFile.Title);
                        item2.Tag = iRecentFile;
                        item2.Click += recentItem_Click;
                        mnuOpen.DropDownItems.Add(item2);
                    }
                }
            }
        }

        void recentItem_Click(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;
            var recentItem = (RecentFile)item.Tag;
            OpenFile(recentItem.FileName, removeRecent: true);
        }


        #region Menu

        private void mnuFile_Click(object sender, EventArgs e)
        {
            using (var frm = new NewDiskForm())
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    AllowSetForegroundWindowToExplorer();
                    var fileName = frm.FileName;
                    try
                    {
                        var newDocument = new VirtualDisk(fileName);
                        UpdateData(newDocument.FileName);
                        VhdFileName = newDocument.FileName;
                        Recent.Push(fileName);
                        UpdateRecent();
                        using (var form = new AttachForm(new FileInfo(fileName), false, true))
                        {
                            form.StartPosition = FormStartPosition.CenterParent;
                            form.ShowDialog(this);
                        }
                    }
                    catch (Exception ex)
                    {
                        var exFile = new FileInfo(fileName);
                        MessageBox.ShowError(this, string.Format("Cannot open \"{0}\".\n\n{1}", exFile.Name, ex.Message));
                    }
                }
            }
        }

        private void mnuOpen_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.AddExtension = true;
                dialog.CheckFileExists = true;
                dialog.CheckPathExists = true;
                dialog.DefaultExt = "vhd";
                if ((Environment.OSVersion.Version.Major * 1000000 + Environment.OSVersion.Version.Minor) >= 6000002)
                { //show if equal to or higher than Windows 8
                    dialog.Filter = "Supported files (*.vhd; *.vhdx; *.iso)|*.vhd;*.vhdx;*.iso|Virtual disk files (*.vhd; *.vhdx)|*.vhd;*.vhdx|ISO image files (*.iso)|*.iso|All files (*.*)|*.*";
                }
                else
                {
                    dialog.Filter = "Virtual disk files (*.vhd)|*.vhd|All files (*.*)|*.*";
                }
                dialog.Multiselect = false;
                dialog.ShowReadOnly = false;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    OpenFile(dialog.FileName);
                }
            }
        }

        private void OpenFile(string fileName, bool removeRecent = false)
        {
            var file = new FileInfo(fileName);
            try
            {
                if (!file.Exists) { throw new IOException("File not found."); }

                var isIntegrityStream = ReFS.HasIntegrityStream(file);
                if (isIntegrityStream && MessageBox.ShowWarning(this, string.Format("Integrity stream is enabled for \"{0}\".\n\nVirtual disk does not support ReFS integrity streams.\n\nDo you wish to remove integrity stream?", file.Name), MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    using (var frm = new RemoveIntegrityStreamForm(file))
                    {
                        frm.ShowDialog(this);
                    }
                }


                var newDocument = new VirtualDisk(fileName);
                VhdFileName = newDocument.FileName;
                Recent.Push(fileName);
                UpdateRecent();
                UpdateData(newDocument.FileName);
                VhdFileName = newDocument.FileName;
            }
            catch (Exception ex)
            {
                if (removeRecent)
                {
                    if (MessageBox.ShowError(this, string.Format("Cannot open \"{0}\".\n\n{1}\n\nDo you wish to remove it from list?", file.Name, ex.Message), MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        Recent.Remove(fileName);
                        UpdateRecent();
                    }
                }
                else
                {
                    MessageBox.ShowError(this, string.Format("Cannot open \"{0}\".\n\n{1}", file.Name, ex.Message));
                }
            }
        }


        private void mnuRefresh_Click(object sender, EventArgs e)
        {
            UpdateData(VhdFileName);
            CheckErrors();
        }

        private void mnuAttach_ButtonClick(object sender, EventArgs e)
        {
            if (VhdFileName == null) { return; }

            if (Settings.UseService)
            {
                using (var form = new AttachForm(new FileInfo(VhdFileName), false, false))
                {
                    form.StartPosition = FormStartPosition.CenterParent;
                    form.ShowDialog(this);
                }
                UpdateData(VhdFileName);
            }
            else
            {
                mnu.Enabled = false;
                var exe = Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName, "VhdAttachExecutor.exe");
                var startInfo = Utility.GetProcessStartInfo(exe, @"/Attach """ + VhdFileName + @"""");
                Cursor = Cursors.WaitCursor;
                bwExecutor.RunWorkerAsync(startInfo);
            }
            AllowSetForegroundWindowToExplorer();
        }

        private void mnuAttachReadOnly_Click(object sender, EventArgs e)
        {
            if (VhdFileName == null) { return; }

            if (Settings.UseService)
            {
                using (var form = new AttachForm(new FileInfo(VhdFileName), true, false))
                {
                    form.StartPosition = FormStartPosition.CenterParent;
                    form.ShowDialog(this);
                }
                UpdateData(VhdFileName);
            }
            else
            {
                mnu.Enabled = false;
                var exe = Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName, "VhdAttachExecutor.exe");
                var startInfo = Utility.GetProcessStartInfo(exe, @"/Attach """ + VhdFileName + @""" /ReadOnly");
                Cursor = Cursors.WaitCursor;
                bwExecutor.RunWorkerAsync(startInfo);
            }
            AllowSetForegroundWindowToExplorer();
        }

        private void mnuDetach_Click(object sender, EventArgs e)
        {
            if (VhdFileName == null) { return; }

            if (Settings.UseService)
            {

                using (var form = new DetachForm(new[] { new FileInfo(VhdFileName) }))
                {
                    form.StartPosition = FormStartPosition.CenterParent;
                    form.ShowDialog(this);
                }
                UpdateData(VhdFileName);

            }
            else
            {

                mnu.Enabled = false;

                var exe = Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName, "VhdAttachExecutor.exe");
                var startInfo = Utility.GetProcessStartInfo(exe, @"/Detach """ + VhdFileName + @"""");

                Cursor = Cursors.WaitCursor;
                bwExecutor.RunWorkerAsync(startInfo);

            }
        }


        private void mnuAutomount_DropDownOpening(object sender, EventArgs e)
        {
            bool isAutoMountNormal = false;
            bool isAutoMountReadonly = false;
            foreach (var fwo in ServiceSettings.AutoAttachVhdList)
            {
                if (string.Compare(VhdFileName, fwo.FileName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    isAutoMountNormal = !fwo.ReadOnly;
                    isAutoMountReadonly = fwo.ReadOnly;
                    break;
                }
            }

            mnuAutomountNormal.Enabled = !isAutoMountNormal;
            mnuAutomountReadonly.Enabled = !isAutoMountReadonly;
            mnuAutomountDisable.Enabled = isAutoMountNormal | isAutoMountReadonly;

            if (VhdFileName == null)
            {
                mnuAutomount.Tag = null;
                mnuAutomount.Text = "Auto-mount";
                mnuAutomount.Image = mnuAutomountDisable.Image;
            }
            else if (isAutoMountNormal)
            {
                mnuAutomount.Tag = true;
                mnuAutomount.Text = "Auto-mounted";
                mnuAutomount.Image = mnuAutomountNormal.Image;
            }
            else if (isAutoMountReadonly)
            {
                mnuAutomount.Tag = true;
                mnuAutomount.Text = "Auto-mounted";
                mnuAutomount.Image = mnuAutomountReadonly.Image;
            }
            else
            {
                mnuAutomount.Tag = false;
                mnuAutomount.Text = "Not auto-mounted";
                mnuAutomount.Image = mnuAutomountDisable.Image;
            }
        }

        private void mnuAutomount_ButtonClick(object sender, EventArgs e)
        {
            var senderObject = ((ToolStripDropDownItem)sender).Tag;
            if (senderObject != null)
            {
                var isMounted = (bool)senderObject;
                if (isMounted)
                {
                    mnuAutomountDisable_Click(null, null);
                }
                else
                {
                    mnuAutomountNormal_Click(null, null);
                }
            }
        }

        private void mnuAutomountNormal_Click(object sender, EventArgs e)
        {
            var list = new FileWithOptionsCollection(ServiceSettings.AutoAttachVhdList);
            list.Remove(VhdFileName);
            list.Add(new FileWithOptions(VhdFileName));
            SaveAutomountSettings(list);
            mnuAutomount_DropDownOpening(null, null);
        }

        private void mnuAutomountReadonly_Click(object sender, EventArgs e)
        {
            var list = new FileWithOptionsCollection(ServiceSettings.AutoAttachVhdList);
            list.Remove(VhdFileName);
            list.Add(new FileWithOptions(VhdFileName) { ReadOnly = true });
            SaveAutomountSettings(list);
            mnuAutomount_DropDownOpening(null, null);
        }

        private void mnuAutomountDisable_Click(object sender, EventArgs e)
        {
            var list = new FileWithOptionsCollection(ServiceSettings.AutoAttachVhdList);
            list.Remove(VhdFileName);
            SaveAutomountSettings(list);
            mnuAutomount_DropDownOpening(null, null);
        }

        private void SaveAutomountSettings(IEnumerable<FileWithOptions> files)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                var vhds = new List<string>();
                foreach (FileWithOptions file in files)
                {
                    vhds.Add(file.ToString());
                }
                var resAA = PipeClient.WriteAutoAttachSettings(vhds.ToArray());
                if (resAA.IsError)
                {
                    MessageBox.ShowError(this, resAA.Message);
                }
            }
            catch (IOException ex)
            {
                Messages.ShowServiceIOException(this, ex);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }


        private void mnuDrive_DropDownOpening(object sender, EventArgs e)
        {
            string newMenuTitle = null;

            string attachedDevice = null;
            try
            {
                using (var document = new VirtualDisk(VhdFileName))
                {
                    document.Open(VirtualDiskAccessMask.GetInfo);
                    attachedDevice = document.GetAttachedPath();
                }
            }
            catch { }

            mnuDrive.Tag = null;
            mnuDrive.Text = "Drive";
            mnuDrive.DropDownItems.Clear();

            if (attachedDevice != null)
            {
                var volumes = new List<Volume>(Volume.GetVolumesOnPhysicalDrive(attachedDevice));
                var availableVolumes = new List<Volume>();
                foreach (var volume in volumes)
                {
                    if (volume.DriveLetter2 != null)
                    {
                        availableVolumes.Add(volume);
                    }
                }

                if (availableVolumes.Count > 0)
                {
                    foreach (var volume in availableVolumes)
                    {
                        mnuDrive.DropDownItems.Add(new ToolStripMenuItem("Open " + volume.DriveLetter3, null, mnuDriveOpen_Click) { Tag = volume });
                        if (newMenuTitle == null)
                        {
                            mnuDrive.Tag = volume;
                            newMenuTitle = volume.DriveLetter3;
                        }
                    }
                }

                if (volumes.Count > 0)
                {
                    if (mnuDrive.DropDownItems.Count > 0)
                    {
                        mnuDrive.DropDownItems.Add(new ToolStripSeparator());
                    }
                    foreach (var volume in volumes)
                    {
                        if (volume.DriveLetter2 != null)
                        {
                            mnuDrive.DropDownItems.Add(new ToolStripMenuItem("Change drive letter " + volume.DriveLetter2, null, mnuDriveLetter_Click) { Tag = volume });
                        }
                        else
                        {
                            if (mnuDrive.Tag == null) { mnuDrive.Tag = volume; }
                            mnuDrive.DropDownItems.Add(new ToolStripMenuItem("Add drive letter", null, mnuDriveLetter_Click) { Tag = volume });
                        }
                    }
                }
            }

            if (newMenuTitle == null) { newMenuTitle = "Drive"; }
            mnuDrive.Text = newMenuTitle;

            mnuDrive.Enabled = (mnuDrive.DropDownItems.Count > 0);
        }

        private void mnuDrive_ButtonClick(object sender, EventArgs e)
        {
            var volume = ((ToolStripDropDownItem)sender).Tag as Volume;
            if (volume != null)
            {
                if (volume.DriveLetter2 != null)
                {
                    mnuDriveOpen_Click(sender, e);
                }
                else
                {
                    mnuDriveLetter_Click(sender, e);
                }
            }
        }

        private void mnuDriveOpen_Click(object sender, EventArgs e)
        {
            var volume = ((ToolStripDropDownItem)sender).Tag as Volume;
            if (volume != null)
            {
                var drive = new DriveInfo(volume.DriveLetter3);
                if (drive.IsReady)
                {
                    Process.Start(volume.DriveLetter3);
                }
                else
                {
                    Process.Start("explorer.exe", "/select," + volume.DriveLetter3);
                }
            }
        }

        private void mnuDriveLetter_Click(object sender, EventArgs e)
        {
            var volume = ((ToolStripDropDownItem)sender).Tag as Volume;
            using (var frm = new ChangeDriveLetterForm(volume))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    UpdateData(VhdFileName);
                }
            }
        }


        private void mnuAppOptions_Click(object sender, EventArgs e)
        {
            using (var form = new SettingsForm())
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    staErrorStolenExtension.Visible = !ServiceSettings.ContextMenuVhd;
                    UpdateData(VhdFileName);
                }
            }
        }


        private void mnuAppFeedback_Click(object sender, EventArgs e)
        {
            mnuHelpReportABug_Click(null, null);
        }

        private void mnuAppUpgrade_Click(object sender, EventArgs e)
        {
            Upgrade.ShowDialog(this, new Uri("https://medo64.com/upgrade/"));
        }

        private void mnuAppAbout_Click(object sender, EventArgs e)
        {
            mnuHelpAbout_Click(null, null);
        }

        private void mnuHelpReportABug_Click(object sender, EventArgs e)
        {
            ErrorReport.ShowDialog(this, null, new Uri("https://medo64.com/feedback/"));
        }

        private void mnuHelpAbout_Click(object sender, EventArgs e)
        {
            AboutBox.ShowDialog(this, new Uri("https://www.medo64.com/vhdattach/"));
        }

        #endregion


        #region ContextMenu: List

        private void mnxList_Opening(object sender, CancelEventArgs e)
        {
            mnxListCopy.Enabled = (list.SelectedItems.Count > 0);
            mnxListSelectAll.Enabled = (list.Items.Count > 0);
        }


        private void mnxListCopy_Click(object sender, EventArgs e)
        {
            if (list.SelectedItems.Count > 0)
            {
                var maxLength = 0;
                foreach (ListViewItem item in list.Items)
                {
                    if (maxLength < item.Text.Length) { maxLength = item.Text.Length; }
                }

                var sb = new StringBuilder();
                foreach (ListViewItem item in list.SelectedItems)
                {
                    var key = item.Text;
                    if (key.Length < maxLength)
                    {
                        if (key.Length > 0)
                        {
                            key += " ";
                            if (key.Length < maxLength)
                            {
                                key += new string('.', maxLength - key.Length);
                            }
                        }
                    }
                    var value = item.SubItems[1].Text;
                    if (key.Length > 0)
                    {
                        sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}: {1}", key, value));
                    }
                    else
                    {
                        sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}  {1}", new string(' ', maxLength), value));
                    }
                }
                Clipboard.SetText(sb.ToString());
            }
        }

        private void mnxListSelectAll_Click(object sender, EventArgs e)
        {
            foreach (var iItem in list.Items)
            {
                ((ListViewItem)iItem).Selected = true;
            }
        }

        #endregion


        private void OpenFromCommandLineArgs()
        { //goes through all files until it can open one and redirects all other files to new instances with /OpenOrExit argument. That argument ensures that each new instance will close in case of error.
            var filesToOpen = Args.Current.GetValues(null);
            FileInfo iFile;
            for (int i = 0; i < filesToOpen.Length; ++i)
            {
                iFile = new FileInfo(filesToOpen[i]);
                try
                {
                    var newDocument = new VirtualDisk(iFile.FullName);
                    UpdateData(newDocument.FileName);
                    VhdFileName = newDocument.FileName;
                    Recent.Push(iFile.FullName);

                    //send all other files to second instances
                    for (int j = i + 1; j < filesToOpen.Length; ++j)
                    {
                        var jFile = new FileInfo(filesToOpen[j]);
                        Process.Start(Utility.GetProcessStartInfo(Assembly.GetExecutingAssembly().Location, @"/ OpenOrExit """ + jFile.FullName + @""""));
                        Thread.Sleep(100 / Environment.ProcessorCount);
                    }
                    break; //i

                }
                catch (Exception ex)
                {
                    MessageBox.ShowError(this, string.Format("Cannot open \"{0}\".\n\n{1}", iFile.Name, ex.Message));
                    if (Args.Current.ContainsKey("OpenOrExit"))
                    {
                        Environment.Exit(2);
                    }
                }
            }
        }

        private string GetFileTitle(string vhdFileName)
        {
            string fileNamePart;
            if (vhdFileName == null)
            {
                fileNamePart = "Untitled";
            }
            else
            {
                var fi = new FileInfo(vhdFileName);
                fileNamePart = fi.Name;
            }
            return fileNamePart;
        }

        private void bwExecutor_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var startInfo = (ProcessStartInfo)e.Argument;
                startInfo.Arguments += string.Format(CultureInfo.InvariantCulture, " /ParentWindow=\"{0},{1},{2},{3}\"", Left, Top, Width, Height);
                using (var process = new Process())
                {
                    process.StartInfo = startInfo;
                    process.Start();
                    process.WaitForExit();
                }
                Thread.Sleep(250);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message, ex);
            }
        }

        private void bwExecutor_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.ShowError(this, string.Format("Error during execution of external process.\n\n{0}", e.Error.Message));
            }
            mnu.Enabled = true;
            UpdateData(VhdFileName);
            Cursor = Cursors.Default;
        }


        private static void AllowSetForegroundWindowToExplorer()
        {
            var pathToExplorer = Path.Combine(Environment.GetEnvironmentVariable("SystemRoot"), "explorer.exe");
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    var mainModule = process.MainModule;
                    if (mainModule != null)
                    {
                        var fileName = mainModule.FileName;
                        if ((fileName != null) && (fileName.Equals(pathToExplorer, StringComparison.OrdinalIgnoreCase)))
                        {
                            NativeMethods.AllowSetForegroundWindow(process.Id);
                        }
                    }
                }
                catch (Exception) { }
            }
        }


        private void tmrUpdateMenu_Tick(object sender, EventArgs e)
        {
#if DEBUG
            var sw = Stopwatch.StartNew();
#endif
            if (VhdFileName == null)
            {
                mnuRefresh.Enabled = false;
                mnuAttach.Enabled = false;
                mnuDetach.Enabled = false;
                mnuAutomount.Enabled = false;
                mnuDrive.Enabled = false;
                return;
            }

            try
            {
                mnuRefresh.Enabled = true;

                if (!File.Exists(VhdFileName)) { return; }
                using (var document = new VirtualDisk(VhdFileName))
                {
                    document.Open(VirtualDiskAccessMask.GetInfo | VirtualDiskAccessMask.Detach); //Workaround: The VirtualDiskAccessMask parameter must include the VIRTUAL_DISK_ACCESS_DETACH (0x00040000) flag.
                    string attachedDevice = null;
                    try
                    {
                        attachedDevice = document.GetAttachedPath();
                    }
                    catch { }

                    mnuAttach.Enabled = string.IsNullOrEmpty(attachedDevice);
                    mnuDetach.Enabled = !mnuAttach.Enabled;
                    mnuAutomount.Enabled = true;
                    mnuDrive.Enabled = true;
                    if (!mnuAutomount.DropDownButtonPressed) { mnuAutomount_DropDownOpening(null, null); }
                    if (!mnuDrive.DropDownButtonPressed) { mnuDrive_DropDownOpening(null, null); }
                }
            }
            catch { }
#if DEBUG
            sw.Stop();
            Debug.WriteLine("VhdAttach: Menu update in " + sw.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture) + " milliseconds.");
#endif
        }


        private static class NativeMethods
        {

            [DllImportAttribute("user32.dll", EntryPoint = "AllowSetForegroundWindow")]
            [return: MarshalAsAttribute(UnmanagedType.Bool)]
            internal static extern bool AllowSetForegroundWindow(int dwProcessId);

        }


        private void list_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                e.Effect = (files.Length == 1) ? DragDropEffects.Move : DragDropEffects.None;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void list_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length == 1)
                {
                    try
                    {
                        var newDocument = new VirtualDisk(files[0]);
                        UpdateData(newDocument.FileName);
                        VhdFileName = newDocument.FileName;
                        Recent.Push(files[0]);
                        UpdateRecent();
                    }
                    catch (Exception ex)
                    {
                        var exFile = new FileInfo(files[0]);
                        MessageBox.ShowError(this, string.Format("Cannot open \"{0}\".\n\n{1}", exFile.Name, ex.Message));
                    }
                }
            }
        }

        private void staStolenExtensionText_Click(object sender, EventArgs e)
        {
            mnuAppOptions_Click(null, null);
        }

        private void staErrorServiceMissingText_Click(object sender, EventArgs e)
        {
            try
            {
                Utility.ForceInstallService();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.ShowWarning(this, string.Format("Cannot install service.\n\n{0}", ex.Message));
            }
            CheckErrors();
        }

        private void staErrorServiceNotRunningText_Click(object sender, EventArgs e)
        {
            try
            {
                Utility.ForceStartService();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.ShowWarning(this, string.Format("Cannot install service.\n\n{0}", ex.Message));
            }
            CheckErrors();
        }


        private void CheckErrors()
        {
            staErrorStolenExtension.Visible = false;
            staErrorServiceMissing.Visible = false;
            staErrorServiceNotRunning.Visible = false;

            using (var service = new ServiceController("VhdAttach"))
            {
                try
                {
                    if (service.Status != ServiceControllerStatus.Running)
                    {
                        staErrorServiceNotRunning.Visible = true;
                    }
                }
                catch (InvalidOperationException)
                {
                    staErrorServiceMissing.Visible = true;
                }
            }

            if (ServiceSettings.ContextMenuVhd == false)
            {
                staErrorStolenExtension.Visible = true;
            }
        }

        private void ToggleMenu()
        {
            if (mnu.ContainsFocus)
            {
                list.Select();
            }
            else
            {
                mnu.Select();
                mnu.Items[0].Select();
            }
        }


        private void bwCheckForUpgrade_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Cancel = true;

            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < 3000)
            { //wait for three seconds
                Thread.Sleep(100);
                if (bwCheckForUpgrade.CancellationPending) { return; }
            }

            var file = Upgrade.GetUpgradeFile(new Uri("https://medo64.com/upgrade/"));
            if (file != null)
            {
                if (bwCheckForUpgrade.CancellationPending) { return; }
                e.Cancel = false;
            }
        }

        private void bwCheckForUpgrade_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled && (e.Error == null))
            {
                Helper.UpdateToolstripImage(mnuApp, "mnuAppUpgrade");
                mnuAppUpgrade.Text = "Upgrade is available";
            }
        }

    }
}
