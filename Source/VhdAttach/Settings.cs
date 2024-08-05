using System.Globalization;

namespace VhdAttach
{

    internal static class Settings
    {

        public static bool ShowMenu
        {
            get => Medo.Configuration.Settings.Read("ShowMenu", false);
            set => Medo.Configuration.Settings.Write("ShowMenu", value);
        }

        public static bool UseService
        {
            get => Medo.Configuration.Settings.Read("UseService", true);
            set => Medo.Configuration.Settings.Write("UseService", value);
        }


        public static long LastSize
        {
            get
            {
                if (long.TryParse(Medo.Configuration.Settings.Read("LastSize", "104857600"), NumberStyles.Integer, CultureInfo.InvariantCulture, out long size))
                {
                    return size;
                }

                return 0;
            }
            set => Medo.Configuration.Settings.Write("LastSize", value.ToString(CultureInfo.InvariantCulture));
        }

        public static string LastSizeUnit
        {
            get => (Medo.Configuration.Settings.Read("LastSizeUnit", "GB").Equals("MB", StringComparison.OrdinalIgnoreCase)) ? "MB" : "GB";
            set => Medo.Configuration.Settings.Write("LastSizeUnit", value);
        }

        public static bool LastSizeThousandBased
        {
            get => Medo.Configuration.Settings.Read("LastSizeThousandBased", false);
            set => Medo.Configuration.Settings.Write("LastSizeThousandBased", value);
        }

        public static bool LastSizeFixed
        {
            get => Medo.Configuration.Settings.Read("LastSizeFixed", false);
            set => Medo.Configuration.Settings.Write("LastSizeFixed", value);
        }

        public static bool LastSizeVhdX
        {
            get => Medo.Configuration.Settings.Read("LastSizeVhdX", false);
            set => Medo.Configuration.Settings.Write("LastSizeVhdX", value);
        }

        public static int WriteBufferSize => 1024 * 1024;


        public static double ScaleFactor
        {
            get => Medo.Configuration.Settings.Read("ScaleFactor", 0.0);
            set => Medo.Configuration.Settings.Write("ScaleFactor", 0.0);
        }

    }

}
