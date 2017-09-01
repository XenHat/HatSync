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
    public class NotificationManager
    {
        public static void PushNotificationToOS(string content, string title = "")
        {
            if (!Properties.Settings.Default.ShowNotifications)
            {
                return;
            }
            if (title == "")
            {
                title = Program.ProductName;
            }
            ProcessIcon.ni.BalloonTipTitle = title;
            ProcessIcon.ni.BalloonTipText = content;
            ProcessIcon.ni.ShowBalloonTip(1);
        }
    }
}
