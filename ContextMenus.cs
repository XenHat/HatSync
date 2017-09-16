using MaxwellGPUIdle.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace MaxwellGPUIdle
{
    public class TrackedToolStripMenuItem
    {
        private Lazy<ToolStripMenuItem> _TemplateToolStripMenuItemList = new Lazy<ToolStripMenuItem>(() => _CreateTemplateToolStripMenuItem());

        public ToolStripMenuItem Value
        {
            get
            {
                return _TemplateToolStripMenuItemList.Value;
            }
        }

        private static ToolStripMenuItem _CreateTemplateToolStripMenuItem()
        {
            ToolStripMenuItem instance = new ToolStripMenuItem();
            return instance;
        }
    }

    /// <summary>
    /// </summary>
    internal class MenuGenerator
    {
        // This method handles the Closing event. The ToolStripDropDown control is not allowed to
        // close unless the Done menu item is clicked or the Close method is called explicitly. The
        // Done menu item is enabled only after both of the other menu items have been selected.
        private void ContextMenuStrip_Closing(
            object sender,
            ToolStripDropDownClosingEventArgs e)
        {
        }

        public static class ContextMenus
        {
            private static readonly List<EventHandler> menu_events_handlers = new List<EventHandler>()
        {
            Notification_Setting_Click,
            Startup_Click,
            KillOnIdle_Click,
            KillDropbox_Click,
            Set_Power_Plan_Click,
        };

            private static readonly List<string> menu_names = new List<string>()
        {
            "Notifications",
            "Run at Login",
            "Kill on Idle",
            "Kill Dropbox",
            "Auto. Power Plan",
        };

            /// <summary>
            /// Is the About box displayed?
            /// </summary>
            private static bool isAboutLoaded = false;

            private static ToolStripSeparator sSeparator = new ToolStripSeparator();

            /// <summary>
            /// Creates this instance.
            /// </summary>
            /// <returns>ContextMenuStrip</returns>
            public static void CreateFeedsMenu()
            {
                // Warning: This leaves a window in which the menu doesn't exist. I'll fix that later
                // when the leak is gone.
                if (Program.sTrayIcon.ContextMenuStrip != null)
                {
                    Program.sTrayIcon.ContextMenuStrip.Dispose();
                }

                // Add the default menu options.
                Program.sTrayIcon.ContextMenuStrip = new ContextMenuStrip();
                Program.sTrayIcon.ContextMenuStrip.ShowImageMargin = true;

                // DEBUG
                //TrackedToolStripMenuItem item = new TrackedToolStripMenuItem();
                //ProcessIcon.ni.ContextMenuStrip.Items.Add(item);

                // Regenerate a new list with the current settings.
                List<bool> menu_items_enabled = new List<bool>()
                {
                    Settings.Default.ShowNotifications,
                    Settings.Default.AutomaticStartup,
                    Settings.Default.KillOnIdle,
                    Settings.Default.KillDropbox,
                    Settings.Default.ForceOnDemandPowerPlan,
                };

                for (int i = 0; i < menu_names.Count; i++)
                {
                    var LoopItem = new TrackedToolStripMenuItem().Value;
                    //ToolStripMenuItem watt = new ToolStripMenuItem();
                    LoopItem.Text = menu_names[i];
                    LoopItem.Click += menu_events_handlers[i];
                    // TODO: Still need to avoid rebuilding the menu every time the menu changes
                    if (menu_items_enabled[i])
                    {
                        LoopItem.Image = Resources.checkmark;
                    }

                    Program.sTrayIcon.ContextMenuStrip.Items.Add(LoopItem);
                };

                /// ----------------------------------------------------------------------------------
                /// Always present or static menu items

                // WOAH, this creates a memory leak!
                //sTrayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator()); // Separator.
                Program.sTrayIcon.ContextMenuStrip.Items.Add(sSeparator);

                // Add one entry to this menu to kill everything
                var item = new TrackedToolStripMenuItem();
                item.Value.Text = "Force Idle Now!";
                item.Value.Image = Resources.Rss;
                item.Value.Click += delegate (object sender, EventArgs e) { Kill_Click(sender, e); };

                Program.sTrayIcon.ContextMenuStrip.Items.Add(item.Value);

                item = new TrackedToolStripMenuItem();
                item.Value.Text = "Add Executable";
                item.Value.Image = Resources.Plus;
                item.Value.Click += delegate (object sender, EventArgs e) { AddExecutable(sender, e); };
                Program.sTrayIcon.ContextMenuStrip.Items.Add(item.Value);

                item = new TrackedToolStripMenuItem();
                item.Value.Text = "About";
                item.Value.Image = Resources.About;
                item.Value.Click += delegate (object sender, EventArgs e) { About_Click(sender, e); };
                Program.sTrayIcon.ContextMenuStrip.Items.Add(item.Value);

                item = new TrackedToolStripMenuItem();
                item.Value.Text = "Exit";
                item.Value.Image = Resources.Exit;
                item.Value.Click += delegate (object sender, EventArgs e) { Exit_Click(sender, e); };
                Program.sTrayIcon.ContextMenuStrip.Items.Add(item.Value);
                /// end ----------------------------------------------------------------------------------
                //if (sTrayIcon.ContextMenuStrip != null)
                //{
                //    sTrayIcon.ContextMenuStrip.Dispose();
                //}
                //item.Dispose();

                GC.Collect();
                GC.WaitForFullGCComplete();
            }

            /// <summary>
            /// Handles the Click event of the Kill Background Processes control.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">
            /// The <see cref="System.EventArgs" /> instance containing the event data.
            /// </param>
            public static void Kill_Click(object sender, EventArgs e)
            {
                Program.DoIdleTasks();
            }

            public static void RegenerateMenu()
            {
                CreateFeedsMenu();
            }

            /// <summary>
            /// Handles the Click event of the About control.
            /// </summary>
            /// <param name="sender">The source of the event.</param>
            /// <param name="e">
            /// The <see cref="System.EventArgs" /> instance containing the event data.
            /// </param>
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
            /// <param name="e">
            /// The <see cref="System.EventArgs" /> instance containing the event data.
            /// </param>
            private static void AddExecutable(object sender, EventArgs e)
            {
                new AddFeed().ShowDialog();
            }

            /// <summary>
            /// Handles the Click event of the Exit control.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">
            /// The <see cref="System.EventArgs" /> instance containing the event data.
            /// </param>
            private static void Exit_Click(object sender, EventArgs e)
            {
                // Quit without further ado.
                Program.sTrayIcon.Visible = false;
                Application.Exit();
            }

            /// <summary>
            /// Handles the Click event of the Explorer control.
            /// </summary>
            /// <param name="sender">The source of the event.</param>
            /// <param name="e">
            /// The <see cref="System.EventArgs" /> instance containing the event data.
            /// </param>
            private static void Explorer_Click(object sender, EventArgs e)
            {
                Process.Start("explorer", null);
            }

            private static void FeedEntry_Click(object sender, EventArgs e, string u)
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

            private static void KillDropbox_Click(object sender, EventArgs e)
            {
                // TODO: Shouldn't we use the event data?
                Settings.Default.KillDropbox = !Settings.Default.KillDropbox;
                Settings.Default.Save();
                RegenerateMenu();
            }

            /// <summary>
            /// Handles the Click event of the KillOnIdle control.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">
            /// The <see cref="System.EventArgs" /> instance containing the event data.
            /// </param>
            private static void KillOnIdle_Click(object sender, EventArgs e)
            {
                // TODO: Shouldn't we use the event data?
                Settings.Default.KillOnIdle = !Settings.Default.KillOnIdle;
                Settings.Default.Save();
                RegenerateMenu();
            }

            /// <summary>
            /// Handles the Click event of the Notification Setting control.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">
            /// The <see cref="System.EventArgs" /> instance containing the event data.
            /// </param>
            private static void Notification_Setting_Click(object sender, EventArgs e)
            {
                // Flipping here can cause bugs, be more explicit so that the value is always right.
                Settings.Default.ShowNotifications = !Settings.Default.ShowNotifications;
                Settings.Default.Save();
                RegenerateMenu();
            }

            /// <summary>
            /// Handles the Click event of the Add Feed control.
            /// </summary>
            /// <param name="sender">The source of the event.</param>
            /// <param name="e">
            /// The <see cref="System.EventArgs" /> instance containing the event data.
            /// </param>
            private static void Set_Power_Plan_Click(object sender, EventArgs e)
            {
                Settings.Default.ForceOnDemandPowerPlan = !Settings.Default.ForceOnDemandPowerPlan;
                Settings.Default.Save();
                RegenerateMenu();
            }

            /// <summary>
            /// Handles the Click event of the Startup control.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">
            /// The <see cref="System.EventArgs" /> instance containing the event data.
            /// </param>
            private static void Startup_Click(object sender, EventArgs e)
            {
                Settings.Default.AutomaticStartup = !Settings.Default.AutomaticStartup;
                Settings.Default.Save();
                RegenerateMenu();
                Integration.AddToStartup();
            }
        }
    }

    internal class MyXmlReader : XmlTextReader
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
