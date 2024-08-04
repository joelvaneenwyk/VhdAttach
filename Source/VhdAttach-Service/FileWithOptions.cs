using System;
using System.Collections.Generic;
using System.Text;

namespace VhdAttachCommon
{
    public class FileWithOptions
    {

        public FileWithOptions(string fileNameWithOptions)
        {
            if (fileNameWithOptions.StartsWith("/"))
            {
                /*
                 * Each file can have additional settings area that starts with / and ends with next /.
                 * E.g. "/readonly,nodriveletter/D:\Test.vhd"
                 */
                var iEndPipe = fileNameWithOptions.IndexOf("/", 1);
                var additionalSettings = fileNameWithOptions.Substring(1, iEndPipe - 1);
                FileName = fileNameWithOptions.Substring(iEndPipe + 1);
                foreach (var setting in additionalSettings.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    switch (additionalSettings.ToUpperInvariant())
                    {
                        case "READONLY": ReadOnly = true; break;
                        case "NODRIVELETTER": NoDriveLetter = true; break;
                    }
                }
            }
            else
            {
                FileName = fileNameWithOptions;
            }
        }

        public string FileName { get; private set; }
        public bool ReadOnly { get; set; }
        public bool NoDriveLetter { get; set; }

        public override string ToString()
        {
            var options = new List<string>();
            if (ReadOnly) { options.Add("readonly"); }
            if (NoDriveLetter) { options.Add("nodriveletter"); }

            var sb = new StringBuilder();
            if (options.Count >= 1)
            {
                sb.Append("/");
                for (int i = 0; i < options.Count; i++)
                {
                    if (i > 0) { sb.Append(","); }
                    sb.Append(options[i]);
                }
                sb.Append("/");
            }
            sb.Append(FileName);
            return sb.ToString();
        }

    }
}