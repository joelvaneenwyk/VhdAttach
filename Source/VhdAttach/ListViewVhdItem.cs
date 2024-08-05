using VhdAttachCommon;

namespace VhdAttach
{
    internal class ListViewVhdItem : ListViewItem
    {

        public ListViewVhdItem(FileWithOptions fwo)
        {
            FileName = fwo.FileName;
            IsReadOnly = fwo.ReadOnly;
            HasNoDriveLetter = fwo.NoDriveLetter;

            try
            {
                var file = new FileInfo(FileName);
                Text = file.Name;

                try
                {
                    if (file.Exists)
                    {
                        if (file.IsReadOnly)
                        {
                            ToolTipText = "File is read-only." + Environment.NewLine + FileName;
                            ImageIndex = 1;
                        }
                        else
                        {
                            ToolTipText = FileName;
                            ImageIndex = (IsReadOnly) ? 4 : 0;
                        }
                    }
                    else
                    {
                        ToolTipText = "File not found." + Environment.NewLine + FileName;
                        ImageIndex = 2;
                    }
                }
                catch (Exception ex)
                {
                    ToolTipText = ex.Message + Environment.NewLine + FileName;
                    ImageIndex = 3;
                }
            }
            catch (ArgumentException)
            {
                var segments = FileName.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
                if (segments.Length > 0)
                {
                    Text = segments[segments.Length - 1];
                }
                else
                {
                    Text = FileName;
                }
                ToolTipText = "Cannot parse file name \"" + FileName + "\"";
                ImageIndex = 3;
            }

        }


        public string FileName { get; private set; }

        private bool _isReadOnly;
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set
            {
                _isReadOnly = value;
                if ((ImageIndex == 0) || (ImageIndex == 4))
                {
                    ImageIndex = (value) ? 4 : 0;
                }
            }
        }

        public bool HasNoDriveLetter { get; private set; }


        public string GetSettingFileName()
        {
            var fwo = new FileWithOptions(FileName);
            fwo.ReadOnly = IsReadOnly;
            fwo.NoDriveLetter = HasNoDriveLetter;
            return fwo.ToString();
        }


        public override string ToString()
        {
            return GetSettingFileName();
        }
    }
}
