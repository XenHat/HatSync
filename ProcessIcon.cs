using MaxwellGPUIdle.Properties;
using System;
using System.Windows.Forms;

namespace MaxwellGPUIdle
{
    /// <summary>
    /// </summary>
    internal class ProcessIcon : IDisposable
    {
        /// <summary>
        /// Static instance of the Tray Icon
        /// </summary>
        public static NotifyIcon ni;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessIcon" /> class.
        /// </summary>
        public ProcessIcon()
        {
            // TODO: Never regenerate a new process icon, simply re-assign a new menu to it
            // Instantiate the NotifyIcon object.
            if (ni != null)
            {
                Dispose();
            }
            ni = new NotifyIcon();
        }

        /// <summary>
        /// Displays the icon in the system tray.
        /// </summary>
        public void Display()
        {
            // Put the icon in the system tray
            ni.Icon = Resources.MaxwellGPUIdle;
            ni.Text = "MaxwellGPUIdle";
            ni.Visible = true;

            // Attach a context menu.
            ContextMenus.RegenerateMenu();
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public void Dispose()
        {
            // When the application closes, this will remove the icon from the system tray immediately.
            ni.Dispose();
        }
    }
}
