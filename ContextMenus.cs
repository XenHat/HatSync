// This is an independent project of an individual developer. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

namespace HatSync
{
    public class TrackedToolStripMenuItem
    {
        public System.Windows.Forms.ToolStripMenuItem Value => _templateToolStripMenuItemList.Value;
        private readonly System.Lazy<System.Windows.Forms.ToolStripMenuItem> _templateToolStripMenuItemList = new System.Lazy<System.Windows.Forms.ToolStripMenuItem>(() => _CreateTemplateToolStripMenuItem());

        private static System.Windows.Forms.ToolStripMenuItem _CreateTemplateToolStripMenuItem()
        {
            System.Windows.Forms.ToolStripMenuItem instance = new System.Windows.Forms.ToolStripMenuItem();
            return instance;
        }
    }

    /// <summary>
    /// </summary>
    internal static class MenuGenerator
    {
        public static class ContextMenus
        {
            /// <summary>
            /// Creates this instance.
            /// </summary>
            /// <returns>ContextMenuStrip</returns>
            private static void CreateFeedsMenu()
            {
                // Warning: This leaves a window in which the menu doesn't exist. I'll fix that later
                // when the leak is gone.
                if (Program.STrayIcon.ContextMenuStrip != null)
                {
                    Program.STrayIcon.ContextMenuStrip.Dispose();
                }

                // Add the default menu options.
                Program.STrayIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip
                {
                    ShowImageMargin = true
                };

                // DEBUG
                //TrackedToolStripMenuItem item = new TrackedToolStripMenuItem();
                //ProcessIcon.ni.ContextMenuStrip.Items.Add(item);

                // Regenerate a new list with the current settings.
                System.Collections.Generic.List<bool> menuItemsEnabled = new System.Collections.Generic.List<bool>();

                for (var i = 0; i < MenuNames.Count; i++)
                {
                    System.Windows.Forms.ToolStripMenuItem loopItem = new TrackedToolStripMenuItem().Value;
                    //ToolStripMenuItem watt = new ToolStripMenuItem();
                    loopItem.Text = MenuNames[i];
                    loopItem.Click += MenuEventsHandlers[i];
                    // TODO: Still need to avoid rebuilding the menu every time the menu changes
                    if (menuItemsEnabled[i])
                    {
                        loopItem.Image = Properties.Resources.checkmark;
                    }

                    Program.STrayIcon.ContextMenuStrip.Items.Add(loopItem);
                };

                TrackedToolStripMenuItem item = null;

                // Add IPs there for fun.
                if (CachedValues.CachedExternalIpAddressv6 != null)
                {
                    item = new TrackedToolStripMenuItem();
                    item.Value.Text = CachedValues.CachedExternalIpAddressv6.ToString();
                    //item.Value.Image = HatSync.Properties.Resources.About;
                    //item.Value.Click += delegate (object sender, EventArgs e) { About_Click(sender, e); };
                    item.Value.Enabled = false;
                    Program.STrayIcon.ContextMenuStrip.Items.Add(item.Value);
                }
                if (CachedValues.CachedExternalIpAddressv4 != null)
                {
                    item = new TrackedToolStripMenuItem();
                    item.Value.Text = CachedValues.CachedExternalIpAddressv4.ToString();
                    //item.Value.Image = HatSync.Properties.Resources.About;
                    //item.Value.Click += delegate (object sender, EventArgs e) { About_Click(sender, e); };
                    item.Value.Enabled = false;
                    Program.STrayIcon.ContextMenuStrip.Items.Add(item.Value);
                }

                // WOAH, this creates a memory leak!
                //sTrayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator()); // Separator.
                Program.STrayIcon.ContextMenuStrip.Items.Add(SSeparator);

                // The actual menu begins here

                item = new TrackedToolStripMenuItem();
                item.Value.Text = "[TEST] Hash Desktop folder";
                //item.Value.Image = HatSync.Properties.Resources.Rss;
                item.Value.Click += delegate (object sender, System.EventArgs e) { HashTest1_Click(sender, e); };
                Program.STrayIcon.ContextMenuStrip.Items.Add(item.Value);

                item = new TrackedToolStripMenuItem();
                item.Value.Text = "[TEST] Hash C:\\";
                //item.Value.Image = HatSync.Properties.Resources.Rss;
                item.Value.Click += delegate (object sender, System.EventArgs e) { HashTest2_Click(sender, e); };
                Program.STrayIcon.ContextMenuStrip.Items.Add(item.Value);

                item = new TrackedToolStripMenuItem();
                item.Value.Text = "Force IP Update";
                item.Value.Image = Properties.Resources.Rss;
                item.Value.Click += delegate (object sender, System.EventArgs e) { ForceIPUpdate_Click(sender, e); };
                Program.STrayIcon.ContextMenuStrip.Items.Add(item.Value);

                item = new TrackedToolStripMenuItem();
                item.Value.Text = "About";
                item.Value.Image = Properties.Resources.About;
                item.Value.Click += delegate (object sender, System.EventArgs e) { About_Click(sender, e); };
                Program.STrayIcon.ContextMenuStrip.Items.Add(item.Value);

                item = new TrackedToolStripMenuItem();
                item.Value.Text = "Exit";
                item.Value.Image = Properties.Resources.Exit;
                item.Value.Click += delegate (object sender, System.EventArgs e) { Exit_Click(sender, e); };
                Program.STrayIcon.ContextMenuStrip.Items.Add(item.Value);
                // end ----------------------------------------------------------------------------------
                //if (sTrayIcon.ContextMenuStrip != null)
                //{
                //    sTrayIcon.ContextMenuStrip.Dispose();
                //}
                //item.Dispose();

                //System.GC.Collect();
                //System.GC.WaitForFullGCComplete();
            }

