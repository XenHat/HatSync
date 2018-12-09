namespace HatSync
{
    // Codacy throws an error here, but this class is actually partial. Removing this won't compile.
    internal sealed partial class AboutBox : System.Windows.Forms.Form
    {
        public AboutBox()
        {
            InitializeComponent();
            Text = string.Format("About {0}", AssemblyTitle);
            labelProductName.Text = AssemblyProduct;
            labelVersion.Text = string.Format("Version {0}", AssemblyVersion);
            labelCopyright.Text = AssemblyCopyright;
            labelCompanyName.Text = AssemblyCompany;
            textBoxDescription.Text = AssemblyDescription;
        }

        #region Assembly Attribute Accessors

        private static string AssemblyCompany
        {
            get
            {
                object[] attributes = System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(System.Reflection.AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((System.Reflection.AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }

        private static string AssemblyCopyright
        {
            get
            {
                object[] attributes = System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(System.Reflection.AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((System.Reflection.AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        private static string AssemblyDescription
        {
            get
            {
                object[] attributes = System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(System.Reflection.AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((System.Reflection.AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        private static string AssemblyProduct
        {
            get
            {
                object[] attributes = System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(System.Reflection.AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((System.Reflection.AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        private static string AssemblyTitle
        {
            get
            {
                object[] attributes = System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(System.Reflection.AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    System.Reflection.AssemblyTitleAttribute titleAttribute = (System.Reflection.AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        private static string AssemblyVersion => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

        #endregion Assembly Attribute Accessors
    }
}
