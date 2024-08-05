//Josip Medved <jmedved@jmedved.com> http://www.jmedved.com

//2007-12-31: New version.
//2008-05-22: Added Description.
//2008-06-06: Added Copyright.


using System.Globalization;
using System.Reflection;

namespace Medo.Reflection
{

    /// <summary>
    /// Returns various info about assembly that started process.
    /// </summary>
    public static class EntryAssembly
    {

        private readonly static Assembly Assembly = Assembly.GetEntryAssembly();
        //private readonly static AssemblyName AssemblyName = Assembly.GetName();

        /// <summary>
        /// Gets entry assembly's full name.
        /// </summary>
        public static string FullName
        {
            get { return Assembly.GetEntryAssembly().GetName().FullName; }
        }

        /// <summary>
        /// Gets entry assembly's application name.
        /// </summary>
        public static string Name
        {
            get { return Assembly.GetEntryAssembly().GetName().Name; }
        }

        /// <summary>
        /// Gets entry assembly's version.
        /// </summary>
        public static Version Version
        {
            get { return Assembly.GetEntryAssembly().GetName().Version; }
        }

        /// <summary>
        /// Returns entry assembly's version in x.xx format.
        /// </summary>
        public static string ShortVersionString
        {
            get
            {
                Version version = Version;
                return version.Major.ToString("0", CultureInfo.InvariantCulture) + "." + version.Minor.ToString("00", CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Returns entry assembly's version in x.xx.xxx.xxxx format.
        /// </summary>
        public static string LongVersionString
        {
            get
            {
                Version version = Version;
                return version.Major.ToString("0", CultureInfo.CurrentCulture) + "." + version.Minor.ToString("00", CultureInfo.CurrentCulture) + "." + version.Build.ToString("000", CultureInfo.CurrentCulture) + "." + version.Revision.ToString("0000", CultureInfo.CurrentCulture);
            }
        }

        /// <summary>
        /// Returns entry assembly's company or null if not found.
        /// </summary>
        public static string Company
        {
            get
            {
                object[] companyAttributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), true);
                if ((companyAttributes != null) && (companyAttributes.Length >= 1))
                {
                    return ((AssemblyCompanyAttribute)companyAttributes[companyAttributes.Length - 1]).Company;
                }
                return null;
            }
        }

        /// <summary>
        /// Returns entry assembly's title or name if title is not found.
        /// </summary>
        public static string Title
        {
            get
            {
                object[] titleAttributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), true);
                if ((titleAttributes != null) && (titleAttributes.Length >= 1))
                {
                    return ((AssemblyTitleAttribute)titleAttributes[titleAttributes.Length - 1]).Title;
                }

                return Name;
            }
        }

        /// <summary>
        /// Retuns entry's assembly product. If product is not found, title is returned.
        /// </summary>
        public static string Product
        {
            get
            {
                object[] productAttributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), true);
                if ((productAttributes != null) && (productAttributes.Length >= 1))
                {
                    return ((AssemblyProductAttribute)productAttributes[productAttributes.Length - 1]).Product;
                }

                return Title;
            }
        }

        /// <summary>
        /// Retuns entry's assembly description. If description is not found, empty string is returned.
        /// </summary>
        public static string Description
        {
            get
            {
                object[] descriptionAttributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), true);
                if ((descriptionAttributes != null) && (descriptionAttributes.Length >= 1))
                {
                    return ((AssemblyDescriptionAttribute)descriptionAttributes[descriptionAttributes.Length - 1]).Description;
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Retuns entry's assembly copyright. If copyright is not found, empty string is returned.
        /// </summary>
        public static string Copyright
        {
            get
            {
                object[] copyrightAttributes = Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), true);
                if ((copyrightAttributes != null) && (copyrightAttributes.Length >= 1))
                {
                    return ((AssemblyCopyrightAttribute)copyrightAttributes[copyrightAttributes.Length - 1]).Copyright;
                }

                return string.Empty;
            }
        }

    }

}