            /// <summary>
            /// Handles the Click event of the Kill Background Processes control.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">
            /// The <see cref="System.EventArgs" /> instance containing the event data.
            /// </param>
            private static void ForceIPUpdate_Click(object sender, System.EventArgs e)
            {
                IpUpdater.CheckAndSendEmail(true);
            }

            public static void RegenerateMenu()
            {
                CreateFeedsMenu();
            }

            private static readonly System.Collections.Generic.List<System.EventHandler> MenuEventsHandlers = new System.Collections.Generic.List<System.EventHandler>();

            private static readonly System.Collections.Generic.List<string> MenuNames = new System.Collections.Generic.List<string>();

            private static readonly System.Windows.Forms.ToolStripSeparator SSeparator = new System.Windows.Forms.ToolStripSeparator();

            /// <summary>
            /// Is the About box displayed?
            /// </summary>
            private static bool _isAboutLoaded;

            /// <summary>
            /// Handles the Click event of the About control.
            /// </summary>
            /// <param name="sender">The source of the event.</param>
            /// <param name="e">
            /// The <see cref="System.EventArgs" /> instance containing the event data.
            /// </param>
            private static void About_Click(object sender, System.EventArgs e)
            {
                if (!_isAboutLoaded)
                {
                    _isAboutLoaded = true;
                    new AboutBox().ShowDialog();
                    _isAboutLoaded = false;
                }
            }

            /// <summary>
            /// Handles the Click event of the Exit control.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">
            /// The <see cref="System.EventArgs" /> instance containing the event data.
            /// </param>
            private static void Exit_Click(object sender, System.EventArgs e)
            {
                // Quit without further ado.
                Program.STrayIcon.Visible = false;
                System.Windows.Forms.Application.Exit();
            }

            private static void HashTest1_Click(object sender, System.EventArgs e)
            {
                SimpleHasher.HashFolder(System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory));
            }
            private static void HashTest2_Click(object sender, System.EventArgs e)
            {
                SimpleHasher.HashFolder(System.IO.Path.GetPathRoot(System.Environment.SystemDirectory));
            }
        }

        // This method handles the Closing event. The ToolStripDropDown control is not allowed to
        // close unless the Done menu item is clicked or the Close method is called explicitly. The
        // Done menu item is enabled only after both of the other menu items have been selected.
        private static void ContextMenuStrip_Closing(
            object sender, System.Windows.Forms.ToolStripDropDownClosingEventArgs e)
        {
            // TODO: Document or remove.
        }
    }

    internal class MyXmlReader : System.Xml.XmlTextReader
    {
        public MyXmlReader(System.IO.Stream s) : base(s)
        {
        }

        // Wed Oct 07 08:00:07 GMT 2009
        public MyXmlReader(string inputUri) : base(inputUri)
        {
        }

        public override void ReadEndElement()
        {
            if (_readingDate)
            {
                _readingDate = false;
            }
            base.ReadEndElement();
        }

        public override void ReadStartElement()
        {
            if (string.Equals(base.NamespaceURI, string.Empty, System.StringComparison.InvariantCultureIgnoreCase) &&
                (string.Equals(base.LocalName, "lastBuildDate", System.StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(base.LocalName, "pubDate", System.StringComparison.InvariantCultureIgnoreCase)))
            {
                _readingDate = true;
            }
            base.ReadStartElement();
        }

        public override string ReadString()
        {
            if (_readingDate)
            {
                var dateString = base.ReadString();
                if (!System.DateTime.TryParse(dateString, out System.DateTime dt))
                {
                    dt = System.DateTime.ParseExact(dateString, CustomUtcDateTimeFormat, System.Globalization.CultureInfo.InvariantCulture);
                }

                return dt.ToUniversalTime().ToString("R", System.Globalization.CultureInfo.InvariantCulture);
            }
            else
            {
                return base.ReadString();
            }
        }

        private const string CustomUtcDateTimeFormat = "ddd MMM dd HH:mm:ss Z yyyy";
        private bool _readingDate;
    }
}
