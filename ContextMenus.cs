using MaxwellGPUIdle.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace MaxwellGPUIdle
{
    /// <summary>
    /// </summary>
    internal class ContextMenus
    {
        private static readonly List<EventHandler> menu_events_handlers = new List<EventHandler>()
        {
            Notification_Setting_Click,
            Startup_Click,
            KillOnIdle_Click,
            Set_Power_Plan_Click,
        };

        private static readonly List<string> menu_names = new List<string>()
        {
            "Notifications",
            "Run at Login",
            "Kill on Idle",
            "Auto. Power Plan",
        };

        /// <summary>
        /// Is the About box displayed?
        /// </summary>
        private static bool isAboutLoaded = false;

        // Note: Cannot be static or runtime value won't change
        private List<bool> menu_items_enabled = new List<bool>()
        {
            Settings.Default.ShowNotifications,
            Settings.Default.AutomaticStartup,
            Settings.Default.KillOnIdle,
            Settings.Default.ForceOnDemandPowerPlan,
        };

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
            menu.ShowImageMargin = true;

            for (int i = 0; i < menu_names.Count; i++)
            {
                ToolStripMenuItem watt = new ToolStripMenuItem();
                watt.Text = menu_names[i];
                watt.Click += menu_events_handlers[i];
                // TODO: Still need to avoid rebuilding the menu every time the menu changes
                if (menu_items_enabled[i])
                {
                    watt.Image = Resources.checkmark;
                }

                menu.Items.Add(watt);
            };

            /// ----------------------------------------------------------------------------------
            /// Always present or static menu items
            menu.Items.Add(new ToolStripSeparator()); // Separator.

            // Add one entry to this menu to kill everything
            item = new ToolStripMenuItem
            {
                Text = "Force Idle Now!",
                Image = Resources.Rss
            };
            item.Click += delegate (object sender, EventArgs e) { Kill_Click(sender, e); };
            menu.Items.Add(item);

            item = new ToolStripMenuItem
            {
                Text = "Add Executable",
                Image = Resources.Plus
            };
            item.Click += delegate (object sender, EventArgs e) { AddExecutable(sender, e); };
            menu.Items.Add(item);

            item = new ToolStripMenuItem
            {
                Text = "About",
                Image = Resources.About
            };
            item.Click += delegate (object sender, EventArgs e) { About_Click(sender, e); };
            menu.Items.Add(item);

            item = new ToolStripMenuItem
            {
                Text = "Exit",
                Image = Resources.Exit
            };
            item.Click += delegate (object sender, EventArgs e) { Exit_Click(sender, e); };
            menu.Items.Add(item);
            /// end ----------------------------------------------------------------------------------

            return menu;
        }

        /// <summary>
        /// Handles the Click event of the About control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private static void About_Click(object sender, EventArgs e)
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
        private static void AddExecutable(object sender, EventArgs e)
        {
            new AddFeed().ShowDialog();
        }

        /// <summary>
        /// Handles the Click event of the Exit control.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private static void Exit_Click(object sender, EventArgs e)
        {
            // Quit without further ado.
            MaxwellGPUIdle.ProcessIcon.ni.Visible = false;
            Application.Exit();
        }

        /// <summary>
        /// Handles the Click event of the KillOnIdle control.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private static void KillOnIdle_Click(object sender, EventArgs e)
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
        private static void Notification_Setting_Click(object sender, EventArgs e)
        {
            // Flipping here can cause bugs, be more explicit so that the value is always right.
            Settings.Default.ShowNotifications = !Settings.Default.ShowNotifications;
            Settings.Default.Save();
            MaxwellGPUIdle.ProcessIcon.ni.ContextMenuStrip = new ContextMenus().CreateFeedsMenu();
        }

        /// <summary>
        /// Handles the Click event of the Add Feed control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private static void Set_Power_Plan_Click(object sender, EventArgs e)
        {
            Settings.Default.ForceOnDemandPowerPlan = !Settings.Default.ForceOnDemandPowerPlan;
            Settings.Default.Save();
            MaxwellGPUIdle.ProcessIcon.ni.ContextMenuStrip = new ContextMenus().CreateFeedsMenu();
        }

        /// <summary>
        /// Handles the Click event of the Startup control.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private static void Startup_Click(object sender, EventArgs e)
        {
            Settings.Default.AutomaticStartup = !Settings.Default.AutomaticStartup;
            Settings.Default.Save();
            MaxwellGPUIdle.ProcessIcon.ni.ContextMenuStrip = new ContextMenus().CreateFeedsMenu();
            Integration.AddToStartup();
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
            Program.DoIdleTasks();
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
