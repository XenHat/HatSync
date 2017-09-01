using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel.Syndication;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using MaxwellGPUIdle.Properties;

namespace MaxwellGPUIdle
{
    /// <summary>
    /// </summary>
    internal class ContextMenus
    {
        /// <summary>
        /// Is the About box displayed?
        /// </summary>
        private bool isAboutLoaded = false;

        //private string titles = "";

        /// <summary>
        /// Creates this instance.
        /// </summary>
        /// <returns>ContextMenuStrip</returns>
        public ContextMenuStrip CreateFeedsMenu()
        {
            // Add the default menu options.
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.ShowImageMargin = false;
            ToolStripMenuItem item;

            // Add one entry to this menu to kill everything
            item = new ToolStripMenuItem
            {
                Text = "Force Idle Now!",
            };
            item.Click += delegate (object sender, EventArgs e) { Kill_Click(sender, e); };
            menu.Items.Add(item); // Add menu entry with the feed name
            menu.Items.Add(new ToolStripSeparator()); // Separator.
            string temporaryRssFile = System.IO.Path.GetTempFileName();

            foreach (string process_name in Settings.Default.KnownGPUProcesses)
            {
                item = new ToolStripMenuItem
                {
                    Text = process_name,
                };
                item.Click += delegate (object sender, EventArgs e) { FeedEntry_Click(sender, e, process_name); };
                menu.Items.Add(item); // Add menu entry with the feed name
            }

            return menu;
        }

        public ContextMenuStrip CreateLoadingMenu()
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.ShowImageMargin = false;
            ToolStripMenuItem item;
            // Add a feed.
            item = new ToolStripMenuItem()
            {
                Text = "Loading..."
            };
            item.Enabled = false;
            menu.Items.Add(item);
            return menu;
        }

        public ContextMenuStrip CreateOptionsMenu()
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.ShowImageMargin = true;
            // TODO: Static
            List<string> menu_names = new List<string>()
            {
                "Notifications",
                "Run at Login",
                "Kill on Idle",
                "Add Executable",
                "About",
                "Quit"
            };
            List<System.Drawing.Image> menu_resources = new List<System.Drawing.Image>()
            {
                Resources.checkmark,
                Resources.checkmark,
                Resources.checkmark,
                Resources.Rss,
                Resources.About,
                Resources.Exit
            };
            List<EventHandler> menu_events_handlers = new List<EventHandler>()
            {
                Notification_Setting_Click,
                Startup_Click,
                KillOnIdle_Click,
                AddExecutable,
                About_Click,
                Exit_Click
            };
            List<bool> menu_items_enabled = new List<bool>()
            {
                Settings.Default.ShowNotifications,
                Settings.Default.AutomaticStartup,
                Settings.Default.KillOnIdle,
                true,
                true,
                true
            };

            for (int i = 0; i < menu_names.Count; i++)
            {
                ToolStripMenuItem item = new ToolStripMenuItem();
                item.Text = menu_names[i];
                item.Click += menu_events_handlers[i];
                if (menu_items_enabled[i])
                {
                    item.Image = menu_resources[i];
                }
                menu.Items.Add(item);
            };

            //System.GC.Collect(3, System.GCCollectionMode.Forced);
            //System.GC.WaitForFullGCComplete();

            return menu;
        }

        /// <summary>
        /// Handles the Click event of the About control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void About_Click(object sender, EventArgs e)
        {
            if (!isAboutLoaded)
            {
                isAboutLoaded = true;
                new AboutBox().ShowDialog();
                isAboutLoaded = false;
            }
        }

        /// <summary>
        /// Handles the Click event of the Add Feed control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void AddExecutable(object sender, EventArgs e)
        {
            new AddFeed().ShowDialog();
        }

        /// <summary>
        /// Handles the Click event of the Exit control.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void Exit_Click(object sender, EventArgs e)
        {
            // Quit without further ado.
            MaxwellGPUIdle.ProcessIcon.ni.Visible = false;
            Application.Exit();
        }

        /// <summary>
        /// Handles the Click event of the Explorer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void Explorer_Click(object sender, EventArgs e)
        {
            Process.Start("explorer", null);
        }

        private void FeedEntry_Click(object sender, EventArgs e, string u)
        {
            try
            {
                ProcessDestroyer.KillProcessByName(u);
            }
            catch (Exception ex)
            {
                Program.ExceptionHandler(ex);
            }
        }

        /// <summary>
        /// Handles the Click event of the Kill Background Processes control.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void Kill_Click(object sender, EventArgs e)
        {
            MaxwellGPUIdle.ProcessDestroyer.KillCompilerProcesses();
        }

        /// <summary>
        /// Handles the Click event of the KillOnIdle control.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void KillOnIdle_Click(object sender, EventArgs e)
        {
            // TODO: Shouldn't we use the event data?
            Settings.Default.KillOnIdle = !Settings.Default.KillOnIdle;
            Settings.Default.Save();
            MaxwellGPUIdle.ProcessIcon.ni.ContextMenuStrip = new ContextMenus().CreateFeedsMenu();
        }

        /// <summary>
        /// Handles the Click event of the Notification Setting control.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void Notification_Setting_Click(object sender, EventArgs e)
        {
            // Flipping here can cause bugs, be more explicit so that the value is always right.
            if (Settings.Default.ShowNotifications)
            {
                Settings.Default.ShowNotifications = false;
            }
            else
            {
                Settings.Default.ShowNotifications = true;
            }
            Settings.Default.Save();
            MaxwellGPUIdle.ProcessIcon.ni.ContextMenuStrip = new ContextMenus().CreateFeedsMenu();
        }

        /// <summary>
        /// Handles the Click event of the Startup control.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void Startup_Click(object sender, EventArgs e)
        {
            bool startUp = !Settings.Default.AutomaticStartup;
            Integration.AddToStartup(startUp);
            Settings.Default.AutomaticStartup = startUp;
            Settings.Default.Save();
            MaxwellGPUIdle.ProcessIcon.ni.ContextMenuStrip = new ContextMenus().CreateFeedsMenu();
        }

        private class MyXmlReader : XmlTextReader
        {
            private const string CustomUtcDateTimeFormat = "ddd MMM dd HH:mm:ss Z yyyy";
            private bool readingDate = false;
            // Wed Oct 07 08:00:07 GMT 2009

            public MyXmlReader(Stream s) : base(s)
            {
            }

            public MyXmlReader(string inputUri) : base(inputUri)
            {
            }

            public override void ReadEndElement()
            {
                if (readingDate)
                {
                    readingDate = false;
                }
                base.ReadEndElement();
            }

            public override void ReadStartElement()
            {
                if (string.Equals(base.NamespaceURI, string.Empty, StringComparison.InvariantCultureIgnoreCase) &&
                    (string.Equals(base.LocalName, "lastBuildDate", StringComparison.InvariantCultureIgnoreCase) ||
                    string.Equals(base.LocalName, "pubDate", StringComparison.InvariantCultureIgnoreCase)))
                {
                    readingDate = true;
                }
                base.ReadStartElement();
            }

            public override string ReadString()
            {
                if (readingDate)
                {
                    string dateString = base.ReadString();
                    DateTime dt;
                    if (!DateTime.TryParse(dateString, out dt))
                        dt = DateTime.ParseExact(dateString, CustomUtcDateTimeFormat, System.Globalization.CultureInfo.InvariantCulture);
                    return dt.ToUniversalTime().ToString("R", System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    return base.ReadString();
                }
            }
        }
    }
}
